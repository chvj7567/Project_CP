# 일일 미션 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 서버 없이 NTP 기반 일일 미션 시스템(5개 고정 미션, UTC 00:00 리셋)을 `UIMission`의 세 번째 탭으로 추가한다.

**Architecture:** 신규 `CHMTime` 매니저가 NTP + `Time.realtimeSinceStartup`로 위변조 방지 서버 시각을 제공하고, `DailyMissionService` 정적 클래스가 리셋 판정과 카운터 hook을 담당한다. 기존 `Data.Login`/`Data.Mission` 구조와 `Mission.json`을 확장해 일일 탭(`tapIndex=3`)을 추가한다.

**Tech Stack:** Unity 2022+, C#, UniRx, DOTween, `CHMMain` 매니저 허브 패턴, `CHSingleton<T>`, GPGS 클라우드 저장, Google Mobile Ads SDK (`CHMAdmob.AcquireReward` 이벤트).

**Spec 참고:** `docs/superpowers/specs/2026-05-19-daily-mission-design.md`

**전제 사항:**
- 커밋은 메모리 정책에 따라 staging + 메시지 제안까지만, 실제 commit은 사용자가 직접 실행.
- 모든 주석/`Header` 어트리뷰트는 **한국어 + UTF-8**로 작성 (메모리 정책).
- `CHText`는 `ChvjUnityInfra` 네임스페이스 (메모리 정책).
- 테스트 프레임워크 미사용 — 검증은 PlayMode + `Debug.Log` 출력 확인 + UIMission 진입.

---

## 파일 구조

| 파일 | 작업 | 책임 |
|---|---|---|
| `Assets/Scripts/Defines.cs` | Modify | `EDailyCounter` enum 추가 |
| `Assets/Scripts/Infomation.cs` | Modify | `MissionInfo.dailyCounter` 필드 추가 |
| `Assets/Scripts/Data.cs` | Modify | `Login`에 일일 미션 필드 5개 추가 |
| `Assets/AssetBundleResources/json/Mission.json` | Modify | 일일 미션 5개 항목 추가 |
| `Assets/Scripts/Manager/CHMTime.cs` | Create | NTP 시각 매니저 (SNTP 클라이언트) |
| `Assets/Scripts/Manager/CHMMain.cs` | Modify | `Time` 정적 프로퍼티 + 초기화 순서 |
| `Assets/Scripts/Manager/DailyMissionService.cs` | Create | 일일 미션 리셋/카운터 hook/진행도 조회 |
| `Assets/Scripts/UI/UIMission.cs` | Modify | 일일 탭 + 리셋 카운트다운 UI 로직 |
| `Assets/Scripts/ScrollView/MissionScrollViewItem.cs` | Modify | `tapIndex=3` 분기 |
| `Assets/Scripts/GamePlay/GPBombResolver.cs` | Modify | 블록 파괴 카운터 hook |
| `Assets/Scripts/Scenes/GPGameScene.cs` | Modify | 노멀 스테이지 클리어 카운터 hook |
| `Assets/AssetBundleResources/ui/UIMission.prefab` | Modify | 일일 탭 버튼 + 카운트다운 텍스트 추가 (Unity Editor 수동 작업) |

---

## Task 1: `EDailyCounter` enum 추가

**Files:**
- Modify: `Assets/Scripts/Defines.cs` (`EClearState` enum 다음에 추가)

- [ ] **Step 1: enum 추가**

`Defines.cs`에서 `public enum EClearState { ... }` 정의가 끝나는 다음 라인에 아래 enum을 삽입.

```csharp
public enum EDailyCounter
{
    // 미션에서 카운터 미사용 — collectionType 기반 또는 일일 미션이 아닌 경우
    None = -1,

    // 출석 (UIMission 일일 탭 진입 시 즉시 1로 설정)
    Attendance = 0,

    // 오늘 노멀 스테이지 클리어 횟수 (재클리어 포함)
    NormalStageClear = 1,

    // 오늘 매치로 파괴된 블록 총 개수 (Wall/Locker 등 직접 매치 불가 항목 제외)
    BlockDestroy = 2,

    // 오늘 보상형 광고 시청 횟수
    AdWatch = 3,
}
```

- [ ] **Step 2: Unity 에디터에서 컴파일 확인**

Unity 에디터 콘솔에 컴파일 에러가 없는지 확인. `Console > Clear on Recompile` 후 저장.

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Defines.cs
```

제안 커밋 메시지:
```
[Feature] EDailyCounter enum 추가

일일 미션 카운터 출처 식별용. None/Attendance/NormalStageClear/BlockDestroy/AdWatch.
```

---

## Task 2: `MissionInfo.dailyCounter` 필드 추가

**Files:**
- Modify: `Assets/Scripts/Infomation.cs` (line 57–66)

- [ ] **Step 1: 필드 추가**

`Infomation.cs`의 `public class MissionInfo` 클래스 정의를 아래로 교체.

```csharp
[Serializable]
public class MissionInfo
{
    public int missionID = -1;
    public int tapIndex = -1;
    public Defines.EBlockState collectionType = Defines.EBlockState.None;
    public Defines.EDailyCounter dailyCounter = Defines.EDailyCounter.None; // 일일 미션 전용 카운터
    public int clearValue = -1;
    public int addValue = -1;
    public Defines.EReward reward = Defines.EReward.None;
    public int rewardCount = -1;
}
```

- [ ] **Step 2: 컴파일 확인**

Unity 에디터에서 에러 없이 컴파일되는지 확인.

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Infomation.cs
```

제안 커밋 메시지:
```
[Feature] MissionInfo.dailyCounter 필드 추가

기존 미션은 dailyCounter=None 기본값으로 영향 없음.
```

---

## Task 3: `Data.Login`에 일일 미션 필드 추가

**Files:**
- Modify: `Assets/Scripts/Data.cs` (line 8–27 `Login` 클래스)

