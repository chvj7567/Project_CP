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
    //# M3a: 폭탄 시스템(생성/발동/조합) 통합.
    //# [주의] 순수 3-매치(HScore==3, VScore==0, squareMatch==false) 는 폭탄 생성 없이 기존과 동일.
    //# 4매치·교차·사각 매치는 CreateBombs 가 특수블록을 잔존시키므로 메트릭(점수·이동수)이 M2 기준과 달라진다.
    //# → 53-stage 회귀는 "크래시 없음/CapHit 신규 없음" 기준인지 "M2 동일 메트릭" 기준인지 확인 필요(DONE_WITH_CONCERNS).
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

        //# M3a: 폭탄 발동·범위 RNG. seed+2 로 분리.
        private SimBombResolver _bombResolver;

        //# M3a: 조합 색폭탄 승급 RNG. seed+3 으로 분리.
        private System.Random _comboRng;

        //# M3a: 화살표 생성 방향 토글(arrowPangIndex). result.Turns 홀짝으로 결정성 유지.
        //# 실게임 _arrowPangIndex 는 0/1 토글, 여기선 턴수 기반으로 동일 패턴 모방.
        private int ArrowPangIndex(int turns) { return turns % 2; }

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
                //# M3a: 폭탄 계열 — Arrow1~6(10~15), CatPang(18), 5색폭탄 PinkBomb(19)~BlueBomb(23), RainbowPang(52).
                //# Fish(24)/Ball(53) 은 추가 안 함 → M3b 에서 대응.
                //# 주의: PinkBomb=19, BlueBomb=23 (Defines.cs 확인). 지시서의 YellowBomb..PinkBomb 은 오타 — 실제 하한은 PinkBomb.
                //# SimGame.cs L331 의 _comboRng.Next(PinkBomb, BlueBomb+1) 와 동일 범위 규약.
                if (st >= EBlockState.Arrow1 && st <= EBlockState.Arrow6)
                {
                    continue;
                }
                if (st == EBlockState.CatPang)
                {
                    continue;
                }
                if (st >= EBlockState.PinkBomb && st <= EBlockState.BlueBomb)
                {
                    continue;
                }
                if (st == EBlockState.RainbowPang)
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
            //# M3a: 폭탄용 RNG(seed+2), 조합 색폭탄 승급용 RNG(seed+3).
            _bombResolver = new SimBombResolver(new System.Random(seed + 2));
            _comboRng = new System.Random(seed + 3);

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
                    //# 리셔플 경로: moveIndex 없음(-1, -1). 이 경로에는 특수블록 생성 없음.
                    ResolveCascades(board, checker, gravity, s, ref result, -1, -1);
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
                int r1 = m.Row;
                int c1 = m.Col;
                int r2 = m.Row + m.Dr;
                int c2 = m.Col + m.Dc;

                board.Swap(r1, c1, r2, c2);

                //# M3a: 스왑한 두 블록. swap 후 좌표는 고정이므로 r1,c1/r2,c2 로 직접 접근.
                SimBlock blockA = board.Grid[r1, c1];
                SimBlock blockB = board.Grid[r2, c2];
                int mi1 = blockA.Index;
                int mi2 = blockB.Index;

                //# M3a: 조합 분기 — 한쪽이라도 폭탄이면 SimComboResolver 로 판정.
                //# 주의: 조합 분기는 checker.CheckMap 없이 바로 발동하고 self-consume 후 ResolveCascades 에 넘긴다.
                //# ResolveCascades 첫 줄이 CheckMap → ResetAllMatch 이므로, 마크를 살리려면 분기 내에서
                //# 직접 cleared 집계 + gravity 를 수행해야 한다.
                SimComboResolver.EComboResult combo = SimComboResolver.Resolve(blockA, blockB);
                if (combo != SimComboResolver.EComboResult.None)
                {
                    HandleCombo(board, checker, gravity, s, ref result, combo, blockA, blockB, r1, c1, r2, c2);

                    result.MovesUsed += 1;
                    if (s.IsMoveMode)
                    {
                        moveBudget -= 1;
                        if (moveBudget <= 0)
                        {
                            result.FailReason = EFailReason.MoveOver;
                            return result;
                        }
                    }
                    if (CheckClear(board, s, result.FinalScore))
                    {
                        result.Clear = true;
                        return result;
                    }
                    continue;
                }

                //# 일반 매치 검사 경로.
                checker.SetMoveIndices(mi1, mi2);
                checker.CheckMap(board);
                if (checker.IsMatch == false)
                {
                    //# 매치 못 만든 swap → 되돌림(이동 차감 없음). 폭탄 조합 수는 이미 위 분기에서 처리.
                    board.Swap(r1, c1, r2, c2);
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

                ResolveCascades(board, checker, gravity, s, ref result, mi1, mi2);

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

        //# M3a: 조합 분기 실행. 마크 → self-consume(cleared 집계 + gravity) → ResolveCascades 연쇄.
        //# ResolveCascades 가 CheckMap → ResetAllMatch 로 시작하므로, 여기서 조합 마크를 직접 소비해야 한다.
        private void HandleCombo(
            SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s,
            ref SimResult result,
            SimComboResolver.EComboResult combo,
            SimBlock blockA, SimBlock blockB,
            int r1, int c1, int r2, int c2)
        {
            switch (combo)
            {
                case SimComboResolver.EComboResult.BoomAll:
                {
                    //# 색폭탄 2개: 전체 폭발. 마크 후 self-consume.
                    _bombResolver.BoomAll(board, checker);
                    ConsumeMarkedBlocks(board, checker, gravity, s, ref result);
                    break;
                }
                case SimComboResolver.EComboResult.Boom3:
                {
                    //# PinkBomb + 상대방: 상대방 색 전부 제거. PinkBomb 위치 판정.
                    int pinkRow;
                    int pinkCol;
                    EBlockState targetColor;
                    if (blockA.State == EBlockState.PinkBomb)
                    {
                        pinkRow = r1;
                        pinkCol = c1;
                        targetColor = blockB.State;
                    }
                    else
                    {
                        pinkRow = r2;
                        pinkCol = c2;
                        targetColor = blockA.State;
                    }
                    _bombResolver.Boom3(board, pinkRow, pinkCol, targetColor);
                    ConsumeMarkedBlocks(board, checker, gravity, s, ref result);
                    break;
                }
                case SimComboResolver.EComboResult.DetonateA:
                {
                    //# 한쪽 폭탄만 단독 발동(색폭탄·화살표·CatPang·Rainbow 모두 경유 가능).
                    DetonateSingle(board, checker, gravity, s, ref result, blockA, r1, c1);
                    break;
                }
                case SimComboResolver.EComboResult.DetonateB:
                {
                    //# 한쪽 폭탄만 단독 발동.
                    DetonateSingle(board, checker, gravity, s, ref result, blockB, r2, c2);
                    break;
                }
                case SimComboResolver.EComboResult.MergeToColorBomb:
                {
                    //# 화살표/CatPang 2개 → A 칸을 색폭탄으로 승급, B 칸 Match=true.
                    //# 실게임 L800-802: Random.Range(PinkBomb, BlueBomb+1). 시드 RNG(_comboRng) 사용.
                    int colorBombInt = _comboRng.Next((int)EBlockState.PinkBomb, (int)EBlockState.BlueBomb + 1);
                    blockA.ChangeBlockState = (EBlockState)colorBombInt;
                    blockB.Match = true;
                    //# ApplyChanges 로 A 칸 상태전이 반영 후 B 칸 소비.
                    SimSpecialBlocks.ApplyChanges(board);
                    ConsumeMarkedBlocks(board, checker, gravity, s, ref result);
                    break;
                }
            }

            //# 조합 후 연쇄 흡수. moveIndex 는 조합 수라 -1 로 전달(폭탄 생성 대상 이동 칸 없음).
            SimSpecialBlocks.RunCreators(board, _creatorRng);
            SimSpecialBlocks.ApplyChanges(board);
            ResolveCascades(board, checker, gravity, s, ref result, -1, -1);
        }

        //# M3a: 단일 폭탄 발동 헬퍼. BombKind 전 종류를 처리한다.
        //# DetonateA/B 는 IsSpecialBomb 단독뿐 아니라 Arrow/CatPang/Rainbow 도 경유하므로 전 BombKind 필요.
        //# 실게임 AfterDrag L793-805: block1.Bomb() / block2.Bomb() 이 전 종류를 발동함과 동일.
        private void DetonateSingle(
            SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s,
            ref SimResult result,
            SimBlock bomb, int row, int col)
        {
            //# 폭탄을 match 표시 후 DetonateMatchedBombs 로 기하 발동 — 중복 호출 없이 전 종류 커버.
            bomb.Match = true;
            DetonateMatchedBombs(board, checker);
            ConsumeMarkedBlocks(board, checker, gravity, s, ref result);
        }

        //# M3a: 조합 분기의 self-consume. match=true 인 블록을 점수 집계 + gravity 로 직접 소비.
        //# ResolveCascades 가 CheckMap → ResetAllMatch 로 시작하므로 조합 분기는 여기서 소비해야 한다.
        private static void ConsumeMarkedBlocks(
            SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s,
            ref SimResult result)
        {
            int cleared = 0;
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
            result.FinalScore += cleared;
            result.Turns += 1;
            gravity.Apply(board, s.BlockTypeCount);
            SimSpecialBlocks.ApplyChanges(board);
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
        //# M3a: 매치 branch 에 CreateBombs + DetonateMatchedBombs 추가.
        //# moveIndex1/2: 이번 수의 이동 칸 index. 리셔플/조합 경로는 -1,-1 전달.
        private void ResolveCascades(
            SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s,
            ref SimResult result, int moveIndex1, int moveIndex2)
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

                checker.SetMoveIndices(moveIndex1, moveIndex2);
                checker.CheckMap(board);
                bool matchedThisIter = checker.IsMatch;
                bool collectedThisIter = false;

                if (matchedThisIter)
                {
                    //# M3a (1) 생성: 매치 점수로 특수블록 생성. 생성된 칸은 Match=false 가 되어 낙하/제거 제외.
                    //# 실게임 AfterDrag do-while 825: CreateBombBlock 이 RemoveMatchBlock 바로 뒤, DownBlock 전.
                    SimBombFactory.CreateBombs(board, ArrowPangIndex(result.Turns), moveIndex1, moveIndex2);

                    //# M3a (2) 발동: 매치 상태인 기존 폭탄을 발동. 이번 iteration 당 1회만 호출.
                    //# 발동이 새로 match 로 만든 칸은 gravity 로 소비되지 않고 다음 cascade iteration 이 흡수(연쇄).
                    //# MaxCascadeDepth 캡이 무한 연쇄의 종국 안전망.
                    DetonateMatchedBombs(board, checker);

                    int cleared = 0;
                    //# M3a (3) 점수 + 인접 데미지: match=true 인 블록 집계. 생성으로 match=false 된 폭탄은 제외.
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

                    //# M3a: 2번째 cascade iteration 부터는 moveIndex 무효(-1). 특수블록 생성 위치 편향 방지.
                    moveIndex1 = -1;
                    moveIndex2 = -1;

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

        //# M3a: 매치 상태인 폭탄을 발동. iteration 당 1회 호출, 기존 MaxCascadeDepth 가 무한 연쇄 종국 안전망.
        //# 한 번 match=true 된 폭탄은 이후 gravity 로 보드에서 제거되므로 같은 칸이 두 번 발동되지 않는다.
        private void DetonateMatchedBombs(SimBoard board, SimMatchChecker checker)
        {
            for (int r = 0; r < board.Size; ++r)
            {
                for (int c = 0; c < board.Size; ++c)
                {
                    SimBlock b = board.Grid[r, c];
                    if (b.Match == false)
                        continue;
                    switch (b.BombKind())
                    {
                        case SimBlock.EBombKind.Bomb1:   _bombResolver.Bomb1(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb2:   _bombResolver.Bomb2(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb4:   _bombResolver.Bomb4(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb5:   _bombResolver.Bomb5(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb6:   _bombResolver.Bomb6(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb7:   _bombResolver.Bomb7(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb8:   _bombResolver.Bomb8(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb9:   _bombResolver.Bomb9(board, checker, r, c);   break;
                        case SimBlock.EBombKind.Bomb10:  _bombResolver.Bomb10(board, checker, r, c);  break;
                        case SimBlock.EBombKind.Bomb11:  _bombResolver.Bomb11(board, checker, r, c);  break;
                        case SimBlock.EBombKind.Bomb12:  _bombResolver.Bomb12(board, checker, r, c);  break;
                        case SimBlock.EBombKind.Rainbow: _bombResolver.RainbowPang(board, b);          break;
                    }
                }
            }
        }

        //# M3a: 테스트 전용 노출. _bombResolver 미주입 시 로컬 생성(화이트박스 단위 테스트용).
        //# CheckMap 을 호출하지 않는다 — 테스트가 직접 Match 플래그를 세운 상태 그대로 발동.
        internal void TestDetonate(SimBoard board, SimMatchChecker checker)
        {
            if (_bombResolver == null)
            {
                _bombResolver = new SimBombResolver(new System.Random(0));
            }
            DetonateMatchedBombs(board, checker);
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
