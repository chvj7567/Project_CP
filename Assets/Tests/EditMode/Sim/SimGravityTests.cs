using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimGravityTests
    {
        [Test]
        public void SimBoard_Swap_은_두칸_state를_교환한다()
        {
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Cat1);
            board.SetState(0, 1, EBlockState.Cat2);
            board.Swap(0, 0, 0, 1);
            Assert.AreEqual(EBlockState.Cat2, board.GetState(0, 0));
            Assert.AreEqual(EBlockState.Cat1, board.GetState(0, 1));
        }

        [Test]
        public void SimBoard_IsValid_는_범위밖을_거른다()
        {
            SimBoard board = new SimBoard(3);
            Assert.IsTrue(board.IsValid(0, 0));
            Assert.IsTrue(board.IsValid(2, 2));
            Assert.IsFalse(board.IsValid(-1, 0));
            Assert.IsFalse(board.IsValid(3, 0));
        }

        [Test]
        public void 낙하_매치제거칸_위블록이_바닥으로_떨어진다()
        {
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Cat1);
            board.Grid[1, 0].Match = true;
            board.SetState(1, 0, EBlockState.Cat2);
            board.Grid[2, 0].Match = true;
            board.SetState(2, 0, EBlockState.Cat3);

            new SimGravity(seed: 1).Apply(board, blockTypeCount: 5);

            Assert.AreEqual(EBlockState.Cat1, board.GetState(2, 0));
            Assert.IsTrue(board.Grid[0, 0].IsNormal());
            Assert.IsTrue(board.Grid[1, 0].IsNormal());
            Assert.IsFalse(board.Grid[2, 0].Match, "낙하 후 match 해제 기대");
        }

        [Test]
        public void 리필_시드가_같으면_결과가_같다()
        {
            SimBoard a = new SimBoard(5);
            SimBoard b = new SimBoard(5);
            foreach (SimBlock bl in a.Grid)
            {
                bl.Match = true;
            }
            foreach (SimBlock bl in b.Grid)
            {
                bl.Match = true;
            }
            new SimGravity(seed: 42).Apply(a, 5);
            new SimGravity(seed: 42).Apply(b, 5);
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    Assert.AreEqual(a.GetState(r, c), b.GetState(r, c), $"({r},{c}) 시드 재현 실패");
                }
            }
        }
    }
}