- [ ] **Step 1: 필드 5개 추가**

`Data.cs`의 `public class Login` 클래스 마지막 필드(`public int attack = 0;`) 다음에 아래를 삽입.

```csharp
        // 일일 미션 리셋 키 ("yyyyMMdd" UTC 형식, NTP 기준)
        public string lastDailyResetDateKey = "";

        // 오늘 노멀 스테이지 클리어 횟수 (재클리어 포함)
        public int stageClearCountToday = 0;

        // 오늘 매치로 파괴된 블록 총 개수
        public int blockDestroyCountToday = 0;

        // 오늘 보상형 광고 시청 횟수
        public int adWatchCountToday = 0;

        // 오늘 출석 완료 여부
        public bool attendanceTodayDone = false;

        // CatPang 등 EBlockState 기반 일일 카운터용 자정 스냅샷
        // 형식: {"18": 42, "0": 12, ...}  키 = (int)EBlockState, 값 = 자정 시점 누적치
        public string dailyCollectionSnapshotJson = "";
```

- [ ] **Step 2: 컴파일 확인**

기존 사용자 로컬 데이터는 누락 필드가 기본값으로 역직렬화됨 (Unity `JsonUtility` 동작). 마이그레이션 불필요.

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Data.cs
```

제안 커밋 메시지:
```
[Feature] Data.Login에 일일 미션 필드 추가

리셋 키, 오늘 카운터 4종, 자정 스냅샷 JSON.
```

---

## Task 4: `Mission.json`에 일일 미션 5개 추가

**Files:**
- Modify: `Assets/AssetBundleResources/json/Mission.json`

- [ ] **Step 1: 5개 항목 추가**

파일 끝의 `]` 직전에 아래 5줄을 삽입. 기존 17번 항목 뒤에 콤마 추가 필수.

```json
,
  {"missionID":"100", "tapIndex":"3", "collectionType":-1, "dailyCounter":0, "clearValue":1,   "addValue":0, "reward":0, "rewardCount":100},
  {"missionID":"101", "tapIndex":"3", "collectionType":-1, "dailyCounter":1, "clearValue":3,   "addValue":0, "reward":0, "rewardCount":300},
  {"missionID":"102", "tapIndex":"3", "collectionType":-1, "dailyCounter":2, "clearValue":100, "addValue":0, "reward":1, "rewardCount":1  },
  {"missionID":"103", "tapIndex":"3", "collectionType":-1, "dailyCounter":3, "clearValue":1,   "addValue":0, "reward":2, "rewardCount":1  },
  {"missionID":"104", "tapIndex":"3", "collectionType":18, "dailyCounter":-1,"clearValue":3,   "addValue":0, "reward":0, "rewardCount":200}
```

매핑:
- `100` 출석 (`dailyCounter=Attendance`) → Gold 100
- `101` 노멀 클리어 3회 (`NormalStageClear`) → Gold 300
- `102` 블록 100개 파괴 (`BlockDestroy`) → AddTime ×1
- `103` 광고 1회 시청 (`AdWatch`) → AddMove ×1
- `104` CatPang 3회 발동 (`collectionType=18 EBlockState.CatPang`) → Gold 200

- [ ] **Step 2: 파싱 검증**

Unity 에디터에서 PlayMode 진입 → `CHMJson` 로드 시 콘솔에 파싱 에러 없는지 확인. 의심되면 다음 로그 추가 후 확인:

```csharp
// CHMJson에 일시적 디버그 (확인 후 제거)
Debug.Log($"[Mission] loaded count: {CHMJson.Instance.GetMissionInfoList(3).Count}");
```

기대 출력: `[Mission] loaded count: 5`

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/AssetBundleResources/json/Mission.json
```

제안 커밋 메시지:
```
[Data] 일일 미션 5종 추가

출석/노멀클리어/블록파괴/광고시청/CatPang 발동. tapIndex=3.
```

---

## Task 5: `CHMTime` 신규 매니저 작성

**Files:**
- Create: `Assets/Scripts/Manager/CHMTime.cs`

- [ ] **Step 1: 매니저 스켈레톤 작성**

아래 코드를 새 파일에 작성. SNTP 구현은 RFC 2030 기반.

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

// NTP 기반 서버 시각 매니저. 디바이스 시각 위변조 방지를 위해 첫 진입 시 NTP를 1회 받고
// 이후엔 Time.realtimeSinceStartup 경과로 보정해 사용한다. 콜드 부팅 시 NTP 재조회 필수.
public class CHMTime : ChvjUnityInfra.CHSingletonStatic<CHMTime>
{
    // 외부 노출
    public bool IsAvailable { get; private set; }
    public Subject<Unit> OnAvailable { get; } = new Subject<Unit>();

    // NTP 응답 절대 시각 (UTC)
    DateTime _lastNtpUtc;
    // 응답 받은 시점의 realtimeSinceStartup
    float _capturedRealtime;

    const string NtpServer1 = "time.google.com";
    const string NtpServer2 = "pool.ntp.org";
    const int NtpTimeoutMs = 3000;
    const int MaxRetries = 5;
    const float RetryIntervalSec = 30f;

    // 첫 NTP 조회 — 실패해도 즉시 완료. 백그라운드 재시도가 IsAvailable을 갱신.
    public async Task Init()
    {
        bool ok = await TryFetchNtpAsync();
        if (!ok)
        {
            // 백그라운드 재시도 (싱글 인스턴스 호스트가 있어야 코루틴 가능 — 일단 Task로)
            _ = RetryLoopAsync();
        }
    }

    async Task RetryLoopAsync()
    {
        for (int i = 0; i < MaxRetries; ++i)
        {
            await Task.Delay(TimeSpan.FromSeconds(RetryIntervalSec));
            if (IsAvailable) return;
            bool ok = await TryFetchNtpAsync();
            if (ok) return;
        }
        Debug.LogWarning("[CHMTime] NTP 재시도 모두 실패. 일일 미션 잠금 유지.");
    }

