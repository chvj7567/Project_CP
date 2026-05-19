# 일일 미션 시스템 설계 (Daily Mission)

- **작성일:** 2026-05-19
- **대상 브랜치:** Package
- **관련 시스템:** `CHMMain`, `CHMData`, `CHMJson`, `CHMAdmob`, `CHMGPGS`, `UIMission`, `MissionScrollViewItem`, `GPGameScene`, `GPBoard`

## 1. 목적과 범위

서버 인프라 없이 클라이언트 단에서 동작하는 일일 미션 시스템을 추가한다. 기존 `UIMission`에 세 번째 탭("일일")으로 통합하며, 매일 동일한 5개의 고정 미션을 출제한다. 리셋 기준 시각은 NTP 서버 기준 UTC 00:00 (한국시간 09:00)이며, 디바이스 시각 위변조에 대해 강한 방어를 제공한다.

### 범위에 포함되는 것

- `CHMTime` 신규 매니저 (NTP 시간 조회 + 캐시 기반 시간 계산)
- `Data.Login`에 일일 카운터 필드 추가
- `Infomation.MissionInfo`에 `dailyCounter` 필드 + `EDailyCounter` enum 신규
- `DailyMissionService` 정적 클래스 (리셋 판정, 카운터 hook, 진행도 조회)
- `UIMission`에 일일 탭 + 리셋 카운트다운 UI
- `MissionScrollViewItem`에 `tapIndex=3` 분기
- `Mission.json`에 5개 미션 추가
- 기존 게임 코드(`GPGameScene`, `GPBoard`, `CHMAdmob`)에 카운터 hook 삽입

### 범위에서 제외되는 것

- 미션 풀 랜덤 선택, 요일별 세트 — 향후 확장
- 연속 출석(streak) 보너스, 가챠 티켓 등 신규 보상 — 향후 확장
- GPGS 미연동 사용자에 대한 별도 강화 보호
- 글로벌 출시 대응 (타임존별 리셋) — UTC 통일 운영

## 2. 결정 사항 요약

| 항목 | 결정 |
|---|---|
| UI 진입점 | `UIMission`에 `tapIndex=3` (일일) 탭 추가 |
| 콘텐츠 종류 | 플레이 행위 + 출석 보상 혼합 |
| 미션 개수/구성 | 매일 동일한 5개 고정 |
| 보상 종류 | 기존 `EReward` 3종 (Gold, AddTime, AddMove) |
| 리셋 시각 | NTP 기준 UTC 00:00 (KST 09:00) |
| 시간 출처 | NTP (첫 진입 캐싱 + `Time.realtimeSinceStartup` 보정) |
| NTP 실패 폴백 | 일일 탭 잠금 + 백그라운드 재시도 (다른 탭은 정상) |
| 보상 수령 방식 | 기존 패턴 (임계값 달성 시 수령 버튼 활성화 → 클릭 시 지급) |
| 시간 매니저 위치 | 신규 `CHMTime` 매니저 (`CHMMain.Time`으로 접근) |
| 카운터 출처 식별 | `MissionInfo`에 `EDailyCounter dailyCounter` 필드 추가 |

## 3. 아키텍처

```
┌─────────────────────────────────────────────────────┐
│                  CHMMain (싱글톤)                   │
│  Update() → CHMUI.UpdateUI()                        │
└─────────────────────────────────────────────────────┘
   ├─ CHMTime  [신규]
   │   ├─ TaskCompletionSource Init (NTP 비동기 조회)
   │   ├─ UtcNow { get; }
   │   ├─ IsAvailable { get; }
   │   ├─ GetUtcDateKey() → "yyyyMMdd"
   │   └─ GetSecondsUntilNextUtcMidnight()
   │
   ├─ CHMData
   │   └─ Data.Login에 일일 카운터/리셋키 필드 추가
   │
   ├─ CHMAdmob
   │   └─ 광고 시청 완료 콜백 → DailyMissionService.OnAdWatched()
   │
   └─ DailyMissionService [신규 정적 클래스]
       ├─ CheckAndResetIfNeeded()
       ├─ MarkAttendance(), OnStageClear(), OnBlockDestroyed(), OnAdWatched()
       ├─ GetDailyProgress(MissionInfo)
       └─ GetResetCountdown()
```

