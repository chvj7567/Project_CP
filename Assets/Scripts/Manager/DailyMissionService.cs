using System;
using System.Collections.Generic;
using UnityEngine;

// 일일 미션 리셋 판정 + 카운터 hook + 진행도 조회 일원화.
// 모든 진입점에서 가장 먼저 CheckAndResetIfNeeded()를 호출해 자정 넘김을 처리.
// 시각 출처: CHMMain.Time.UtcNow — NTP 수신 시 서버 UTC, 미수신 시 디바이스 UTC 폴백.
// (NTP 폴백 시 디바이스 시각 위변조로 일일 리셋을 부당하게 트리거 가능하지만,
//  플레이 자체는 진행되어야 한다는 UX 우선으로 결정. 엄격한 위변조 방지가 필요해지면
//  여기 가드를 부활시키고 OnAvailable 큐잉 패턴으로 재구성할 것.)
public static class DailyMissionService
{
    const int DailyTapIndex = 3;

    // 일일 탭 진입 시, 게임 클리어 시, 광고 시청 시, 블록 파괴 시 매번 호출.
    public static void CheckAndResetIfNeeded()
    {
        Data.Login login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;

        string todayKey = CHMMain.Time.GetUtcDateKey();
        if (login.lastDailyResetDateKey == todayKey) return;

        // 자정 스냅샷 갱신 — CatPang 등 EBlockState 누적치
        login.dailyCollectionSnapshotJson = SerializeCurrentCollectionSnapshot();

        login.lastDailyResetDateKey = todayKey;
        login.stageClearCountToday = 0;
        login.blockDestroyCountToday = 0;
        login.adWatchCountToday = 0;
        login.attendanceTodayDone = false;

        // 일일 미션 진행 상태 초기화
        foreach (Infomation.MissionInfo info in CHMJson.Instance.GetMissionInfoList(DailyTapIndex))
        {
            Data.Mission data = CHMData.Instance.GetMissionData(info.missionID.ToString());
            if (data == null) continue;
            data.clearState = Defines.EClearState.NotDoing;
            data.startValue = 0;
            data.repeatCount = 0;
        }

        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
        Debug.Log($"[DailyMission] 자정 리셋 완료. todayKey={todayKey}");
    }

    public static void MarkAttendance()
    {
        CheckAndResetIfNeeded();
        Data.Login login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null || login.attendanceTodayDone) return;
        login.attendanceTodayDone = true;
        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
    }

    public static void OnStageClear()
    {
        CheckAndResetIfNeeded();
        Data.Login login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;
        login.stageClearCountToday++;
        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
    }

    public static void OnBlockDestroyed(int count)
    {
        CheckAndResetIfNeeded();
        Data.Login login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;
        login.blockDestroyCountToday += count;
        // 잦은 호출이므로 SaveData는 호출하지 않음 — 게임 종료/씬 전환/다른 카운터 hook에서 저장됨
        // (정확성보다 디스크 I/O 최소화 우선. 게임 강제 종료 시 일부 누락 허용)
    }

    public static void OnAdWatched()
    {
        CheckAndResetIfNeeded();
        Data.Login login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;
        login.adWatchCountToday++;
        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
    }

    // MissionScrollViewItem에서 호출. 진행도(현재값) 반환.
    public static int GetDailyProgress(Infomation.MissionInfo info)
    {
        Data.Login login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return 0;

        if (info.dailyCounter != Defines.EDailyCounter.None)
        {
            switch (info.dailyCounter)
            {
                case Defines.EDailyCounter.Attendance:       return login.attendanceTodayDone ? 1 : 0;
                case Defines.EDailyCounter.NormalStageClear: return login.stageClearCountToday;
                case Defines.EDailyCounter.BlockDestroy:     return login.blockDestroyCountToday;
                case Defines.EDailyCounter.AdWatch:          return login.adWatchCountToday;
                default: return 0;
            }
        }

        if (info.collectionType != Defines.EBlockState.None)
        {
            int snapshot = GetSnapshotValue(login.dailyCollectionSnapshotJson, (int)info.collectionType);
            Data.Collection col = CHMData.Instance.GetCollectionData(info.collectionType.ToString());
            int current = col != null ? col.value : 0;
            return Mathf.Max(0, current - snapshot);
        }

        return 0;
    }

    // 일일 미션 리셋까지 남은 시간 표시. CHMTime이 NTP 미수신 시 디바이스 UTC로 폴백한다.
    public static string GetResetCountdown()
    {
        double sec = CHMMain.Time.GetSecondsUntilNextUtcMidnight();
        TimeSpan ts = TimeSpan.FromSeconds(sec);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    // ===== 내부 =====

    // 현재 모든 Collection 값을 EBlockState 기준 dict로 스냅샷 (자정 시 호출).
    // 기존 dict 엔트리만 순회 — GetCollectionData 호출은 빈 엔트리를 생성하므로 회피.
    static string SerializeCurrentCollectionSnapshot()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{");
        bool first = true;
        Dictionary<string, Data.Collection> dic = CHMData.Instance.collectionLocalDataDic;
        if (dic != null)
        {
            foreach (KeyValuePair<string, Data.Collection> kvp in dic)
            {
                // dict key는 EBlockState.ToString() (이름). int로 변환 필요.
                if (!Enum.TryParse<Defines.EBlockState>(kvp.Key, out Defines.EBlockState bs)) continue;
                if (bs == Defines.EBlockState.None) continue;
                int v = kvp.Value != null ? kvp.Value.value : 0;
                // 0인 항목은 생략 (저장 용량 절감). 전제: Collection.value는 monotonically non-decreasing.
                if (v == 0) continue;
                if (!first) sb.Append(",");
                sb.Append($"\"{(int)bs}\":{v}");
                first = false;
            }
        }
        sb.Append("}");
        return sb.ToString();
    }

    // {"18":42,"0":12} 형식에서 key의 값 추출. 없으면 0.
    static int GetSnapshotValue(string json, int key)
    {
        if (string.IsNullOrEmpty(json)) return 0;
        // 단순 파싱 — 키 형식이 "키:값" 고정이므로 정규식/JsonUtility 없이 처리
        string target = $"\"{key}\":";
        int idx = json.IndexOf(target);
        if (idx < 0) return 0;
        idx += target.Length;
        int endIdx = json.IndexOfAny(new[] { ',', '}' }, idx);
        if (endIdx < 0) return 0;
        string valStr = json.Substring(idx, endIdx - idx).Trim();
        return int.TryParse(valStr, out int v) ? v : 0;
    }
}