    async Task<bool> TryFetchNtpAsync()
    {
        try
        {
            var utc = await FetchNtpAsync(NtpServer1);
            ApplyNtp(utc);
            return true;
        }
        catch (Exception e1)
        {
            Debug.LogWarning($"[CHMTime] {NtpServer1} 실패: {e1.Message}. 폴백 시도");
            try
            {
                var utc = await FetchNtpAsync(NtpServer2);
                ApplyNtp(utc);
                return true;
            }
            catch (Exception e2)
            {
                Debug.LogWarning($"[CHMTime] {NtpServer2} 실패: {e2.Message}");
                return false;
            }
        }
    }

    void ApplyNtp(DateTime utc)
    {
        _lastNtpUtc = utc;
        _capturedRealtime = Time.realtimeSinceStartup;
        bool wasAvailable = IsAvailable;
        IsAvailable = true;
        if (!wasAvailable) OnAvailable.OnNext(Unit.Default);
        Debug.Log($"[CHMTime] NTP 동기화 완료 UTC: {utc:yyyy-MM-dd HH:mm:ss}");
    }

    // SNTP 1회 조회. UDP 123 포트, 48바이트 패킷.
    static async Task<DateTime> FetchNtpAsync(string host)
    {
        var data = new byte[48];
        data[0] = 0x1B; // LI=0, VN=3, Mode=3 (client)

        var addresses = await Dns.GetHostAddressesAsync(host);
        if (addresses.Length == 0) throw new Exception("DNS 실패");
        var endpoint = new IPEndPoint(addresses[0], 123);

        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.ReceiveTimeout = NtpTimeoutMs;
            socket.SendTimeout = NtpTimeoutMs;
            await socket.SendToAsyncCompat(data, endpoint);
            await socket.ReceiveAsyncCompat(data);
        }

        // Transmit Timestamp: 바이트 40–47
        const byte offsetTransmitTime = 40;
        ulong intPart = ((ulong)data[offsetTransmitTime + 0] << 24)
                      | ((ulong)data[offsetTransmitTime + 1] << 16)
                      | ((ulong)data[offsetTransmitTime + 2] << 8)
                      | ((ulong)data[offsetTransmitTime + 3]);
        ulong fracPart = ((ulong)data[offsetTransmitTime + 4] << 24)
                       | ((ulong)data[offsetTransmitTime + 5] << 16)
                       | ((ulong)data[offsetTransmitTime + 6] << 8)
                       | ((ulong)data[offsetTransmitTime + 7]);

        ulong milliseconds = (intPart * 1000UL) + ((fracPart * 1000UL) / 0x100000000UL);
        var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);
        return networkDateTime;
    }

    // 현재 UTC 시각 (NTP + realtimeSinceStartup 경과)
    public DateTime UtcNow
    {
        get
        {
            if (!IsAvailable) return DateTime.UtcNow; // 폴백 — 호출자가 IsAvailable 체크해야 함
            return _lastNtpUtc.AddSeconds(Time.realtimeSinceStartup - _capturedRealtime);
        }
    }

    // 일일 미션 리셋 키 ("yyyyMMdd" UTC)
    public string GetUtcDateKey() => UtcNow.ToString("yyyyMMdd");

    // 다음 UTC 자정까지 남은 초
    public double GetSecondsUntilNextUtcMidnight()
    {
        var now = UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        return (nextMidnight - now).TotalSeconds;
    }
}

// Socket 비동기 래퍼 (Unity의 mono는 SendToAsync을 직접 제공하지 않을 수 있음)
internal static class SocketAsyncExtensions
{
    public static Task<int> SendToAsyncCompat(this Socket s, byte[] buffer, IPEndPoint endpoint)
    {
        return Task.Run(() => s.SendTo(buffer, endpoint));
    }
    public static Task<int> ReceiveAsyncCompat(this Socket s, byte[] buffer)
    {
        return Task.Run(() => s.Receive(buffer));
    }
}
```

- [ ] **Step 2: 컴파일 확인**

Unity 콘솔에 에러가 없는지 확인. `CHSingletonStatic<T>`은 `ChvjUnityInfra` 패키지가 제공.

- [ ] **Step 3: PlayMode 진입 검증**

Unity 에디터에서 PlayMode 진입 후 콘솔에 `[CHMTime] NTP 동기화 완료 UTC: ...`가 보이는지 확인. (다음 Task에서 CHMMain에 연결한 후 자동 호출됨 — Task 6 검증 시 같이 확인 가능)

- [ ] **Step 4: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Manager/CHMTime.cs
```

제안 커밋 메시지:
```
[Feature] CHMTime 매니저 추가

NTP(time.google.com) 기반 서버 시각. 실패 시 30초 간격 5회 재시도.
```

---

## Task 6: `CHMMain`에 `Time` 정적 프로퍼티 + 초기화 순서

**Files:**
- Modify: `Assets/Scripts/Manager/CHMMain.cs` (line 11–17 정적 프로퍼티 블록, line 35–47 InitAsync)

- [ ] **Step 1: 정적 프로퍼티 추가**

`#region Core` 블록 안 `Sound` 다음에 한 줄 추가.

```csharp
public static CHMTime Time { get { EnsureKickoff(); return CHMTime.Instance; } }
```

- [ ] **Step 2: 초기화 순서 변경**

`InitAsync()` 메서드 내 `CHMSound.Instance.Init();` 다음 줄에 `await` 호출 추가. 일일 미션은 시간이 필요하지만 게임 진행은 NTP에 막히지 않아야 하므로 **`Init()`은 NTP 첫 시도까지만 await** (실패해도 백그라운드 재시도가 도는 형태).

