# 헤드리스 시뮬 하니스 M2 (정적 특수블록) — 설계 (spec)

> 상태: **설계 확정 대기 (사용자 리뷰 게이트).** brainstorming 결정 락 완료.
> 선행: M1 (일반매치+낙하/리필+승패) 완료·커밋됨. M2 는 M1 시뮬 코어를 확장한다.

## 목표

M1 은 일반 고양이 블록(Cat1~7)만 처리해 stage 1~150 중 **4개만** 측정 가능했다(146개는 특수블록 포함으로 Unsupported). M2 는 **폭탄과 무관한 정적 특수블록**(Wall/Potal/Creator/CatBox)을 추가해 측정 가능 범위를 넓힌다. 폭탄 의존 요소(Fish/Ball/Arrow/특수폭탄/Rainbow)는 **M3** 로 미룬다.

## 범위 (brainstorming 확정)

### M2 대상 (폭탄 무관)
- **Wall** (16) — 매치 안 됨, 낙하 차단, 인접 매치로 데미지받아 제거
- **Potal** (17) — HP 블록, 데미지로 제거
- **WallCreator** (45) / **PotalCreator** (46) — 매 턴 주변 일반블록을 Wall/Potal 로 변환, 자신은 데미지로 HP 감소
- **CatBox1~5** (40~44) — 위 칸이 받는 고양이면 HP 감소 + 위 블록 수집 제거

### M2 비대상 (→ M3)
폭탄 연쇄 12종, Fish(24, 폭탄 변환), Ball(53, Potal 변환), RainbowPang(52), 초기배치 Arrow(10~15)/특수폭탄(19~23), 폭탄 생성(4매치→Arrow, 5/사각→특수폭탄).

### 범위 밖 (→ M4)
보스 스테이지 100개(group≥100000). `GPBossController` 가 `Observable.Timer` 로 HP 자동감소+스킬 발동 = **실시간 의존**이라 헤드리스 정확 시뮬 불가. 별도 시간모델 설계 필요.

## 확정 결정 (락)

### 1. HP / 데미지 모델 (실게임 정독 — Block.Damage / GPMatchChecker)
- `SimBlock` 에 `int Hp` 추가 (M1 엔 없었음). `bool CheckHp` 추가(목표블록 판정용).
- **Damage** (`Block.Damage` 409-429 포팅): 턴당 1회 가드(`checkDamage`) + HP≥1 + **박스 아님** 일 때만 HP-1. HP→0 이면 `ChangeBlockState`=랜덤 일반블록(`Random(0, blockTypeCount)`). 박스(CatBox)는 Damage 대상 아님 — CatInTheBox 로만 감소.
- **인접 데미지** (`GPMatchChecker.CheckArround/DamageBlock` 223-241): 매치 제거 블록의 **상하좌우** 4칸을 Damage. RainbowPang 은 changeNormalBlock=false 로 Damage(M3). M2 엔 Rainbow 없으므로 일반 Damage 만.
- **checkDamage 리셋**: 매 CheckMap 시작 시 전 칸 `ResetCheckWallDamage`(`GPMatchChecker.CheckMap` 42-44). SimMatchChecker.CheckMap 에 동일 추가.

### 2. CatBox 수집 (실게임 Block.CatInTheBox / CheckInBoxBlock 636-682)
- 박스 **위 칸**(row-1) 블록을 `GetBaseCat` 으로 원본 고양이 환산 → 박스가 받는 고양이면(CatBox1=Cat1/Cat6, CatBox2=Cat2/Cat7, CatBox3=Cat3, CatBox4=Cat4, CatBox5=Cat5) 박스 HP-1 + **위 블록 제거(match=true)**.
- 매 턴 루프에서 `CatInTheBox` 스캔(`GPGameScene.CatInTheBox` 727-754) → 수집 발생하면 추가 연쇄.

### 3. Creator 블록 (실게임 GPBombResolver.BlockCreatorBlock 462-492)
- WallCreator/PotalCreator: 매 턴 자신 주변(랜덤 방향) 일반블록 1개를 Wall/Potal 로 변환(`changeBlockState`), 자신 Damage. HP 0 이면 중단.
- AfterDrag 턴 루프에서 1회 호출(`checkCreateBlock` 가드).

### 4. 상태 전이 적용 (실게임 GPGameScene.UpdateMap 565-617)
- 매 턴 `changeBlockState != None` 인 칸을 그 상태로 전환 + `SetHp(changeHp)`. M1 SimGravity 가 이미 매치 제거+리필하므로, **changeBlockState/changeHp 적용 단계를 턴 루프에 추가**.