### 초기화 순서

기존: `CHMJson.Init()` → `CHMPool.Init()` → `CHMSound.Init()`

변경: `CHMJson.Init()` → `CHMTime.Init()` → `CHMPool.Init()` → `CHMSound.Init()`

`CHMTime.Init()`은 다른 매니저처럼 `TaskCompletionSource`를 사용해 비동기 완료 통지. NTP 실패 시에도 `Init` 자체는 즉시 완료 처리하되 `IsAvailable=false`로 유지하고 백그라운드 재시도 코루틴을 띄운다 (30초 간격, 최대 5회). 게임 진행 전체가 NTP에 막히지 않게 한다.

## 4. 데이터 스키마

### Defines.cs

```csharp
public enum EDailyCounter
{
    None = -1,
    Attendance,        // 출석 (진입 즉시)
    NormalStageClear,  // 오늘 노멀 클리어 수
    BlockDestroy,      // 오늘 블록 파괴 총합
    AdWatch,           // 오늘 광고 시청 수
}
```

`tapIndex=3` 미션은 `dailyCounter`가 `None`이 아니거나, `collectionType`이 `None`이 아니어야 한다 (둘 중 하나로 진행도 출처 지정). CatPang(M5)는 `dailyCounter=None` + `collectionType=CatPang` 조합이며, 카운터는 일일 별도 필드가 아니라 "오늘 자정 이후 누적치"를 `Collection` 스냅샷과 차분으로 계산한다 (자세한 사항은 6장 참고).

### Infomation.MissionInfo 확장

```csharp
[Serializable]
public class MissionInfo
{
    public int missionID = -1;
    public int tapIndex = -1;
    public Defines.EBlockState collectionType = Defines.EBlockState.None;
    public Defines.EDailyCounter dailyCounter = Defines.EDailyCounter.None;  // 신규
    public int clearValue = -1;
    public int addValue = -1;
    public Defines.EReward reward = Defines.EReward.None;
    public int rewardCount = -1;
}
```

### Data.Login 확장

```csharp
[Serializable]
public class Login
{
    // ... 기존 필드 ...

    // 일일 미션
    public string lastDailyResetDateKey = "";   // "yyyyMMdd" UTC
    public int stageClearCountToday = 0;
    public int blockDestroyCountToday = 0;
    public int adWatchCountToday = 0;
    public bool attendanceTodayDone = false;

    // CatPang 등 EBlockState 기반 일일 카운터용 스냅샷
    // JSON 형식: {"18": 42, "0": 12, ...}  키 = (int)EBlockState, 값 = 자정 시점 누적치
    public string dailyCollectionSnapshotJson = "";
}
```

`Data.Mission`(기존)은 그대로 사용. 일일 미션 항목도 `key = missionID.ToString()`으로 동일 구조에 저장. 단 `repeatCount`는 일일 미션에서는 사용하지 않는다 (하루 1회 클리어).

### Mission.json 추가 항목

```json
{ "missionID":"100", "tapIndex":"3", "collectionType":-1, "dailyCounter":0, "clearValue":1,   "addValue":0, "reward":0, "rewardCount":100 },
{ "missionID":"101", "tapIndex":"3", "collectionType":-1, "dailyCounter":1, "clearValue":3,   "addValue":0, "reward":0, "rewardCount":300 },
{ "missionID":"102", "tapIndex":"3", "collectionType":-1, "dailyCounter":2, "clearValue":100, "addValue":0, "reward":1, "rewardCount":1   },
{ "missionID":"103", "tapIndex":"3", "collectionType":-1, "dailyCounter":3, "clearValue":1,   "addValue":0, "reward":2, "rewardCount":1   },
{ "missionID":"104", "tapIndex":"3", "collectionType":18, "dailyCounter":-1,"clearValue":3,   "addValue":0, "reward":0, "rewardCount":200 }
```

- `100` 출석 → Gold 100
- `101` 노멀 클리어 3회 → Gold 300
- `102` 블록 파괴 100개 → AddTime ×1
- `103` 광고 시청 1회 → AddMove ×1
- `104` CatPang 폭탄 3회 → Gold 200

### CHMTime PlayerPrefs 캐시

