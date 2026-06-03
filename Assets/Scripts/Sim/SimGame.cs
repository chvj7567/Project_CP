using static Defines;

namespace CatPang.Sim
{
    //# 한 판 결과. 클리어 여부 / 실패사유 / 이동수 / 턴수(연쇄 포함) / 최종점수 / M1 미지원 여부.
    public struct SimResult
    {
        public int Stage;
        public bool Clear;
        public EFailReason FailReason;
        public int MovesUsed;
        public int Turns;        //# 연쇄 포함 총 해소 횟수
        public int FinalScore;
        public bool Unsupported; //# 시간모드/특수블록/폭탄 등 M1 미지원
        public bool CapHit;      //# 무한방지 캡(턴/iteration/연쇄/리셔플) 으로 강제 종료됨 → 메트릭 신뢰 불가
        public string PolicyName;
    }

    //# 한 스테이지를 끝까지 자동 플레이. AfterDrag 턴 루프 + Update 승패를 순수 포팅.
    //# 지원 범위: 이동제한(MoveCount>0) 또는 무제한+점수목표 + 일반블록만. 시간모드/특수블록은 Unsupported.
    public class SimGame
    {
        //# 무제한 점수게임 도달 캡(턴 수). 점수목표 스테이지도 충분히 도달 가능하되 비현실적으로 크지 않게.
        //# (기존 100000 은 한 판이 수십초 걸려 9000판 배치가 비실용적 → 현실적 값으로 축소.)
        private const int MaxTurns = 5000;

        //# 메인 루프 iteration 절대 캡. Turns 와 무관하게 모든 경로(리셔플·null수 포함)를 세 무한루프·교착 방지.
        //# Creator↔리셔플 번갈아 진행(매치는 되나 목표블록 안 줄어듦) 교착도 이 캡으로 확실히 탈출.
        private const int MaxIterations = 10000;
        //# 연속 리셔플(진전 없는 무수) 한계. 보드가 Wall/Creator 로 영구 막히면 이 한계로 미클리어 탈출.
        private const int MaxReshuffleStreak = 50;
        //# 한 수의 연쇄(ResolveCascades) 깊이 절대 캡. 정상 연쇄는 수십 회를 넘지 않는다.
        //# 리필이 매번 새 매치를 만들거나 CatBox 가 영구 급식되는 비정상 상태를 이 캡으로 끊어 메인 루프로 복귀시킨다.
        //# (메인 루프 가드는 ResolveCascades 밖에 있어, 여기서 멈추지 않으면 영원히 제어가 안 돌아온다.)
        //# 200 은 정상 연쇄(통상 5~15회)를 절대 자르지 않으면서 병리 케이스 비용을 작게 묶는 값.
        private const int MaxCascadeDepth = 200;

        //# Creator 변환 방향 RNG. Run 시작에서 seed+1 로 주입(데미지 RNG seed 와 분리).
        private System.Random _creatorRng;

        //# 진단용: 마지막 Run 의 최종 보드(in-place 변이라 참조 1회 저장으로 종국 상태 반영).
        //# Creator 사망 여부·잔존 목표블록 census 검사에 사용. 정식 메트릭 경로엔 미사용.
        public SimBoard LastBoard { get; private set; }