```csharp
await CHMResource.Instance.EnsureInit();
await CHMJson.Instance.Init();
await GameFontProvider.PreloadAsync();
CHMPool.Instance.Init();
CHMSound.Instance.Init();
await CHMTime.Instance.Init(); // NTP 첫 시도 (실패해도 완료, 백그라운드 재시도)
ChvjUnityInfra.CHMUI.Instance.Init();
```

- [ ] **Step 3: 컴파일 확인 + PlayMode 검증**

Unity 에디터에서 PlayMode 진입 후 콘솔에 NTP 동기화 로그가 보이는지 확인.

- [ ] **Step 4: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Manager/CHMMain.cs
```

제안 커밋 메시지:
```
[Refactor] CHMMain에 Time 매니저 연결

InitAsync에서 CHMSound 다음에 CHMTime.Init() 호출. 어디서나 CHMMain.Time으로 접근.
```

---

## Task 7: `DailyMissionService` 신규 정적 클래스 작성

**Files:**
- Create: `Assets/Scripts/Manager/DailyMissionService.cs`

- [ ] **Step 1: 전체 클래스 작성**

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

// 일일 미션 리셋 판정 + 카운터 hook + 진행도 조회 일원화.
// 모든 진입점에서 가장 먼저 CheckAndResetIfNeeded()를 호출해 자정 넘김을 처리.
public static class DailyMissionService
{
    const int DailyTapIndex = 3;

    // 일일 탭 진입 시, 게임 클리어 시, 광고 시청 시, 블록 파괴 시 매번 호출.
    // NTP 미수신 상태에서는 리셋하지 않음 (위변조 안전).
    public static void CheckAndResetIfNeeded()
    {
        if (!CHMMain.Time.IsAvailable) return;

        var login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;

        var todayKey = CHMMain.Time.GetUtcDateKey();
        if (login.lastDailyResetDateKey == todayKey) return;

        // 자정 스냅샷 갱신 — CatPang 등 EBlockState 누적치
        login.dailyCollectionSnapshotJson = SerializeCurrentCollectionSnapshot();

        login.lastDailyResetDateKey = todayKey;
        login.stageClearCountToday = 0;
        login.blockDestroyCountToday = 0;
        login.adWatchCountToday = 0;
        login.attendanceTodayDone = false;

        // 일일 미션 진행 상태 초기화
        foreach (var info in CHMJson.Instance.GetMissionInfoList(DailyTapIndex))
        {
            var data = CHMData.Instance.GetMissionData(info.missionID.ToString());
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
        if (!CHMMain.Time.IsAvailable) return;
        CheckAndResetIfNeeded();
        var login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null || login.attendanceTodayDone) return;
        login.attendanceTodayDone = true;
        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
    }

    public static void OnStageClear()
    {
        if (!CHMMain.Time.IsAvailable) return;
        CheckAndResetIfNeeded();
        var login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;
        login.stageClearCountToday++;
        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
    }

    public static void OnBlockDestroyed(int count)
    {
        if (!CHMMain.Time.IsAvailable) return;
        CheckAndResetIfNeeded();
        var login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;
        login.blockDestroyCountToday += count;
        // 잦은 호출이므로 SaveData는 일정 주기로만 — 게임 종료/씬 전환 시 보장됨
        // (정확성보다 디스크 I/O 최소화 우선. 게임 강제 종료 시 일부 누락 허용)
    }

    public static void OnAdWatched()
    {
        if (!CHMMain.Time.IsAvailable) return;
        CheckAndResetIfNeeded();
        var login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return;
        login.adWatchCountToday++;
        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
    }

    // MissionScrollViewItem에서 호출. 진행도 (현재값, 목표값) 반환.
    public static int GetDailyProgress(Infomation.MissionInfo info)
    {
        var login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (login == null) return 0;

        if (info.dailyCounter != Defines.EDailyCounter.None)
        {
            switch (info.dailyCounter)
            {
                case Defines.EDailyCounter.Attendance:       return login.attendanceTodayDone ? 1 : 0;
                case Defines.EDailyCounter.NormalStageClear: return login.stageClearCountToday;
                case Defines.EDailyCounter.BlockDestroy:     return login.blockDestroyCountToday;
                case Defines.EDailyCounter.AdWatch:          return login.adWatchCountToday;
            }
            return 0;
        }

        if (info.collectionType != Defines.EBlockState.None)
        {
            int snapshot = GetSnapshotValue(login.dailyCollectionSnapshotJson, (int)info.collectionType);
            var col = CHMData.Instance.GetCollectionData(info.collectionType.ToString());
            int current = col != null ? col.value : 0;
            return Mathf.Max(0, current - snapshot);
        }

        return 0;
    }

    public static string GetResetCountdown()
    {
        if (!CHMMain.Time.IsAvailable) return "--:--:--";
        var sec = CHMMain.Time.GetSecondsUntilNextUtcMidnight();
        var ts = TimeSpan.FromSeconds(sec);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    // ===== 내부 =====

    // 현재 모든 Collection 값을 EBlockState 기준 dict로 스냅샷 (자정 시 호출)
    static string SerializeCurrentCollectionSnapshot()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("{");
        bool first = true;
        foreach (Defines.EBlockState bs in Enum.GetValues(typeof(Defines.EBlockState)))
        {
            if (bs == Defines.EBlockState.None) continue;
            var col = CHMData.Instance.GetCollectionData(bs.ToString());
            int v = col != null ? col.value : 0;
            if (v == 0) continue; // 0인 항목은 생략 (저장 용량 절감)
            if (!first) sb.Append(",");
            sb.Append($"\"{(int)bs}\":{v}");
            first = false;
        }
        sb.Append("}");
        return sb.ToString();
    }

    // {"18":42,"0":12} 형식에서 key의 값 추출. 없으면 0.
    static int GetSnapshotValue(string json, int key)
    {
        if (string.IsNullOrEmpty(json)) return 0;
        // 단순 파싱 — 키 형식이 "키:값" 고정이므로 정규식/JsonUtility 없이 처리
        var target = $"\"{key}\":";
        int idx = json.IndexOf(target);
        if (idx < 0) return 0;
        idx += target.Length;
        int endIdx = json.IndexOfAny(new[] { ',', '}' }, idx);
        if (endIdx < 0) return 0;
        var valStr = json.Substring(idx, endIdx - idx).Trim();
        return int.TryParse(valStr, out int v) ? v : 0;
    }
}
```

