# 헤드리스 시뮬 하니스 M3b (Fish/Ball 이동블록) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fish/Ball 이동블록(맨아래줄 도달 변환·목표블록 판정)을 헤드리스 Sim 에 추가해 남은 47개 stage 를 unlock → 노멀모드 150/150.

**Architecture:** M3a 토대(`Assets/Scripts/Sim/`) 확장. 실 `GPGameScene.SetDissapearBlock` 의 맨아래줄 변환 로직을 `SimDisappearResolver` 로 포팅(비순수 메서드라 포팅+단위, Ball 확산은 결정적이라 강한 단위 골든). 낙하는 기존 `SimGravity` 재사용(Fish/Ball 은 IsWallLike 가 아니라 이미 하강).

**Tech Stack:** Unity 6, C#, NUnit EditMode, 자작 `SimTestRunner`.

**규약:** Rule 01(자동 `git commit` 금지 — staging + 한글 메시지). Rule 02(`//#` 주석, `var` 금지, `!` 금지 → `== false`/`== null`, 가드절 중괄호 없이 개행). plain 무인자 `[Test]`. `.meta` 는 Unity 생성.

**실행 모델:** 서브에이전트(gameplay-programmer)는 MCP 가 없어 컴파일/테스트 실행을 못 한다 → 코드+테스트 작성 + `git add` 까지만. 컴파일·`SimTestRunner` 실행·진단은 오케스트레이터가 수행. 회귀 기준선: **PASS 62 / FAIL 0**.

**참조 (실게임 — 골든 원천):**
- `Assets/Scripts/Scenes/GPGameScene.cs` L691~725 `SetDissapearBlock` — Fish→`Random(PinkBomb..BlueBomb)`, Ball→`Potal`(hp `PortalBlockHp=5`)+좌우 확산(hp `BallPortalStartHp=4`↓, 비일반 정지; 우측은 `ballHp<=0` break, 좌측 없음). 상수 L36~39.
- `Assets/Scripts/GamePlay/GPMatchChecker.cs` L248~252 `ChangeMatchState` — `!IsFishBlock() && !IsFixdBlock() && != PinkBomb` 이면 match(즉 Fish 제외, Ball 포함).
- `Assets/Scripts/Block.cs` `CanNotDragBlock`(L553, Fish 포함/Ball 미포함), `IsFishBlock`/`IsBallBlock`.
- 승패: `GPGameScene.cs` L190·L1005 — `GetHp()>0 || IsFishBlock() || IsBallBlock()` 이면 미클리어.

**현재 Sim 구조 (통합 지점):**
- `SimGame.ResolveCascades(board, checker, gravity, s, ref result, mi1, mi2)` — cascade 루프. 호출처: 일반수(L275), 리셔플(L190), 조합 self-consume.
- `SimGame.CheckClear(board, s, score)` (L563) — 현재: RainbowPang skip, `CheckHp()==false`(Creator) skip, `Hp>0` 이면 미클리어. **Fish/Ball 판정 자리 주석 L575 존재.**
- `SimGame` RNG: `_creatorRng`(seed+1), `_bombResolver`(seed+2), `_comboRng`(seed+3). 신규 `_disappearRng`(seed+4).
- `SimBlock`: `IsWall/IsCatBox/IsCreator/IsWallLike/CanNotDrag/IsNormal/IsBomb/IsSpecialBomb/CheckHp`, `ChangeBlockState/ChangeHp`, `Hp`.
- `SimSpecialBlocks.ApplyChanges(board)` — ChangeBlockState 예약을 보드에 적용.

---

## File Structure

