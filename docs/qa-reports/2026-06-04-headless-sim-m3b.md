# M3b 헤드리스 시뮬 — Fish/Ball 이동블록 완료 리포트

작성일: 2026-06-04
대상: `Assets/Scripts/Sim/` (M3b — Fish/Ball 이동블록)
플랜: `docs/superpowers/plans/2026-06-04-headless-sim-harness-m3b.md` (4 Task, subagent-driven)

## 요약

| 항목 | 결과 |
|---|---|
| **커버리지** | **supported 103 → 150 / 150** (M3b 가 남은 **47개 Fish/Ball stage 전부 unlock** = 노멀모드 100%) |
| 단위·골든 테스트 | **PASS 68 / FAIL 0** |
| 진단(150판) | capHit **12/150** (8%), 신규 무한루프·크래시 **0**, 총 38초 |
| 보스(group≥100000) | M4 별도 |

## 구현 (4 Task, subagent-driven)

1. **SimBlock Fish/Ball 분류 + Fish 면역** — `IsFish()`/`IsBall()`, `CanNotDrag()` 에 Fish(드래그 불가), `ChangeMatchState` 에 Fish 제외(폭탄 면역). Ball 은 드래그·폭탄제거 가능(유지). 실 `Block.CanNotDragBlock`/`GPMatchChecker.ChangeMatchState` 미러.
2. **SimDisappearResolver** — 실 `GPGameScene.SetDissapearBlock` L691~725 포팅. 맨아래줄 Fish→색폭탄(시드 RNG), Ball→Potal(hp5)+좌우 확산(hp4↓, 우측 `ballHp<=0` 가드/좌측 무가드 비대칭 충실). **Ball 좌우 확산 hp 분포 결정적 골든**.
3. **SimGame 통합** — `_disappearRng`(seed+4), ResolveCascades settle 후 disappear+ApplyChanges 1회, `CheckClear` 에 Fish/Ball 목표블록 판정(hp 무관, 실 Update L190·L1005), `IsSupported` 에 Fish/Ball 추가.
4. **커버리지 재측정** — CountSupported **150/150**, Diagnose 무한루프 0.

## 메커니즘 (실게임 충실)
- **낙하**: Fish/Ball 은 `IsWallLike` 가 아니라 기존 `SimGravity` 가 매 턴 하강시킴(코드 변경 없음). 맨아래줄 도달 시 변환.
- **Fish**: 폭탄/매치/드래그 불가 → 유일 탈출 = 맨아래줄→색폭탄(Fish 목표 제거).
- **Ball**: 드래그·폭탄제거 가능, 또는 맨아래줄→Potal+확산(목표 전환, 이후 Potal 깨야 클리어).
- **승패**: Fish/Ball 은 hp 무관 목표블록 → 보드에 남으면 미클리어.

## 진단 상세 (RandomAi 1판/stage, seed42)
- supported=150, capHit=12 (10·33·40·53·56·59·67·74·82·84·119·150), 총 38101ms.
- M3a(5/103) 대비 신규 7개(33·53·56·59·67·84·150)는 Fish/Ball stage — RandomAi 가 Fish 를 맨아래줄에 보내거나 Ball→Potal→Potal제거를 캡 안에 못 끝냄. 하드 패턴(실게임도 Fish 는 벽 아래 갇히면 정체) — sim 버그 후보 아님, 벽/이동블록 타게팅 AI 로 winnable 검증 가능(선택).
- **신규 무한루프·크래시 0** — 통합 안전. disappear 는 settle 후 1회 스캔이라 새 cascade 미유발.

## 헤드리스 시뮬 하니스 전체 진척 (M1~M3b)
| 마일스톤 | 범위 | 커버리지 |
|---|---|---|
| M1 | 일반 매치+낙하+승패 | 4/150 측정 |
| M2 | 정적 특수블록 + 드래그규칙 | 59/150 |
| M3a | 폭탄 시스템(생성·발동·연쇄·조합) | 103/150 |
| **M3b** | **Fish/Ball 이동블록** | **150/150** (노멀모드 100%) |
| M4 (예정) | 보스 AI(실시간 타이머) | 보스 100 stage |

## 남은 작업
- **M4**: 보스 스테이지(group≥100000, 실시간 타이머 의존) — 별도 설계.
- capHit 12개 벽/이동블록 타게팅 AI winnable 검증(선택).
- 전체 클리어율 배치(9000판) — 이제 150 stage 전부 대상 가능.

## 산출물
- 신규: `SimDisappearResolver.cs`
- 확장: `SimBlock.cs`, `SimMatchChecker.cs`, `SimGame.cs`
- 테스트: `SimDisappearTests.cs`(신규), `SimGameTests.cs`(확장)
- 측정: `sim-output/m2-coverage-count.txt`(150/150), `sim-output/m2-diagnose.txt`