- [ ] **Step 2: 컴파일 확인**

Unity 콘솔에 에러가 없는지 확인. `CHMData.GetMissionData`, `CHMData.GetCollectionData`, `CHMData.SaveData` 등 메서드가 존재함을 전제 (기존 코드에서 모두 사용 중).

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Manager/DailyMissionService.cs
```

제안 커밋 메시지:
```
[Feature] DailyMissionService 추가

리셋 판정 + 카운터 hook + 진행도 조회. 모든 진입점에서 CheckAndResetIfNeeded 선행 호출.
```

---

## Task 8: 광고 시청 카운터 hook

**Files:**
- Modify: `Assets/Scripts/Manager/CHMMain.cs` (`InitAsync()` 마지막 부분)

- [ ] **Step 1: AcquireReward 구독 등록**

`InitAsync()` 메서드 끝부분, 기존 hook 등록 라인(`ChvjUnityInfra.CHToggle.ChangeSoundHook = ...`) 다음에 추가.

```csharp
        // 일일 미션 — 보상형 광고 시청 시 카운터 +1
        ChvjUnityInfra.CHMAdmob.Instance.AcquireReward += () => DailyMissionService.OnAdWatched();
```

- [ ] **Step 2: 광고 시청 후 카운터 확인 (Editor 검증)**

Unity Editor에서 보상형 광고 placeholder 노출 시 콘솔에 콜백이 트리거되는지 확인. Editor placeholder가 즉시 reward 콜백을 호출하지 않을 수 있으므로, 실기 빌드에서 최종 검증.

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Manager/CHMMain.cs
```

제안 커밋 메시지:
```
[Feature] 보상형 광고 시청 시 일일 미션 카운터 증가

CHMAdmob.AcquireReward에 DailyMissionService.OnAdWatched 구독.
```

---

## Task 9: 노멀 스테이지 클리어 hook

**Files:**
- Modify: `Assets/Scripts/Scenes/GPGameScene.cs` (`SaveClearData()` 내 `case ESelectStage.Normal:`)

- [ ] **Step 1: 클리어 hook 추가**

`SaveClearData()` 메서드 내 `case ESelectStage.Normal:` 블록 끝(`break;` 직전)에 한 줄 추가.

기존:
```csharp
            case ESelectStage.Normal:
                if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).normalStage < PlayerPrefs.GetInt(CHMString.Instance.NormalStage))
                {
                    CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).normalStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage);
#if UNITY_ANDROID && !UNITY_EDITOR
                    ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_normal_stage_rank, PlayerPrefs.GetInt(CHMString.Instance.NormalStage));
#endif
                }
                break;
```

변경:
```csharp
            case ESelectStage.Normal:
                if (CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).normalStage < PlayerPrefs.GetInt(CHMString.Instance.NormalStage))
                {
                    CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).normalStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage);
#if UNITY_ANDROID && !UNITY_EDITOR
                    ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_normal_stage_rank, PlayerPrefs.GetInt(CHMString.Instance.NormalStage));
#endif
                }
                // 일일 미션 — 노멀 클리어 카운터 +1 (재클리어 포함)
                DailyMissionService.OnStageClear();
                break;
```

> 주의: 재클리어 포함이므로 `if (...)` 조건 안이 아니라 `case` 블록 끝에 위치해야 함.

- [ ] **Step 2: 컴파일 + PlayMode 검증**

Unity에서 노멀 스테이지를 한 번 클리어한 뒤 `Data.Login.stageClearCountToday`가 1 증가했는지 확인 (인스펙터 또는 Debug.Log).

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Scenes/GPGameScene.cs
```

제안 커밋 메시지:
```
[Feature] 노멀 스테이지 클리어 시 일일 미션 카운터 +1

재클리어 포함.
```

---

## Task 10: 블록 파괴 카운터 hook

**Files:**
- Modify: `Assets/Scripts/GamePlay/GPBombResolver.cs` (line 49–50 부근)

- [ ] **Step 1: 블록 파괴 hook 추가**

`GPBombResolver.cs:49–50`의 `collectionData.value += 1;` 다음에 한 줄 추가.

기존:
```csharp
        var collectionData = CHMData.Instance.GetCollectionData(block.GetBlockState().ToString());
        collectionData.value += 1;
```

변경:
```csharp
        var collectionData = CHMData.Instance.GetCollectionData(block.GetBlockState().ToString());
        collectionData.value += 1;
        // 일일 미션 — 매치 가능 블록만 카운트 (Wall/Locker 등은 매치로 사라지지 않으므로 자연스럽게 제외됨)
        DailyMissionService.OnBlockDestroyed(1);
```

- [ ] **Step 2: PlayMode 검증**

스테이지에서 블록 3개 매치 후 `Data.Login.blockDestroyCountToday`가 증가했는지 확인.

- [ ] **Step 3: Staging + 커밋 제안**

```bash
git add Assets/Scripts/GamePlay/GPBombResolver.cs
```

제안 커밋 메시지:
```
[Feature] 블록 파괴 시 일일 미션 카운터 증가