| 파일 | 변경 | 책임 |
|---|---|---|
| `Assets/Scripts/Sim/SimBlock.cs` | 수정 | `IsFish()`/`IsBall()`, `CanNotDrag()` 에 Fish 추가 |
| `Assets/Scripts/Sim/SimMatchChecker.cs` | 수정 | `ChangeMatchState` 에 Fish 제외 |
| `Assets/Scripts/Sim/SimDisappearResolver.cs` | 신규 | 맨아래줄 Fish→색폭탄(시드)·Ball→Potal+확산 (실 SetDissapearBlock 포팅) |
| `Assets/Scripts/Sim/SimGame.cs` | 수정 | disappear 호출(ResolveCascades settle 후), CheckClear Fish/Ball, IsSupported Fish/Ball, `_disappearRng` |
| `Assets/Tests/EditMode/Sim/SimDisappearTests.cs` | 신규 | Fish/Ball 단위·확산 골든 |
| `Assets/Tests/EditMode/Sim/SimGameTests.cs` | 수정 | IsSupported Fish/Ball, CheckClear, 낙하 통합 |

---

## Task 1: SimBlock Fish/Ball 분류 + 드래그/폭탄 면역

**Files:**
- Modify: `Assets/Scripts/Sim/SimBlock.cs`
- Modify: `Assets/Scripts/Sim/SimMatchChecker.cs`
- Test: `Assets/Tests/EditMode/Sim/SimDisappearTests.cs` (신규)

- [ ] **Step 1: SimBlock 에 IsFish/IsBall + CanNotDrag Fish**

`SimBlock.cs` 에 분류 추가(기존 IsWall 등 옆):
```csharp
public bool IsFish()
{
    return State == EBlockState.Fish;
}
public bool IsBall()
{
    return State == EBlockState.Ball;
}
```
`CanNotDrag()` 에 Fish 추가(Ball 은 추가 안 함 — 드래그 가능). 현재:
```csharp
public bool CanNotDrag()
{
    return IsWall() || State == EBlockState.Potal || IsCatBox() || IsCreator()
        || State == EBlockState.RainbowPang;
}
```
→ `|| IsFish()` 추가:
```csharp
public bool CanNotDrag()
{
    return IsWall() || State == EBlockState.Potal || IsCatBox() || IsCreator()
        || State == EBlockState.RainbowPang || IsFish();
}
```

- [ ] **Step 2: SimMatchChecker.ChangeMatchState 에 Fish 제외**

실 ChangeMatchState 는 Fish 를 match 에서 제외(폭탄 제거 불가). 현재 Sim ChangeMatchState 의 제외 조건(RainbowPang hp>0 / 고정블록 / PinkBomb) 에 Fish 추가:
```csharp
public void ChangeMatchState(SimBoard board, int row, int col)
{
    if (board.IsValid(row, col) == false)
        return;
    SimBlock b = board.Grid[row, col];
    if (b.State == EBlockState.RainbowPang && b.Hp > 0)
        return;
    if (b.IsWall() || b.State == EBlockState.Potal || b.IsCatBox() || b.IsCreator())
        return;
    if (b.IsFish())
        return;
    if (b.State == EBlockState.PinkBomb)
        return;
    b.Match = true;
}
```
(현재 ChangeMatchState 의 정확한 형태를 읽고 Fish 가드만 추가. Ball 은 제외 안 함 — Ball 은 폭탄으로 제거 가능.)

- [ ] **Step 3: 단위 테스트 — Fish 면역 / Ball 제거가능 / Fish 드래그불가**

`SimDisappearTests.cs` 생성:
```csharp
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimDisappearTests
    {
        [Test]
        public void Fish는_폭탄범위에서_제거안됨_Ball은_제거됨()
        {
            SimBoard b = new SimBoard(3);
            b.SetState(1, 1, EBlockState.Cat1);
            b.SetState(0, 1, EBlockState.Fish);
            b.SetState(1, 0, EBlockState.Ball);
            SimMatchChecker m = new SimMatchChecker();
            m.ChangeMatchState(b, 0, 1); //# Fish
            m.ChangeMatchState(b, 1, 0); //# Ball
            Assert.IsFalse(b.Grid[0, 1].Match, "Fish 는 폭탄으로 제거 불가");
            Assert.IsTrue(b.Grid[1, 0].Match, "Ball 은 폭탄으로 제거 가능");
        }

        [Test]
        public void Fish는_드래그불가_Ball은_드래그가능()
        {
            Assert.IsTrue(new SimBlock { State = EBlockState.Fish }.CanNotDrag(), "Fish 드래그 불가");
            Assert.IsFalse(new SimBlock { State = EBlockState.Ball }.CanNotDrag(), "Ball 드래그 가능");
        }
    }
}
```

