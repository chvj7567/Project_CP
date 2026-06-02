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
        public string PolicyName;
    }

    //# 한 스테이지를 끝까지 자동 플레이. AfterDrag 턴 루프 + Update 승패를 순수 포팅.
    //# 지원 범위: 이동제한(MoveCount>0) 또는 무제한+점수목표 + 일반블록만. 시간모드/특수블록은 Unsupported.
    public class SimGame
    {
        //# 무제한 점수게임 무한루프 방지 캡(턴 수). 점수목표 스테이지도 충분히 도달 가능한 여유값.
        private const int MaxTurns = 100000;

        //# 지원 = 시간모드 아님 + 모든 칸이 None/일반(Cat1~7). 특수블록·폭탄 있으면 미지원.
        private static bool IsSupported(SimStageData s)
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

            //# 보드 구성: 레코드(None 아님)면 그 state, None 이면 랜덤 일반(시드 RNG).
            SimBoard board = BuildInitialBoard(s, seed);
            //# 시작 보드의 기존 매치 정리 (실게임 CreateMap 은 매치 없는 상태로 시작).
            StabilizeStartBoard(board, checker, gravity, s);

            //# 시작 직후 점수목표 0/없음 등 즉시 클리어 케이스.
            if (CheckClear(s, result.FinalScore))
            {
                result.Clear = true;
                return result;
            }

            int moveBudget = s.IsMoveMode ? s.MoveCount : int.MaxValue;

            while (true)
            {
                //# 안전장치: 무제한 점수게임이 도달 못하고 도는 경우 탈출(MoveOver 로 처리).
                if (result.Turns > MaxTurns)
                {
                    result.FailReason = EFailReason.MoveOver;
                    return result;
                }

                if (checker.CanPlay(board) == false)
                {
                    //# 무수(無手) → 전체 리필(실게임 셔플 대용). 점수 없이 보드만 재생성.
                    foreach (SimBlock b in board.Grid)
                    {
                        b.Match = true;
                    }
                    gravity.Apply(board, s.BlockTypeCount);
                    ResolveCascades(board, checker, gravity, s, ref result);
                    if (CheckClear(s, result.FinalScore))
                    {
                        result.Clear = true;
                        return result;
                    }
                    continue;
                }

                SimMove? mv = policy.ChooseMove(board, checker);
                //# CanPlay true 와 모순 방지(다음 루프서 재셔플 시도).
                if (mv == null)
                    continue;

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
                ResolveCascades(board, checker, gravity, s, ref result);

                if (CheckClear(s, result.FinalScore))
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
                    EBlockState st = s.InitialStates[r * s.BoardSize + c];
                    if (st == EBlockState.None)
                    {
                        st = (EBlockState)rng.Next(0, s.BlockTypeCount);
                    }
                    board.SetState(r, c, st);
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

        //# 매치 제거→점수(블록당 1점)→낙하/리필→재검사 를 매치 없을 때까지(연쇄).
        private void ResolveCascades(SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s, ref SimResult result)
        {
            checker.CheckMap(board);
            while (checker.IsMatch)
            {
                int cleared = 0;
                foreach (SimBlock b in board.Grid)
                {
                    if (b.Match)
                    {
                        ++cleared;
                    }
                }
                result.FinalScore += cleared; //# 실게임 RemoveMatchBlock: 제거 블록당 +1
                result.Turns += 1;
                gravity.Apply(board, s.BlockTypeCount);
                checker.CheckMap(board);
            }
        }

        //# M1 = 목표블록 없음(일반블록만). targetScore>0 면 점수 도달이 클리어, targetScore<=0 면 즉시(목표 없음).
        private static bool CheckClear(SimStageData s, int score)
        {
            if (s.TargetScore > 0)
                return score >= s.TargetScore;

            return true;
        }
    }
}