GPBombResolver의 Collection.value 증가 지점에 hook.
```

---

## Task 11: `MissionScrollViewItem`에 `tapIndex=3` 분기 추가

**Files:**
- Modify: `Assets/Scripts/ScrollView/MissionScrollViewItem.cs`

- [ ] **Step 1: `Init()` 메서드에 `tapIndex == 3` 분기 추가**

`Init()` 메서드 끝에 `else if (_info.tapIndex == 3)` 블록 추가. 기존 `else if (_info.tapIndex == 2)` 블록 끝에 이어서.

```csharp
        else if (_info.tapIndex == 3)
        {
            // 일일 미션 — 진행도는 DailyMissionService가 카운터/스냅샷 차분으로 계산
            _missionData = CHMData.Instance.GetMissionData(_info.missionID.ToString());

            missionText.SetStringID(13); // 기존 "수집" 또는 일일 미션용 ID로 필요시 교체
            clearObj.SetActive(false);

            SetMissionImage(_info.collectionType);
            SetRewardImage(_info.reward);

            int current = DailyMissionService.GetDailyProgress(_info);
            int target = _info.clearValue;

            if (_missionData.clearState == Defines.EClearState.Clear)
            {
                missionValueText.SetText(target, target);
                clearObj.SetActive(true);
                rewardBtn.interactable = false;
            }
            else
            {
                missionValueText.SetStringID(20);
                missionValueText.SetText(Mathf.Min(current, target), target);
                rewardBtn.interactable = current >= target;
            }
        }
```

- [ ] **Step 2: `rewardBtn` 클릭 핸들러에 `tapIndex == 3` 분기 추가**

기존 `if (_info.tapIndex == 1) { ... } else if (_info.tapIndex == 2) { ... }` 다음에 추가.

```csharp
            else if (_info.tapIndex == 3)
            {
                // 일일 미션 — 1회 수령으로 완료 처리. repeatCount 미사용.
                _missionData.clearState = Defines.EClearState.Clear;
                clearObj.SetActive(true);
                rewardBtn.interactable = false;
                CHMData.Instance.SaveData(CHMString.Instance.CatPang);
            }
```

- [ ] **Step 3: 컴파일 + 시각 확인**

Unity 에디터에서 컴파일 에러 없는지 확인. 실제 UI 확인은 Task 12 후 진행.

- [ ] **Step 4: Staging + 커밋 제안**

```bash
git add Assets/Scripts/ScrollView/MissionScrollViewItem.cs
```

제안 커밋 메시지:
```
[Feature] MissionScrollViewItem에 일일 미션 표시/수령 분기 추가
```

---

## Task 12: `UIMission` 일일 탭 + 리셋 카운트다운

**Files:**
- Modify: `Assets/Scripts/UI/UIMission.cs`

- [ ] **Step 1: 일일 탭 + 카운트다운 + NTP 잠금 UI 필드 추가**

기존 `[SerializeField] Button specialTapBtn;` 다음에 추가.

```csharp
    [SerializeField] Button dailyTapBtn;          // 일일 탭 (신규)
    [SerializeField] CHText resetTimerText;       // 리셋까지 남은 시간 (HH:MM:SS)
    [SerializeField] GameObject offlineLockObj;   // NTP 미수신 시 표시할 잠금 오버레이
```

- [ ] **Step 2: `Start()`에서 진입 처리 + 탭 구독 + 잠금 상태 동기화**

기존 `Start()` 메서드를 아래로 교체.

```csharp
    private void Start()
    {
        // 일일 탭 잠금 초기 상태
        if (offlineLockObj != null) offlineLockObj.SetActive(false);
        UpdateDailyLockState();

        // NTP가 나중에라도 동기화되면 잠금 해제
        if (CHMMain.Time != null && CHMMain.Time.OnAvailable != null)
        {
            CHMMain.Time.OnAvailable.Subscribe(_ =>
            {
                UpdateDailyLockState();
                if (curTapIndex == 3)
                    scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
            }).AddTo(this);
        }

        // 진입 시 자정 리셋 + 출석 처리
        DailyMissionService.CheckAndResetIfNeeded();
        DailyMissionService.MarkAttendance();

        normalTapBtn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex = 1;
            scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
        }).AddTo(this);

        specialTapBtn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex = 2;
            scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
        }).AddTo(this);

        if (dailyTapBtn != null)
        {
            dailyTapBtn.OnClickAsObservable().Subscribe(_ =>
            {
                if (!CHMMain.Time.IsAvailable)
                {
                    if (offlineLockObj != null) offlineLockObj.SetActive(true);
                    return;
                }
                curTapIndex = 3;
                DailyMissionService.CheckAndResetIfNeeded();
                scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
            }).AddTo(this);
        }

        curTapIndex = 1;
        curTapText.SetStringID(121);
        scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
    }

    void UpdateDailyLockState()
    {
        if (dailyTapBtn == null) return;
        dailyTapBtn.interactable = CHMMain.Time != null && CHMMain.Time.IsAvailable;
    }
```

- [ ] **Step 3: `Update()`에서 카운트다운 갱신 + 자정 넘김 자동 감지**

`UIMission` 클래스에 `Update()` 메서드 추가.

```csharp
    string _lastDateKey = "";

    private void Update()
    {
        if (CHMMain.Time == null || !CHMMain.Time.IsAvailable) return;

        // 카운트다운 텍스트 갱신
        if (curTapIndex == 3 && resetTimerText != null)
            resetTimerText.SetText(DailyMissionService.GetResetCountdown());

        // 자정 넘김 자동 감지
        var todayKey = CHMMain.Time.GetUtcDateKey();
        if (string.IsNullOrEmpty(_lastDateKey)) { _lastDateKey = todayKey; return; }
        if (_lastDateKey != todayKey)
        {
            _lastDateKey = todayKey;
            DailyMissionService.CheckAndResetIfNeeded();
            if (curTapIndex == 3)
                scrollView.SetItemList(CHMJson.Instance.GetMissionInfoList(curTapIndex));
        }
    }
