#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# M3a Task2 — 매치 런렝스(hScore/vScore) 및 squareMatch 추적 골든/단위 테스트.
    public class SimMatchScoreGoldenTests
    {
        //# 가로 4매치 보드에서 실게임/Sim 의 hScore 가 일치하는지 골든 비교.
        [Test]
        public void 골든_가로4매치_hScore_실게임과_일치()
        {
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };

            //# 실게임 측 — GPMatchChecker.CheckMap 호출 후 arr[0,c].hScore 수집.
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard gb = RealBlockFactory.CreateGPBoard(arr, arr.GetLength(0));
            GPMatchChecker gm = RealBlockFactory.CreateGPMatchChecker(gb, 5);
            gm.CheckMap(test: false);
            int[] realH = new int[5];
            for (int c = 0; c < 5; ++c)
            {
                realH[c] = arr[0, c].hScore;
            }
            RealBlockFactory.Destroy(arr);

            //# Sim 측 — SimMatchChecker.CheckMap 호출 후 Grid[0,c].HScore 수집.
            SimBoard sb = new SimBoard(5);
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                }
            }
            SimMatchChecker sc = new SimMatchChecker();
            sc.CheckMap(sb);

            for (int c = 0; c < 5; ++c)
            {
                Assert.AreEqual(realH[c], sb.Grid[0, c].HScore, $"(0,{c}) hScore 실게임과 Sim 이 일치해야 한다");
            }
        }

        //# Sim 단위 — 2×2 동색 사각 매치 시 SquareMatch 가 정확히 한 칸만 true.
        //# moveIndex 미설정(-1) 이므로 else 분기 → 좌상단(0,0) 이 SquareMatch.
        [Test]
        public void Sim_2x2사각매치_SquareMatch가_정확히_한칸만_true()
        {
            //# 5×5 보드. (0,0)~(1,1) 이 Cat1 동색 — 나머지는 매치 안 되도록 교대 배치.
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
            };

            SimBoard sb = new SimBoard(5);
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                }
            }
            SimMatchChecker sc = new SimMatchChecker();
            sc.CheckMap(sb);

            int squareMatchCount = 0;
            foreach (SimBlock b in sb.Grid)
            {
                if (b.SquareMatch)
                {
                    ++squareMatchCount;
                }
            }

            Assert.AreEqual(1, squareMatchCount, "SquareMatch 는 보드 전체에서 정확히 한 칸이어야 한다");
            Assert.IsTrue(sb.Grid[0, 0].SquareMatch, "moveIndex 미설정 시 좌상단(0,0) 이 SquareMatch 여야 한다");
        }
    }
}
#endif
