# 언어 설정 클라우드 덮어쓰기 버그 수정 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** GPGS 로그인 시 클라우드 데이터가 로컬 `languageType`을 덮어써 언어가 바뀌는 버그를, 병합 시 언어를 로컬 값으로 보존하도록 수정한다.

**Architecture:** `CHMData.LoadCloudData`의 충돌 병합 루프에서 일일 미션 필드처럼 `languageType`도 로컬 우선으로 보존(단, 날짜 비교와 무관하게 항상). 언어는 기기 로컬 전용 — 클라우드 로드 시 무시한다. 최초 기기언어·옵션 변경·재실행 영속성은 기존 코드(LoadLocalData line 44 신규유저 한정, UISetting SaveData, CHMString 읽기)로 이미 보장됨.

**Tech Stack:** Unity 6 / C# / GPGS Cloud Save (`ChvjUnityInfra.CHMGPGS`) / JsonUtility

> 관련 spec: `docs/superpowers/specs/2026-06-04-language-cloud-overwrite-fix-design.md`

---

### Task 1: LoadCloudData 병합 시 languageType 로컬 보존

**Files:**
- Modify: `Assets/Scripts/Manager/CHMData.cs` (`LoadCloudData`, line 186~208 병합 루프)

- [ ] **Step 1: 병합 루프에 언어 보존 한 줄 추가**

`cloudLogin` 획득(line 191) 직후, 날짜 비교 if/else 앞에 추가:

```csharp
var cloudLogin = kvp.Value;

//# 언어는 기기 로컬 전용 — 클라우드 값으로 덮어쓰지 않는다 (옵션에서만 변경)
cloudLogin.languageType = localLogin.languageType;
```

전체 맥락(수정 후):
```csharp
foreach (var kvp in cloudDict)
{
    if (loginLocalDataDic.TryGetValue(kvp.Key, out var localLogin) == false)
        continue;

    var cloudLogin = kvp.Value;

    //# 언어는 기기 로컬 전용 — 클라우드 값으로 덮어쓰지 않는다 (옵션에서만 변경)
    cloudLogin.languageType = localLogin.languageType;

    // 로컬 날짜 키가 클라우드보다 미래이면 로컬 일일 필드 우선
    if (string.Compare(localLogin.lastDailyResetDateKey, cloudLogin.lastDailyResetDateKey, StringComparison.Ordinal) > 0)
    {
        // ... 기존 일일 미션 필드 보존 (무수정)
    }
    else
    {
        // ... 기존 로그 (무수정)
    }
}
```

- [ ] **Step 2: 코딩 룰 확인**

신규 줄은 대입문이라 `var` 없음. `!` 없음. 주석 `//#`. 기존 `var cloudLogin`/`var localLogin` 라인은 미수정 보존(적용범위 밖).

- [ ] **Step 3: 컴파일 확인 (에디터)**

Unity 콘솔 컴파일 에러 없음 확인. `localLogin`·`cloudLogin` 모두 `Data.Login`이라 `languageType` 대입 유효.

---

### Task 2: 스모크 검증

- [ ] **수정 확인** — 클라우드에 영어 저장된 계정을 한국어 기기에서 GPGS 로그인 → 로그인 후에도 한국어 유지.
- [ ] **옵션 변경 영속** — 옵션에서 영어 변경 → 앱 재실행 시 영어 시작 → GPGS 재로그인해도 영어 유지.
- [ ] **최초 실행** — 신규 유저(로컬 파일 없음)는 기기 언어로 시작.
- [ ] **회귀** — 일일 미션 충돌 병합(날짜 비교)은 기존대로 동작.

---

## Self-Review

- **Spec coverage**: spec §4(수정 지점)→Task 1, §6(스모크)→Task 2. §3(정상 동작 무수정)은 변경 없음 항목이라 태스크 불요. 갭 없음.
- **Placeholder scan**: 없음 (기존 무수정 블록은 "// ... 기존" 으로 명시, 변경 코드는 전량 표기).
- **Type consistency**: `cloudLogin.languageType`/`localLogin.languageType` 모두 `Data.Login.languageType` (`Defines.ELanguageType`) 일치.
