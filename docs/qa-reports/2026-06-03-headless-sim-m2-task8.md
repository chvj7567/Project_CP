# M2 헤드리스 시뮬 — Task 8 배치 커버리지 검증 리포트

작성일: 2026-06-03
대상: `Assets/Scripts/Sim/` (M2 — 정적 특수블록 Wall/Potal/Creator/CatBox)

## 요약

| 항목 | 결과 |
|---|---|
| **무한루프 위기** | **해소** — 원인·수정 아래 |
| **커버리지 게이트 (목표 59/150)** | **달성: supported=59 / unsupported=91 / missing=0** |
| 결정적 메트릭(클리어/정당 MoveOver) stage | **53 / 59** |
| MaxTurns 캡 stage(무제한이동·RandomAi 미클리어) | 6 / 59 — 35·73·74·77·82·119 (sim 버그 아님) |
| **동결 wall 버그** | **해결** — 근본원인·수정 §5 |
| 단위 테스트 회귀 | PASS 33 / FAIL 0 (모든 수정 후 유지) |

## 1. 무한루프 위기 — 해소

- **근본 원인**: `SimGame.ResolveCascades`의 `while(true)` 연쇄 루프에 iteration 캡이 없었다. 메인 루프 가드(`iterations++`)는 ResolveCascades **밖**이라, 일단 연쇄 루프가 발산하면 제어가 메인으로 돌아오지 못해 가드가 영원히 안 걸렸다. → Unity 메인스레드 무한 점유.
- **수정**: `MaxCascadeDepth = 200` 추가 (정상 연쇄는 5~15회라 절대 안 잘림). 더불어 `MaxTurns=5000`, `MaxIterations=10000`, `MaxReshuffleStreak=50` 캡으로 모든 경로 종료 보장.
- **검증**: 진단 150판이 47초에 완주(이전엔 무한 행). 캡 도달 시 `SimResult.CapHit=true`로 표시.

## 2. 커버리지 게이트 — 59/150 달성

`SimGame.IsSupported`(시간모드 아님 + 블록타입 스캔, play 없는 순수 사전분류)로 stage 1~150 집계.
- 결과 파일: `sim-output/m2-coverage-count.txt`
- supported 59개는 StageBlock.json 독립 추정과 일치.

## 3. 53/59 깨끗한 메트릭

`sim-output/m2-diagnose.txt` — supported 59개 각 1판(RandomAi). 53개가 캡 미도달로 신뢰 가능한 turns/moves/score/클리어 메트릭 생산. 전체 47초.

## 4. 6개 CapHit stage — 분석

전부 **moveCount=-1(무제한 이동) + 작은 목표점수 Creator stage**. RandomAi가 점수(16000+)는 쌓지만 목표블록(Wall)을 캡 안에 다 못 없애 grind.

- **Creator는 정상적으로 죽는다** — census에서 `Creator hp>0(활성)`이 거의 0 (stage82: 활성0/소진18, stage119: 활성0/소진9). 초기 "Creator 불멸" 가설은 **반증됨**. Sim `RunCreators` ≈ 실게임 `GPBombResolver.BlockCreatorBlock` (변환마다 creator 1데미지, hp0이면 스킵).

## 5. 발견·수정한 버그 + 잔존 버그

### 수정: SimGravity 전환예약 유실 (`CompactSegment`)
- 블록 이동·리필 시 `State`/`Hp`만 복사하고 `ChangeBlockState`/`ChangeHp`를 함께 옮기지·비우지 않아, 전환 대기 블록이 이동 시 예약을 잃거나 리필 칸이 잔존 예약을 물어 유령블록 발생.
- 수정: 이동 시 전환예약 동행 + 리필 칸 예약 초기화. 회귀 PASS 33 유지. 일부 stage 동결 wall 감소(74: 4→3, 129: 4→1).

### 해결: hp≤0 Wall 동결 — 근본원인 = 고정블록 불법 스왑
- **증상**: stage82(11개)·stage119(45개) 등에 Hp≤0인데 Wall 상태로 남은 블록. gravity 수정으로 안 바뀜(byte-identical).
- **진단 체인**: 동결 wall 전부 `ChangeBlockState==None` → Damage 경로 의심 → 해당 stage 초기 wall 0개(전부 Creator) → 동결 수 ∝ WallCreator 수 → **국소화: WallCreator가 만든 wall의 스왑**.
- **근본원인**: `SimBoard.Swap` 은 두 칸의 `State` 만 교환하고 `Hp`/`ChangeBlockState` 는 칸에 남긴다. 그런데 `SimMatchChecker.CanPlay` 와 AI 정책(`SimMoveFinder.FindValidMoves`)이 **고정블록(Wall 등)까지 스왑 후보로** 삼았다. WallCreator-생성 Wall(Hp=1)을 인접 cat 과 스왑하면 → Wall **State** 만 cat 칸으로 가고 **Hp=1 은 원래 칸에 잔류** → 도착 칸이 `State=Wall, Hp=-1, ChangeBlockState=None` = 정확히 동결 wall.
- **실게임 대조**: 실게임 `Block.CanNotDragBlock()` = Wall/Potal/Fish/CatBox/Creator/RainbowPang 은 드래그 불가. `GPMatchChecker` CanPlay 는 스왑 시 **양쪽 칸 모두 `!CanNotDragBlock()`** 확인(L168·188·193·198·203). Sim 엔 이 제약이 없었다.
- **수정**: `SimBlock.CanNotDrag()`(M2: Wall/Potal/CatBox/Creator) 추가. `FindValidMoves` 와 `CanPlay` 에서 **양쪽 칸 모두 드래그 가능할 때만** 스왑. → 6개 stage 전부 동결 wall **0** 확인, 전체 진단 소요 47초→17초(2.7×), 회귀 PASS 33 유지.
- **부수 교정**: stage35 가 수정 전 clear=True(345턴)였으나 수정 후 미클리어 — 예전 "클리어"는 불법 wall-스왑으로 벽을 비켜 친 **가짜 클리어**였다. 즉 이 버그는 일부 clean stage 메트릭도 오염시키고 있었고(advisor 경고대로) 수정으로 바로잡힘.

## 6. 최종 6개 MaxTurns 캡 stage — 정상(버그 아님)
35·73·74·77·82·119. 전부 moveCount=-1(무제한이동) + 점수목표 작음(175~400). RandomAi 가 점수(17000~19000)는 도달하나 **살아있는 Wall(hp>0 6~9개)을 5000턴 안에 다 못 없애** grind. Creator 는 전부 사망(활성=0), 동결 wall 0. → 숙련 플레이/Greedy 로 winnable 한지(AI 강도)의 문제이지 sim 정확성 문제 아님. 메트릭 "RandomAi 는 캡 내 미클리어"는 유효한 측정.

## 7. 남은 작업
1. **전체 클리어율 배치** — 캡 stage가 판당 1.5~4.5초로 빨라져(동결 제거) 9000판도 현실적. 단 6개 MaxTurns 캡 stage는 Greedy 로도 검증 권장.
2. **6개 stage Greedy winnable 검증** — 숙련 정책이 클리어하면 "하드 stage"로 확정.
3. **NUnit 교차검증** — 자작 SimTestRunner 결과를 Test Runner 수동 클릭과 대조.

## 산출물

- `sim-output/m2-coverage-count.txt` — 59/150 분류
- `sim-output/m2-diagnose.txt` — 59개 메트릭 + CapHit 식별
- `sim-output/m2-caphitter-diagnose.txt` — 6개 census(Creator 사망·Wall hp버킷·전환예약 분포)
