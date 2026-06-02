using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimMatchGoldenTests
    {
        [Test]
        public void 실_GPMatchChecker_직호출로_가로3매치를_판정한다()
        {
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };
            Block[,] arr = RealBlockFactory.CreateBoard(states);

            GPBoard board = new GPBoard();
            board.Init(arr, 3, new Dictionary<EBlockState, Sprite>(), 0f, 0, default);
            GPMatchChecker matcher = new GPMatchChecker();
            matcher.Init(board, 5);

            matcher.CheckMap(test: false);

            Assert.IsTrue(arr[0, 0].IsMatch());
            Assert.IsTrue(arr[0, 1].IsMatch());
            Assert.IsTrue(arr[0, 2].IsMatch());
            Assert.IsFalse(arr[1, 1].IsMatch());

            RealBlockFactory.Destroy(arr);
        }

        //# 실게임 GPMatchChecker 직호출로 매치 마스크 캡처(정답). Block 은 리플렉션 생성 후 정리.
        private static bool[,] RealMatchMask(EBlockState[,] states, int blockTypeCount)
        {
            int size = states.GetLength(0);
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard board = new GPBoard();
            board.Init(arr, size, new Dictionary<EBlockState, Sprite>(), 0f, 0, default);
            GPMatchChecker matcher = new GPMatchChecker();
            matcher.Init(board, blockTypeCount);
            matcher.CheckMap(test: false);
            bool[,] mask = new bool[size, size];
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    mask[r, c] = arr[r, c].IsMatch();
                }
            }
            RealBlockFactory.Destroy(arr);
            return mask;
        }

        //# SimMatchChecker 의 매치 마스크 캡처(검증 대상).
        private static bool[,] SimMatchMask(EBlockState[,] states)
        {
            int size = states.GetLength(0);
            SimBoard board = new SimBoard(size);
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    board.SetState(r, c, states[r, c]);
                }
            }
            new SimMatchChecker().CheckMap(board);
            bool[,] mask = new bool[size, size];
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    mask[r, c] = board.Grid[r, c].Match;
                }
            }
            return mask;
        }

        //# 실게임 마스크와 시뮬 마스크가 모든 칸에서 동일한지 단언.
        private static void AssertSameMask(EBlockState[,] states, int blockTypeCount)
        {
            bool[,] real = RealMatchMask(states, blockTypeCount);
            bool[,] sim = SimMatchMask(states);
            int size = states.GetLength(0);
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    Assert.AreEqual(real[r, c], sim[r, c], $"({r},{c}) 매치 마스크 불일치");
                }
            }
        }

        [Test]
        public void 골든_가로3매치_동등()
        {
            AssertSameMask(new EBlockState[,]
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1 },
                { EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
            }, 5);
        }

        [Test]
        public void 골든_세로3매치_동등()
        {
            AssertSameMask(new EBlockState[,]
            {
                { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
            }, 5);
        }

        [Test]
        public void 골든_사각매치_동등()
        {
            AssertSameMask(new EBlockState[,]
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
            }, 5);
        }

        [Test]
        public void 골든_매치없음_동등()
        {
            AssertSameMask(new EBlockState[,]
            {
                { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1 },
                { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1 },
            }, 5);
        }
    }
}
