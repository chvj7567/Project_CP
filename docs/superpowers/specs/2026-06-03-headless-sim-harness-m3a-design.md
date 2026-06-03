# 헤드리스 시뮬 하니스 M3a — 폭탄 시스템 설계

작성일: 2026-06-03
선행: M1(일반매치+낙하/리필+승패), M2(정적 특수블록 Wall/Potal/Creator/CatBox + 드래그규칙)

## 1. 목표

CatPang 의 **폭탄 계열 블록**을 헤드리스 시뮬에서 측정 가능하게 한다. 대상: Arrow1~6, CatPang, 5색 특수폭탄(Yellow/Orange/Blue/Green/Pink), 폭탄 발동효과(Bomb1~12 / BoomAll / Boom3), 폭탄 조합, RainbowPang(색폭탄 살포).

**M3a 범위 밖**: Fish/Ball 이동블록(M3b), 보스 group≥100000(M4), 이펙트/사운드/애니메이션 타이밍.

### 측정 효과
- 노멀모드는 time=-1 이라 미지원 사유는 전부 "초기 레이아웃의 미지원 블록". 초기 레이아웃에 폭탄 계열을 가진 stage(Arrow ~30, 색폭탄 ~25, CatPang 8, Rainbow 27 — 중복 포함)가 unlock 후보.
- **부수 효과**: 현재 M2 Sim 은 4매치/2×2매치를 폭탄 생성 없이 그냥 제거한다(부정확). M3a 는 기존 supported 59 stage 의 **폭탄 생성 정확도도 향상**시킨다.
- 한 stage 가 폭탄 + Fish/Ball 을 동시 보유하면 M3b 완료 후에야 완전 unlock.

## 2. 아키텍처 (실게임 구조 미러 + 순수부 골든)

기존 `Assets/Scripts/Sim/` 토대를 확장. 각 순수 조각을 실게임 카운터파트와 런타임 골든, 통합부는 수작업 골든(M1/M2 하이브리드 패턴 계승).

| 파일 | 책임 | 골든 대상 |
|---|---|---|
| `SimMatchChecker` **확장** | 블록별 `HScore`/`VScore`/`SquareMatch` 런렝스 추적, `ChangeMatchState(r,c)`, `SetMoveIndices`, 이동인덱스 추적 | 실 `GPMatchChecker.CheckMap` |
| `SimBombFactory` (신규) | 매치→특수블록 생성(실 `CreateBombBlock`): 2×2→CatPang, h&v 교차(≥3)→Arrow5/6, hScore>3→Arrow1/4, vScore>3→Arrow3/2. `_arrowPangIndex` 분기 + 이동인덱스 우선 위치 | 실 `GPBombResolver.CreateBombBlock` |
| `SimBombResolver` (신규) | 발동 기하 Bomb1~12 / BoomAll / Boom3 / RainbowPang. 각 BombN 은 고정 셀 집합에 `ChangeMatchState` | 실 `GPBombResolver.BombN` |
| `SimComboResolver` (신규) | 두 블록 스왑 조합 디스패치(아래 §4) | 실 `GPGameScene.AfterDrag` 조합부 수작업 포팅 + 단위 |
| `SimBlock` **확장** | `HScore`/`VScore`/`SquareMatch` 필드, 폭탄 분류 헬퍼(`IsBomb`/`IsSpecialBomb`/`IsArrow`), `Bomb()` 디스패치(state→BombN) | — |
| `SimGame` **확장** | 턴루프에 폭탄 발동(연쇄) + 조합 통합, 매치 후 `SimBombFactory` 호출 | 수작업 골든(M1식) + 단위 |
| `ISimAiPolicy` **확장** | 폭탄 스왑/조합 후보를 수 탐색에 포함(능동) | 수작업 + 단위 |

### 파일 경계 원칙
각 신규 파일은 단일 책임. `SimBombResolver`(발동 기하)와 `SimBombFactory`(생성)와 `SimComboResolver`(스왑 조합)는 분리 — 독립 골든·테스트 가능.

## 3. 매치→생성 (SimBombFactory)

실 `GPBombResolver.CreateBombBlock(moveIndex1, moveIndex2)` 포팅:
- **2×2 정사각(`SquareMatch`)** → `CatPang` 생성.
- **가로·세로 동시 ≥3 교차**(hScore≥3 && vScore≥3) → `_arrowPangIndex==1 ? Arrow5 : Arrow6`.
- **hScore>3** → 이동인덱스 칸 우선 `Arrow1/Arrow4`, 없으면 매치 시작 칸. `_arrowPangIndex` 분기.
- **vScore>3** → 이동인덱스 칸 우선 `Arrow3/Arrow2`, 없으면 매치 시작 칸.
- 생성 칸은 매치에서 빠지고 그 자리에 특수블록 배치(실 `CreateNewBlock`). 나머지 매치 칸은 일반 제거.

전제: `SimMatchChecker` 가 CheckMap 때 각 블록의 hScore/vScore(연속 길이)와 squareMatch, 그리고 이번 수에서 이동한 두 블록의 index 를 기록해야 한다.

## 4. 조합 디스패치 (SimComboResolver) — 실 AfterDrag 787~804

