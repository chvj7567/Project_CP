using static Defines;

namespace CatPang.Sim
{
    //# 실 GPBombResolver 의 발동 기하(Bomb1~12/BoomAll/Boom3/RainbowPang) 순수 포팅.
    //# 각 BombN 은 고정 셀 집합에 checker.ChangeMatchState. 이펙트/사운드/점수 없음.
    //# 포팅 출처: GPBombResolver.cs 각 메서드의 _matcher.ChangeMatchState 호출 패턴을 1:1 복사.
    public class SimBombResolver
    {
        private readonly System.Random _rng;

        public SimBombResolver(System.Random rng)
        {
            _rng = rng;
        }

        //# 실 Bomb1(GPBombResolver L95-111): 자기 포함 3×3 — block.match + 8방향 ChangeMatchState.
        public void Bomb1(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            checker.ChangeMatchState(board, row - 1, col - 1);
            checker.ChangeMatchState(board, row - 1, col);
            checker.ChangeMatchState(board, row - 1, col + 1);
            checker.ChangeMatchState(board, row,     col - 1);
            checker.ChangeMatchState(board, row,     col + 1);
            checker.ChangeMatchState(board, row + 1, col - 1);
            checker.ChangeMatchState(board, row + 1, col);
            checker.ChangeMatchState(board, row + 1, col + 1);
        }

        //# 실 Bomb2(GPBombResolver L113-123): 십자(가로+세로) — block.match + 세로줄 전체 + 가로줄 전체.
        public void Bomb2(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int i = 0; i < board.Size; ++i)
            {
                checker.ChangeMatchState(board, i, col);
            }
            for (int i = 0; i < board.Size; ++i)
            {
                checker.ChangeMatchState(board, row, i);
            }
        }