| 키 | 타입 | 의미 |
|---|---|---|
| `CHMTime.LastNtpUtcTicks` | long | 마지막 NTP 응답의 UTC ticks |
| `CHMTime.LastNtpDeviceUtcTicks` | long | 응답 받은 시점 디바이스 UTC ticks (위변조 검증용) |

런타임 메모리:
- `_lastNtpUtc` (DateTime)
- `_capturedRealtime` (float, `Time.realtimeSinceStartup` 스냅샷)

## 5. CHMTime 동작

### NTP 조회

- **프로토콜:** SNTP (RFC 5905 simple subset). UDP 123포트로 48바이트 패킷 송수신.
- **서버:** `time.google.com` 1순위, `pool.ntp.org` 폴백.
- **타임아웃:** 3초.
- **외부 패키지 사용하지 않음** (직접 구현 60줄 내외).

성공 시:
```
_lastNtpUtc = ntpResponse;
_capturedRealtime = Time.realtimeSinceStartup;
PlayerPrefs에 LastNtpUtcTicks, LastNtpDeviceUtcTicks 저장.
IsAvailable = true;
```

### UtcNow 계산

```
UtcNow = _lastNtpUtc + TimeSpan.FromSeconds(Time.realtimeSinceStartup - _capturedRealtime)
```

### 재부팅 후 재조회

콜드 부팅 시 `Time.realtimeSinceStartup` 기준점이 사라지므로 NTP 재조회 필수. `PlayerPrefs`의 `LastNtpUtcTicks`는 검증용으로만 사용한다 (위변조 탐지용 — 자세한 사항은 9장).

### 백그라운드 재시도

NTP 실패 시 30초 간격으로 최대 5회 재시도. 성공 시 즉시 `IsAvailable=true`로 전환하고 `UIMission`이 열려 있으면 일일 탭을 활성화한다. 게임 어디서든 `CHMMain.Time.OnAvailable` `Subject<Unit>` 옵저버블을 구독해 알림 받을 수 있다.

## 6. DailyMissionService 동작

### CheckAndResetIfNeeded

```
if (!CHMMain.Time.IsAvailable) return;
todayKey = CHMMain.Time.GetUtcDateKey();
if (login.lastDailyResetDateKey == todayKey) return;

login.lastDailyResetDateKey = todayKey;
login.stageClearCountToday = 0;
login.blockDestroyCountToday = 0;
login.adWatchCountToday = 0;
login.attendanceTodayDone = false;
login.dailyCollectionSnapshotJson = SerializeCurrentCollectionSnapshot();

foreach (info in tapIndex=3 미션 목록)
{
    data = CHMData.GetMissionData(info.missionID.ToString());
    data.clearState = NotDoing;
    data.startValue = 0;
    data.repeatCount = 0;
}

CHMData.SaveData(CatPang);
```

### GetDailyProgress(MissionInfo info)

```
if (info.dailyCounter != None)
    switch (info.dailyCounter)
    {
        case Attendance:       return login.attendanceTodayDone ? 1 : 0;
        case NormalStageClear: return login.stageClearCountToday;
        case BlockDestroy:     return login.blockDestroyCountToday;
        case AdWatch:          return login.adWatchCountToday;
    }
else if (info.collectionType != None)
{
    // 자정 스냅샷과의 차분
    snapshot = login.dailyCollectionSnapshotJson에서 collectionType의 값 (없으면 0);
    current = CHMData.GetCollectionData(collectionType.ToString()).value;
    return current - snapshot;
}
```

### 카운터 hook 위치

| 카운터 | hook 위치 | 호출 |
|---|---|---|
| `attendanceTodayDone` | `UIMission.Start()` | `DailyMissionService.MarkAttendance()` |
| `stageClearCountToday` | `GPGameScene` 게임 클리어 처리부 (노멀 스테이지일 때만, **재클리어 포함**) | `DailyMissionService.OnStageClear()` |
| `blockDestroyCountToday` | 기존 블록 파괴 카운트 증가 지점 (`Collection.value++` 또는 동등 위치). **모든 EBlockState 합산** (Wall, Locker 등 일반 매치로 사라지지 않는 항목 제외) | `DailyMissionService.OnBlockDestroyed(int count)` |
| `adWatchCountToday` | `CHMAdmob` 보상형/전면 광고 시청 완료 콜백 | `DailyMissionService.OnAdWatched()` |

