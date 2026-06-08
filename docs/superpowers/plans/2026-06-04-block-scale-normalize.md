# 블럭 비율 정규화 (9×9 기준 풋프린트) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 보드가 9×9보다 작아도 보드 전체가 9×9와 동일한 화면 풋프린트를 채우도록 블럭 크기·간격을 `factor = 9 / boardSize` 배 확대한다.

**Architecture:** 레이아웃 단일 진실 지점인 `CHInstantiateButton`에서 `buttonWidth/Height(+margin)`에 factor를 곱해 간격·히트판정·낙하거리를 일괄 보정하고, 블럭 `localScale`/스폰 트윈 목표 스케일을 factor로 맞춘다. 9×9는 factor=1.0으로 기존과 동일(회귀 없음).

**Tech Stack:** Unity 6 / C# / UGUI RectTransform / DOTween

> 관련 spec: `docs/superpowers/specs/2026-06-04-block-scale-normalize-design.md`

---

### Task 1: CHInstantiateButton — factor 산출 및 블럭 크기·스케일 적용

**Files:**
- Modify: `Assets/Scripts/Function/CHInstantiateButton.cs`

대상: `InstantiateButton()` (line 51~102), 신규 `GetScaleFactor()` 노출.

- [ ] **Step 1: 기준 상수 + factor 필드 추가**

클래스 상단 static 필드 영역에 추가:

```csharp
//# 9×9 기준 풋프린트 정규화 배율
const int ReferenceBoardSize = 9;
static float scaleFactor = 1f;

static public float GetScaleFactor()
{
    return scaleFactor;
}
```

- [ ] **Step 2: InstantiateButton에서 factor 계산 후 크기·간격·스케일에 반영**

`buttonWidth`/`buttonHeight` 산출부(line 65~66) 직후에 factor 계산 및 적용:

```csharp
buttonWidth = Mathf.Abs(buttonRectTransform.rect.x * 2);
buttonHeight = Mathf.Abs(buttonRectTransform.rect.y * 2);

//# 정사각 보드 전제 — 작은 보드일수록 9×9 풋프린트를 채우도록 확대
scaleFactor = (float)ReferenceBoardSize / _horizontalCount;
buttonWidth *= scaleFactor;
buttonHeight *= scaleFactor;
margin *= scaleFactor;
//# 좌표 산출(posDict)은 필드 margin이 아니라 파라미터 _margin을 쓰므로 함께 동기화
_margin *= scaleFactor;
```

> delta(2026-06-04 구현 시 발견): 좌표 산출 `posDict`(line 94)는 필드 `margin`이 아니라 **파라미터 `_margin`** 을 사용한다. 따라서 `_margin *= scaleFactor` 한 줄을 함께 넣어야 작은 보드에서 간격의 margin 성분까지 factor를 추종한다. 9×9는 `*= 1f`라 무영향.

블럭 인스턴스화 루프(line 84~99)에서 각 블럭 localScale 적용 — `rectTransform.anchoredPosition = pos.Value;` 다음 줄에 추가:

```csharp
rectTransform.localScale = Vector3.one * scaleFactor;
```

> 주의: `posDict` 좌표 산출(line 77)은 이미 갱신된 `buttonWidth`/`margin`을 쓰므로 자동 반영됨. `GetHorizontalDistance`/`GetVerticalDistance`/`GetBlockInfo`는 갱신된 `buttonWidth`를 그대로 사용 → 별도 수정 불필요.

- [ ] **Step 3: 코딩 룰 확인**

`var` 없음, `!` 없음, 주석 `//#`, 가드 절 형식 확인.

- [ ] **Step 4: 컴파일 확인 (에디터)**

Unity 콘솔에 컴파일 에러 없음 확인.

---

### Task 2: GPGameScene — 스폰 트윈 목표 스케일을 factor로

**Files:**
- Modify: `Assets/Scripts/Scenes/GPGameScene.cs` (CreateMap, line 483 부근)

- [ ] **Step 1: DOScale 목표값 변경**

```csharp
//# (변경 전)
block.rectTransform.DOScale(1f, delay);

//# (변경 후) — CHInstantiateButton이 산출한 factor 목표로 트윈
block.rectTransform.DOScale(CHInstantiateButton.GetScaleFactor(), delay);
```

- [ ] **Step 2: x 중앙 정렬 확인 (수정 없음)**

`moveDis = CHInstantiateButton.GetHorizontalDistance() * (boardSize - 1) / 2` 는 factor 반영된 거리 사용 → 그대로 둠.

