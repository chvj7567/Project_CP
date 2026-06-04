using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimDisappearTests
    {
        [Test]
        public void Fish는_폭탄범위에서_제거안됨_Ball은_제거됨()
        {
            SimBoard b = new SimBoard(3);
            b.SetState(1, 1, EBlockState.Cat1);
            b.SetState(0, 1, EBlockState.Fish);
            b.SetState(1, 0, EBlockState.Ball);
            SimMatchChecker m = new SimMatchChecker();
            m.ChangeMatchState(b, 0, 1); //# Fish
            m.ChangeMatchState(b, 1, 0); //# Ball
            Assert.IsFalse(b.Grid[0, 1].Match, "Fish 는 폭탄으로 제거 불가");
            Assert.IsTrue(b.Grid[1, 0].Match, "Ball 은 폭탄으로 제거 가능");
        }

        [Test]
        public void Fish는_드래그불가_Ball은_드래그가능()
        {
            Assert.IsTrue(new SimBlock { State = EBlockState.Fish }.CanNotDrag(), "Fish 드래그 불가");
            Assert.IsFalse(new SimBlock { State = EBlockState.Ball }.CanNotDrag(), "Ball 드래그 가능");
        }

        //# Task2: Ball 맨아래줄 → Potal(hp5) + 좌우 확산 hp 분포.
        [Test]
        public void Ball_맨아래줄_Potal과_좌우확산_hp분포()
        {
            SimBoard b = new SimBoard(7);
            int row = b.Size - 1;
            //# 위쪽 6줄은 임의 일반블록으로 채움(Resolve 는 맨아래줄만 스캔 — 상단 무관).
            for (int c = 0; c < 7; ++c)
            {
                for (int r = 0; r < 6; ++r)
                {
                    b.SetState(r, c, (EBlockState)(c % 3));
                }
            }
            //# 아래줄 전체 Cat1, col=3 에 Ball.
            for (int c = 0; c < 7; ++c)
            {
                b.SetState(row, c, EBlockState.Cat1);
            }
            b.SetState(row, 3, EBlockState.Ball);

            SimDisappearResolver.Resolve(b, new System.Random(1));

            Assert.AreEqual(EBlockState.Potal, b.Grid[row, 3].ChangeBlockState, "Ball→Potal");
            Assert.AreEqual(5, b.Grid[row, 3].ChangeHp, "Ball Potal hp5");
            Assert.AreEqual(4, b.Grid[row, 4].ChangeHp, "우측1 hp4");
            Assert.AreEqual(3, b.Grid[row, 5].ChangeHp, "우측2 hp3");
            Assert.AreEqual(2, b.Grid[row, 6].ChangeHp, "우측3 hp2");
            Assert.AreEqual(4, b.Grid[row, 2].ChangeHp, "좌측1 hp4");
            Assert.AreEqual(3, b.Grid[row, 1].ChangeHp, "좌측2 hp3");
            Assert.AreEqual(2, b.Grid[row, 0].ChangeHp, "좌측3 hp2");
        }

        //# Task2: Fish 맨아래줄 → 색폭탄(PinkBomb~BlueBomb) 예약. 시드 동일 → 동일 결과.
        [Test]
        public void Fish_맨아래줄_색폭탄으로_변환예약_시드결정적()
        {
            SimBoard b1 = new SimBoard(3);
            b1.SetState(2, 1, EBlockState.Fish);
            SimDisappearResolver.Resolve(b1, new System.Random(42));
            EBlockState conv = b1.Grid[2, 1].ChangeBlockState;
            Assert.IsTrue(
                conv >= EBlockState.PinkBomb && conv <= EBlockState.BlueBomb,
                "Fish → 색폭탄(PinkBomb=19 ~ BlueBomb=23)");

            SimBoard b2 = new SimBoard(3);
            b2.SetState(2, 1, EBlockState.Fish);
            SimDisappearResolver.Resolve(b2, new System.Random(42));
            Assert.AreEqual(conv, b2.Grid[2, 1].ChangeBlockState, "시드 동일 → 변환 동일");
        }
    }
}
