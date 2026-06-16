#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# 실게임 GPMatchChecker 인접 데미지 ↔ SimMatchChecker 동등성 골든.
    public class SimSpecialBlockGoldenTests
    {
        //# 실게임: 3x3, 가운데 행 Cat1 3개(매치) + (0,1) Wall(hp1). 매치 제거 시 CheckArround 로 Wall 데미지.
        //# 실 GPMatchChecker.CheckMap → RemoveMatchBlock 경로 대신, 직접 CheckArround 를 호출해 인접데미지만 골든.
        //# Wall hp=1 로 둠: 데미지 시 hp 1→0 이라 Block.Damage 의 hpText.SetText(hp>0) 분기를 안 타 NRE 회피
        //# (bare Block 은 직렬화 필드 hpText 가 null). 골든 본질(인접 데미지로 Wall hp 감소)은 hp 1→0 으로 검증.
        [Test]
        public void 골든_매치인접_Wall이_데미지받는다()
        {
            //# 보드: row1 = Cat1 Cat1 Cat1 (가로3매치), (0,1)=Wall hp1 (매치블록 (1,1) 위)
            EBlockState[,] states =
            {
                { EBlockState.Cat2, EBlockState.Wall, EBlockState.Cat2 },
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };
            int[,] hps = { {-1,1,-1}, {-1,-1,-1}, {-1,-1,-1} };

            //# 실게임 골든: Wall 의 hp 가 매치블록 인접 데미지로 줄어드는지
            Block[,] arr = RealBlockFactory.CreateBoard(states, hps);
            GPBoard board = new GPBoard();
            board.Init(arr, 3, new Dictionary<EBlockState, Sprite>(), 0f, 0, default);
            GPMatchChecker matcher = new GPMatchChecker();
            matcher.Init(board, 5);
            matcher.CheckMap(test: false); //# 매치 표시 + ResetCheckWallDamage
            //# 매치된 (1,1) 의 위 칸 (0,1) Wall 에 인접 데미지
            matcher.CheckArround(1, 1);
            int realWallHp = arr[0, 1].GetHp();
            RealBlockFactory.Destroy(arr);

            //# Sim: 동일 보드 구성
            SimBoard sb = new SimBoard(3);
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                    sb.Grid[r, c].Hp = hps[r, c];
                }
            }
            SimMatchChecker sc = new SimMatchChecker();
            sc.CheckMap(sb);
            sc.CheckArround(sb, 1, 1);
            int simWallHp = sb.Grid[0, 1].Hp;

            Assert.AreEqual(realWallHp, simWallHp, "실게임 Wall hp 와 Sim Wall hp 일치");
        }

        //# 실게임 Block.CatInTheBox ↔ Sim 동등성 — "박스가 받는 고양이인지" 판정(BoxAccepts).
        //# NRE 회피: 실게임 CatInTheBox 는 hp>0 진입 시 무조건 hpText 접근(SetText/SetActive, Block.cs 674·678).
        //# bare Block 은 hpText(CHText) 가 null → hp>0 경로는 NRE 불가피. 더미 CHText 주입도 RequireComponent(TMP_Text)
        //# 초기화에서 NRE 남. 따라서 **hp<=0 박스로 호출** → 실게임은 `if(hp<=0) return true`(Block.cs:667-668)로
        //# hpText 접근 없이 판정만 반환. 받음/안받음 두 케이스로 BoxAccepts 동등성을 검증한다.
        //# (hp 감소(decrement)는 SimSpecialBlocks 단위 테스트로 분리 — 실게임 hpText 의존이라 골든 불가.)
        [Test]
        public void 골든_CatBox_받는고양이_판정이_실게임과_일치한다()
        {
            //# CatBox1 은 Cat1/Cat6 을 받고 Cat2 는 안 받음. 박스 hp=0 으로 두어 hpText 접근 회피.
            AssertBoxAcceptsMatchesReal(EBlockState.CatBox1, EBlockState.Cat1, expectAccept: true);
            AssertBoxAcceptsMatchesReal(EBlockState.CatBox1, EBlockState.Cat6, expectAccept: true);
            AssertBoxAcceptsMatchesReal(EBlockState.CatBox1, EBlockState.Cat2, expectAccept: false);
            //# CatBox3 은 Cat3 만.
            AssertBoxAcceptsMatchesReal(EBlockState.CatBox3, EBlockState.Cat3, expectAccept: true);
            AssertBoxAcceptsMatchesReal(EBlockState.CatBox3, EBlockState.Cat1, expectAccept: false);
        }

        //# hp=0 박스 위에 upCat 을 두고 실게임/Sim CatInTheBox 결과가 expectAccept 와 일치하는지.
        private static void AssertBoxAcceptsMatchesReal(EBlockState box, EBlockState upCat, bool expectAccept)
        {
            //# 3x3, (1,0)=box(hp0), (0,0)=upCat. 나머지는 매치 안 생기게 채움.
            EBlockState[,] states =
            {
                { upCat, EBlockState.Cat7, EBlockState.Cat2 },
                { box, EBlockState.Cat3, EBlockState.Cat7 },
                { EBlockState.Cat2, EBlockState.Cat7, EBlockState.Cat3 },
            };
            int[,] hps = { {-1,-1,-1}, {0,-1,-1}, {-1,-1,-1} };

            Block[,] arr = RealBlockFactory.CreateBoard(states, hps);
            //# 실게임: hp<=0 이라 CatInTheBox 는 hpText 접근 없이 BoxAccepts 판정만 반환.
            bool realResult = arr[1, 0].CatInTheBox(arr[0, 0].GetBlockState());
            RealBlockFactory.Destroy(arr);

            SimBlock simBox = new SimBlock { State = box, Hp = 0 };
            bool simResult = SimSpecialBlocks.CatInTheBox(simBox, upCat);

            Assert.AreEqual(realResult, simResult, $"{box} ← {upCat}: 실게임/Sim 판정 일치");
            Assert.AreEqual(expectAccept, realResult, $"{box} ← {upCat}: 기대 판정 {expectAccept}");
        }
    }
}
#endif