- [ ] **Step 4: 컴파일+실행(오케스트레이터). 스테이징.**

기대 PASS 64. 스테이징: `git add Assets/Scripts/Sim/SimBlock.cs Assets/Scripts/Sim/SimMatchChecker.cs Assets/Tests/EditMode/Sim/SimDisappearTests.cs`(+신규 .meta).
메시지: `# [feat] - Sim M3b: SimBlock Fish/Ball 분류 + Fish 드래그/폭탄 면역`

---

## Task 2: SimDisappearResolver — 맨아래줄 변환

**Files:**
- Create: `Assets/Scripts/Sim/SimDisappearResolver.cs`
- Test: `Assets/Tests/EditMode/Sim/SimDisappearTests.cs` (확장)

- [ ] **Step 1: SimDisappearResolver 구현 (실 SetDissapearBlock L691~725 포팅)**

맨아래줄(row=Size-1) 스캔: Fish→색폭탄 예약(시드 RNG), Ball→Potal(hp5)+좌우 확산 예약. ChangeBlockState/ChangeHp 예약만 하고 적용은 호출측 ApplyChanges.
```csharp
using static Defines;

namespace CatPang.Sim
{
    //# 실 GPGameScene.SetDissapearBlock(L691~725) 포팅: 맨아래줄 Fish/Ball 변환 예약.
    //# Fish → 색폭탄(PinkBomb..BlueBomb, 시드 RNG). Ball → Potal(hp5) + 좌우 일반블록 Potal 확산(hp4↓).
    //# 예약만(ChangeBlockState/ChangeHp), 적용은 SimSpecialBlocks.ApplyChanges.
    public static class SimDisappearResolver
    {
        private const int PortalBlockHp = 5;     //# 실 GPGameScene.PortalBlockHp
        private const int BallPortalStartHp = 4; //# 실 GPGameScene.BallPortalStartHp

        public static void Resolve(SimBoard board, System.Random rng)
        {
            int row = board.Size - 1;
            for (int i = 0; i < board.Size; ++i)
            {
                SimBlock block = board.Grid[row, i];
                if (block.IsFish())
                {
                    //# 실 L700: Random(PinkBomb, BlueBomb+1). PinkBomb=19..BlueBomb=23.
                    block.ChangeBlockState = (EBlockState)rng.Next((int)EBlockState.PinkBomb, (int)EBlockState.BlueBomb + 1);
                }
                else if (block.IsBall())
                {
                    block.ChangeBlockState = EBlockState.Potal;
                    block.ChangeHp = PortalBlockHp;

                    //# 우측 확산 — hp4 부터 1씩 감소, ballHp<=0 또는 비일반(미제거) 만나면 정지.
                    int ballHp = BallPortalStartHp;
                    for (int k = i + 1; k < board.Size; ++k)
                    {
                        SimBlock cb = board.Grid[row, k];
                        if (ballHp <= 0)
                            break;
                        if (cb.IsNormal() || cb.Match)
                        {
                            cb.ChangeBlockState = EBlockState.Potal;
                            cb.ChangeHp = ballHp;
                            ballHp -= 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    //# 좌측 확산 — 실게임은 ballHp<=0 가드 없음(좌측은 hp 0/음수 Potal 가능). 충실 포팅.
                    ballHp = BallPortalStartHp;
                    for (int k = i - 1; k >= 0; --k)
                    {
                        SimBlock cb = board.Grid[row, k];
                        if (cb.IsNormal() || cb.Match)
                        {
                            cb.ChangeBlockState = EBlockState.Potal;
                            cb.ChangeHp = ballHp;
                            ballHp -= 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
```
(실 L713 은 `cb.changeHp = ballHp--` — 후위 감소라 현재 ballHp 를 쓰고 감소. 위 코드는 `ChangeHp = ballHp; ballHp -= 1;` 로 동일. 실 `cb.remove` 는 Sim `Match`(그 턴 제거 예정)에 대응.)

