# 블럭 비율 정규화 — 9×9 기준 풋프린트 고정 (Design)

- **날짜**: 2026-06-04
- **단계**: 프로토타입 (start-develop-simple)
- **상태**: 승인됨 (사용자 리뷰 대기)

## 1. 목표

보드 크기(`boardSize`, 2~9 가변)가 9×9보다 작아도, **보드 전체가 9×9와 동일한 화면 풋프린트를 채우도록** 블럭 1개당 크기와 간격을 확대한다. 작은 보드일수록 블럭이 커진다.

- 스케일 배율: `factor = ReferenceBoardSize / boardSize`, `ReferenceBoardSize = 9`
- 예: 3×3 → factor 3.0 (블럭 약 3배), 9×9 → factor 1.0 (기존과 완전 동일)

## 2. 기준 결정 (Decision Lock)

| 항목 | 결정 |
|---|---|
| 기준 영역 | **9×9 전체 풋프린트 고정** — 작은 보드도 9×9가 차지하던 가로폭(9칸×간격)을 꽉 채움 |
| 배율 공식 | `factor = (float)ReferenceBoardSize / boardSize`, `ReferenceBoardSize = 9` |
| 블럭 전제 | 정사각 보드 (`horizontalCount == verticalCount == boardSize`) |
| 세로 정렬 | 기존 유지 — y는 top 기준, x만 중앙 정렬 (9×9 풋프린트와 동일하므로 추가 y정렬 불필요) |
| 9×9 회귀 | factor=1.0 → 기존 동작과 완전 동일해야 함 |

## 3. 적용 지점

### 3.1 `Assets/Scripts/Function/CHInstantiateButton.cs` — 단일 진실 지점

`InstantiateButton()` 내부에서 프리팹 origin 크기로 산출하는 `buttonWidth`/`buttonHeight`에 `factor`를 곱한다.

- `ReferenceBoardSize = 9` 상수 도입.
- `factor = (float)ReferenceBoardSize / _horizontalCount` (정사각 전제, `_horizontalCount == _verticalCount`).
- `buttonWidth`, `buttonHeight`를 `factor` 배로 키워 저장.
  - 이렇게 하면 다음이 **모두 자동으로 일관되게** 커진다:
    - 간격 산출 `GetHorizontalDistance()` / `GetVerticalDistance()` (`buttonWidth + margin`)
    - 블럭 배치 좌표 (`posDict` 의 `buttonWidth` 기반 오프셋)
    - 드래그 히트 판정 `GetBlockInfo()` 의 `buttonWidth / 2f`
- 인스턴스화한 각 블럭의 `rectTransform.localScale = Vector3.one * factor` 로 시각 크기 확대 (스프라이트가 셀에 맞게 커짐).

> 주의: `margin`은 factor를 곱하지 않는다(원래 의도가 간격 여백이면 고정이 자연스러움). 단, 풋프린트를 9×9와 정밀히 일치시키려면 margin도 곱하는 편이 정확하다 — **margin도 factor 배 적용**하여 9×9 풋프린트를 정확히 보존한다. (margin 기본값 0이라 실질 영향은 없을 가능성이 높음)

### 3.2 `Assets/Scripts/Scenes/GPGameScene.cs` — 트윈 목표 스케일 연동

`CreateMap()` (현재 line 483 부근):

```csharp
block.rectTransform.DOScale(1f, delay);   //# (변경 전) 항상 1배 목표
block.rectTransform.DOScale(factor, delay); //# (변경 후) factor 목표
```

- `factor` 값은 `CHInstantiateButton`에서 조회할 수 있도록 `static float GetScaleFactor()` (또는 동등 프로퍼티)를 노출한다.
- x 중앙 정렬 `moveDis = GetHorizontalDistance() * (boardSize - 1) / 2` 는 factor 반영된 `GetHorizontalDistance()`를 그대로 쓰므로 **수정 불필요**.

## 4. 자동 추종(수정 불필요) 항목

- 낙하 애니메이션 (`DOAnchorPosY` 등) — `anchoredPosition` 기반
- 폭탄 이펙트 위치 — 블럭 `rectTransform.position` 기반
- 매치/폭탄 범위 계산 — row/col 인덱스 기반(픽셀 무관)

## 5. 검증 (스모크)

1. **9×9 스테이지** — factor=1.0, 블럭 크기·간격·정렬이 기존과 동일 (회귀 없음).
2. **3×3(또는 작은) 스테이지** — 보드 전체가 9×9와 동일한 가로 영역을 채우고, 블럭이 약 3배 커지며, 드래그·매치·낙하가 정상 동작.
3. **드래그 히트 판정** — 커진 블럭에서도 인접 스왑이 올바른 블럭을 잡는다.

## 6. 범위 밖 (YAGNI)

- 비정사각 보드 (`horizontalCount != verticalCount`)
- boardSize > 9 (축소 케이스)
- 런타임 동적 보드 리사이즈
- 세로 방향 별도 중앙 정렬 로직

## 7. 코딩 룰 준수 메모

- `var` 금지 → 명시적 타입 (Rule 02 §3)
- `!` 금지 → `== false` / `== null` (Rule 02 §4)
- 주석 `//#` 접두 (Rule 02 §1)
- 가드 절 중괄호 없이 개행 (Rule 02 §2)
