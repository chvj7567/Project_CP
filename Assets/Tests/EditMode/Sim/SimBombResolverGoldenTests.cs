using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# Task4 골든 검증: SimBombResolver 각 BombN 의 발동 셀 집합을 실 GPBombResolver 와 비교.
    //# 실 BombN 호출 전 EnsureCHMDataCollection() 으로 CHMData NRE 회피(Task1 스파이크 확인 패턴).
    //# BoomAll 은 block 인자 없으므로 전용 캡처 경로 사용.
    //# Boom3/RainbowPang 은 기하가 아닌 Sim 단위 테스트로 별도 검증.
    public class SimBombResolverGoldenTests
    {
        //# ── BombN 골든 테스트 (11개) ──────────────────────────────────────────

        [Test]
        public void 골든_Bomb1_CatPang_3x3_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.CatPang, 4, 4,
                (sr, sb, sc) => sr.Bomb1(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb2_Arrow5_십자_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.Arrow5, 4, 4,
                (sr, sb, sc) => sr.Bomb2(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb4_Arrow1_가로줄_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.Arrow1, 4, 4,
                (sr, sb, sc) => sr.Bomb4(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb5_Arrow3_세로줄_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.Arrow3, 4, 4,
                (sr, sb, sc) => sr.Bomb5(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb6_Arrow6_X대각_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.Arrow6, 4, 4,
                (sr, sb, sc) => sr.Bomb6(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb7_Arrow2_슬래시대각_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.Arrow2, 4, 4,
                (sr, sb, sc) => sr.Bomb7(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb8_Arrow4_백슬래시대각_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.Arrow4, 4, 4,
                (sr, sb, sc) => sr.Bomb8(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb9_YellowBomb_마름모_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.YellowBomb, 4, 4,
                (sr, sb, sc) => sr.Bomb9(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb10_OrangeBomb_5x5테두리_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.OrangeBomb, 4, 4,
                (sr, sb, sc) => sr.Bomb10(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb11_BlueBomb_5x5모서리_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.BlueBomb, 4, 4,
                (sr, sb, sc) => sr.Bomb11(sb, sc, 4, 4));
        }

        [Test]
        public void 골든_Bomb12_GreenBomb_5x5변형_셀일치()
        {
            AssertBombMatchesReal(
                EBlockState.GreenBomb, 4, 4,
                (sr, sb, sc) => sr.Bomb12(sb, sc, 4, 4));
        }

        //# ── BoomAll 골든 테스트 ────────────────────────────────────────────────

        //# BoomAll 은 block 인자 없음 → 전용 캡처 경로. 9×9 Cat1 보드에서 전체 81칸 match 비교.
        [Test]
        public void 골든_BoomAll_전체81칸_셀일치()
        {
            EBlockState[,] states = Fill(9, EBlockState.Cat1);

            //# 실 GPBombResolver BoomAll 캡처
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard gb = RealBlockFactory.CreateGPBoard(arr, arr.GetLength(0));
            GPMatchChecker gm = RealBlockFactory.CreateGPMatchChecker(gb, 5);
            int pangMax = (int)EPangEffect.Max;
            List<ParticleSystem> pangList = new List<ParticleSystem>(pangMax);
            for (int i = 0; i < pangMax; ++i)
            {
                pangList.Add(null);
            }
            GPBombResolver res = new GPBombResolver();
            res.Init(gb, gm,
                () => System.Threading.Tasks.Task.CompletedTask,
                _ => { },
                (ps, pos) => null,
                _ => { },
                pangList, null, default(CancellationToken));
            RealBlockFactory.EnsureCHMDataCollection();
            res.BoomAll(false).GetAwaiter().GetResult();

            HashSet<(int, int)> real = new HashSet<(int, int)>();
            for (int r = 0; r < arr.GetLength(0); ++r)
            {
                for (int c = 0; c < arr.GetLength(1); ++c)
                {
                    if (arr[r, c].match)
                    {
                        real.Add((r, c));
                    }
                }
            }
            RealBlockFactory.Destroy(arr);

            //# Sim BoomAll 캡처
            SimBoard sb = BuildSimBoard(states);
            SimMatchChecker sc = new SimMatchChecker();
            SimBombResolver sr = new SimBombResolver(new System.Random(1));
            sr.BoomAll(sb, sc);

            HashSet<(int, int)> sim = CollectMatched(sb);

            Assert.That(sim, Is.EquivalentTo(real), "BoomAll 발동 셀 집합 일치 (전체 81칸 기대)");
        }

        //# ── Boom3 Sim 단위 테스트 ─────────────────────────────────────────────

        //# 실 Boom3 은 async+DOTween 이므로 골든 비교 대신 Sim 단위 검증.
        //# Cat1 채움 보드에서 (4,4)를 PinkBomb 으로 설정 후 Boom3(targetColor=Cat1) → Cat1 전부 match.
        [Test]
        public void Sim_Boom3_대상색_전부_match()
        {
            SimBoard board = new SimBoard(5);
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    board.SetState(r, c, EBlockState.Cat1);
                }
            }
            board.SetState(2, 2, EBlockState.PinkBomb);

            SimBombResolver sr = new SimBombResolver(new System.Random(1));
            sr.Boom3(board, 2, 2, EBlockState.Cat1);

            //# (2,2) specialBlock match=true + Cat1 24칸 match → 총 25칸 모두 match
            int matchCount = 0;
            for (int r = 0; r < 5; ++r)
            {
                for (int c = 0; c < 5; ++c)
                {
                    if (board.Grid[r, c].Match)
                    {
                        ++matchCount;
                    }
                }
            }
            Assert.AreEqual(25, matchCount, "Boom3: PinkBomb 자신 + Cat1 24칸 모두 match");
        }

        //# 엣지: 대상색이 없으면 specialBlock 만 match.
        [Test]
        public void Sim_Boom3_대상색_없으면_specialBlock만_match()
        {
            SimBoard board = new SimBoard(3);
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    board.SetState(r, c, EBlockState.Cat2);
                }
            }
            board.SetState(1, 1, EBlockState.PinkBomb);

            SimBombResolver sr = new SimBombResolver(new System.Random(1));
            sr.Boom3(board, 1, 1, EBlockState.Cat1);

            int matchCount = 0;
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    if (board.Grid[r, c].Match)
                    {
                        ++matchCount;
                    }
                }
            }
            Assert.AreEqual(1, matchCount, "Boom3: 대상색 없으면 specialBlock(PinkBomb) 1칸만 match");
        }

        //# ── RainbowPang Sim 단위 테스트 ───────────────────────────────────────

        //# 시드 결정적 살포 개수: hp=0 RainbowPang 에서 PinkBomb..BlueBomb 5종 색폭탄 5개 살포 예약(ChangeBlockState).
        [Test]
        public void Sim_RainbowPang_hp0_시드결정적_5개살포()
        {
            //# 9×9 Cat1 보드, (4,4)에 hp=0 RainbowPang.
            SimBoard board = new SimBoard(9);
            for (int r = 0; r < 9; ++r)
            {
                for (int c = 0; c < 9; ++c)
                {
                    board.SetState(r, c, EBlockState.Cat1);
                }
            }
            board.Grid[4, 4].State = EBlockState.RainbowPang;
            board.Grid[4, 4].Hp = 0;   //# hp<=0 → 발동 조건

            SimBombResolver sr = new SimBombResolver(new System.Random(42));
            sr.RainbowPang(board, board.Grid[4, 4]);

            //# self match=true
            Assert.IsTrue(board.Grid[4, 4].Match, "RainbowPang self.Match=true");

            //# ChangeBlockState 가 예약된 칸 수 = 5 (PinkBomb~BlueBomb 각 1개)
            int changeCount = 0;
            for (int r = 0; r < 9; ++r)
            {
                for (int c = 0; c < 9; ++c)
                {
                    if (board.Grid[r, c].ChangeBlockState != EBlockState.None)
                    {
                        ++changeCount;
                    }
                }
            }
            Assert.AreEqual(5, changeCount, "RainbowPang: PinkBomb~BlueBomb 5종 각 1개 살포");
        }

        //# 엣지: hp>0 이면 발동 안 함.
        [Test]
        public void Sim_RainbowPang_hp_양수이면_발동안함()
        {
            SimBoard board = new SimBoard(3);
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    board.SetState(r, c, EBlockState.Cat1);
                }
            }
            board.Grid[1, 1].State = EBlockState.RainbowPang;
            board.Grid[1, 1].Hp = 1;

            SimBombResolver sr = new SimBombResolver(new System.Random(0));
            sr.RainbowPang(board, board.Grid[1, 1]);

            Assert.IsFalse(board.Grid[1, 1].Match, "RainbowPang hp>0 이면 self.Match=false(발동 안 함)");
        }

        //# ── 공통 헬퍼 ─────────────────────────────────────────────────────────

        //# (br,bc)에 bombState 두고 실/Sim BombN 발동 → match 셀 집합을 Is.EquivalentTo 비교.
        private static void AssertBombMatchesReal(
            EBlockState bombState,
            int br, int bc,
            Action<SimBombResolver, SimBoard, SimMatchChecker> simRun)
        {
            EBlockState[,] states = Fill(9, EBlockState.Cat1);
            states[br, bc] = bombState;

            HashSet<(int, int)> real = CaptureReal(states, br, bc, bombState);

            SimBoard sb = BuildSimBoard(states);
            SimMatchChecker sc = new SimMatchChecker();
            SimBombResolver sr = new SimBombResolver(new System.Random(1));
            simRun(sr, sb, sc);

            HashSet<(int, int)> sim = CollectMatched(sb);

            Assert.That(sim, Is.EquivalentTo(real),
                $"{bombState} 발동 셀 집합 — Sim 과 실게임 일치");
        }

        //# 실 GPBombResolver 에서 지정 BombN 을 호출하고 match=true 인 셀 좌표 집합을 반환.
        private static HashSet<(int, int)> CaptureReal(
            EBlockState[,] states, int br, int bc, EBlockState bombState)
        {
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard gb = RealBlockFactory.CreateGPBoard(arr, arr.GetLength(0));
            GPMatchChecker gm = RealBlockFactory.CreateGPMatchChecker(gb, 5);

            int pangMax = (int)EPangEffect.Max;
            List<ParticleSystem> pangList = new List<ParticleSystem>(pangMax);
            for (int i = 0; i < pangMax; ++i)
            {
                pangList.Add(null);
            }

            //# Bomb4/5/6/7/8 의 실 메서드가 createEffect 반환값에 .Rotate() 를 호출한다.
            //# null stub 이면 NRE — 폭탄 블록의 rectTransform 을 반환해 역참조 안전하게 한다.
            //# .Rotate() 결과는 match 캡처에 영향 없음(좌표만 읽음).
            RectTransform bombRt = arr[br, bc].rectTransform;

            GPBombResolver res = new GPBombResolver();
            res.Init(gb, gm,
                () => System.Threading.Tasks.Task.CompletedTask,
                _ => { },
                (ps, pos) => bombRt,
                _ => { },
                pangList, null, default(CancellationToken));

            RealBlockFactory.EnsureCHMDataCollection();
            InvokeRealBomb(res, arr[br, bc], bombState);

            HashSet<(int, int)> set = new HashSet<(int, int)>();
            for (int r = 0; r < arr.GetLength(0); ++r)
            {
                for (int c = 0; c < arr.GetLength(1); ++c)
                {
                    if (arr[r, c].match)
                    {
                        set.Add((r, c));
                    }
                }
            }

            RealBlockFactory.Destroy(arr);
            return set;
        }

        //# CLAUDE.md 폭탄 매핑대로 실 BombN 을 async→GetAwaiter().GetResult() 로 동기 호출.
        //# BoomAll 은 block 인자 없으므로 별도 경로(골든_BoomAll 테스트)에서 직접 호출.
        private static void InvokeRealBomb(GPBombResolver res, Block block, EBlockState state)
        {
            switch (state)
            {
                case EBlockState.CatPang:    res.Bomb1(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.Arrow5:     res.Bomb2(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.Arrow1:     res.Bomb4(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.Arrow3:     res.Bomb5(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.Arrow6:     res.Bomb6(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.Arrow2:     res.Bomb7(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.Arrow4:     res.Bomb8(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.YellowBomb: res.Bomb9(block,  false).GetAwaiter().GetResult(); break;
                case EBlockState.OrangeBomb: res.Bomb10(block, false).GetAwaiter().GetResult(); break;
                case EBlockState.BlueBomb:   res.Bomb11(block, false).GetAwaiter().GetResult(); break;
                case EBlockState.GreenBomb:  res.Bomb12(block, false).GetAwaiter().GetResult(); break;
                default:
                    Assert.Fail($"InvokeRealBomb: 미지원 bombState={state}");
                    break;
            }
        }

        private static SimBoard BuildSimBoard(EBlockState[,] states)
        {
            int n = states.GetLength(0);
            SimBoard sb = new SimBoard(n);
            for (int r = 0; r < n; ++r)
            {
                for (int c = 0; c < n; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                }
            }
            return sb;
        }

        private static HashSet<(int, int)> CollectMatched(SimBoard sb)
        {
            HashSet<(int, int)> set = new HashSet<(int, int)>();
            for (int r = 0; r < sb.Size; ++r)
            {
                for (int c = 0; c < sb.Size; ++c)
                {
                    if (sb.Grid[r, c].Match)
                    {
                        set.Add((r, c));
                    }
                }
            }
            return set;
        }

        private static EBlockState[,] Fill(int n, EBlockState s)
        {
            EBlockState[,] a = new EBlockState[n, n];
            for (int r = 0; r < n; ++r)
            {
                for (int c = 0; c < n; ++c)
                {
                    a[r, c] = s;
                }
            }
            return a;
        }
    }
}
