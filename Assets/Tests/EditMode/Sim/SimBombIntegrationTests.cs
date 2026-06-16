#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# M3a Task3: SimBlock 분류 헬퍼·BombKind + SimMatchChecker.ChangeMatchState 단위 검증.
    public class SimBombIntegrationTests
    {
        //# 정상 케이스: 주요 블록 타입의 분류 헬퍼와 BombKind 디스패치 일치 확인.
        [Test]
        public void 분류헬퍼_폭탄종류를_구분한다()
        {
            SimBlock catpang = new SimBlock { State = EBlockState.CatPang };
            SimBlock arrow1  = new SimBlock { State = EBlockState.Arrow1 };
            SimBlock pink    = new SimBlock { State = EBlockState.PinkBomb };
            SimBlock rainbow = new SimBlock { State = EBlockState.RainbowPang };

            Assert.IsTrue(catpang.IsBomb(),    "CatPang IsBomb");
            Assert.IsTrue(arrow1.IsArrow(),    "Arrow1 IsArrow");
            Assert.IsTrue(arrow1.IsBomb(),     "Arrow1 IsBomb");
            Assert.IsTrue(pink.IsSpecialBomb(), "Pink IsSpecialBomb");
            Assert.IsTrue(pink.IsBomb(),        "Pink IsBomb(실 IsBombBlock 집합에 포함)");
            Assert.IsTrue(rainbow.CanNotDrag(), "Rainbow 드래그불가");

            Assert.AreEqual(SimBlock.EBombKind.Bomb1, catpang.BombKind(), "CatPang→Bomb1");
            Assert.AreEqual(SimBlock.EBombKind.Bomb4, arrow1.BombKind(),  "Arrow1→Bomb4");
            Assert.AreEqual(SimBlock.EBombKind.None,  pink.BombKind(),    "PinkBomb→None(단독 발동 없음)");
            Assert.AreEqual(SimBlock.EBombKind.Rainbow, rainbow.BombKind(), "Rainbow→Rainbow");
        }

        //# 엣지 케이스: ChangeMatchState 의 고정블록·PinkBomb 제외, 일반블록 포함 확인.
        [Test]
        public void ChangeMatchState_고정블록과_PinkBomb는_제외()
        {
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Cat1);
            board.SetState(0, 1, EBlockState.Wall);
            board.Grid[0, 1].Hp = 1;
            board.SetState(0, 2, EBlockState.PinkBomb);

            SimMatchChecker m = new SimMatchChecker();
            m.ChangeMatchState(board, 0, 0);
            m.ChangeMatchState(board, 0, 1);
            m.ChangeMatchState(board, 0, 2);

            Assert.IsTrue(board.Grid[0, 0].Match,  "일반블록 match");
            Assert.IsFalse(board.Grid[0, 1].Match, "Wall 제외");
            Assert.IsFalse(board.Grid[0, 2].Match, "PinkBomb 제외");
        }

        //# Task7: SimGame.TestDetonate — match 된 CatPang 발동으로 3×3 이상 match.
        [Test]
        public void 턴루프_match된_CatPang이_발동되어_주변을_친다()
        {
            //# 5x5 Cat1, 중앙 CatPang 을 match 표시 후 TestDetonate → 3x3 이상 match.
            SimBoard b = new SimBoard(5);
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    b.SetState(r, c, EBlockState.Cat1);
                }
            }
            b.SetState(2, 2, EBlockState.CatPang);
            b.Grid[2, 2].Match = true;
            SimMatchChecker m = new SimMatchChecker();
            new SimGame().TestDetonate(b, m);
            int matched = 0;
            foreach (SimBlock x in b.Grid)
            {
                if (x.Match)
                {
                    ++matched;
                }
            }
            Assert.GreaterOrEqual(matched, 9, "CatPang 발동으로 3x3 이상 match");
        }

        //# Task8: FindValidMoves 폭탄 관여 수 포함 검증.
        [Test]
        public void AI_인접_두폭탄_스왑이_유효수()
        {
            //# 일반 매치가 발생하지 않는 보드(Cat1/2/3 혼합) + 중앙 인접 폭탄 쌍.
            //# 폭탄 분기 없으면 유효수 0 → HasValue false. 분기 있으면 폭탄 쌍이 후보로 잡힘.
            SimBoard b = new SimBoard(3);
            EBlockState[,] layout =
            {
                { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Arrow1, EBlockState.Arrow3 },
                { EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
            };
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    b.SetState(r, c, layout[r, c]);
                }
            }
            SimMove? mv = new RandomAiPolicy(1).ChooseMove(b, new SimMatchChecker());
            Assert.IsTrue(mv.HasValue, "폭탄 인접 스왑이 유효 수로 잡힘");
        }

        [Test]
        public void AI_RainbowPang은_스왑후보_아님()
        {
            //# RainbowPang 은 CanNotDrag → 인접 일반블록과도 스왑 불가. 보드에 매치도 없으면 유효수 0.
            SimBoard b = new SimBoard(3);
            EBlockState[,] s =
            {
                { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.RainbowPang, EBlockState.Cat1 },
                { EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
            };
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    b.SetState(r, c, s[r, c]);
                }
            }
            SimMove? mv = new RandomAiPolicy(1).ChooseMove(b, new SimMatchChecker());
            Assert.IsFalse(mv.HasValue, "Rainbow 인접 스왑은 후보 아님 + 매치 없음 → 유효수 0");
        }

        //# Task6: SimComboResolver — 실게임 AfterDrag 분기 순서 검증.
        [Test]
        public void 조합_분기가_실게임순서를_따른다()
        {
            SimBlock pink = new SimBlock { State = EBlockState.PinkBomb };
            SimBlock blue = new SimBlock { State = EBlockState.BlueBomb };
            SimBlock cat  = new SimBlock { State = EBlockState.Cat1 };
            SimBlock a1   = new SimBlock { State = EBlockState.Arrow1 };
            SimBlock a3   = new SimBlock { State = EBlockState.Arrow3 };

            //# 색폭탄 2개 → BoomAll (L787)
            Assert.AreEqual(SimComboResolver.EComboResult.BoomAll,          SimComboResolver.Resolve(blue, blue));
            //# Pink + 일반 → Boom3 (L789~792)
            Assert.AreEqual(SimComboResolver.EComboResult.Boom3,            SimComboResolver.Resolve(pink, cat));
            //# 한쪽만 색폭탄 → 그 폭탄 발동 (L793~796)
            Assert.AreEqual(SimComboResolver.EComboResult.DetonateA,        SimComboResolver.Resolve(blue, cat));
            //# 화살표 2개 → 색폭탄 승급 (L797~803)
            Assert.AreEqual(SimComboResolver.EComboResult.MergeToColorBomb, SimComboResolver.Resolve(a1, a3));
            //# 한쪽만 화살표 → 발동 (L804~805)
            Assert.AreEqual(SimComboResolver.EComboResult.DetonateB,        SimComboResolver.Resolve(cat, a1));
            //# 둘 다 일반 → None
            Assert.AreEqual(SimComboResolver.EComboResult.None,             SimComboResolver.Resolve(cat, cat));

            //# 순서 검증: Pink + 색폭탄(Blue) → BoomAll(L787 선행), Boom3(L789) 아님.
            Assert.AreEqual(SimComboResolver.EComboResult.BoomAll,          SimComboResolver.Resolve(pink, blue));
            //# 순서 검증: 비특수폭탄(Arrow) + 색폭탄(Blue) → DetonateB(L795), MergeToColorBomb(L797) 아님.
            Assert.AreEqual(SimComboResolver.EComboResult.DetonateB,        SimComboResolver.Resolve(a1, blue));
        }
    }
}
#endif
