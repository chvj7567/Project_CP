using System;
using static Defines;

namespace CatPang.Sim
{
    //# 낙하 + 리필. M1: Wall 없음 → 각 열에서 비매치 블록을 바닥으로 모으고 빈칸을 위에 랜덤 리필.
    //# RNG 는 시드 주입(System.Random)으로 결정적 — UnityEngine.Random 미사용.
    //# 리필 범위 0..blockTypeCount-1 = Cat1(0)..  (GPGameScene.UpdateMap 의 Random(0, blockTypeCount) 와 동일).
    public class SimGravity
    {
        private readonly Random _rng;

        public SimGravity(int seed)
        {
            _rng = new Random(seed);
        }

        public void Apply(SimBoard board, int blockTypeCount)
        {
            int size = board.Size;
            for (int c = 0; c < size; ++c)
            {
                int writeRow = size - 1;
                for (int r = size - 1; r >= 0; --r)
                {
                    if (board.Grid[r, c].Match == false)
                    {
                        board.Grid[writeRow, c].State = board.Grid[r, c].State;
                        --writeRow;
                    }
                }

                for (int r = writeRow; r >= 0; --r)
                {
                    board.Grid[r, c].State = (EBlockState)_rng.Next(0, blockTypeCount);
                }
            }

            board.ResetAllMatch();
        }
    }
}
