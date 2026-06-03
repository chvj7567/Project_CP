using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using CatPang.Sim;
using static Defines;

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

        //# 커버리지 게이트(Task8 목표 59/150) 전용: play 없이 IsSupported 분류만으로 supported 수 집계.
        //# IsSupported 는 IsTimeMode + InitialStates 블록타입 스캔(턴루프 진입 전) 이라 hang 위험 0, 밀리초 완료.
        //# 결과를 파일에 기록 — MCP 응답 timeout 과 무관하게 결과 보존. 반환 문자열도 짧음.
        public static string CountSupported()
        {
            string stageJson = File.ReadAllText(StageJsonPath);
            string stageBlockJson = File.ReadAllText(StageBlockJsonPath);

            int supported = 0;
            int unsupported = 0;
            int missing = 0;
            StringBuilder supportedList = new StringBuilder();
            StringBuilder unsupportedList = new StringBuilder();

            for (int stage = StageFrom; stage <= StageTo; ++stage)
            {
                SimStageData data;
                try
                {
                    data = SimStageLoader.Load(stageJson, stageBlockJson, stage, normalMode: true);
                }
                catch
                {
                    ++missing;
                    continue;
                }

                if (SimGame.IsSupported(data))
                {
                    ++supported;
                    supportedList.Append(stage).Append(' ');
                }
                else
                {
                    ++unsupported;
                    unsupportedList.Append(stage).Append(' ');
                }
            }

            string body = $"M2 커버리지 카운트 (play 없이 IsSupported 분류만)\n"
                + $"supported={supported} / unsupported={unsupported} / missing={missing} (총 {StageTo - StageFrom + 1} stage)\n\n"
                + $"supported stages:\n{supportedList}\n\n"
                + $"unsupported stages:\n{unsupportedList}\n";

            string dir = "docs/qa-reports/sim-output";
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "m2-coverage-count.txt");
            File.WriteAllText(path, body);

            return $"supported={supported} unsupported={unsupported} missing={missing} → {path}";
        }

        //# Creator 사망 판별: cap-hitter 6개에 RandomAi 1판 + 최종보드 census 를 stage 마다 파일에 증분 기록.
        //# 핵심 질문 — "Creator 가 죽는가?". 살아있는 Creator + 다수 Wall 잔존 = Creator 불멸 버그(53개 오염 위험).
        //# (Greedy 는 캡-히터에서 분단위로 느려 30s MCP 디스패치 한계로 드롭 → 제외. census 는 Random 만으로 충분.)
        //# 증분 기록: stage 1개 끝날 때마다 File.AppendAllText → 중단돼도 부분결과 보존.
        public static string DiagnoseCapHitters()
        {
            string stageJson = File.ReadAllText(StageJsonPath);
            string stageBlockJson = File.ReadAllText(StageBlockJsonPath);

            int[] stages = { 73, 74, 77, 82, 119, 129 };

            string dir = "docs/qa-reports/sim-output";
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "m2-caphitter-diagnose.txt");
            File.WriteAllText(path, "M2 cap-hitter 판별 (RandomAi seed42, Creator 사망 여부)\n\n");

            foreach (int stage in stages)
            {
                SimStageData data = SimStageLoader.Load(stageJson, stageBlockJson, stage, normalMode: true);
                SimGame game = new SimGame();
                SimResult r = game.Run(data, new RandomAiPolicy(42), 42);

                //# 최종보드 census — hp 버킷 포함. hp<=0 인데 Wall/Creator 상태로 남은 블록 = Sim 전환버그 신호.
                int wallHpPos = 0;
                int wallHpZero = 0;   //# Hp<=0 인데 Wall 상태(전환 안 됨 → 버그)
                int wallZeroPending = 0; //# 그 중 ChangeBlockState!=None (예약은 됐는데 미적용 → Apply경로 버그)
                int wallZeroNoConv = 0;  //# 그 중 ChangeBlockState==None (예약 자체가 안 됨 → Damage경로 버그)
                int creatorHpPos = 0; //# Hp>0 — 아직 활성(Wall 계속 생성)
                int creatorHpZero = 0;
                int catBox = 0;
                int liveTarget = 0;
                SimBoard b = game.LastBoard;
                if (b != null)
                {
                    foreach (SimBlock blk in b.Grid)
                    {
                        if (blk.IsWall())
                        {
                            if (blk.Hp > 0)
                            {
                                ++wallHpPos;
                            }
                            else
                            {
                                ++wallHpZero;
                                if (blk.ChangeBlockState != EBlockState.None) ++wallZeroPending; else ++wallZeroNoConv;
                            }
                        }
                        if (blk.IsCreator())
                        {
                            if (blk.Hp > 0) ++creatorHpPos; else ++creatorHpZero;
                        }
                        if (blk.IsCatBox())
                            ++catBox;
                        if (blk.CheckHp() && blk.Hp > 0)
                            ++liveTarget;
                    }
                }

                string mark = r.CapHit ? " [CAP]" : "";
                string block = $"=== stage{stage} (target={data.TargetScore} moveCount={data.MoveCount}) ===\n"
                    + $"  Random: clear={r.Clear} turns={r.Turns} moves={r.MovesUsed} score={r.FinalScore} fail={r.FailReason}{mark}\n"
                    + $"          Wall: hp>0={wallHpPos} hp<=0={wallHpZero}(예약있음={wallZeroPending} 예약없음={wallZeroNoConv}) | Creator: hp>0(활성)={creatorHpPos} hp<=0(소진)={creatorHpZero} | CatBox={catBox} | 잔존목표={liveTarget}\n\n";
                File.AppendAllText(path, block);
            }

            return $"done → {path}";
        }

        //# 진단용: supported stage 만 1판씩 돌려 메트릭 + CapHit(무한방지 캡 도달=메트릭 신뢰불가) stage 식별.
        //# 결과 전부 파일 기록 — MCP 응답 timeout 과 무관하게 결과 보존. 반환은 짧은 요약.
        //# (unsupported 는 play 진입 전 즉시 반환이라 빠름 — 시간은 supported stage 에서만 발생.)
        public static string Diagnose()
        {
            string stageJson = File.ReadAllText(StageJsonPath);
            string stageBlockJson = File.ReadAllText(StageBlockJsonPath);

            int supported = 0;
            int unsupported = 0;
            int capHit = 0;
            long totalMs = 0;
            StringBuilder lines = new StringBuilder();
            StringBuilder capList = new StringBuilder();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            for (int stage = StageFrom; stage <= StageTo; ++stage)
            {
                SimStageData data;
                try
                {
                    data = SimStageLoader.Load(stageJson, stageBlockJson, stage, normalMode: true);
                }
                catch
                {
                    continue;
                }

                if (SimGame.IsSupported(data) == false)
                {
                    ++unsupported;
                    continue;
                }
                ++supported;

                sw.Restart();
                SimResult r = new SimGame().Run(data, new RandomAiPolicy(42), 42);
                sw.Stop();
                totalMs += sw.ElapsedMilliseconds;

                string mark = r.CapHit ? " [CAP]" : "";
                lines.Append($"stage{stage}: {sw.ElapsedMilliseconds}ms clear={r.Clear} turns={r.Turns} moves={r.MovesUsed} score={r.FinalScore} fail={r.FailReason}{mark}\n");

                if (r.CapHit)
                {
                    ++capHit;
                    capList.Append(stage).Append(' ');
                }
            }

            string body = $"M2 진단 (supported stage 각 1판, RandomAi seed42)\n"
                + $"supported={supported} unsupported={unsupported} capHit={capHit} 총소요={totalMs}ms\n\n"
                + $"CapHit stages (무한방지 캡 도달 → 메트릭 신뢰불가, sim 버그 후보):\n{capList}\n\n"
                + $"--- 전체 supported stage 메트릭 ---\n{lines}";

            string dir = "docs/qa-reports/sim-output";
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "m2-diagnose.txt");
            File.WriteAllText(path, body);

            return $"supported={supported} unsupported={unsupported} capHit={capHit} totalMs={totalMs} → {path}";
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

        //# docs/qa-reports/sim-output/m2-batch-{timestamp}.json 에 수동 StringBuilder JSON 저장. 저장 경로 반환.
        private static string WriteJson(SimMetrics metrics)
        {
            string dir = "docs/qa-reports/sim-output";
            Directory.CreateDirectory(dir);
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string path = Path.Combine(dir, $"m2-batch-{timestamp}.json");

            string note = "M2 노멀모드. 정적 특수블록(Wall/Potal/Creator/CatBox) 지원 추가. 모드별 세그먼트: Move=클리어율+avgMoves, ScoreGoal=avgTurns, Unsupported(M3블록 Fish/Ball/Arrow/폭탄/Rainbow)=제외. 실게임 골든: 매치판정/인접데미지/CatBox판정. 나머지(낙하/턴루프/승패/Creator)는 수작업 골든";

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
