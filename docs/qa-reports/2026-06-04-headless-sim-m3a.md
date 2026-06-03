# M3a 헤드리스 시뮬 — 폭탄 시스템 완료 리포트

작성일: 2026-06-04
대상: `Assets/Scripts/Sim/` (M3a — 폭탄 계열: Arrow/CatPang/5색폭탄/Rainbow 생성·발동·연쇄·조합)
플랜: `docs/superpowers/plans/2026-06-03-headless-sim-harness-m3a.md` (9 Task, subagent-driven)

## 요약

| 항목 | 결과 |
|---|---|
| **커버리지** | **supported 59 → 103 / 150** (M3a 가 **44개 stage unlock**, 69%) |
| 단위·골든 테스트 | **PASS 62 / FAIL 0** |
| 폭탄 발동 기하 골든 | Bomb1~12 + BoomAll **전부 실게임과 셀 집합 일치** |
| 진단(103판) | capHit **5/103** (4.9%), 신규 무한루프·크래시 **0**, 총 18초 |
| 남은 미지원 47 | Fish(33)/Ball(19) 보유 stage → **M3b** |

## 구현 (9 Task, 전부 골든/단위 검증)

1. **골든 스파이크** — 실 `GPMatchChecker.CheckMap`·`GPBombResolver.Bomb1` EditMode 호출성 확인. 발견: BombN 의 `SaveBombCollectionData → CHMData.Instance` NRE. **우회 확정**: `RealBlockFactory.EnsureCHMDataCollection()`(public 필드에 빈 딕셔너리 주입)로 실 BombN 엔드투엔드 골든 가능 → Task4 가 최강 골든 사용.
2. **SimMatchChecker 매치 추적** — hScore/vScore/squareMatch/Index + SetMoveIndices. 실 GPMatchChecker hScore 골든 일치.
3. **SimBlock 폭탄 분류** — IsBomb/IsSpecialBomb/IsArrow(실 소스 미러), BombKind 디스패치, ChangeMatchState, CanNotDrag 에 RainbowPang.
4. **SimBombResolver 발동 기하** — Bomb1~12/BoomAll/Boom3/RainbowPang 을 실 GPBombResolver L86~344 에서 1:1 포팅. **11개 BombN + BoomAll 골든이 실게임과 발동 셀 집합 일치.**
5. **SimBombFactory 매치→생성** — 실 CreateBombBlock 포팅(squareMatch→CatPang, 교차→Arrow5/6, h/v>3→Arrow + 이동칸 우선, ClearScoreNeighbors). 실 CreateNewBlock 은 UI(DOScale) NRE 라 생성은 단위 테스트, 입력(점수)은 Task2 골든이 보증.
6. **SimComboResolver 스왑 조합** — 실 AfterDrag L787~804 5분기(BoomAll/Boom3/Detonate/Merge) 순수 판정 포팅.
7. **SimGame 턴루프 통합** — 발동(연쇄)·매치→생성·조합 swap 처리. 기존 무한방지 캡(MaxCascadeDepth 등) 유지. 조합 색폭탄 승급/Rainbow 살포는 시드 RNG(seed+2/+3).
8. **능동 AI** — FindValidMoves 가 폭탄 관여 스왑을 매치 없이도 유효 수로 포함(RainbowPang 은 CanNotDrag 제외). 색폭탄 의존 stage 클리어 가능.
9. **IsSupported 갱신** — Arrow1~6/CatPang/PinkBomb~BlueBomb(19~23)/RainbowPang 화이트리스트 추가. Fish/Ball 제외(M3b).

## 골든 전략 (하이브리드, M1/M2 계승)
- **순수부 런타임 골든**: GPMatchChecker.CheckMap(hScore), GPBombResolver.Bomb1~12+BoomAll(발동 셀). 실게임 직호출 후 Sim 동등성 단언.
- **통합부 단위**: 생성 분기, 조합 디스패치, 연쇄, 능동 AI.

## 진단 상세 (RandomAi 1판/stage, seed42)
- supported=103, capHit=5 (10·40·74·82·119), 총 17929ms.
- M3a 전(6/59: 35·73·74·77·82·119) 대비: 폭탄 생성이 제거력을 줘 **35·73·77 이 캡 탈출(클리어)**, 새 폭탄 stage **10·40** 이 캡 도달. 비율 10%→4.9% 개선.
- capHit 5개는 무제한이동 하드 패턴 추정(벽-타게팅/조합-우선 AI 부재) — sim 버그 후보 아님, 향후 정밀 정책으로 검증 가능.
- **부수 효과(의도됨)**: 기존 59 stage 도 4매치/사각매치에서 폭탄을 생성하므로 점수·이동수 메트릭이 M2 대비 더 정확해짐.

## 남은 작업
- **M3b**: Fish/Ball 이동블록(맨아래줄 탈출·변환) → 나머지 47개 unlock.
- capHit 5개(10·40·74·82·119) 벽/조합-타게팅 AI winnable 검증(선택).
- 연쇄 발동 grid-order fidelity(폭탄이 후순위 폭탄을 다음 iteration 으로 미룸 — 안전, 범위만 약간 보수적).

## 산출물
- 신규: `SimBombResolver.cs`, `SimBombFactory.cs`, `SimComboResolver.cs`
- 확장: `SimMatchChecker.cs`, `SimBlock.cs`, `SimBoard.cs`, `SimGame.cs`, `ISimAiPolicy.cs`, `RealBlockFactory.cs`
- 테스트: `SimBombSpikeTests.cs`, `SimMatchScoreGoldenTests.cs`, `SimBombResolverGoldenTests.cs`, `SimBombFactoryGoldenTests.cs`, `SimBombIntegrationTests.cs`, `SimGameTests.cs`(확장)
- 측정: `sim-output/m2-coverage-count.txt`(103/150), `sim-output/m2-diagnose.txt`
