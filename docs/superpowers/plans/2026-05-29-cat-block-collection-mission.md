# Cat1~5 기본 고양이 블록 수집 미션 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 미션 탭 tapIndex 1에 Cat1~5 기본 고양이 블록 수집 미션 5개를 추가하고, 일반 고양이 블록 매치가 수집량으로 적립되도록 보완한다.

**Architecture:** 매치 제거 지점(`GPGameScene.RemoveMatchBlock`)에서 `Block.GetBaseCat`로 정규화한 색별 수집량을 `Data.Collection`에 누적한다. 폭탄은 `GetBaseCat→None`으로 제외돼 기존 `SaveBombCollectionData` 경로와 이중 카운트되지 않는다. 미션 항목은 Mission.json + String JSON 데이터로만 추가하며 미션 UI(`MissionScrollViewItem`)는 collectionType 기반으로 이미 동작하므로 변경하지 않는다.

**Tech Stack:** Unity 6 (Assembly-CSharp, 글로벌 네임스페이스), Addressables(`"Resource"` 라벨), Unity Test Framework(NUnit, EditMode). spec: `docs/superpowers/specs/2026-05-29-cat-block-collection-mission-design.md`

---

## File Structure

| 파일 | 책임 | 변경 |
|---|---|---|
| `Assets/Scripts/Scenes/GPGameScene.cs` | 매치 제거 시 일반 고양이 수집 적립 | `RemoveMatchBlock` 루프 내 3줄 추가 |
| `Assets/AssetBundleResources/json/Mission.json` | 미션 데이터 | tapIndex 1 맨 앞에 missionID 18~22 (5행) |
| `Assets/AssetBundleResources/json/StringKorea.json` | 한국어 미션 설명 | stringID 180~184 |
| `Assets/AssetBundleResources/json/StringEnglish.json` | 영어 미션 설명 | stringID 180~184 |
| `Assets/Tests/EditMode/` (신규) | EditMode 테스트 + asmdef | GetBaseCat 정규화 계약 + JSON 데이터 검증 |

> **테스트 인프라 결정 (BLOCKER 후보):** 현재 프로젝트에 `Assets/Tests/EditMode/` 폴더도 테스트 asmdef도 없고, 게임 코드는 asmdef 없는 기본 `Assembly-CSharp` 에 있다. asmdef 기반 테스트 어셈블리는 `Assembly-CSharp` 를 직접 참조할 수 없다. Task 1에서 이 인프라를 세우되, `Block.GetBaseCat` 가 `private const SkinCatCount` 에 의존하므로 **테스트는 `GetBaseCat` 의 public 반환 계약만 검증**한다(내부 상수 비참조). 이 결정이 막히면 game-designer/test-engineer 단계에서 사용자에게 에스컬레이션.

---

## Task 1: EditMode 테스트 인프라 + GetBaseCat 정규화 계약 테스트

**Files:**
- Create: `Assets/Tests/EditMode/Lair.Tests.EditMode.asmdef`
- Create: `Assets/Tests/EditMode/BlockCollectionNormalizeTests.cs`
- Reference: `Assets/Scripts/Block.cs:353-364` (`public static EBlockState GetBaseCat`)

- [ ] **Step 1: asmdef 작성** — `Assembly-CSharp` 참조가 가능하도록 구성. UTF는 다음 내용:

```json
{
    "name": "Lair.Tests.EditMode",
    "rootNamespace": "",
    "references": ["UnityEngine.TestRunner", "UnityEditor.TestRunner"],
    "includePlatforms": ["Editor"],
    "optionalUnityReferences": ["TestAssemblies"],
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"],
    "autoReferenced": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

> `Assembly-CSharp` 가 asmdef 테스트 어셈블리에서 안 보이면, test-engineer 가 (a) 게임 코드에 production asmdef 추가 또는 (b) 테스트를 `Assembly-CSharp-Editor` 경유로 두는 방식 중 택1 (사용자 확인). 본 플랜은 (a) 없이 진행 가능한 범위를 기본으로 한다.

- [ ] **Step 2: 실패하는 테스트 작성** (한국어 메서드명 규칙)

```csharp
using NUnit.Framework;
using Defines;

