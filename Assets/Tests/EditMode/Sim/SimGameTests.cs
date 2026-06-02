using System.IO;
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimGameTests
    {
        private static string StageJson => File.ReadAllText("Assets/AssetBundleResources/json/Stage.json");
        private static string StageBlockJson => File.ReadAllText("Assets/AssetBundleResources/json/StageBlock.json");

        [Test]
        public void 로더_스테이지1_메타와_블록배치를_읽는다()
        {
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, stage: 1, normalMode: false);
            Assert.AreEqual(1, s.Stage);
            Assert.AreEqual(3, s.BoardSize);                  //# 실제 stage1 boardSize=3
            Assert.AreEqual(100, s.TargetScore);              //# 실제 stage1 targetScore=100
            Assert.AreEqual(3, s.BlockTypeCount);             //# 실제 stage1 blockTypeCount=3
            Assert.AreEqual(s.BoardSize * s.BoardSize, s.InitialStates.Length);
        }

        [Test]
        public void 로더_노멀모드는_시간제거_목표점수절반()
        {
            //# stage1: time=10, targetScore=100, moveCount=-1.
            //# 실제 ApplyNormalModifiers → time=-1, targetScore=100/2=50, moveCount 은 그대로(-1).
            SimStageData n = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
            Assert.IsTrue(n.Time <= 0, "노멀은 시간제한 제거");
            Assert.AreEqual(50, n.TargetScore);
            Assert.AreEqual(-1, n.MoveCount);
        }

        [Test]
        public void 로더_노멀모드_무목표스테이지는_이동2배()
        {
            //# stage13: time=-1, targetScore=-1(무목표), moveCount=20.
            //# 실제 ApplyNormalModifiers → targetScore<=0 이라 else 분기 → moveCount=20*2=40.
            SimStageData n = SimStageLoader.Load(StageJson, StageBlockJson, 13, normalMode: true);
            Assert.AreEqual(40, n.MoveCount);
            Assert.AreEqual(-1, n.TargetScore);
        }

        [Test]
        public void AI_랜덤정책은_유효한_매치수를_고른다()
        {
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Cat1);
            board.SetState(0, 1, EBlockState.Cat1);
            board.SetState(0, 2, EBlockState.Cat2);
            board.SetState(1, 0, EBlockState.Cat3);
            board.SetState(1, 1, EBlockState.Cat2);
            board.SetState(1, 2, EBlockState.Cat1);
            board.SetState(2, 0, EBlockState.Cat2);
            board.SetState(2, 1, EBlockState.Cat3);
            board.SetState(2, 2, EBlockState.Cat3);

            SimMove? mv = new RandomAiPolicy(seed: 1).ChooseMove(board, new SimMatchChecker());
            Assert.IsTrue(mv.HasValue);

            SimMove m = mv.Value;
            board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
            SimMatchChecker checker = new SimMatchChecker();
            checker.CheckMap(board);
            Assert.IsTrue(checker.IsMatch, "AI 가 고른 수는 매치를 만들어야");
        }

        [Test]
        public void 한판_이동소진_점수미달이면_MoveOver()
        {
            //# stage13 은 StageBlock.json 에 Wall(16)/Potal(17) 이 있어 IsSupported=false → Unsupported 로 빠진다.
            //# 따라서 이동제한 MoveOver 검증은 일반블록만 있는 stage1(노멀) 을 base 로 쓰고 필드로 이동모드를 강제한다.
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
            s.MoveCount = 1;              //# 이동 1회로 강제(이동모드)
            s.TargetScore = int.MaxValue; //# 달성 불가 → 반드시 MoveOver 로 종료
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            Assert.IsFalse(r.Clear);
            Assert.AreEqual(EFailReason.MoveOver, r.FailReason);
        }

        [Test]
        public void 한판_목표0이면_즉시_클리어()
        {
            //# 위 MoveOver 케이스와 양방향 대칭: 같은 지원 stage1 을 base 로 목표점수만 0 으로 → 즉시 클리어.
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
            s.TargetScore = 0;
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            Assert.IsTrue(r.Clear);
            Assert.AreEqual(EFailReason.None, r.FailReason);
        }

        [Test]
        public void 한판_시간모드는_Unsupported()
        {
            //# 하드 stage1 원본: time=10 (시간모드) → M1 실시간 불가 → Unsupported
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: false);
            Assert.IsTrue(s.IsTimeMode, "stage1 하드는 시간모드여야(전제 확인)");
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            Assert.IsTrue(r.Unsupported);
        }

        [Test]
        public void 한판_특수블록스테이지는_Unsupported()
        {
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
            s.InitialStates[0] = EBlockState.Wall; //# 특수블록 강제 삽입 → Unsupported
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            Assert.IsTrue(r.Unsupported);
        }

        [Test]
        public void 메트릭_클리어율을_집계한다()
        {
            //# Clear 2판 + MoveOver 1판(총 3 plays) 누적 → 클리어율 2/3.
            //# 키는 Stage|PolicyName 이므로 Add 의 결과와 Get 의 인자를 동일 값으로 맞춘다.
            SimMetrics metrics = new SimMetrics();
            metrics.Add(new SimResult { Stage = 5, PolicyName = "Random", Clear = true });
            metrics.Add(new SimResult { Stage = 5, PolicyName = "Random", Clear = true });
            metrics.Add(new SimResult { Stage = 5, PolicyName = "Random", Clear = false, FailReason = EFailReason.MoveOver });

            StageMetric sm = metrics.Get(5, "Random");
            Assert.AreEqual(3, sm.Plays);
            Assert.AreEqual(2, sm.Clears);
            Assert.AreEqual(1, sm.MoveOver);
            Assert.AreEqual(2f / 3f, sm.ClearRate, 0.001f);
        }

        [Test]
        public void 메트릭_미지원판은_집계제외_별도카운트()
        {
            //# Unsupported 판은 Plays 에 들어가지 않고 Unsupported 카운트만 올라간다.
            SimMetrics metrics = new SimMetrics();
            metrics.Add(new SimResult { Stage = 7, PolicyName = "Greedy", Unsupported = true });

            StageMetric sm = metrics.Get(7, "Greedy");
            Assert.AreEqual(0, sm.Plays);
            Assert.AreEqual(1, sm.Unsupported);
        }
    }
}
