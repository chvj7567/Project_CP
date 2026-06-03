using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# M3a 골든 스파이크: 실 GPMatchChecker/GPBombResolver 를 bare EditMode 에서 호출 가능한지 확인.
    //# 스파이크 A — CheckMap hScore 설정 경로가 NRE 없이 동작하는지 (폭탄 생성 골든 전제).
    //# 스파이크 B — Bomb1 3x3 경로에서 CHMData 접근(SaveBombCollectionData) 이 NRE 를 내는지 확인.
    public class SimBombSpikeTests
    {
        //# 스파이크 A: 실 GPMatchChecker.CheckMap 이 bare EditMode 에서 NRE 없이 동작하고
        //# 가로 4매치 블록의 hScore 가 SetScore 로 4 가 되는지(폭탄 생성 골든 전제).
        [Test]
        public void 스파이크_실매치체커_가로4매치_hScore설정()
        {
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };

            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard board = RealBlockFactory.CreateGPBoard(arr, arr.GetLength(0));
            GPMatchChecker matcher = RealBlockFactory.CreateGPMatchChecker(board, 5);

            matcher.CheckMap(test: false);

            int hScore00 = arr[0, 0].hScore;
            RealBlockFactory.Destroy(arr);

            Assert.AreEqual(4, hScore00, "가로 4매치 블록 hScore=4 (실게임 SetScore)");
        }

        //# 스파이크 B: 실 GPBombResolver.Bomb1(3x3) 호출 가능 여부 + CHMData NRE 유무 확인.
        //# SaveBombCollectionData → CHMData.Instance.GetCollectionData 가 bare EditMode 에서 NRE 를 내면
        //# Assert.Inconclusive 로 스파이크 발견을 기록한다(골든 harness 에 seam 필요).
        //# NRE 없이 통과하면 9칸 match 를 검증한다.
        [Test]
        public void 스파이크_실밤리졸버_Bomb1_3x3_셀캡처()
        {
            EBlockState[,] states = Fill5x5(EBlockState.Cat1);
            states[2, 2] = EBlockState.CatPang;

            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard board = RealBlockFactory.CreateGPBoard(arr, arr.GetLength(0));
            GPMatchChecker matcher = RealBlockFactory.CreateGPMatchChecker(board, 5);

            //# pangEffectList 는 EPangEffect.Max(=9) 크기의 null 리스트로 채운다.
            //# stub createEffect 는 ps/pos 인자를 역참조하지 않으므로 null 엔트리는 안전.
            int pangEffectMax = (int)EPangEffect.Max;
            List<ParticleSystem> pangEffectList = new List<ParticleSystem>(pangEffectMax);
            for (int i = 0; i < pangEffectMax; ++i)
            {
                pangEffectList.Add(null);
            }

            GPBombResolver resolver = new GPBombResolver();
            resolver.Init(
                board,
                matcher,
                () => System.Threading.Tasks.Task.CompletedTask,
                _ => { },
                (ps, pos) => null,
                _ => { },
                pangEffectList,
                null,
                default(CancellationToken));

            //# CHMData seam — 스파이크에서 SaveBombCollectionData 의 CHMData NRE 가 확인됨.
            //# 빈 collection 딕셔너리를 주입하면 실 Bomb1 이 끝까지 동작한다(엔드투엔드 골든 가능).
            RealBlockFactory.EnsureCHMDataCollection();
            resolver.Bomb1(arr[2, 2], false).GetAwaiter().GetResult();

            int matchCount = 0;
            foreach (Block b in arr)
            {
                if (b.match)
                {
                    ++matchCount;
                }
            }
            RealBlockFactory.Destroy(arr);

            Assert.AreEqual(9, matchCount, "Bomb1 은 3x3 9칸 match");
        }

        private static EBlockState[,] Fill5x5(EBlockState s)
        {
            EBlockState[,] a = new EBlockState[5, 5];
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    a[r, c] = s;
                }
            }
            return a;
        }
    }
}
