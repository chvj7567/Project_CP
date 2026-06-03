using System.Collections.Generic;
using static Defines;

namespace CatPang.Sim
{
    //# GPMatchChecker 의 3·사각 매치 + CanPlay 를 순수 재구현. M1: 일반블록만, 폭탄/특수 미고려.
    public class SimMatchChecker
    {
        public const int MinMatchCount = 3;
        public bool IsMatch;

        //# 사각 → 행별 3매치 → 열별 3매치 순서. GPMatchChecker.CheckMap 과 동일. 호출 시 IsMatch 초기화.
        public void CheckMap(SimBoard board)
        {
            IsMatch = false;
            board.ResetAllMatch();
            //# 실게임 GPMatchChecker.CheckMap 42-44: 매 검사 시작 시 데미지 가드 리셋.
            foreach (SimBlock b in board.Grid)
            {
                b.ResetCheckDamage();
            }
            int size = board.Size;

            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    CheckSquare(board, r, c);
                }
            }

            for (int r = 0; r < size; ++r)
            {
                List<SimBlock> line = new List<SimBlock>();
                for (int c = 0; c < size; ++c)
                {
                    line.Add(board.Grid[r, c]);
                }
                Check3(line);
            }

            for (int c = 0; c < size; ++c)
            {
                List<SimBlock> line = new List<SimBlock>();
                for (int r = 0; r < size; ++r)
                {
                    line.Add(board.Grid[r, c]);
                }
                Check3(line);
            }
        }

        //# (r,c)(r+1,c)(r,c+1)(r+1,c+1) 4칸이 모두 일반·동일·미매치면 4칸 Match. 좌상단 SquareMatch.
        private void CheckSquare(SimBoard board, int r, int c)
        {
            if (board.IsValid(r + 1, c + 1) == false)
                return;

            SimBlock a = board.Grid[r, c];
            SimBlock b = board.Grid[r + 1, c];
            SimBlock d = board.Grid[r, c + 1];
            SimBlock e = board.Grid[r + 1, c + 1];

            if (a.IsNormal() == false || b.IsNormal() == false || d.IsNormal() == false || e.IsNormal() == false)
                return;
            if (a.Match || b.Match || d.Match || e.Match)
                return;
            if (a.State != b.State || a.State != d.State || a.State != e.State)
                return;

            a.Match = b.Match = d.Match = e.Match = true;
            a.SquareMatch = true; //# 이동관여 칸 추적은 M2. M1 은 좌상단 고정.
            IsMatch = true;
        }

        //# 한 행/열에서 동일 일반블록이 MinMatchCount 이상 연속이면 Match. 비일반 블록서 카운트 리셋.
        private void Check3(List<SimBlock> line)
        {
            EBlockState state = EBlockState.None;
            int count = 0;
            for (int i = 0; i < line.Count; ++i)
            {
                SimBlock b = line[i];
                if (b.IsNormal() == false)
                {
                    state = EBlockState.None;
                    count = 0;
                    continue;
                }

                if (state == EBlockState.None)
                {
                    state = b.State;
                    count = 1;
                }
                else if (state == b.State)
                {
                    ++count;
                    if (count >= MinMatchCount)
                    {
                        int t = i;
                        for (int j = 0; j < count; ++j)
                        {
                            line[t--].Match = true;
                        }
                        IsMatch = true;
                    }
                }
                else
                {
                    state = b.State;
                    count = 1;
                }
            }
        }

        //# 가능한 수 존재 여부: 모든 칸 상/하/좌/우 인접 swap 후 매치 검사. swap 은 항상 복원.
        public bool CanPlay(SimBoard board)
        {
            int size = board.Size;
            (int dr, int dc)[] dirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    foreach ((int dr, int dc) d in dirs)
                    {
                        int nr = r + d.dr;
                        int nc = c + d.dc;
                        if (board.IsValid(nr, nc) == false)
                            continue;

                        //# 실게임 규칙: 양쪽 칸 모두 드래그 가능해야 스왑(고정블록 제외).
                        if (board.Grid[r, c].CanNotDrag() || board.Grid[nr, nc].CanNotDrag())
                            continue;

                        board.Swap(r, c, nr, nc);
                        CheckMap(board);
                        bool matched = IsMatch;
                        board.Swap(r, c, nr, nc);
                        if (matched)
                        {
                            board.ResetAllMatch();
                            return true;
                        }
                    }
                }
            }
            board.ResetAllMatch();
            return false;
        }

        //# 실게임 GPMatchChecker.CheckArround 223-241: (row,col) 의 상하좌우를 데미지.
        public void CheckArround(SimBoard board, int row, int col, int blockTypeCount = 5)
        {
            if (board.IsValid(row, col) == false)
                return;
            DamageBlock(board, row - 1, col, blockTypeCount);
            DamageBlock(board, row, col + 1, blockTypeCount);
            DamageBlock(board, row, col - 1, blockTypeCount);
            DamageBlock(board, row + 1, col, blockTypeCount);
        }

        //# 실게임 DamageBlock 232-241. M2 엔 RainbowPang 없으므로 일반 Damage.
        public void DamageBlock(SimBoard board, int row, int col, int blockTypeCount)
        {
            if (board.IsValid(row, col))
            {
                board.Grid[row, col].Damage(blockTypeCount);
            }
        }
    }
}