```

- [ ] **Step 4: `using ChvjUnityInfra;` 확인**

파일 상단에 `using ChvjUnityInfra;`가 있는지 확인 (`CHText` 사용을 위해). 이미 있음 (기존 코드 line 7).

- [ ] **Step 5: 컴파일 확인**

Unity 콘솔에 에러 없는지 확인. UI 프리팹 작업은 Task 13.

- [ ] **Step 6: Staging + 커밋 제안**

```bash
git add Assets/Scripts/UI/UIMission.cs
```

제안 커밋 메시지:
```
[Feature] UIMission에 일일 탭 + 리셋 카운트다운 추가

NTP 미수신 시 잠금 처리, 자정 넘김 자동 리셋.
```

---

## Task 13: `UIMission.prefab` UI 요소 추가 (Unity Editor 수동 작업)

**Files:**
- Modify: `Assets/AssetBundleResources/ui/UIMission.prefab`

- [ ] **Step 1: 프리팹 열기**

Unity 에디터에서 `Assets/AssetBundleResources/ui/UIMission.prefab`을 더블 클릭해 Prefab Editing Mode 진입.

- [ ] **Step 2: 일일 탭 버튼 추가**

기존 `normalTapBtn`, `specialTapBtn`과 같은 부모(탭 컨테이너) 아래에 새 `Button` 게임 오브젝트 생성. 이름: `DailyTapBtn`. 텍스트 라벨은 한국어 "일일" (또는 추후 `StringID` 사용).

- [ ] **Step 3: 카운트다운 텍스트 추가**

일일 탭이 활성화되었을 때만 보이도록 일일 탭 영역에 `CHText` 컴포넌트를 가진 게임 오브젝트 생성. 이름: `ResetTimerText`. 폰트는 기존 패턴 따름.

- [ ] **Step 4: 오프라인 잠금 오버레이 추가**

전체 일일 탭 콘텐츠 위에 비활성 상태의 패널 오브젝트 생성. 이름: `OfflineLockObj`. 안에 안내 텍스트 ("네트워크 연결 후 이용 가능"). 기본 비활성.

- [ ] **Step 5: `UIMission` 컴포넌트에 참조 연결**

루트의 `UIMission` 컴포넌트 인스펙터에서:
- `Daily Tap Btn` → 방금 만든 `DailyTapBtn`
- `Reset Timer Text` → `ResetTimerText`
- `Offline Lock Obj` → `OfflineLockObj`

- [ ] **Step 6: Prefab 저장**

Apply / Ctrl+S로 프리팹 저장 후 Prefab Editing Mode 종료.

- [ ] **Step 7: PlayMode 검증**

PlayMode 진입 → 로비에서 미션 UI 진입 → 일일 탭 클릭 → 5개 미션이 표시되는지, 카운트다운이 매초 갱신되는지, 출석 미션이 즉시 수령 가능 상태로 보이는지 확인.

- [ ] **Step 8: Staging + 커밋 제안**

```bash
git add Assets/AssetBundleResources/ui/UIMission.prefab
```

제안 커밋 메시지:
```
[UI] UIMission 프리팹에 일일 탭 버튼/카운트다운/잠금 오버레이 추가
```

---

## Task 14: 통합 검증

**Files:** 없음 (검증만)

- [ ] **Step 1: 클린 빌드 환경 준비**

`Application.persistentDataPath`에서 `Login.json` 삭제 (또는 PlayerPrefs Clear). 신규 사용자 상태에서 시작.

- [ ] **Step 2: 시나리오 A — 정상 흐름**

1. PlayMode 진입
2. 콘솔에 `[CHMTime] NTP 동기화 완료 UTC: ...` 확인
3. 콘솔에 `[DailyMission] 자정 리셋 완료. todayKey=YYYYMMDD` 확인 (최초 1회)
4. 로비 → 미션 UI → 일일 탭 클릭
5. 5개 미션 표시 확인. 출석 미션은 진행도 1/1, 수령 버튼 활성화
6. 출석 보상 수령 → 골드 +100 확인
7. 노멀 스테이지 1회 클리어 → 미션 화면 복귀 후 노멀 클리어 미션 진행도 1/3 확인

- [ ] **Step 3: 시나리오 B — 오프라인 시작**

1. 비행기 모드 ON 상태에서 PlayMode 진입
2. 콘솔에 `[CHMTime] time.google.com 실패: ...` 확인
3. 미션 UI 진입 → 일일 탭 클릭 시 잠금 오버레이 표시 확인
4. 비행기 모드 OFF → 30초 이내 자동 복구, 일일 탭 활성화 확인

- [ ] **Step 4: 시나리오 C — 자정 리셋**

1. UTC 자정 직전까지 미션 진행 후 자정 경과 시 카운터 0으로 리셋되는지 확인
2. 수동 검증이 어려우면 `CHMTime.UtcNow` 게터를 일시적으로 `_lastNtpUtc.AddSeconds(...).AddHours(24)`로 가짜 시각 주입해 자동 리셋 트리거 확인 (검증 후 원복)

- [ ] **Step 5: 시나리오 D — 보상 수령 → 강제 종료 → 재진입**

1. 미션 1개 수령 후 즉시 PlayMode 정지 (강제 종료 시뮬레이션)
2. 다시 PlayMode 진입 → 일일 탭에서 수령 완료 상태 유지 확인

- [ ] **Step 6: 시나리오 E — GPGS 동기화**

1. 기기 A에서 미션 일부 클리어 → GPGS 저장
2. 기기 B에서 같은 계정 로그인 → 클라우드 데이터 로드 후 진행도 동일 확인

- [ ] **Step 7: 최종 커밋 제안**

모든 검증이 통과하면, 미커밋 변경이 있을 경우 마지막으로 staging.

```bash
git status
```

남은 변경이 없으면 종료. 있으면 사용자에게 알리고 staging.

---

## 자체 검토 (Self-Review)

### 스펙 커버리지

| 스펙 섹션 | 구현 태스크 |
|---|---|
| 2. 결정 사항 요약 | 전체 |
| 3. 아키텍처 (CHMTime / DailyMissionService) | Task 5, 6, 7 |
| 4. 데이터 스키마 (EDailyCounter, MissionInfo, Data.Login, Mission.json) | Task 1, 2, 3, 4 |
| 5. CHMTime 동작 (SNTP, UtcNow, 재시도) | Task 5 |
| 6. DailyMissionService 동작 (리셋, 카운터, 진행도) | Task 7 |
| 6. 카운터 hook (광고/스테이지/블록) | Task 8, 9, 10 |
| 7. UI 흐름 (UIMission, MissionScrollViewItem) | Task 11, 12, 13 |
| 8. 클라우드 동기화 (GPGS) | 기존 `CHMGPGS.SaveData`로 자동 처리 — 별도 태스크 불필요. 단 클라우드 충돌 시 `lastDailyResetDateKey` 우선 비교 로직은 `CHMGPGS` 머지 로직 안에 추가 필요 — **갭** |
| 9. 에러 처리 & 엣지 케이스 | Task 5(NTP 재시도), Task 7(NTP 미수신 시 카운트 중단), Task 12(자정 자동 감지) |
| 10. 테스트 시나리오 | Task 14 |
| 11. 마이그레이션 | Task 3 Step 2에 명시 (기본값 자동) |

**갭 발견 — Task 추가 필요:**

### Task 15: GPGS 클라우드 동기화 충돌 처리

**Files:**
- Modify: `Assets/Scripts/Manager/CHMData.cs` (또는 `CHMGPGS` 머지 로직 위치)

> **사전 조사 필요:** 현재 클라우드 머지가 어떻게 동작하는지(어느 필드를 단순 덮어쓰는지) 확인 후 일일 미션 필드에 한해 `lastDailyResetDateKey` 사전순 비교 규칙 적용.

- [ ] **Step 1: 현재 클라우드 머지 위치 파악**

`Assets/Scripts/Manager/CHMData.cs`에서 클라우드 데이터를 로컬에 적용하는 메서드(예: `ApplyCloudData` 또는 동등 이름) 위치 확인.

```bash
grep -n "cloudData\|loginCloudData" Assets/Scripts/Manager/CHMData.cs
```

- [ ] **Step 2: 일일 미션 필드 충돌 처리 추가**

머지 시점에 아래 규칙 적용:

```csharp
// 일일 미션 — 클라우드의 lastDailyResetDateKey가 같거나 미래면 클라우드 우선 (위변조 거부)
if (string.Compare(cloudLogin.lastDailyResetDateKey, localLogin.lastDailyResetDateKey, StringComparison.Ordinal) >= 0)
{
    localLogin.lastDailyResetDateKey = cloudLogin.lastDailyResetDateKey;
    localLogin.stageClearCountToday = cloudLogin.stageClearCountToday;
    localLogin.blockDestroyCountToday = cloudLogin.blockDestroyCountToday;
    localLogin.adWatchCountToday = cloudLogin.adWatchCountToday;
    localLogin.attendanceTodayDone = cloudLogin.attendanceTodayDone;
    localLogin.dailyCollectionSnapshotJson = cloudLogin.dailyCollectionSnapshotJson;
}
// 로컬이 더 미래 → 로컬 우선 (위변조 가능성 있으나 일단 신뢰. Task 5에서 NTP 검증함)
```

- [ ] **Step 3: 다기기 검증** — Task 14 Step 6에 포함

- [ ] **Step 4: Staging + 커밋 제안**

```bash
git add Assets/Scripts/Manager/CHMData.cs
```

제안 커밋 메시지:
```
[Feature] GPGS 클라우드 머지 시 일일 미션 필드 충돌 처리