public class BlockCollectionNormalizeTests
{
    [Test]
    public void 기본고양이는_자기색을_반환한다()
    {
        Assert.AreEqual(EBlockState.Cat1, Block.GetBaseCat(EBlockState.Cat1));
        Assert.AreEqual(EBlockState.Cat5, Block.GetBaseCat(EBlockState.Cat5));
        Assert.AreEqual(EBlockState.Cat7, Block.GetBaseCat(EBlockState.Cat7));
    }

    [Test]
    public void 스킨고양이는_기본색으로_합산된다()
    {
        //# CatCrown1(54) → Cat1, 6테마 × Cat1~5 모두 기본색 매핑
        Assert.AreEqual(EBlockState.Cat1, Block.GetBaseCat(EBlockState.CatCrown1));
        Assert.AreEqual(EBlockState.Cat1, Block.GetBaseCat(EBlockState.CatStrawberry1));
        Assert.AreEqual(EBlockState.Cat5, Block.GetBaseCat(EBlockState.CatCrown5));
    }

    [Test]
    public void 폭탄과_벽은_None을_반환한다()
    {
        //# 이중 카운트 회피의 핵심 계약 — 폭탄/특수폭탄/Wall 은 적립 경로에서 제외
        Assert.AreEqual(EBlockState.None, Block.GetBaseCat(EBlockState.Arrow1));
        Assert.AreEqual(EBlockState.None, Block.GetBaseCat(EBlockState.PinkBomb));
        Assert.AreEqual(EBlockState.None, Block.GetBaseCat(EBlockState.CatPang));
        Assert.AreEqual(EBlockState.None, Block.GetBaseCat(EBlockState.Wall));
    }
}
```

> 주의: `EBlockState` 의 실제 네임스페이스를 `Defines.cs` 에서 확인해 `using` 을 맞춘다 (글로벌이면 `using` 제거). 스킨 enum 명(`CatCrown1`/`CatStrawberry1`/`CatCrown5`)은 `Defines.cs:151~` 의 실제 항목명으로 확인 후 사용.

- [ ] **Step 3: 테스트 실행 → 컴파일/통과 확인**

Run: Unity `Window > General > Test Runner > EditMode > Run All` (또는 UnityMCP 테스트 실행)
Expected: 3개 테스트 PASS (기존 `GetBaseCat` 동작을 핀하는 characterization 테스트)

- [ ] **Step 4: 커밋(안)** — Rule 01: 자동 커밋 금지. `git add` + 메시지(안)만.

```
# [test] - GetBaseCat 고양이 블록 정규화 계약 테스트 추가
```

---

## Task 2: 일반 고양이 블록 수집 적립 (GPGameScene)

**Files:**
- Modify: `Assets/Scripts/Scenes/GPGameScene.cs` — `RemoveMatchBlock()` 루프, `DailyMissionService.OnBlockDestroyed(1);` 직후

- [ ] **Step 1: 적립 코드 추가** — 매치 제거 1회/블록 게이트(`!block.remove`) 안, 일일미션 카운트 옆:

```csharp
                    // 일일 미션 — 매치로 사라지는 블록 카운트 (Wall/Locker 등 IsMatch=false인 항목 자동 제외)
                    DailyMissionService.OnBlockDestroyed(1);
                    //# tapIndex1 Cat 수집 미션 — 일반 고양이 블록 누적. 스킨은 GetBaseCat으로 기본 색(Cat1~5)에 합산.
                    //# 폭탄/특수폭탄/Wall 등은 GetBaseCat이 None을 반환해 제외 (폭탄은 SaveBombCollectionData에서 별도 적립)
                    EBlockState collectBaseCat = Block.GetBaseCat(block.GetBlockState());
                    if (collectBaseCat != EBlockState.None)
                    {
                        CHMData.Instance.GetCollectionData(collectBaseCat.ToString()).value += 1;
                    }
                    block.rectTransform.DOScale(0f, delay);