- [ ] **Step 2: 단위 골든 — Ball 좌우 확산 hp 분포**

`SimDisappearTests.cs` 에 추가:
```csharp
        [Test]
        public void Ball_맨아래줄_Potal과_좌우확산_hp분포()
        {
            //# 1x7 맨아래줄: [Cat Cat Cat Ball Cat Cat Cat] (row=0, Size=7 단일행 보드로 단순화 불가 → Size=2 보드의 아래줄 사용)
            SimBoard b = new SimBoard(7);
            int row = b.Size - 1;
            //# 위쪽 줄은 매치 안 생기게 임의 채움.
            for (int c = 0; c < 7; ++c)
            {
                for (int r = 0; r < 6; ++r)
                {
                    b.SetState(r, c, (EBlockState)(c % 3));
                }
            }
            //# 아래줄: 가운데(3) Ball, 양옆 일반.
            for (int c = 0; c < 7; ++c)
            {
                b.SetState(row, c, EBlockState.Cat1);
            }
            b.SetState(row, 3, EBlockState.Ball);

            SimDisappearResolver.Resolve(b, new System.Random(1));

            //# Ball 칸: Potal hp5
            Assert.AreEqual(EBlockState.Potal, b.Grid[row, 3].ChangeBlockState, "Ball→Potal");
            Assert.AreEqual(5, b.Grid[row, 3].ChangeHp, "Ball Potal hp5");
            //# 우측 4,5,6: hp 4,3,2
            Assert.AreEqual(4, b.Grid[row, 4].ChangeHp, "우측1 hp4");
            Assert.AreEqual(3, b.Grid[row, 5].ChangeHp, "우측2 hp3");
            Assert.AreEqual(2, b.Grid[row, 6].ChangeHp, "우측3 hp2");
            //# 좌측 2,1,0: hp 4,3,2
            Assert.AreEqual(4, b.Grid[row, 2].ChangeHp, "좌측1 hp4");
            Assert.AreEqual(3, b.Grid[row, 1].ChangeHp, "좌측2 hp3");
            Assert.AreEqual(2, b.Grid[row, 0].ChangeHp, "좌측3 hp2");
        }

        [Test]
        public void Fish_맨아래줄_색폭탄으로_변환예약_시드결정적()
        {
            SimBoard b1 = new SimBoard(3);
            b1.SetState(2, 1, EBlockState.Fish);
            SimDisappearResolver.Resolve(b1, new System.Random(42));
            EBlockState conv = b1.Grid[2, 1].ChangeBlockState;
            Assert.IsTrue(conv >= EBlockState.PinkBomb && conv <= EBlockState.BlueBomb, "Fish→색폭탄(19~23)");

            //# 동일 시드 → 동일 결과(결정성).
            SimBoard b2 = new SimBoard(3);
            b2.SetState(2, 1, EBlockState.Fish);
            SimDisappearResolver.Resolve(b2, new System.Random(42));
            Assert.AreEqual(conv, b2.Grid[2, 1].ChangeBlockState, "시드 동일 → 변환 동일");
        }
```
(Ball 확산 테스트의 아래줄은 전부 Cat1 이라 우측 확산이 7칸 경계까지: i=3, 우측 k=4(hp4),5(hp3),6(hp2) 후 경계. 좌측 k=2(hp4),1(hp3),0(hp2) 후 경계. ballHp 가 0 이 되기 전 경계 도달. 검증값은 그에 맞춤.)

- [ ] **Step 3: 컴파일+실행(오케스트레이터). 스테이징.**

기대 PASS 66. 스테이징: `git add Assets/Scripts/Sim/SimDisappearResolver.cs Assets/Tests/EditMode/Sim/SimDisappearTests.cs`(+신규 .meta).
메시지: `# [feat] - Sim M3b: SimDisappearResolver 맨아래줄 Fish/Ball 변환`

---

## Task 3: SimGame 통합 — 턴루프 disappear + CheckClear + IsSupported