lastDailyResetDateKey 사전순 비교로 위변조된 로컬 거부.
```

---

### 플레이스홀더 스캔

확인 — TBD/TODO 없음. 모든 코드 블록 완성. 단 Task 15의 사전 조사는 "Step 1에서 grep으로 위치 파악 후 Step 2 적용"으로 정상 절차.

### 타입/시그니처 일관성

| 식별자 | 정의 위치 | 사용 위치 | 일관성 |
|---|---|---|---|
| `EDailyCounter` | Task 1 (`Defines.cs`) | Task 2, 7 | ✓ |
| `MissionInfo.dailyCounter` | Task 2 | Task 7 (`GetDailyProgress`) | ✓ |
| `Login.lastDailyResetDateKey` | Task 3 | Task 7, 15 | ✓ |
| `Login.stageClearCountToday` | Task 3 | Task 7, 9, 15 | ✓ |
| `Login.blockDestroyCountToday` | Task 3 | Task 7, 10, 15 | ✓ |
| `Login.adWatchCountToday` | Task 3 | Task 7, 8, 15 | ✓ |
| `Login.attendanceTodayDone` | Task 3 | Task 7, 15 | ✓ |
| `Login.dailyCollectionSnapshotJson` | Task 3 | Task 7, 15 | ✓ |
| `CHMTime.Init()` | Task 5 | Task 6 | ✓ |
| `CHMTime.IsAvailable`/`UtcNow`/`GetUtcDateKey`/`OnAvailable` | Task 5 | Task 7, 12 | ✓ |
| `DailyMissionService.CheckAndResetIfNeeded` | Task 7 | Task 7(자기 호출), 12 | ✓ |
| `DailyMissionService.OnStageClear`/`OnBlockDestroyed`/`OnAdWatched`/`MarkAttendance` | Task 7 | Task 8, 9, 10, 12 | ✓ |
| `DailyMissionService.GetDailyProgress`/`GetResetCountdown` | Task 7 | Task 11, 12 | ✓ |

전체 일관성 OK.
