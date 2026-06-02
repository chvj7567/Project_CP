using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using CatPang.Sim;

namespace CatPang.Sim.EditorTools
{
    //# M1 노멀모드 배치 시뮬 러너. 메뉴에서 stage 1~150 × {Random,Greedy} × 30판 돌려 메트릭 JSON 출력.
    //# 에디터 전용(자동 테스트 대상 아님). 컴파일만 보장.
    public static class SimBatchRunner
    {
        private const int Seed = 42;
        private const int PlaysPerStageStrategy = 30;
        private const int StageFrom = 1;
        private const int StageTo = 150;

        private const string StageJsonPath = "Assets/AssetBundleResources/json/Stage.json";
        private const string StageBlockJsonPath = "Assets/AssetBundleResources/json/StageBlock.json";

        [MenuItem("CatPang/Sim/Run Batch (M1 Normal)")]
        private static void RunBatch()
        {
            string stageJson = File.ReadAllText(StageJsonPath);
            string stageBlockJson = File.ReadAllText(StageBlockJsonPath);

            string[] strategies = { "Random", "Greedy" };
            SimMetrics metrics = new SimMetrics();

            for (int stage = StageFrom; stage <= StageTo; ++stage)
            {
                foreach (string strategy in strategies)
                {
                    SimStageData data;
                    try
                    {
                        data = SimStageLoader.Load(stageJson, stageBlockJson, stage, normalMode: true);
                    }
                    catch
                    {
                        //# 해당 stage 메타 없음 → 건너뜀.
                        continue;
                    }

                    metrics.SetMode(stage, strategy, ClassifyMode(data));

                    for (int i = 0; i < PlaysPerStageStrategy; ++i)
                    {
                        int seed = Seed + i;
                        ISimAiPolicy policy = strategy == "Greedy"
                            ? (ISimAiPolicy)new GreedyAiPolicy(seed)
                            : new RandomAiPolicy(seed);
                        SimResult r = new SimGame().Run(data, policy, seed);
                        metrics.Add(r);
                    }
                }
            }

            string path = WriteJson(metrics);
            Debug.Log($"[SimBatchRunner] M1 노멀모드 배치 완료 → {path}");
            AssetDatabase.Refresh();
        }

        //# 로더 판정 기준 모드 분류: 시간모드→Unsupported, 이동모드→Move, 점수목표(>0)→ScoreGoal, 그 외→Unsupported.
        private static string ClassifyMode(SimStageData data)
        {
            if (data.IsTimeMode)
                return "Unsupported";
            if (data.IsMoveMode)
                return "Move";
            if (data.TargetScore > 0)
                return "ScoreGoal";

            return "Unsupported";
        }

        //# docs/qa-reports/sim-output/m1-batch-{timestamp}.json 에 수동 StringBuilder JSON 저장. 저장 경로 반환.
        private static string WriteJson(SimMetrics metrics)
        {
            string dir = "docs/qa-reports/sim-output";
            Directory.CreateDirectory(dir);
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string path = Path.Combine(dir, $"m1-batch-{timestamp}.json");

            string note = "M1 노멀모드. 모드별 세그먼트: Move=클리어율+avgMoves, ScoreGoal=avgTurns, Unsupported=제외. Task3 매치판정만 실게임 골든 검증, 나머지는 수작업 골든";

            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            sb.Append("\"note\":\"").Append(note).Append("\",");
            sb.Append("\"seed\":").Append(Seed).Append(',');
            sb.Append("\"playsPerStageStrategy\":").Append(PlaysPerStageStrategy).Append(',');
            sb.Append("\"metrics\":[");

            bool first = true;
            foreach (StageMetric m in metrics.All())
            {
                if (first == false)
                {
                    sb.Append(',');
                }
                first = false;
                AppendMetric(sb, m);
            }

            sb.Append("]}");

            File.WriteAllText(path, sb.ToString());
            return path;
        }

        private static void AppendMetric(StringBuilder sb, StageMetric m)
        {
            sb.Append('{');
            sb.Append("\"stage\":").Append(m.Stage).Append(',');
            sb.Append("\"policy\":\"").Append(m.Policy).Append("\",");
            sb.Append("\"mode\":\"").Append(m.Mode).Append("\",");
            sb.Append("\"plays\":").Append(m.Plays).Append(',');
            sb.Append("\"clears\":").Append(m.Clears).Append(',');
            sb.Append("\"clearRate\":").Append(F(m.ClearRate)).Append(',');
            sb.Append("\"avgMoves\":").Append(F(m.AvgMoves)).Append(',');
            sb.Append("\"avgTurns\":").Append(F(m.AvgTurns)).Append(',');
            sb.Append("\"moveOver\":").Append(m.MoveOver).Append(',');
            sb.Append("\"timeOver\":").Append(m.TimeOver).Append(',');
            sb.Append("\"unsupported\":").Append(m.Unsupported);
            sb.Append('}');
        }

        //# float → 로케일 무관 JSON 숫자 문자열(소수점은 항상 '.').
        private static string F(float value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