**Files:**
- Modify: `Assets/Scripts/Sim/SimGame.cs`
- Test: `Assets/Tests/EditMode/Sim/SimGameTests.cs` (확장)

- [ ] **Step 1: `_disappearRng` 필드 + Run 주입**

`_comboRng` 옆에:
```csharp
//# M3b: Fish 색폭탄 변환 RNG. seed+4 로 분리.
private System.Random _disappearRng;
```
Run 의 RNG 주입부(`_comboRng = new System.Random(seed + 3);` 다음):
```csharp
_disappearRng = new System.Random(seed + 4);
```

- [ ] **Step 2: ResolveCascades settle 후 disappear 1회**

`ResolveCascades` 의 cascade 루프가 끝난 직후(보드 settle, return 직전)에 disappear + ApplyChanges 추가. ResolveCascades 끝부분을 읽고 cascade while 루프 종료 후에:
```csharp
//# M3b: cascade settle 후 맨아래줄 Fish/Ball 변환(실 AfterDrag 의 SetDissapearBlock).
//# 변환은 예약 → ApplyChanges 로 적용. 변환된 색폭탄/Potal 은 자동 매치 안 되므로 추가 cascade 불필요.
SimDisappearResolver.Resolve(board, _disappearRng);
SimSpecialBlocks.ApplyChanges(board);
```
(ResolveCascades 가 `private void` 이고 `_disappearRng` 는 인스턴스 필드라 접근 가능. 리셔플 경로/조합 경로도 ResolveCascades 를 거치므로 일괄 적용된다.)

- [ ] **Step 3: CheckClear 에 Fish/Ball 목표블록 판정**

`CheckClear`(L563) 의 목표블록 루프에서 Fish/Ball 을 hp 무관 목표로. 현재 L575~577:
```csharp
//# M3 대비 Fish/Ball 검사 자리(M2 엔 해당 블록 없음).
if (b.Hp > 0)
    return false;
```
→ Fish/Ball 추가:
```csharp
//# M3b: Fish/Ball 은 hp 무관 목표블록(실 Update L190·L1005). 보드에 남으면 미클리어.
if (b.IsFish() || b.IsBall())
    return false;
if (b.Hp > 0)
    return false;
```

- [ ] **Step 4: IsSupported 에 Fish/Ball 추가**

`IsSupported`(L62) 의 허용 블록 스캔에 추가(RainbowPang continue 다음, `return false` 전):
```csharp
//# M3b: Fish/Ball 이동블록 지원.
if (st == EBlockState.Fish || st == EBlockState.Ball)
{
    continue;
}
```

- [ ] **Step 5: 통합 테스트 — Fish/Ball 낙하·변환·승패**

`SimGameTests.cs` 에 추가(상단 StageJson/StageBlockJson 프로퍼티 재사용):
```csharp
        [Test]
        public void IsSupported_Fish와_Ball_stage가_지원된다()
        {
            //# stage31(초기 Fish 보유) → 이제 supported. stage131(초기 Ball 보유) → supported.
            SimStageData fishStage = SimStageLoader.Load(StageJson, StageBlockJson, 31, normalMode: true);
            Assert.IsFalse(new SimGame().Run(fishStage, new RandomAiPolicy(1), 1).Unsupported, "Fish stage 지원(M3b)");
            SimStageData ballStage = SimStageLoader.Load(StageJson, StageBlockJson, 131, normalMode: true);
            Assert.IsFalse(new SimGame().Run(ballStage, new RandomAiPolicy(1), 1).Unsupported, "Ball stage 지원(M3b)");
        }

        [Test]
        public void 낙하_Fish가_매턴_하강해_맨아래줄에서_색폭탄변환()
        {
            //# 3x3, (0,1) Fish, 나머지 일반(매치 안 생기게). gravity 한 번에 Fish 가 한 칸 하강.
            //# 직접 gravity 호출로 낙하만 검증(전체 Run 은 비결정 매치라 화이트박스로).
            SimBoard b = new SimBoard(3);
            b.SetState(0, 1, EBlockState.Fish);
            b.SetState(1, 1, EBlockState.Cat1);
            b.SetState(2, 1, EBlockState.Cat2);
            b.Grid[2, 1].Match = true; //# 맨아래 칸 제거 → Fish 가 한 칸 하강
            new SimGravity(1).Apply(b, blockTypeCount: 3);
            //# Fish 가 (0,1)→(1,1) 로 하강(맨아래 제거로 위가 내려옴).
            Assert.AreEqual(EBlockState.Fish, b.GetState(1, 1), "Fish 가 한 칸 하강");
        }
```
(낙하 테스트는 Fish 가 IsWallLike 가 아니라 gravity 로 하강함을 확인. 맨아래줄 도달 변환은 Task2 단위가 커버.)