### 5. 승패 조건 — **목표블록 제거 AND 점수** (실게임 Update 177-206, 사용자 정정)
M1 의 "점수만" 은 목표블록 0개인 특수 케이스였다. M2 일반화:
```
CheckClear(board, score, s):
    //# 1. 목표블록 제거 — checkHp 칸 중 HP>0 / Fish / Ball 남으면 미클리어
    foreach 칸:
        if state == RainbowPang: continue
        if CheckHp == false: continue          // Creator 등 목표 아님
        if Hp > 0 or IsFish or IsBall: return false
    //# 2. 점수 (목표 있으면)
    if TargetScore > 0 and score < TargetScore: return false
    return true                                 // 둘 다(AND) 만족
```
- M2 엔 Fish/Ball 없으므로 1단계는 HP>0 검사만 실질 동작. (Fish/Ball 검사 코드는 M3 대비 미리 둠)
- `CheckHp` = 실게임 `CheckHpBlock`: WallCreator/PotalCreator 는 false(목표 아님), 나머지 특수블록 true.

### 6. 낙하 차단 (실게임 GPBoard.DownBlock — Wall 위 블록 낙하 막음)
- M1 SimGravity 는 단순 중력. M2 는 **Wall(및 IsWallBlock 류: CatBox/Creator 포함)이 낙하를 차단** — 각 열에서 Wall 아래로는 위 블록이 안 떨어진다. `GPBoard.DownBlock` 100-106 의 wallRow 로직 포팅.

## 검증 방식 (M1 그대로 유지)
- **자동 테스트 러너** `SimTestRunner` 재사용 (plain `[Test]`, MCP editor_invoke_method 자동 실행).
- **실게임 직호출 골든**: M2 의 핵심 로직을 EditMode 에서 실게임 직접 호출해 정답 캡처 → Sim 동등성 assert.
  - Damage/인접데미지: 실 `GPMatchChecker.CheckArround` + 실 `Block`(HP 세팅) 직호출 → HP 변화 골든.
  - CatBox: 실 `Block.CatInTheBox` 직호출 → 수집 판정 골든.
  - (Creator 는 내부 `UnityEngine.Random` 방향 의존 → 결정적 골든 어려움 → **수작업 골든 폴백**, spec 명시.)
- **배치 러너**: M2 후 stage 1~150 재측정. **목표 커버리지(StageBlock.json 직접 카운트, 2026-06-03 실측): M1 4개 → M2 59개**(+55). 나머지 91개는 M3 전용 블록(폭탄/Fish/Ball/Arrow/Rainbow) 포함 → M3 필요. M2 완료 시 배치 결과 supported=59 가 검증 기준.

### 검증 신뢰 경계 (리포트 명기)
- 실게임 골든: Damage/CatInTheBox.
- 수작업 골든(self-referential): Creator(랜덤 방향), 승패 AND 로직, 낙하 차단, 턴 루프 통합.

## 파일 구조 (제안 — plan 에서 확정)
- 수정: `SimBlock.cs`(Hp/CheckHp 추가), `SimBoard.cs`(Wall 차단 헬퍼), `SimMatchChecker.cs`(ResetCheckWallDamage + CheckArround/Damage), `SimGravity.cs`(Wall 낙하 차단), `SimGame.cs`(턴 루프에 Damage/CatBox/Creator/상태전이 + CheckClear AND 로직), `SimStageData.cs`(블록별 hp 로드 — StageBlock.json 의 hp 필드)
- 신규: `SimSpecialBlocks.cs`(또는 SimGame 내 — Damage/CatBox/Creator 로직 응집), 골든 테스트 `SimSpecialBlockGoldenTests.cs`
- `IsSupported` 갱신: Wall/Potal/Creator/CatBox 를 supported 에 추가(Fish/Ball/폭탄/Arrow/Rainbow 는 여전히 Unsupported → M3).

## plan 에서 먼저 풀 것 (전제 검증)
- [ ] StageBlock.json 의 `hp` 필드가 블록별로 어떻게 들어오나 — SimStageData.InitialStates 에 HP 동반 로드 설계.
- [ ] 실 `Block` 을 HP 세팅한 채 EditMode 생성 가능한지(M1 RealBlockFactory 확장) — Damage 골든 전제.
- [ ] M2 대상 블록만 쓰는 stage 가 실제로 몇 개나 supported 로 바뀌나(StageBlock.json 직접 카운트 — M1 의 146 분석 재활용).