        //# M2 지원 = 시간모드 아님 + 모든 칸이 {None, Cat1~7, Wall, Potal, Creator, CatBox}.
        //# Fish/Ball/Arrow/특수폭탄/Rainbow/CatPang 있으면 미지원(→M3).
        //# public: 커버리지 카운트(play 없이 분류만)를 외부 러너에서 직접 호출하기 위함.
        public static bool IsSupported(SimStageData s)
        {
            if (s.IsTimeMode)
                return false;

            foreach (EBlockState st in s.InitialStates)
            {
                if (st == EBlockState.None)
                {
                    continue;
                }
                if (st >= EBlockState.Cat1 && st <= EBlockState.Cat7)
                {
                    continue;
                }
                if (st == EBlockState.Wall || st == EBlockState.Potal)
                {
                    continue;
                }
                if (st == EBlockState.WallCreator || st == EBlockState.PotalCreator)
                {
                    continue;
                }
                if (st >= EBlockState.CatBox1 && st <= EBlockState.CatBox5)
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public SimResult Run(SimStageData s, ISimAiPolicy policy, int seed)
        {
            SimResult result = new SimResult
            {
                Stage = s.Stage,
                PolicyName = policy.Name,
                FailReason = EFailReason.None,
            };

            if (IsSupported(s) == false)
            {
                result.Unsupported = true;
                return result;
            }

            SimMatchChecker checker = new SimMatchChecker();
            SimGravity gravity = new SimGravity(seed);

            //# Damage 의 일반블록 전환 RNG 시드 주입(결정성). _creatorRng 는 분리된 시드(seed+1).
            SimBlock.SetDamageRng(new System.Random(seed));
            _creatorRng = new System.Random(seed + 1);

            //# 보드 구성: 레코드(None 아님)면 그 state, None 이면 랜덤 일반(시드 RNG).
            SimBoard board = BuildInitialBoard(s, seed);
            LastBoard = board; //# in-place 변이라 이후 어느 return 에서도 최종 상태를 반영.
            //# 시작 보드의 기존 매치 정리 (실게임 CreateMap 은 매치 없는 상태로 시작).
            StabilizeStartBoard(board, checker, gravity, s);

            //# 시작 직후 점수목표 0/없음 + 목표블록 없음 등 즉시 클리어 케이스.
            if (CheckClear(board, s, result.FinalScore))
            {
                result.Clear = true;
                return result;
            }

            int moveBudget = s.IsMoveMode ? s.MoveCount : int.MaxValue;

            //# 무한루프 절대 방지: 메인 루프 iteration 캡. Turns(연쇄 횟수)와 무관하게 모든 경로를 센다.
            //# 리셔플 무한반복(Wall/Creator 로 매치 불가) 이나 policy null 반복도 이 캡으로 탈출.
            int iterations = 0;
            int reshuffleStreak = 0; //# 연속 리셔플 횟수 — 진전 없는 무수 상태 조기 탈출용.

            while (true)
            {
                //# 안전장치: 무제한 점수게임이 도달 못하고 도는 경우 탈출(MoveOver 로 처리).
                ++iterations;
                if (result.Turns > MaxTurns || iterations > MaxIterations)
                {
                    result.FailReason = EFailReason.MoveOver;
                    result.CapHit = true;
                    return result;
                }

                if (checker.CanPlay(board) == false)
                {
                    //# 무수(無手) → 전체 리필(실게임 셔플 대용). 점수 없이 보드만 재생성.
                    //# 연속 리셔플이 한계를 넘으면(보드가 Wall/Creator 로 영구 막힘) 미클리어로 탈출.
                    ++reshuffleStreak;
                    if (reshuffleStreak > MaxReshuffleStreak)
                    {
                        result.FailReason = EFailReason.MoveOver;
                        result.CapHit = true;
                        return result;
                    }
                    foreach (SimBlock b in board.Grid)
                    {
                        if (b.IsNormal())
                        {
                            b.Match = true;
                        }
                    }
                    gravity.Apply(board, s.BlockTypeCount);
                    ResolveCascades(board, checker, gravity, s, ref result);
                    if (CheckClear(board, s, result.FinalScore))
                    {
                        result.Clear = true;
                        return result;
                    }
                    continue;
                }
                reshuffleStreak = 0; //# 유효 수가 생기면 리셔플 연속 카운트 초기화.

                SimMove? mv = policy.ChooseMove(board, checker);
                //# CanPlay true 인데 policy 가 null 이면 무한 방지 위해 리셔플 경로로 흘려보냄(다음 iter 에서 처리).
                if (mv == null)
                {
                    ++reshuffleStreak;
                    if (reshuffleStreak > MaxReshuffleStreak)
                    {
                        result.FailReason = EFailReason.MoveOver;
                        result.CapHit = true;
                        return result;
                    }
                    continue;
                }

                SimMove m = mv.Value;
                board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
                checker.CheckMap(board);
                if (checker.IsMatch == false)
                {
                    //# 매치 못 만든 swap → 되돌림(이동 차감 없음).
                    board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
                    board.ResetAllMatch();
                    continue;
                }

                result.MovesUsed += 1;

                //# 실게임 AfterDrag 840-845 의 checkCreateBlock 1회 호출 — 연쇄 전 단계.
                //# Creator 의 Damage 와 매치 CheckArround 의 _checkDamage 가드 충돌을 피하려고
                //# RunCreators 를 ResolveCascades(매 CheckMap 시작에서 가드 리셋) 보다 먼저 별도 단계로 호출한다.
                //# 단순화: 실게임은 트리거 매치 제거 후 creator 가 도는데, Sim 은 매치 제거 전 1회 — 의도된 차이.
                SimSpecialBlocks.RunCreators(board, _creatorRng);
                SimSpecialBlocks.ApplyChanges(board);

                ResolveCascades(board, checker, gravity, s, ref result);

                if (CheckClear(board, s, result.FinalScore))
                {
                    result.Clear = true;
                    return result;
                }

                if (s.IsMoveMode)
                {
                    moveBudget -= 1;
                    if (moveBudget <= 0)
                    {
                        result.FailReason = EFailReason.MoveOver;
                        return result;
                    }
                }
            }
        }

        //# 레코드 칸은 그 state, None 칸은 랜덤 일반블록(시드 RNG) 으로 채운 시작 보드.
        private SimBoard BuildInitialBoard(SimStageData s, int seed)
        {
            System.Random rng = new System.Random(seed);
            SimBoard board = new SimBoard(s.BoardSize);
            for (int r = 0; r < s.BoardSize; ++r)
            {
                for (int c = 0; c < s.BoardSize; ++c)
                {
                    int idx = r * s.BoardSize + c;
                    EBlockState st = s.InitialStates[idx];
                    int hp = -1;
                    if (st == EBlockState.None)
                    {
                        st = (EBlockState)rng.Next(0, s.BlockTypeCount);
                    }
                    else
                    {
                        hp = s.InitialHps[idx];
                    }
                    board.SetState(r, c, st);
                    board.Grid[r, c].Hp = hp;
                }
            }
            return board;
        }

        //# 시작 보드에 이미 매치가 있으면 점수 없이 정리(실게임 CreateMap 은 매치 없는 상태로 시작).
        //# guard 카운터로 무한방지.
        private void StabilizeStartBoard(SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s)
        {
            checker.CheckMap(board);
            int guard = 0;
            while (checker.IsMatch && guard++ < 1000)
            {
                gravity.Apply(board, s.BlockTypeCount); //# 매치 제거+리필 (시작 정리라 점수 미가산)
                checker.CheckMap(board);
            }
        }

        //# 실게임 AfterDrag do-while 822-852 포팅:
        //#   do { 제거/낙하/리필; UpdateMap; CheckMap } while (isMatch || await CatInTheBox());
        //# `||` 단락 평가라 CatInTheBox(수집)는 그 iteration 에 매치가 없을 때만 실행된다.
        //# => 매치 iteration 과 수집 iteration 은 배타적. 매 iteration 무조건 수집하면
        //#    연쇄 중간 박스 데미지+고양이 제거가 이후 매치를 바꿔 점수/이동/클리어가 발산한다.
        private void ResolveCascades(SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s, ref SimResult result)
        {
            int depth = 0;
            while (true)
            {
                //# 연쇄 깊이 캡 — 비정상 무한 연쇄를 끊고 메인 루프로 복귀(메인 가드가 종국 처리).
                if (++depth > MaxCascadeDepth)
                {
                    result.CapHit = true;
                    return;
                }

                checker.CheckMap(board);
                bool matchedThisIter = checker.IsMatch;
                bool collectedThisIter = false;

                if (matchedThisIter)
                {
                    int cleared = 0;
                    //# 매치 제거 블록 점수 + 인접 데미지(Wall/Potal 깎기).
                    for (int r = 0; r < board.Size; ++r)
                    {
                        for (int c = 0; c < board.Size; ++c)
                        {
                            if (board.Grid[r, c].Match)
                            {
                                ++cleared;
                                checker.CheckArround(board, r, c, s.BlockTypeCount);
                            }
                        }
                    }
                    result.FinalScore += cleared; //# 실게임 RemoveMatchBlock: 제거 블록당 +1
                    result.Turns += 1;

                    gravity.Apply(board, s.BlockTypeCount); //# 매치=true 제거+낙하(Wall 차단)+리필
                    SimSpecialBlocks.ApplyChanges(board);    //# Damage 로 예약된 일반블록 전환 적용
                }
                else
                {
                    //# 매치 없을 때만 CatBox 수집(실게임 `|| await CatInTheBox()` 단락 평가).
                    collectedThisIter = SimSpecialBlocks.CollectCatBoxes(board);
                    if (collectedThisIter)
                    {
                        gravity.Apply(board, s.BlockTypeCount);
                        SimSpecialBlocks.ApplyChanges(board);
                    }
                }

                if ((matchedThisIter || collectedThisIter) == false)
                    break;
            }
        }

        //# 승패: 목표블록(checkHp 이고 hp>0, 또는 Fish/Ball) 없음 AND 점수도달. (실게임 Update 177-206)
        private static bool CheckClear(SimBoard board, SimStageData s, int score)
        {
            //# 1. 목표블록 제거 검사
            for (int r = 0; r < board.Size; ++r)
            {
                for (int c = 0; c < board.Size; ++c)
                {
                    SimBlock b = board.Grid[r, c];
                    if (b.State == EBlockState.RainbowPang)
                        continue;
                    if (b.CheckHp() == false)
                        continue;
                    //# M3 대비 Fish/Ball 검사 자리(M2 엔 해당 블록 없음).
                    if (b.Hp > 0)
                        return false;
                }
            }
            //# 2. 점수
            if (s.TargetScore > 0 && score < s.TargetScore)
                return false;
            return true;
        }
    }
}