- [ ] **Step 6: 컴파일+실행(오케스트레이터). 회귀 PASS 유지. 스테이징.**

기대 PASS 68. 스테이징: `git add Assets/Scripts/Sim/SimGame.cs Assets/Tests/EditMode/Sim/SimGameTests.cs`.
메시지: `# [feat] - Sim M3b: SimGame 턴루프 Fish/Ball 통합 + CheckClear + IsSupported`

---

## Task 4: 커버리지 재측정 + 진단 (오케스트레이터)

**Files:**
- 측정만 (코드 변경 없음). MCP 로 실행.

- [ ] **Step 1: 컴파일 확인 후 `CountSupported` 실행**

`editor_invoke_method` → `CatPang.Sim.EditorTools.SimBatchRunner.CountSupported`. 결과 `docs/qa-reports/sim-output/m2-coverage-count.txt`. **기대: supported=150 / unsupported=0**(보스 제외 노멀모드 전체). 103 → 150 증가분 확인.

- [ ] **Step 2: `Diagnose` 실행 — 150판 무한루프·크래시 0**

`editor_invoke_method` → `SimBatchRunner.Diagnose`. heartbeat 폴링으로 완주 확인. `m2-diagnose.txt` 에서 capHit 수·신규 무한루프 유무 점검. Fish/Ball stage 가 종료(클리어 or 정당 MoveOver)하는지 확인.

- [ ] **Step 3: M3b 리포트 작성 + 스테이징**

`docs/qa-reports/2026-06-04-headless-sim-m3b.md` 에 커버리지 150/150, capHit 목록, Fish/Ball 동작 요약 기록. 변경 Sim 파일 + 테스트(+meta) + 리포트 스테이징.
메시지: `# [feat] - Sim M3b 완료: Fish/Ball 이동블록 → 노멀모드 커버리지 150/150`

---

## Self-Review (작성자 체크)

**Spec 커버리지:** §2 Fish/Ball 메커니즘 → Task1(면역/드래그)+Task2(변환). §3 SimDisappearResolver → Task2. §3 SimBlock/ChangeMatchState → Task1. §3 SimGame(disappear/CheckClear/IsSupported) → Task3. §4 턴루프 순서(settle 후) → Task3 Step2. §5 AI 수동 → 별도 코드 없음(기존 AI 가 Fish/Ball 낙하를 자연 처리, Ball 은 스왑 가능). §7 골든/테스트 → 각 Task 단위 + Task4 측정. §8 IsSupported → Task3 Step4. 누락 없음.

**플레이스홀더:** 없음. ChangeMatchState/CheckClear 의 정확한 현재 형태는 구현자가 소스 확인 후 가드만 추가(코드 제시됨).

**타입 일관성:** `SimBlock.IsFish()/IsBall()/CanNotDrag()`, `SimMatchChecker.ChangeMatchState(board,r,c)`, `SimDisappearResolver.Resolve(board, System.Random)`, `SimGame._disappearRng`, `SimSpecialBlocks.ApplyChanges(board)` — Task 간 일치.

**리스크:** disappear 를 ResolveCascades settle 후 1회 호출 → Fish/Ball 변환 후 추가 cascade 없음(색폭탄/Potal 은 자동 매치 안 됨). 만약 변환이 새 매치를 만들면(드묾) 다음 수에서 처리 — 안전. 진단(Task4)으로 무한루프·크래시 0 확인이 최종 게이트.