두 인접 블록 스왑 시(둘 중 하나 이상이 폭탄 계열):
1. `특수폭탄 && 특수폭탄`(색폭탄 2개) → **BoomAll**(전체 발동).
2. 한쪽 `PinkBomb` → **Boom3**(상대 블록과 같은 색 전부 제거).
3. 한쪽만 `특수폭탄` → 그 폭탄 단독 발동(`Bomb()`).
4. `폭탄 && 폭탄`(화살표/CatPang 2개) → **색폭탄 승급**: 한 칸을 `Random(Pink..Blue)` 색폭탄으로 변환(시드 주입), 다른 칸 제거.
5. 한쪽만 `폭탄` → 그 폭탄 단독 발동.

`특수폭탄(IsSpecialBomb)` = 5색폭탄. `폭탄(IsBomb)` = CatPang/Arrow. 승급 색 선택은 **비결정 → 시드 RNG**.

## 5. 발동 + 연쇄 (SimBombResolver / SimGame 턴루프)

- 각 `BombN` 은 고정 기하 셀 집합에 `ChangeMatchState`(범위 밖·고정블록 처리 포함). 패턴은 실 `GPBombResolver.Bomb1~12`/`Bomb9~12 마름모·테두리` 그대로.
- **수동/능동 통합**: 매치로 생성된 폭탄이 이후 매치에 포함되면 발동(수동); AI 가 폭탄을 스왑/조합하면 발동(능동, §6).
- **연쇄**: 폭탄 발동이 다른 폭탄을 `ChangeMatchState` 로 건드리면 그 폭탄도 발동(실 `ChangeMatchState`→`Block.Bomb()` 경로). 턴루프 연쇄 캡(M2 의 `MaxCascadeDepth`) 내에서 처리.
- **RainbowPang**: hp 0 일 때 발동, 보드의 일반/제거대상 칸에 `PinkBomb..BlueBomb` 를 순서대로 살포(시드 RNG 위치 선택).

## 6. 능동 AI (ISimAiPolicy 확장)

수 탐색에 폭탄 관련 수 포함:
- 일반 매치 수(기존).
- 폭탄 계열을 인접 블록과 스왑해 **발동/조합**되는 수(특수+특수, Pink+일반, 폭탄+폭탄 등).
- 드래그 규칙: 폭탄 계열(Arrow/CatPang/색폭탄)은 `CanNotDrag()==false`(드래그 가능). **RainbowPang 은 드래그 불가**(실 `CanNotDragBlock`). `SimBlock.CanNotDrag()` 에 RainbowPang 추가.
- 색폭탄 의존 stage(조합 필수)도 클리어 가능 → 측정 유효.

## 7. 비결정성 처리

전부 **시드 주입 `System.Random`**(M1/M2 계승): RainbowPang 살포 위치, 폭탄+폭탄 승급 색, 리필. Bomb9~12 의 `UnityEngine.Random.Range` 는 **이펙트 선택만**(셀 기하 무관) → Sim 에서 무시.

## 8. 골든 전략 (하이브리드)

- **순수부 런타임 골든**: `GPMatchChecker.CheckMap`(hScore/vScore/squareMatch), `GPBombResolver.BombN`(ChangeMatchState 셀 집합), `CreateBombBlock`(생성 위치·종류). 실게임 직호출 후 Sim 동등성 단언(M2 `RealBlockFactory` 확장 재사용).
- **통합부 수작업 골든**: 연쇄·조합 디스패치·턴루프 — 단위 테스트.

### ⚠️ Plan Task 1 = 골든 스파이크 (필수 선행)
실 `GPBombResolver`/`Block.Bomb()` 를 EditMode 에서 stub(effect/sound/onBoomTrigger no-op) 으로 호출 가능한지 먼저 확인. **리스크: `SaveBombCollectionData` 가 `CHMData.Instance` 접근** → M2 의 hpText NRE 와 동형 위험. 스파이크에서 우회법(예: CHMData 초기화 or collection 셀 우회 캡처) 확정 후 나머지 진행. M1 선례(골든 전제 스파이크가 접근 전체를 좌우)를 따른다.

## 9. IsSupported 갱신

`SimGame.IsSupported` 화이트리스트에 M3a 블록 추가: Arrow1~6(10~15), CatPang(18), 5색폭탄(19~23), RainbowPang(52). **Fish(24)/Ball(53) 은 여전히 미지원(M3b)**.

## 10. 테스트 / 측정

- EditMode 단위·골든(plain `[Test]`, 자작 `SimTestRunner` 로 실행 — MCP `editor_invoke_method`).
- 회귀: 기존 PASS 33 유지.
- `CountSupported` 재측정 → M3a unlock 수 확인. (Fish/Ball 단독 보유 stage 제외하면 폭탄만 가진 stage 가 supported 로 전환.)
- 기존 59 stage 메트릭 재측정(폭탄 생성 반영 변화 기록).

## 11. 산출물

- `Assets/Scripts/Sim/SimBombResolver.cs`, `SimBombFactory.cs`, `SimComboResolver.cs` (신규)
- `SimMatchChecker.cs`, `SimBlock.cs`, `SimGame.cs`, `ISimAiPolicy.cs` (확장)
- `Assets/Tests/EditMode/Sim/SimBomb*Tests.cs`, `SimBombGoldenTests.cs` (신규)
- `docs/qa-reports/` 측정 리포트