모든 hook은 카운터 +1 후 `CheckAndResetIfNeeded()`를 먼저 호출해서 자정 넘김을 처리한 다음 증가 → `CHMData.SaveData(CatPang)` 호출. NTP 미수신 상태에서는 카운터를 증가시키지 않는다 (정확성 우선).

### 보상 수령 시점

`MissionScrollViewItem` 보상 버튼 클릭 시 (`tapIndex=3` 분기):
1. 보상 지급 (기존 분기 로직 재사용)
2. `_missionData.clearState = Clear`
3. `CHMData.SaveData(CatPang)`

`repeatCount`는 사용하지 않으며, 하루 1회만 수령 가능.

## 7. UI 흐름

### UIMission

- 신규: `dailyTapBtn` (`Inspector`에서 할당), `resetTimerText`
- `Start()` 진입 시:
  1. `DailyMissionService.CheckAndResetIfNeeded()`
  2. `DailyMissionService.MarkAttendance()`
- `Update()`:
  - `curTapIndex == 3 && CHMMain.Time.IsAvailable`이면 `resetTimerText`에 카운트다운 표시
  - 매 프레임 `GetUtcDateKey()` 비교, 변경 감지 시 자동 리셋 + 현재 탭 새로고침
- NTP 미수신 상태에서 일일 탭 클릭 시: 잠금 UI 표시 (예: "네트워크 연결 후 이용 가능").

### MissionScrollViewItem

`Init()`에 `tapIndex == 3` 분기 추가. UI 표시 규칙:
- 진행도 텍스트: `{현재값} / {clearValue}`
- 보상 가능 상태(현재값 >= clearValue, clearState != Clear): 버튼 활성화
- 수령 완료(clearState == Clear): `clearObj.SetActive(true)`, 버튼 비활성화
- 보상 클릭 시 기존 `switch (info.reward)` 분기 그대로 사용

## 8. 클라우드 동기화 (GPGS)

기존 GPGS 저장 슬롯 `"CatPang"`에 `Data.Login`이 직렬화되어 저장되므로 신규 필드는 자동 포함된다. 충돌 처리는 `CHMGPGS` 기존 로직을 그대로 따르되, 일일 미션 한정으로 다음 규칙을 추가한다:

- 클라우드의 `lastDailyResetDateKey`가 로컬보다 같거나 미래(문자열 사전순 비교 가능, "yyyyMMdd" 포맷이므로) → 클라우드 우선 (위변조된 로컬 거부)
- 클라우드가 과거 → 로컬 우선

GPGS 미연동 사용자(`Login.connectGPGS == false`)는 로컬만 사용. 위변조에 더 약해지지만 첫 출시 범위에선 수용한다.

## 9. 에러 처리 & 엣지 케이스

| 케이스 | 처리 |
|---|---|
| NTP 최초 실패 (앱 시작 시 오프라인) | `IsAvailable=false`. 일일 탭 잠금 + 안내. 다른 탭은 정상. 30초 간격 5회 재시도. |
| NTP 성공 후 오프라인 전환 | 캐시된 NTP + `realtimeSinceStartup` 계속 사용. 영향 없음. |
| 앱 콜드 부팅 | `realtimeSinceStartup` 기준점 소실 → NTP 재조회 필수. |
| 디바이스 시각 위변조 후 재부팅 | NTP 응답과 디바이스 UTC의 차이가 24시간 이상이면 정상이라도 한 번 더 검증. 그래도 차이가 크면 디바이스 시각은 단순히 캐시 식별용으로만 사용하고 NTP 값만 신뢰. |
| 자정 직전 진입 → 자정 넘김 | `UIMission.Update()`에서 `GetUtcDateKey()` 비교로 자동 감지 및 리셋. 미수령 보상은 잃는다. |
| GPGS 클라우드 충돌 | 8장 규칙 적용. |
| GPGS 미연동 | 로컬만 사용. |
| 광고 로딩 실패 | 광고가 시청되지 않으면 카운트 없음. `CHMAdmob` 기존 안내 사용. |
| 보상 수령 직후 강제 종료 | 보상 지급 후 `SaveData` 트랜잭션으로 보호. 재진입 시 `clearState=Clear` 유지. |
| 보상 미수령 상태로 자정 넘김 | 진행도 + 미수령 보상 모두 리셋. (의도된 동작) |
| 출석 미션 자동 카운트 | 첫 진입 즉시 `attendanceTodayDone=true`, 보상은 버튼 클릭 시 지급. |
| `Mission.json` 파싱 시 `dailyCounter` 누락 (기존 미션) | `EDailyCounter.None` 기본값. tapIndex 1, 2 미션 동작 영향 없음. |