```

> Rule 02: `//#` 주석 · 명시적 타입 · 가드 절 외 분기 중괄호 필수(이 `if` 는 가드 절이 아니므로 중괄호). `var` / `!` 금지.

- [ ] **Step 2: 컴파일 확인**

Run: UnityMCP `editor_recompile` → `editor_wait_ready` → `editor_read_log {type:Error}`
Expected: 에러 0 (domain_reload 후)

- [ ] **Step 3: 커밋(안)**

```
# [feat] - 일반 고양이 블록 매치 시 색별 수집량 적립
```

---

## Task 3: Mission.json — Cat1~5 미션 5행 추가

**Files:**
- Modify: `Assets/AssetBundleResources/json/Mission.json` — 배열 맨 앞(missionID 1 앞)

- [ ] **Step 1: 5행 추가** — `[` 직후, 기존 missionID 1 행 앞에 삽입:

```json
  {"missionID":"18", "tapIndex":"1", "descStringID":180, "collectionType":0,"clearValue":100, "addValue":100, "reward":0, "rewardCount":100},
  {"missionID":"19", "tapIndex":"1", "descStringID":181, "collectionType":1,"clearValue":100, "addValue":100, "reward":0, "rewardCount":100},
  {"missionID":"20", "tapIndex":"1", "descStringID":182, "collectionType":2,"clearValue":100, "addValue":100, "reward":0, "rewardCount":100},
  {"missionID":"21", "tapIndex":"1", "descStringID":183, "collectionType":3,"clearValue":100, "addValue":100, "reward":0, "rewardCount":100},
  {"missionID":"22", "tapIndex":"1", "descStringID":184, "collectionType":4,"clearValue":100, "addValue":100, "reward":0, "rewardCount":100},
```

- [ ] **Step 2: JSON 파싱 검증**

Run: `[System.IO.File]::ReadAllText(path,[Text.Encoding]::UTF8) | ConvertFrom-Json` (PowerShell)
Expected: PARSE OK, 항목 수 = 기존 23 + 5 = 28

- [ ] **Step 3: 커밋(안)**

```
# [feat] - Mission.json tapIndex1 맨 앞에 Cat1~5 수집 미션 추가
```

---

## Task 4: String JSON — descStringID 180~184

**Files:**
- Modify: `Assets/AssetBundleResources/json/StringKorea.json` — 마지막 항목(179) 뒤
- Modify: `Assets/AssetBundleResources/json/StringEnglish.json` — 마지막 항목(179) 뒤

- [ ] **Step 1: StringKorea.json 5항목 추가** (179 뒤, `]` 앞)

```json
        {"stringID": 180, "value": "고양이 블록 1 터트리기"},
        {"stringID": 181, "value": "고양이 블록 2 터트리기"},
        {"stringID": 182, "value": "고양이 블록 3 터트리기"},
        {"stringID": 183, "value": "고양이 블록 4 터트리기"},
        {"stringID": 184, "value": "고양이 블록 5 터트리기"}
```

> 기존 파일은 들여쓰기된 멀티라인 형식(`"stringID": N` / `"value": "..."`). 기존 스타일에 맞춰 작성한다. 파일 인코딩은 UTF-8(BOM 없음) 유지.

- [ ] **Step 2: StringEnglish.json 5항목 추가**

```json
        {"stringID": 180, "value": "Pop Cat Block 1"},
        {"stringID": 181, "value": "Pop Cat Block 2"},
        {"stringID": 182, "value": "Pop Cat Block 3"},
        {"stringID": 183, "value": "Pop Cat Block 4"},
        {"stringID": 184, "value": "Pop Cat Block 5"}
```

- [ ] **Step 3: 두 파일 UTF-8 파싱 검증** (Task 3 Step 2와 동일 방식, 한국어는 explicit UTF-8 디코딩 필수 — 시스템 코드페이지로 읽으면 모지바케로 파서가 실패함)

Expected: 두 파일 PARSE OK, id 180~184 한국어/영어 값 정상

- [ ] **Step 4: 커밋(안)**

