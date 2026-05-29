# 헤드리스 플레이스루 시뮬레이션 하니스 — 설계 (spec)

> 상태: **설계 확정 대기 (사용자 승인 게이트).** 코드 결합도 검증 완료, 실행 구조·검증 기준·범위 결론 확정.

## 목표

CatPang 매치-3 게임의 실제 스테이지(`Assets/AssetBundleResources/json/Stage.json`,
`StageBlock.json`)를 자동으로 N판 플레이하고, 클리어율·평균 클리어 시간/이동 수·
실패 사유(EFailReason) 분포 등 메트릭을 JSON으로 출력해, **qa-simulator 가 실제
스테이지를 플레이스루 시뮬레이션으로 검증**할 수 있게 하는 검증 인프라.

## 확정 결정 (락)

### 1. 실행 구조: 순수 C# 병행 재구현 (parallel re-implementation)
매치 판정/연쇄/보스 로직을 MonoBehaviour·트윈·코루틴 없는 순수 클래스(예: `SimBoard`/`SimBlock`)로
**별도 검증 전용 구현**. 렌더·대기가 없어 빠르고(초당 수천 판) 결정적 → 시드 재현 가능.
**기존 `Assets/Scripts/GamePlay/` 및 `Block.cs` 는 건드리지 않는다** (회귀 방지).

### 2. "기존 GP* 재사용" 경로 검토 후 기각 (근거)
- GPBoard·GPMatchChecker·GPBombResolver 는 순수 클래스지만 **`Block`(MonoBehaviour)에 강결합**:
  `boardArr` 가 `Block[,]` 이고 판정이 Block 의 **public 필드를 직접 읽고 씀**
  (`match`/`boom`/`squareMatch`/`row`/`col`/`index`/`hScore`/`vScore`/`changeBlockState`,
  `rectTransform.DOScale` 등).
- 게임 진행 루프 전체가 **`GPGameScene`(MonoBehaviour, 1035줄)** 에 있음:
  `CreateMap → AfterDrag → RemoveMatchBlock → DownBlock → UpdateMap → GameEnd`.
  Bomb1~12 구현 11개도 GPGameScene 에 있고 트윈·이펙트·사운드와 섞임.
- → IBlock 추상화로 재사용하려면 Block 의 public 필드 → 프로퍼티 전환 + 모든 GP* 수정 +
  GPGameScene 게임루프 추출이 필요 = "게임 코드 미수정" 락을 깨고 Beta 코드에 회귀 위험.
  **비용 대비 위험 나쁨 → 기각.**

### 3. 동등성 검증 기준(ground truth): 실게임 함수 직호출 골든 캡처
병행 구현은 기준점이 없으면 "두 번째 추측"일 뿐. 진짜 게임 로직을 한 번은 호출해 정답을 떠야 함.
- `GPMatchChecker`(완전 순수, 코루틴/트윈 0) 는 **EditMode 에서 직접 호출 가능** → 골든 캡처 1순위.
  단 `GPMatchChecker`/`GPBoard` 는 `Block` 인스턴스를 요구하므로, 골든 캡처용으로 `Block` 을
  **렌더 없이 생성**할 수 있는지(또는 최소 stub)부터 plan 에서 확인.
- `GPBombResolver` 는 콜백 주입형(`_createEffect`/`_playSound`/`_onBoomTrigger`) 이라 no-op 콜백으로
  골든 캡처 가능성 높음 — plan 에서 검증.
- 직호출이 끝내 불가능한 부분(보스 AI 등)은 **수작업 골든으로 폴백**.

### 4. 범위: 최종 목표 = 전부, 구현은 마일스톤 분할
사용자 선택 = 보스·특수블록까지 전부. 단 병행 재구현으로 한 번에 하면 MVP 과대 →
plan 에서 다음 마일스톤으로 분할(목표는 항상 "전부"):
- **M1** — 일반 매치(3·사각) + 낙하/리필 + 승패(시간/이동 제한). 노멀/하드 클리어율 메트릭. (최단 가치)
- **M2** — 폭탄 생성·연쇄 (Bomb1~12, 특수폭탄).
- **M3** — 특수블록(Wall/Potal/CatBox/Creator/Fish) + 보스 AI(GPBossController) → 보스 100 커버.

## 설계 세부 (plan 입력)

### AI 플레이어 정책
- 매치-3 결정 공간이 큼. **최소 2전략**: (a) 랜덤 유효 수, (b) 탐욕(가장 큰 매치/폭탄 우선).
- 유효 수 탐색은 기존 `GPMatchChecker.CanPlay()` 와 동일 의미를 시뮬 코어에 재구현.

### 메트릭 (출력 JSON)
- 스테이지별: 클리어율, 평균 이동 수(이동제한 모드), 평균 소요 시간/턴, 실패 사유 분포(EFailReason).
- 전략별 비교. (M2+) 폭탄 생성률·연쇄 길이. (M3+) 보스 스킬 발동 빈도.

### 배치 실행 진입점
- EditMode 우선 (project.md `test_paths.edit_mode`). 메뉴 `CatPang/Sim/Run Batch`(seed·N·전략·group 범위 파라미터).
- 결과 JSON 저장 → qa-simulator 가 읽어 `docs/qa-reports/` 에 해석 리포트 작성.

### 코드 위치 (제안 — plan 에서 확정)
- 시뮬 코어: `Assets/Scripts/Sim/` (신규, 게임 코드와 분리)
- 골든/배치: EditMode 테스트 + 에디터 메뉴

## plan 에서 먼저 풀 것 (전제 검증)
- [ ] `Block` 을 EditMode 에서 렌더 없이 생성 가능한가(골든 캡처 전제). 불가면 stub 설계.
- [ ] `GPBombResolver` no-op 콜백 골든 캡처 가능 범위 확정.
- [ ] M1 시뮬 코어 ↔ `GPMatchChecker` 골든 동등성 케이스 N개 목록.