## 10. 테스트 시나리오

### 단위 테스트 가능 항목

- `CHMTime.GetUtcDateKey()`가 입력 DateTime에 대해 정확한 "yyyyMMdd" 반환
- `CHMTime.UtcNow`가 `_lastNtpUtc` 설정 후 `realtimeSinceStartup` 경과에 비례해 증가
- `DailyMissionService.GetDailyProgress()`가 `dailyCounter`별로 올바른 값 반환
- `CheckAndResetIfNeeded()`가 같은 날 호출 시 리셋하지 않고, 다른 날에 호출 시 모든 카운터 0으로 초기화

### 수동 테스트 시나리오

1. **정상 자정 리셋:** 자정 직전 일일 탭에서 진행도 확인 → 자정 경과 후 자동 0으로 리셋되는지
2. **오프라인 시작:** 비행기 모드에서 앱 시작 → 일일 탭 잠금 메시지 확인 → 네트워크 켜고 30초 내 자동 활성화 확인
3. **오프라인 전환:** 온라인에서 일일 미션 진행 중 비행기 모드 → 카운터 정상 증가, 자정 넘김 시에도 정상 리셋
4. **시각 위변조:** 디바이스 시각 1년 후로 설정 → NTP 재조회로 무시되는지
5. **GPGS 동기화:** 기기 A에서 미션 일부 클리어 → 기기 B 로그인 → 진행도 유지 확인
6. **보상 수령 후 종료:** 보상 받고 즉시 앱 강제 종료 → 재진입 시 수령 완료 상태 유지

## 11. 마이그레이션

- 기존 사용자의 `Data.Login`은 신규 필드가 기본값(`""`, `0`, `false`)으로 역직렬화된다. 별도 마이그레이션 코드 불필요.
- 첫 진입 시 `lastDailyResetDateKey == ""` → 즉시 리셋되어 오늘치로 초기화.

## 12. 향후 확장 (범위 외)

- 미션 풀 랜덤 N개 선택 (`dailyCounter` 그대로 활용 가능)
- 요일별 세트 출제
- 연속 출석(streak) 보너스 (`Login.consecutiveAttendanceDays` 추가)
- 신규 보상 종류 (`EReward` 확장)
- 글로벌 출시 시 타임존 분리

## 13. 변경 영향 요약

| 파일 | 변경 내용 |
|---|---|
| `Assets/Scripts/Defines.cs` | `EDailyCounter` enum 신규 |
| `Assets/Scripts/Infomation.cs` | `MissionInfo.dailyCounter` 필드 추가 |
| `Assets/Scripts/Data.cs` | `Login`에 일일 미션 필드 5개 추가 |
| `Assets/Scripts/Manager/CHMMain.cs` | `Time` 정적 프로퍼티 + 초기화 순서 |
| `Assets/Scripts/Manager/CHMTime.cs` | 신규 (NTP 매니저) |
| `Assets/Scripts/Manager/CHMAdmob.cs` | 광고 시청 콜백에 `DailyMissionService.OnAdWatched()` |
| `Assets/Scripts/Manager/DailyMissionService.cs` | 신규 정적 클래스 |
| `Assets/Scripts/UI/UIMission.cs` | 일일 탭 + 리셋 카운트다운 |
| `Assets/Scripts/ScrollView/MissionScrollViewItem.cs` | `tapIndex=3` 분기 |
| `Assets/Scripts/Scenes/GPGameScene.cs` | 노멀 스테이지 클리어 hook |
| `Assets/Scripts/GamePlay/GPBoard.cs` 또는 `GPMatchChecker.cs` | 블록 파괴 hook |
| `Assets/AssetBundleResources/json/Mission.json` | 5개 항목 추가 |
| `Assets/AssetBundleResources/ui/UIMission.prefab` | 일일 탭 버튼 + 리셋 카운트다운 텍스트 추가 |