- [ ] **Step 3: 컴파일 확인**

---

### Task 3: 스모크 검증

- [ ] **9×9 스테이지** — factor=1.0, 블럭 크기·간격·정렬이 기존과 동일.
- [ ] **작은 보드(예: 3×3)** — 보드가 9×9 가로 영역을 채우고 블럭이 약 3배 커지며, 드래그·매치·낙하 정상.
- [ ] **드래그 히트 판정** — 커진 블럭에서 인접 스왑이 올바른 블럭을 잡음.

---

## Self-Review

- **Spec coverage**: spec §3.1(CHInstantiateButton)·§3.2(GPGameScene DOScale)·§5(스모크) 모두 Task 1~3에 매핑됨. margin factor 적용은 spec §3.1 주의문 반영.
- **Placeholder scan**: 없음.
- **Type consistency**: `GetScaleFactor()` (Task 1 정의 ↔ Task 2 호출) 일치, `scaleFactor` 명명 일관.

---

## 후속 버그 수정 delta (2026-06-04)

초기 보드는 정상이나, 블럭을 `localScale=factor`로 키운 탓에 **스케일을 1배로 되돌리거나 sizeDelta(스케일 미반영)를 읽는 지점**이 factor를 못 따라오는 잔여 버그 3건을 보정. (9×9에서는 factor=1 → 전부 `*1`이라 회귀 0.)

| # | 파일 / 라인 | 증상 | 수정 |
|---|---|---|---|
| 1 | `GPBoard.cs` `CreateNewBlock()` (~76) | 매치 후 새로 채워지는 블럭이 1배로 작게 나옴 | `DOScale(1f)`/`Vector3.one` → `GetScaleFactor()` 기준 (`DOScale(scaleFactor)` / `Vector3.one * scaleFactor`) |
| 1b | `GPGameScene.cs` 힌트 펄스 (~245) | `HintPulseScale=1.5` 절대값이라 factor 블럭을 1.5로 줄였다 1로 복귀(잠복 버그) | factor 기준 상대 펄스 `DOScale(scaleFactor * HintPulseScale)` → 복귀 `DOScale(scaleFactor)` |
| 3 | `GPGameScene.cs` 가이드(~879) · `GPTutorial.cs` (`StartTutorial` ~94) | 가이드 홀/핑거가 블럭 `sizeDelta`(스케일 미반영) 기반이라 1배로 작게 보임 | sizeDelta는 유지, `guideHole`/`guideFinger`에 `localScale = Vector3.one * GetScaleFactor()` 적용으로 블럭과 동일 배율 통일 |

**버그 #2 (작은 보드 세로 위치 정렬) — 재수정: Y축 중앙정렬 추가 (2026-06-04)**: 초기 판단(#1로 해소, 수정 불필요)을 **정정한다**. 화면 재확인 결과 작은 보드가 세로로 너무 위에 떠 보이는 별도 위치 버그가 재발견됨. 근본 원인은 `CreateMap`이 **X축만 중앙정렬(`originPos.x -= moveDis`)하고 Y축 보정이 없던 것**. 작은 보드는 윗줄이 9×9와 같은 buttonY에 고정된 채 아래로 자라는데, 블럭이 factor배 커져 보드 전체가 9×9보다 위로 떠 보였다.

수정: X정렬 바로 옆에 Y보정 한 줄 추가 (`GPGameScene.cs` `CreateMap()`).
```csharp
int referenceBoardSize = 9;
float moveDisY = CHInstantiateButton.GetVerticalDistance() * (referenceBoardSize - boardSize) / (2f * referenceBoardSize);
block.originPos.y -= moveDisY;
```
- `GetVerticalDistance()` = factor 반영 세로간격(base × factor, factor=9/boardSize).
- `moveDisY = vDist*(9-n)/18`. n=3: `3·base·6/18 = base` → 위로 떠있던 1칸만큼 아래로. n=9: `(9-9)=0` → **9×9 회귀 0**.
- `originPos`는 단일 진실값이라 X정렬이 전 블럭에 전파되는 것과 동일하게 Y도 한 줄로 전파됨 (`ChangeBlock` swap / `CreateNewBlock` / `DownBlock` / 가이드 모두 originPos 추종). Fish/Ball 맨아랫줄 판정은 boardArr 인덱스 기반이라 픽셀 Y 무관 — 영향 없음. 다른 위치 코드는 추측으로 건드리지 않음.
