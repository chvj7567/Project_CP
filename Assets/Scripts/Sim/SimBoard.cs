using static Defines;

namespace CatPang.Sim
{
    //# SimBlock[,] 그리드. 실게임 GPBoard 의 좌표/스왑 역할만 순수 재구현.
    public class SimBoard
    {
        public readonly int Size;
        public readonly SimBlock[,] Grid;

        public SimBoard(int size)
        {
            Size = size;
            Grid = new SimBlock[size, size];
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    Grid[r, c] = new SimBlock { Row = r, Col = c, State = EBlockState.None };
                }
            }
        }

        public bool IsValid(int r, int c)
        {
            return r >= 0 && r < Size && c >= 0 && c < Size;
        }

        public EBlockState GetState(int r, int c)
        {
            return Grid[r, c].State;
        }

        public void SetState(int r, int c, EBlockState s)
        {
            Grid[r, c].State = s;
        }

        //# 두 칸 state 만 교환(좌표 고정 그리드). 실게임 ChangeBlock 의 시뮬 등가.
        public void Swap(int r1, int c1, int r2, int c2)
        {
            EBlockState tmp = Grid[r1, c1].State;
            Grid[r1, c1].State = Grid[r2, c2].State;
            Grid[r2, c2].State = tmp;
        }

        public void ResetAllMatch()
        {
            foreach (SimBlock b in Grid)
            {
                b.ResetMatch();
            }
        }
    }
}
