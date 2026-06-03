using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimM2Tests
    {
        private static string StageJson => System.IO.File.ReadAllText("Assets/AssetBundleResources/json/Stage.json");
        private static string StageBlockJson => System.IO.File.ReadAllText("Assets/AssetBundleResources/json/StageBlock.json");

        [Test]
        public void 로더_특수블록_HP를_InitialHps에_싣는다()
        {
            //# Wall/Potal/CatBox 가 있는 스테이지면 InitialHps 가 InitialStates 와 같은 길이로 채워진다.
            //# stage 6 은 M2 대상(Wall/Potal/CatBox 류) — InitialStates 와 InitialHps 길이 동일 확인.
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 6, normalMode: true);
            Assert.AreEqual(s.InitialStates.Length, s.InitialHps.Length, "HP 배열 길이 = 상태 배열 길이");
            //# 레코드 없는 칸(None)은 hp -1
            for (int i = 0; i < s.InitialStates.Length; ++i)
            {
                if (s.InitialStates[i] == EBlockState.None)
                {
                    Assert.AreEqual(-1, s.InitialHps[i], $"None 칸 {i} 은 hp -1");
                }
            }
        }

        [Test]
        public void SimBlock_분류헬퍼_특수블록을_구분한다()
        {
            SimBlock wall = new SimBlock { State = EBlockState.Wall };
            SimBlock potal = new SimBlock { State = EBlockState.Potal };
            SimBlock box = new SimBlock { State = EBlockState.CatBox1 };
            SimBlock creator = new SimBlock { State = EBlockState.WallCreator };
            SimBlock cat = new SimBlock { State = EBlockState.Cat1 };

            Assert.IsTrue(wall.IsWall(), "Wall.IsWall");
            Assert.IsTrue(box.IsCatBox(), "CatBox.IsCatBox");
            Assert.IsTrue(creator.IsCreator(), "Creator.IsCreator");
            Assert.IsFalse(cat.IsWall(), "Cat 은 Wall 아님");
            //# CheckHp: Creator 만 false(목표블록 아님)
            Assert.IsTrue(wall.CheckHp(), "Wall 은 목표블록");
            Assert.IsTrue(potal.CheckHp(), "Potal 은 목표블록");
            Assert.IsTrue(box.CheckHp(), "CatBox 는 목표블록");
            Assert.IsFalse(creator.CheckHp(), "Creator 는 목표블록 아님");
        }

        [Test]
        public void SimBlock_Damage_는_HP를_깎고_0이면_일반블록전환_플래그를_세운다()
        {
            SimBlock wall = new SimBlock { State = EBlockState.Wall, Hp = 2 };
            wall.Damage(blockTypeCount: 3);
            Assert.AreEqual(1, wall.Hp, "hp 2→1");
            Assert.AreEqual(EBlockState.None, wall.ChangeBlockState, "아직 전환 예약 없음");

            //# 같은 턴 재호출은 checkDamage 가드로 무시
            wall.Damage(blockTypeCount: 3);
            Assert.AreEqual(1, wall.Hp, "턴당 1회 — 변화 없음");

            //# 다음 턴: 리셋 후 한번 더 → hp 0 → 전환 예약
            wall.ResetCheckDamage();
            wall.Damage(blockTypeCount: 3);
            Assert.AreEqual(0, wall.Hp, "hp 1→0");
            Assert.IsTrue(wall.ChangeBlockState >= EBlockState.Cat1 && wall.ChangeBlockState <= EBlockState.Cat3,
                "hp 0 → 일반블록 전환 예약");
        }

        [Test]
        public void SimBlock_CatBox_는_Damage_무시()
        {
            SimBlock box = new SimBlock { State = EBlockState.CatBox1, Hp = 3 };
            box.ResetCheckDamage();
            box.Damage(blockTypeCount: 3);
            Assert.AreEqual(3, box.Hp, "박스는 Damage 로 안 깎임(CatInTheBox 로만)");
        }

        [Test]
        public void 낙하_Wall아래로는_위블록이_안떨어진다()
        {
            //# 열0: (0,0)Cat1, (1,0)Wall, (2,0)매치제거(빈칸).
            //# Wall 이 낙하 차단 → Cat1 은 Wall 위(0,0)에 머물고, 빈칸(2,0)은 리필.
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Cat1);
            board.SetState(1, 0, EBlockState.Wall); board.Grid[1, 0].Hp = 1;
            board.SetState(2, 0, EBlockState.Cat2); board.Grid[2, 0].Match = true;

            new SimGravity(seed: 1).Apply(board, blockTypeCount: 3);

            Assert.AreEqual(EBlockState.Cat1, board.GetState(0, 0), "Cat1 은 Wall 위에 머묾");
            Assert.AreEqual(EBlockState.Wall, board.GetState(1, 0), "Wall 고정");
            Assert.IsTrue(board.Grid[2, 0].IsNormal(), "Wall 아래 빈칸은 리필");
        }

        [Test]
        public void 상태전이_ChangeBlockState가_다음턴_적용된다()
        {
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Wall);
            board.Grid[0, 0].Hp = 0;
            board.Grid[0, 0].ChangeBlockState = EBlockState.Cat1;
            board.Grid[0, 0].ChangeHp = -1;

            SimSpecialBlocks.ApplyChanges(board);

            Assert.AreEqual(EBlockState.Cat1, board.GetState(0, 0), "예약된 상태로 전환");
            Assert.AreEqual(-1, board.Grid[0, 0].Hp, "전환 후 hp 적용");
            Assert.AreEqual(EBlockState.None, board.Grid[0, 0].ChangeBlockState, "예약 소비됨");
        }

        [Test]
        public void Creator_는_주변_일반블록을_변환예약한다()
        {
            //# WallCreator(hp2) 중앙, 사방 일반블록 → 한 칸이 Wall 변환 예약 + creator 데미지.
            SimBoard board = new SimBoard(3);
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    board.SetState(r, c, EBlockState.Cat1);
                    board.Grid[r, c].Hp = -1;
                }
            }
            board.SetState(1, 1, EBlockState.WallCreator);
            board.Grid[1, 1].Hp = 2;

            SimSpecialBlocks.RunCreators(board, new System.Random(1));

            //# creator 주변에 Wall 변환 예약(ChangeBlockState==Wall)된 칸이 정확히 1개
            int reserved = 0;
            foreach (SimBlock b in board.Grid)
            {
                if (b.ChangeBlockState == EBlockState.Wall)
                {
                    ++reserved;
                }
            }
            Assert.AreEqual(1, reserved, "creator 가 일반블록 1개를 Wall 로 변환 예약");
            Assert.AreEqual(1, board.Grid[1, 1].Hp, "creator 자신 데미지 2→1");
        }

        //# CatBox hp 감소·수집 — 실게임 CatInTheBox 는 hpText 의존이라 골든 불가(SimSpecialBlockGoldenTests 참고).
        //# Sim 자체 동작만 검증: 받는 고양이면 hp 감소(hp>0일 때), CollectCatBoxes 가 위 일반블록을 Match 표시.
        [Test]
        public void CatBox_받는고양이면_hp감소하고_위블록을_수집표시한다()
        {
            //# CatBox1(hp2) (1,0), 위 (0,0)=Cat1(받음). 나머지는 매치 안 생기게.
            SimBoard board = new SimBoard(3);
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat7, EBlockState.Cat2 },
                { EBlockState.CatBox1, EBlockState.Cat3, EBlockState.Cat7 },
                { EBlockState.Cat2, EBlockState.Cat7, EBlockState.Cat3 },
            };
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    board.SetState(r, c, states[r, c]);
                    board.Grid[r, c].Hp = -1;
                }
            }
            board.Grid[1, 0].Hp = 2;

            bool collected = SimSpecialBlocks.CollectCatBoxes(board);

            Assert.IsTrue(collected, "수집 발생");
            Assert.AreEqual(1, board.Grid[1, 0].Hp, "박스 hp 2→1");
            Assert.IsTrue(board.Grid[0, 0].Match, "위 Cat1 이 수집 표시(Match)");
        }

        [Test]
        public void IsSupported_M2블록은_지원된다()
        {
            //# stage6 = Wall(목표블록) 포함 M2 대상. IsSupported 확장으로 지원되어야(Unsupported=false).
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 6, normalMode: true);
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            Assert.IsFalse(r.Unsupported, "stage6(M2 블록)은 지원되어야");
        }

        [Test]
        public void 승패_점수목표_도달불가면_이동소진_미클리어()
        {
            //# 양방향 테스트의 "반드시 미클리어" 축. 실제 stage 의 목표블록은 hp1 이라 연쇄로 우연 제거돼
            //# (stage83 CatBox·stage18 Wall 모두 클리어됨) 신뢰 불가 → 점수목표를 도달불가로 강제한다.
            //# M2 supported stage(6) 로드 후 TargetScore=int.MaxValue + MoveCount=2 → 점수 조건 때문에
            //# CheckClear 의 AND 가 반드시 false → MoveOver. (M1 Task7 의 동일 패턴.)
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 6, normalMode: true);
            s.TargetScore = int.MaxValue;
            s.MoveCount = 2;
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            Assert.IsFalse(r.Clear, "점수목표 도달불가 → 미클리어");
            Assert.AreEqual(EFailReason.MoveOver, r.FailReason, "이동 소진 → MoveOver");
        }

        [Test]
        public void 승패_목표블록없는_stage7은_클리어()
        {
            //# stage7: 목표블록 = Potal 1개(hp-1, 비활성). 실제 차단 목표블록 0개.
            //# normalMode:true → time 제거(지원) + targetScore 1000→500, moveCount -1(무제한).
            //# 무제한 이동으로 점수목표(500) 도달 → Clear.
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 7, normalMode: true);
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            Assert.IsTrue(r.Clear, "차단 목표블록 없음 + 점수목표 도달 → 클리어");
        }

        [Test]
        public void CatBox_안받는고양이면_수집없음()
        {
            //# CatBox1(hp2) 위에 Cat2(안 받음) → 수집 없음, hp 유지.
            SimBoard board = new SimBoard(3);
            EBlockState[,] states =
            {
                { EBlockState.Cat2, EBlockState.Cat7, EBlockState.Cat3 },
                { EBlockState.CatBox1, EBlockState.Cat3, EBlockState.Cat7 },
                { EBlockState.Cat3, EBlockState.Cat7, EBlockState.Cat2 },
            };
            for (int r = 0; r < 3; ++r)
            {
                for (int c = 0; c < 3; ++c)
                {
                    board.SetState(r, c, states[r, c]);
                    board.Grid[r, c].Hp = -1;
                }
            }
            board.Grid[1, 0].Hp = 2;

            bool collected = SimSpecialBlocks.CollectCatBoxes(board);

            Assert.IsFalse(collected, "안 받는 고양이 → 수집 없음");
            Assert.AreEqual(2, board.Grid[1, 0].Hp, "박스 hp 유지");
            Assert.IsFalse(board.Grid[0, 0].Match, "위 Cat2 미수집");
        }
    }
}