```
# [feat] - 미션 설명 문자열 180~184 추가 (한/영)
```

---

## Task 5: 데이터 정합 통합 검증 (선택 — EditMode)

**Files:**
- Create: `Assets/Tests/EditMode/MissionDataIntegrityTests.cs`

- [ ] **Step 1: 미션↔문자열 정합 테스트**

```csharp
using NUnit.Framework;
using System.IO;

public class MissionDataIntegrityTests
{
    [Test]
    public void Cat미션_18에서_22는_descString_180에서_184를_가리킨다()
    {
        //# Mission.json missionID 18~22 의 descStringID 가 180~184 와 1:1 대응하는지,
        //# 두 String JSON 에 해당 stringID 가 존재하는지 검증.
        //# 경로: Application.dataPath 기준 AssetBundleResources/json/*.json 파싱
        //# (구체 파싱 코드는 test-engineer 가 JsonArrayUtility/JsonUtility 패턴으로 작성)
        Assert.Pass("test-engineer 단계에서 구체화");
    }
}
```

> 이 태스크는 test-engineer 가 본격 작성한다 (엣지·회귀·통합). 플랜에서는 검증 대상만 고정: ① missionID 18~22 ↔ descStringID 180~184 매핑, ② collectionType 0~4 가 EBlockState.Cat1~5 와 일치, ③ 한/영 두 파일에 180~184 모두 존재.

- [ ] **Step 2: 실행 → 통과 / 커밋(안)**

```
# [test] - Cat 미션 데이터 정합 통합 테스트 추가
```

---

## 배포 주의 (구현 완료 후)

세 JSON은 `"Resource"` 라벨 Local 그룹으로 로드된다. **APK 빌드 전 `Window > Asset Management > Addressables > Groups > Build` 필수.** 에디터 플레이 테스트는 Addressables Play Mode Script가 "Use Asset Database" 일 때만 실시간 반영(=Existing Build면 옛 데이터).

수동 플레이테스트: 미션 탭 tapIndex 1 진입 → Cat1~5 미션 5개가 최상단 노출 → 스테이지 플레이 후 진행도 상승 → clearValue 100 달성 시 Gold 100 수령 → 다음 목표 200으로 갱신 확인.

---

## Self-Review

**1. Spec coverage:**
- spec §3 코드 변경 필수 → Task 2 ✅
- spec §4.2 Mission.json 18~22 → Task 3 ✅
- spec §4.1 GetBaseCat 정규화·이중카운트 회피 → Task 1(계약 테스트) + Task 2(적용) ✅
- spec §4.3 미션 UI 무변경 → 명시(변경 파일 목록에 미션 UI 없음) ✅
- spec §6 엣지(스킨·Cat6/7·기존 플레이어) → Task 1 스킨 테스트 + 배포주의 ✅
- spec §7 테스트 대상 4종 → Task 1(정규화 3종) + Task 5(데이터 정합) ✅. **단 "RemoveMatchBlock 적립 자체"의 런타임 통합 테스트는 PlayMode 영역이며 test-engineer 가 판단** (현 인프라상 EditMode 단위테스트 불가, GetBaseCat 계약 + 수동 플레이테스트로 커버).
- spec §9 Addressables 리빌드 → 배포 주의 ✅

**2. Placeholder scan:** Task 5는 의도적으로 test-engineer 위임(플레이스홀더가 아니라 검증 대상 고정). Task 1/2/3/4는 실제 코드·명령 포함. ✅

**3. Type consistency:** `Block.GetBaseCat(EBlockState)→EBlockState`, `CHMData.Instance.GetCollectionData(string).value`, `EBlockState.None` — Task 1/2 전체에서 일관. collectionType 정수(0~4) ↔ `EBlockState.Cat1~5` ↔ enum명 `"Cat1"~"Cat5"` 매핑 일관. ✅

**알려진 제약:** EditMode 테스트가 `Assembly-CSharp` 를 참조하는 인프라가 부재 — Task 1 Step 1의 결정이 막히면 test-engineer/사용자 에스컬레이션.