        //# 실 Bomb4(GPBombResolver L154-165): 가로 한 줄 — block.match + 해당 행 전체 ChangeMatchState.
        public void Bomb4(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int i = 0; i < board.Size; ++i)
            {
                checker.ChangeMatchState(board, row, i);
            }
        }

        //# 실 Bomb5(GPBombResolver L168-179): 세로 한 줄 — block.match + 해당 열 전체 ChangeMatchState.
        public void Bomb5(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int i = 0; i < board.Size; ++i)
            {
                checker.ChangeMatchState(board, i, col);
            }
        }

        //# 실 Bomb6(GPBombResolver L182-197): X대각 — block.match + i=0..Size-1 (row±i, col±i) 4조합.
        public void Bomb6(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int i = 0; i < board.Size; ++i)
            {
                checker.ChangeMatchState(board, row - i, col - i);
                checker.ChangeMatchState(board, row - i, col + i);
                checker.ChangeMatchState(board, row + i, col - i);
                checker.ChangeMatchState(board, row + i, col + i);
            }
        }

        //# 실 Bomb7(GPBombResolver L200-215): /대각 — block.match + i=0..Size-1 (row-i,col+i) + (row+i,col-i).
        public void Bomb7(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int i = 0; i < board.Size; ++i)
            {
                checker.ChangeMatchState(board, row - i, col + i);
                checker.ChangeMatchState(board, row + i, col - i);
            }
        }

        //# 실 Bomb8(GPBombResolver L218-233): \대각 — block.match + i=0..Size-1 (row-i,col-i) + (row+i,col+i).
        public void Bomb8(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int i = 0; i < board.Size; ++i)
            {
                checker.ChangeMatchState(board, row - i, col - i);
                checker.ChangeMatchState(board, row + i, col + i);
            }
        }

        //# 실 Bomb9(GPBombResolver L236-255): 마름모 — block.match + 12개 고정 오프셋.
        public void Bomb9(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            checker.ChangeMatchState(board, row - 2, col);
            checker.ChangeMatchState(board, row - 1, col);
            checker.ChangeMatchState(board, row - 1, col - 1);
            checker.ChangeMatchState(board, row - 1, col + 1);
            checker.ChangeMatchState(board, row,     col - 2);
            checker.ChangeMatchState(board, row,     col - 1);
            checker.ChangeMatchState(board, row,     col + 1);
            checker.ChangeMatchState(board, row,     col + 2);
            checker.ChangeMatchState(board, row + 1, col - 1);
            checker.ChangeMatchState(board, row + 1, col + 1);
            checker.ChangeMatchState(board, row + 1, col);
            checker.ChangeMatchState(board, row + 2, col);
        }

        //# 실 Bomb10(GPBombResolver L259-275): 5×5 테두리 — block.match + 16개 고정 오프셋.
        public void Bomb10(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            checker.ChangeMatchState(board, row - 2, col - 2); checker.ChangeMatchState(board, row - 2, col - 1);
            checker.ChangeMatchState(board, row - 2, col);     checker.ChangeMatchState(board, row - 2, col + 1);
            checker.ChangeMatchState(board, row - 2, col + 2); checker.ChangeMatchState(board, row - 1, col - 2);
            checker.ChangeMatchState(board, row - 1, col + 2); checker.ChangeMatchState(board, row,     col - 2);
            checker.ChangeMatchState(board, row,     col + 2); checker.ChangeMatchState(board, row + 1, col - 2);
            checker.ChangeMatchState(board, row + 1, col + 2); checker.ChangeMatchState(board, row + 2, col - 2);
            checker.ChangeMatchState(board, row + 2, col - 1); checker.ChangeMatchState(board, row + 2, col);
            checker.ChangeMatchState(board, row + 2, col + 1); checker.ChangeMatchState(board, row + 2, col + 2);
        }

        //# 실 Bomb11(GPBombResolver L278-292): 5×5 모서리 — block.match + 12개 고정 오프셋.
        public void Bomb11(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            checker.ChangeMatchState(board, row - 2, col - 1); checker.ChangeMatchState(board, row - 1, col - 2);
            checker.ChangeMatchState(board, row - 1, col - 1); checker.ChangeMatchState(board, row - 2, col + 1);
            checker.ChangeMatchState(board, row - 1, col + 2); checker.ChangeMatchState(board, row - 1, col + 1);
            checker.ChangeMatchState(board, row + 2, col - 1); checker.ChangeMatchState(board, row + 1, col - 2);
            checker.ChangeMatchState(board, row + 1, col - 1); checker.ChangeMatchState(board, row + 2, col + 1);
            checker.ChangeMatchState(board, row + 1, col + 2); checker.ChangeMatchState(board, row + 1, col + 1);
        }

        //# 실 Bomb12(GPBombResolver L295-309): 5×5 변형 — block.match + 12개 고정 오프셋.
        public void Bomb12(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            checker.ChangeMatchState(board, row - 2, col - 2); checker.ChangeMatchState(board, row - 2, col - 1);
            checker.ChangeMatchState(board, row - 2, col);     checker.ChangeMatchState(board, row - 2, col + 1);
            checker.ChangeMatchState(board, row - 2, col + 2); checker.ChangeMatchState(board, row - 1, col + 1);
            checker.ChangeMatchState(board, row + 1, col - 1); checker.ChangeMatchState(board, row + 2, col - 2);
            checker.ChangeMatchState(board, row + 2, col - 1); checker.ChangeMatchState(board, row + 2, col);
            checker.ChangeMatchState(board, row + 2, col + 1); checker.ChangeMatchState(board, row + 2, col + 2);
        }

        //# 실 BoomAll(GPBombResolver L86-93): 전체 칸 ChangeMatchState.
        //# 실 버전은 보드 전체 루프만 — block 인자 없음. addBonusScore/onBoomTrigger 는 Sim 무관.
        public void BoomAll(SimBoard board, SimMatchChecker checker)
        {
            for (int r = 0; r < board.Size; ++r)
            {
                for (int c = 0; c < board.Size; ++c)
                {
                    checker.ChangeMatchState(board, r, c);
                }
            }
        }

        //# 실 Boom3(GPBombResolver L125-151): specialBlock.match=true + 보드의 targetColor 색 블록 전부 match.
        //# 실 버전: specialBlock.match + boardArr[i,j].GetBlockState()==blockState 이면 b.match=true.
        //# Sim 에서 specialBlock 은 이미 호출 전 match 처리하거나 여기서 처리. 기하 순수성을 위해 row/col 전달.
        public void Boom3(SimBoard board, int specialRow, int specialCol, EBlockState targetColor)
        {
            board.Grid[specialRow, specialCol].Match = true;
            for (int r = 0; r < board.Size; ++r)
            {
                for (int c = 0; c < board.Size; ++c)
                {
                    if (board.Grid[r, c].State == targetColor)
                    {
                        board.Grid[r, c].Match = true;
                    }
                }
            }
        }

        //# 실 RainbowPang(GPBombResolver L312-333): hp<=0 일 때 PinkBomb..BlueBomb(19..23) 을 순서대로
        //# 일반/매치대상 칸에 랜덤 살포. 실 b.remove → Sim b.Match. 실 b.IsNormalBlock() → Sim b.IsNormal().
        //# EBlockState: PinkBomb=19 Yellow=20 Orange=21 Green=22 Blue=23 → 순방향 ++bs 순서 동일.
        public void RainbowPang(SimBoard board, SimBlock self)
        {
            if (self.Hp > 0)
                return;
            self.Match = true;

            for (EBlockState bs = EBlockState.PinkBomb; bs <= EBlockState.BlueBomb; ++bs)
            {
                while (true)
                {
                    if (HasRainbowTarget(board) == false)
                        break;
                    int r = _rng.Next(0, board.Size);
                    int c = _rng.Next(0, board.Size);
                    SimBlock b = board.Grid[r, c];
                    //# 실 조건: !b.remove && !b.IsNormalBlock() → continue. Sim: match==false && IsNormal()==false → skip.
                    if (b.Match == false && b.IsNormal() == false)
                        continue;
                    if (b.ChangeBlockState != EBlockState.None)
                        continue;
                    b.ChangeBlockState = bs;
                    break;
                }
            }
        }

        //# 실 CheckRainbowTargetBlock(GPBombResolver L336-344) 미러.
        //# 실: b.remove || (b.IsNormalBlock() && b.changeBlockState==None) → Sim: b.Match || (b.IsNormal() && b.ChangeBlockState==None).
        private static bool HasRainbowTarget(SimBoard board)
        {
            foreach (SimBlock b in board.Grid)
            {
                if (b.Match || (b.IsNormal() && b.ChangeBlockState == EBlockState.None))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
