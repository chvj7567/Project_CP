using System.Collections.Generic;
using static Defines;

namespace CatPang.Sim
{
    //# 스테이지×정책 1조합의 누적 집계. 모드(Move/ScoreGoal/Unsupported) 는 로더 판정 결과를 외부에서 세팅.
    public class StageMetric
    {
        public int Stage;
        public string Policy;
        public string Mode; //# "Move" / "ScoreGoal" / "Unsupported" — 모드별 세그먼트 분류

        public int Plays;       //# 실제 플레이된 판수(Unsupported 제외)
        public int Clears;
        public int Unsupported; //# M1 미지원으로 빠진 판수
        public int MoveOver;
        public int TimeOver;

        public long MovesUsedSum;
        public long TurnsSum;

        public float ClearRate => Plays == 0 ? 0f : (float)Clears / Plays;
        public float AvgMoves => Plays == 0 ? 0f : (float)MovesUsedSum / Plays;
        public float AvgTurns => Plays == 0 ? 0f : (float)TurnsSum / Plays;
    }

    //# 여러 판 결과를 (stage|policy) 키로 누적. Unsupported 판은 별도 카운트만 하고 Plays 에서 제외.
    public class SimMetrics
    {
        private readonly Dictionary<string, StageMetric> _byKey = new Dictionary<string, StageMetric>();

        private static string Key(int stage, string policy)
        {
            return $"{stage}|{policy}";
        }

        //# 한 판 결과 누적. 미지원 판은 Unsupported 만 올리고 즉시 종료(Plays 미가산).
        public void Add(SimResult r)
        {
            StageMetric sm = Get(r.Stage, r.PolicyName);
            if (r.Unsupported)
            {
                sm.Unsupported += 1;
                return;
            }

            sm.Plays += 1;
            sm.MovesUsedSum += r.MovesUsed;
            sm.TurnsSum += r.Turns;

            if (r.Clear)
            {
                sm.Clears += 1;
                return;
            }

            //# 비클리어는 실패사유별 카운트. None/HpOver 등은 카운터 없음(조용히 무시).
            switch (r.FailReason)
            {
                case EFailReason.MoveOver:
                    sm.MoveOver += 1;
                    break;
                case EFailReason.TimeOver:
                    sm.TimeOver += 1;
                    break;
                default:
                    break;
            }
        }

        //# (stage,policy) 메트릭. 없으면 생성해 등록.
        public StageMetric Get(int stage, string policy)
        {
            string key = Key(stage, policy);
            if (_byKey.TryGetValue(key, out StageMetric sm))
                return sm;

            sm = new StageMetric { Stage = stage, Policy = policy };
            _byKey.Add(key, sm);
            return sm;
        }

        //# 로더가 판정한 모드 문자열을 메트릭에 기록.
        public void SetMode(int stage, string policy, string mode)
        {
            Get(stage, policy).Mode = mode;
        }

        public IEnumerable<StageMetric> All()
        {
            return _byKey.Values;
        }
    }
}
