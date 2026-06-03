using static Defines;

namespace CatPang.Sim
{
    //# 실 GPBombResolver.CreateBombBlock(L346) 포팅: 매치 점수→특수블록 생성.
    //# arrowPangIndex: 어느 화살표 변형을 낼지 분기(호출측 주입, 실게임 _arrowPangIndex 미러).
    public static class SimBombFactory
    {
        public static void CreateBombs(SimBoard board, int arrowPangIndex, int moveIndex1, int moveIndex2)
        {
            int size = board.Size;

            //# 실 L352~376: 1루프 — squareMatch → CatPang, h&v 교차 → Arrow5/Arrow6.
            //# 교차 생성 후 ClearScoreNeighbors 호출(실 L373) — 인접 팔 점수 초기화로 2루프 중복 생성 방지.
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    SimBlock b = board.Grid[r, c];

                    if (b.SquareMatch)
                    {
                        //# 실 L360~365: squareMatch → CatPang. GetPangType() 은 항상 CatPang 반환(Block.cs L484).
                        CreateAt(b, EBlockState.CatPang);
                    }

                    if (b.HScore >= SimMatchChecker.MinMatchCount && b.VScore >= SimMatchChecker.MinMatchCount)
                    {
                        //# 실 L367~374: 교차 → arrowPangIndex 분기. 생성 후 ClearScoreNeighbors(실 L373).
                        EBlockState crossBomb = arrowPangIndex == 1 ? EBlockState.Arrow5 : EBlockState.Arrow6;
                        CreateAt(b, crossBomb);
                        ClearScoreNeighbors(board, r, c);
                    }
                }
            }

            //# 실 L378~434: 2루프 — hScore>3 → Arrow1/4, vScore>3 → Arrow3/2 (이동칸 우선).
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    SimBlock b = board.Grid[r, c];

                    if (b.HScore > SimMatchChecker.MinMatchCount)
                    {
                        CreateLine(board, r, c, true, arrowPangIndex, moveIndex1, moveIndex2);
                    }
                    else if (b.VScore > SimMatchChecker.MinMatchCount)
                    {
                        CreateLine(board, r, c, false, arrowPangIndex, moveIndex1, moveIndex2);
                    }
                }
            }
        }

        //# 생성 칸: state 교체 + match 해제(낙하/제거 대상에서 빠지고 특수블록으로 잔존).
        private static void CreateAt(SimBlock b, EBlockState bomb)
        {
            b.State = bomb;
            b.Match = false;
            b.Hp = -1;
            b.ResetScore();
        }

        //# 실 ClearScoreNeighbors(L443~459) 포팅: 교차점에서 4방향으로 연속 점수 칸 초기화.
        //# idx=1 시작(교차점 자신 제외). hScore>0 또는 vScore>0 인 칸까지만, 없으면 break.
        private static void ClearScoreNeighbors(SimBoard board, int row, int col)
        {
            int max = board.Size;
            (int dr, int dc)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach ((int dr, int dc) d in dirs)
            {
                for (int idx = 1; idx < max; ++idx)
                {
                    int tr = row + d.dr * idx;
                    int tc = col + d.dc * idx;
                    if (board.IsValid(tr, tc) == false)
                        break;
                    SimBlock tb = board.Grid[tr, tc];
                    if (tb.HScore > 0 || tb.VScore > 0)
                    {
                        tb.ResetScore();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        //# 실 L388~432: 라인 순회 — 이동칸 우선 화살표 배치, 없으면 시작칸. 모든 칸 ResetScore.
        private static void CreateLine(SimBoard board, int r, int c, bool horizontal,
            int arrowPangIndex, int moveIndex1, int moveIndex2)
        {
            SimBlock start = board.Grid[r, c];
            int len = horizontal ? start.HScore : start.VScore;
            bool placed = false;

            for (int idx = 0; idx < len; ++idx)
            {
                int tr = horizontal ? r : r + idx;
                int tc = horizontal ? c + idx : c;
                if (board.IsValid(tr, tc) == false)
                    continue;
                SimBlock tb = board.Grid[tr, tc];
                if (tb.Index == moveIndex1 || tb.Index == moveIndex2)
                {
                    CreateAt(tb, ArrowFor(horizontal, arrowPangIndex));
                    placed = true;
                }
                else
                {
                    tb.ResetScore();
                }
            }

            if (placed == false)
            {
                CreateAt(start, ArrowFor(horizontal, arrowPangIndex));
            }
        }

        //# 실 Arrow 매핑(L398~431): horizontal=true → Arrow1(idx1)/Arrow4(else), false → Arrow3(idx1)/Arrow2(else).
        private static EBlockState ArrowFor(bool horizontal, int arrowPangIndex)
        {
            if (horizontal)
                return arrowPangIndex == 1 ? EBlockState.Arrow1 : EBlockState.Arrow4;
            return arrowPangIndex == 1 ? EBlockState.Arrow3 : EBlockState.Arrow2;
        }
    }
}
