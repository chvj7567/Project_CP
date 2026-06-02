# 헤드리스 시뮬 하니스 M1 (일반 매치 + 낙하/리필 + 승패) Implementation Plan

> **상태: M1 구현 완료 (2026-06-01).** Task 1~8 전부 구현·통과. EditMode 자동 테스트 **19 PASS / 0 FAIL** (자체 러너 SimTestRunner). 배치 러너 실행 성공 → `docs/qa-reports/sim-output/m1-batch-*.json` (gitignore).
>
> **핵심 실측 결과 (M1 의 정직한 한계):** stage 1~150 중 **실제 측정 가능 4개**(stage 1·2·39·49), **146개는 특수블록(Wall/Potal/Fish 등) 포함으로 Unsupported**. 즉 CatPang 콘텐츠는 초반부터 특수블록을 광범위하게 써서 **M1(일반블록 전용)으로는 평가 불가 → M2/M3(폭탄·특수블록·보스)가 실질 필수**. 이게 "추측 아닌 실측" 이라는 하니스 목적이 작동한 증거.
> - 측정된 4개 샘플: stage1[ScoreGoal] Random클리어율100%·avgTurns15.1 vs Greedy13.7 / stage2[Move] Random클리어율6.7%·moveOver28 / stage39·49[ScoreGoal] avgTurns 379·240(고난도).
>
> **검증 신뢰 경계 (M1 리포트 필수 명기):** Task3 **매치 판정만** 실게임 `GPMatchChecker` 골든으로 동등성 검증됨. 낙하/턴루프/승패/점수(Task4/7/8)는 `GPGameScene` 정독 기반 **수작업 골든(self-referential)** — 실게임 직접 대조 아님. PASS 카운트가 이 둘을 뭉뚱그리지 않게.
>
> **남은 교차검증 (M1 done 선언 전 1회):** 자체 러너 SimTestRunner 가 source-of-truth 이므로, 실제 Unity Test Runner(사용자 수동 클릭)로 전체 스위트 1회 교차확인해 NUnit 과 일치 확인 권장 (Sim 네임스페이스 19/19 = 러너 결과 일치).
>
> **알려진 한계 (M2 가 재발견하지 않게):**
> - `SimBlock.IsNormal()` 은 Cat1~7(0~6)만 일반블록으로 봄. 실게임 `IsNormalBlock` 은 스킨 고양이(54~83, `GetBaseCat`)도 일반 취급. M1 배치엔 무영향(스킨은 StageBlock.json 에 없고 런타임 `CheckSelectCatShop` 으로 적용) — M2/M3 에서 스킨 보드 다루면 `IsNormal` 확장 필요.
> - **150 커버 범위 실측(2026-06-03)**: M2/M3 설계 근거. Wall(73 stage)·Potal(57)·Fish(33)·RainbowPang(27)·Creator(25/17)·CatBox(22)·Ball(19) + 초기배치 Arrow/특수폭탄. → **M2=정적(Wall/Potal/Fish/Ball/CatBox), M3=동적+폭탄(Arrow·특수폭탄 연쇄 GPBombResolver 12종·Creator·Rainbow), 보스 AI=M4.**

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** CatPang 일반 매치 스테이지(폭탄·특수블록·보스 제외)를 헤드리스로 N판 자동 플레이해 클리어율·이동수·실패사유 메트릭 JSON을 출력하고, 매치 판정 동등성을 실게임 `GPMatchChecker` 직호출 골든으로 검증한다.

**Architecture:** 순수 C# 병행 재구현. `Assets/Scripts/Sim/` 에 Unity 비종속 시뮬 코어를 만든다. 기존 `GamePlay/`·`Block.cs`·`GPGameScene.cs` 는 **읽기만 하고 수정하지 않는다**(회귀 방지). 매치 판정의 정답은 EditMode 에서 실게임 `GPMatchChecker` 를 직접 호출해 캡처한 골든으로 보증한다.

**Tech Stack:** C# (Unity 6 / Assembly-CSharp), Unity Test Framework (NUnit, EditMode), 기존 `Defines.EBlockState`/`EFailReason` enum 재사용, JsonUtility.

---

## 배경 — 실게임 코드 정독으로 확정한 진실 (이 규칙을 그대로 포팅)

> 출처: `GPMatchChecker.cs` 전문, `GPBoard.cs` 전문, `Block.cs` 전문, `GPGameScene.cs`(CreateMap 473-565 / RemoveMatchBlock 619-689 / Update 177-253 / InitData 404-435), `Infomation.cs`(StageInfo/StageBlockInfo/ApplyNormalModifiers), `Stage.json`·`StageBlock.json` 실제 샘플.

### 모드 분포 실측 (stage 1~150, group<100000) — Task8 메트릭 설계 근거
| 모드 | 하드(원본) | 노멀(변형 후) | M1 |
|---|---|---|---|
| 시간모드 `Time>0` | 73 | 0 | **Unsupported** |
| 이동모드 `MoveCount>0` | 77 | 89 | 정확 — 클리어율 + avgMoves 유효 |
| 무제한+점수목표 `Time≤0,Move≤0,Target>0` | 0 | 50 | 정확 — **클리어율 degenerate(≈100%)** → avgTurns-to-target 로 난이도 봐야 |
| 무제한+목표없음 | 0 | 11 | 즉시 클리어 |

→ **하드는 77/150 만 측정 가능**(시간 73 제외). **노멀은 150 전부 측정 가능**(시간 0).
→ **Task8: 클리어율을 전 모드 평균내지 말 것.** 모드별 세그먼트 — 이동모드=클리어율+avgMoves, 점수목표=avgTurns, 시간모드=Unsupported 카운트.
→ **M1 리포트 명시 의무**: Task3 매치판정만 실게임 골든 검증됨. 낙하/턴루프/승패/점수(Task4/7/8)는 GPGameScene 정독 기반 수작업 골든(self-referential) — 실게임 직접 대조 아님. PASS 카운트가 이 둘을 뭉뚱그리지 않게 리포트에 경계 표기.

### 데이터 스키마 (실제)
- **Stage.json** = `StageInfo[]` (루트가 곧 배열). 필드:
  `{ "group":int, "stage":int, "tutorialID":int, "blockTypeCount":int, "boardSize":int, "time":float, "targetScore":int, "moveCount":int }`
  (`mode`/`limitTime`/`row`/`col` 같은 필드는 **없다**.)
- **StageBlock.json** = `StageBlockInfo[]` **칸별 평면 레코드** 리스트:
  `{ "stage":int, "blockState":int, "hp":int, "row":int, "col":int, "tutorialBlock":bool }`
  한 스테이지의 칸들이 여러 레코드로 나열됨. `stage` 로 필터해서 모은다.
- **보드 구성**(CreateMap): 각 칸 `(r,c)` 에 대해 StageBlock 레코드를 찾아 → 있으면 그 `blockState`,
  **없으면 `Random(0, blockTypeCount)` 일반블록**. 시작 보드는 매치/가능수 없을 때까지 재생성.
- **모드 판정**: `time > 0` → 시간제한, `moveCount > 0` → 이동제한, **둘 다 ≤0 → 무제한**.
- **노멀 변형**(`StageInfo.ApplyNormalModifiers` 실제 — Infomation.cs:46-51): `time = -1; if (targetScore > 0) targetScore /= 2; else if (moveCount > 0) moveCount *= 2;`.
  → **(Task5 실측 정정)** 노멀 모드가 "항상 이동제한"이 **아니다**. `targetScore>0` 스테이지(대다수 노멀)는 시간만 제거되고 **무제한+점수목표(절반)** 가 된다. `targetScore<=0 && moveCount>0` 스테이지만 이동 2배. `NormalModeBaseMoveCount=30` 상수는 **존재하지 않음**(plan 초안 오류).
  → 하드 모드는 원본 그대로(시간/이동/무제한 혼재).

### 점수 (실제 — RemoveMatchBlock 631)
- 매치되어 제거되는 블록 **1개당 `curScore += 1`**. (M1 일반 매치 점수는 **제거 블록 수와 동일**.)
- `bonusScore`(폭탄/보스 보너스)는 **M1 비대상**.

### 매치 판정 (GPMatchChecker)
- **3-매치**(`Check3Match`): 같은 행/열에서 동일 `EBlockState` 연속 `MinMatchCount(=3)`+ → 그 블록 `match=true`.
  일반블록만(`IsNormalBlock && !Fixd && !Bomb && !Fish`). 연속 끊기면 카운트 리셋.
- **사각 매치**(`CheckSquareMatch`): `(r,c)(r+1,c)(r,c+1)(r+1,c+1)` 4칸 모두 일반·동일·미매치 → 4칸 `match=true`,
  이동 관여 칸 1개 `squareMatch=true`(없으면 좌상단). (squareMatch 는 폭탄 생성용 — M1 은 플래그만 보존.)
- **CheckMap 순서**: 모든 칸 사각 → 행별 3매치 → 열별 3매치. `isMatch` 는 하나라도 매치면 true(누적, 호출 전 false 초기화 필요).
- **CanPlay**: 모든 칸을 상/하/좌/우 인접칸과 swap → `CheckMap(test)` → 매치 생기면 "가능한 수 있음". (M1 폭탄 없음 → 인접 swap 검사만.)

### 낙하/리필
- **낙하**(`GPBoard.DownBlock`): Wall 차단 고려(M1 엔 Wall 없음 → 단순 중력). 각 열에서 비매치 블록이 바닥으로, 빈칸이 위로.
- **리필**(`GPGameScene.UpdateMap`): 매치 제거된 칸에 `Random(0, blockTypeCount)` 일반블록 생성.

### 턴 루프 (GPGameScene.AfterDrag, M1 관련 경로만)
swap → `CheckMap` → 매치 없으면 swap 되돌리고 종료(이동 차감 없음) → 매치 있으면
`do { RemoveMatchBlock(제거+점수); DownBlock(낙하); UpdateMap(리필); CheckMap } while(isMatch)`(연쇄) →
유효 드래그면 `moveCount -= 1`.

### 승패 (GPGameScene.Update 177-206 — M1 = 일반블록만이라 "목표블록" 없음)
- `clear` 초기값: 보드에 남은 목표블록(HP>0/Fish/Ball)이 없으면 true. **M1 은 항상 true 로 시작**(일반블록뿐).
- `useTargetScore = targetScore > 0`. 이때 `curScore < targetScore` 면 `clear = false`.
- **시간모드**(`time>0`, 시간 소진 시): 점수 판정으로 GameEnd. ← **실시간 의존. M1 헤드리스는 시간을 못 돌림.**
- **이동모드**(`moveCount>0`): `moveCount<=0` 일 때 GameEnd(clear). 점수 도달이면 클리어, 미달이면 실패(MoveOver).
- **무제한 + 점수목표**: 점수 도달(`clear` true) 즉시 GameEnd(true).
- 실패 사유(BuildFailInfo): 시간초과면 `TimeOver`, 아니면 `MoveOver`.

### → M1 정확 시뮬 경계 (중요 — Task5 실측 반영)
모드는 "노멀/하드" 라벨이 아니라 **변형 적용 후의 time/moveCount/targetScore 조합**으로 결정한다 (`SimStageData.IsTimeMode = Time>0`, `IsMoveMode = MoveCount>0`):
- **이동제한 (`MoveCount>0`) = 완전 정확 시뮬 가능.** (이동 예산 소진 → MoveOver)
- **무제한+점수목표 (`Time<=0 && MoveCount<=0 && TargetScore>0`) = 정확**(점수 게임, 무한방지 턴 캡). ← 노멀 변형된 대다수 스테이지가 여기 해당(targetScore 절반).
- **시간모드 (`Time>0`) = 실시간 의존 → M1 정확 불가 → `Unsupported`**(메트릭 제외+카운트). 하드 원본의 시간 스테이지가 해당.
- **특수블록/폭탄 포함 스테이지 = M1 비대상 → `Unsupported`.**

> Task 7/8 주의: "노멀=이동제한" 가정 금지. 승패는 `IsTimeMode`(Unsupported) / `IsMoveMode`(예산 소진 MoveOver) / 그 외(무제한 점수목표, 턴 캡) 로 분기. 테스트에서 이동제한을 검증하려면 `targetScore<=0 && moveCount>0` 인 스테이지(예: stage13: time=-1, targetScore=-1, moveCount=20)를 쓴다.

> **M1 비대상(M2/M3)**: 폭탄 생성·연쇄, 특수블록(Wall/Potal/CatBox/Creator/Fish/Ball), 보스 AI, 시간모드 실시간 모델.

---

## 파일 구조

| 파일 | 책임 |
|---|---|
| `Assets/Scripts/Sim/SimBlock.cs` | 순수 데이터 블록(state + match 플래그 + 좌표) |
| `Assets/Scripts/Sim/SimBoard.cs` | `SimBlock[,]` 그리드 + Swap/유효좌표/리셋 |
| `Assets/Scripts/Sim/SimMatchChecker.cs` | 3·사각 매치 + CanPlay. `GPMatchChecker` 규칙 포팅 |
| `Assets/Scripts/Sim/SimGravity.cs` | 낙하 + 리필(시드 RNG). `DownBlock`+`UpdateMap` 포팅 |
| `Assets/Scripts/Sim/SimStageData.cs` | 스테이지 1건(메타+블록배치) + JSON DTO |
| `Assets/Scripts/Sim/SimStageLoader.cs` | Stage.json+StageBlock.json 파싱·병합·노멀변형 |
| `Assets/Scripts/Sim/ISimAiPolicy.cs` | AI 수 선택 인터페이스 + Random/Greedy + SimMove |
| `Assets/Scripts/Sim/SimGame.cs` | 한 판 턴 루프 → SimResult(클리어/실패사유/이동수/턴수) |
| `Assets/Scripts/Sim/SimMetrics.cs` | 판 결과 누적 → (stage,policy) 집계 |
| `Assets/Scripts/Sim/Editor/SimBatchRunner.cs` | 메뉴 `CatPang/Sim/Run Batch` — 배치→JSON 저장 |
| `Assets/Tests/EditMode/Sim/RealBlockFactory.cs` | (테스트) 실 `Block` 렌더 없이 생성(리플렉션) |
| `Assets/Tests/EditMode/Sim/SimMatchGoldenTests.cs` | 실 `GPMatchChecker` ↔ `SimMatchChecker` 골든 동등성 |
| `Assets/Tests/EditMode/Sim/SimGravityTests.cs` | 보드/낙하/리필 단위 테스트 |
| `Assets/Tests/EditMode/Sim/SimGameTests.cs` | 로더/턴루프/승패/AI/메트릭 통합 테스트 |

> EditMode asmdef 는 기존 `Assets/Tests/EditMode/CatPang.Tests.EditMode.asmdef`(Assembly-CSharp 참조) 를 그대로 쓴다. Sim 코어는 `Assets/Scripts/` 아래라 Assembly-CSharp 에 포함 → 테스트에서 보임. `Sim/Editor/` 만 에디터 어셈블리.

---

## Task 1: 골든 캡처 전제 스파이크 — 실 Block 을 EditMode 에서 생성

**왜 먼저:** 매치 동등성의 정답은 실 `GPMatchChecker` 직호출 골든. 그러려면 `Block[,]` 를 EditMode 에서 만들어야 하는데 `Block.SetBlockState` 가 UI(`img`/`hpText`)를 건드려 NRE. **private `blockState` 필드를 리플렉션으로 직접 세팅**해 우회 가능한지 먼저 증명. 실패 시 이후 골든 접근을 바꿔야 하므로 1번. **이 스텝이 빨간불이면 멈추고 사용자에게 보고.**

**Files:**
- Create: `Assets/Tests/EditMode/Sim/RealBlockFactory.cs`
- Test: `Assets/Tests/EditMode/Sim/SimMatchGoldenTests.cs` (스파이크 1개)

- [ ] **Step 1: RealBlockFactory 작성**

```csharp
using System.Reflection;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# 실게임 Block 을 렌더/UI 없이 EditMode 에서 생성해 GPMatchChecker 골든 캡처에 쓴다.
    //# Block.SetBlockState 는 img/hpText 등 UI 를 건드려 NRE → private blockState 를 리플렉션으로 직접 세팅.
    public static class RealBlockFactory
    {
        private static readonly FieldInfo BlockStateField =
            typeof(Block).GetField("blockState", BindingFlags.NonPublic | BindingFlags.Instance);

        public static Block[,] CreateBoard(EBlockState[,] states)
        {
            int size = states.GetLength(0);
            Block[,] arr = new Block[size, size];
            for (int r = 0; r < size; ++r)
                for (int c = 0; c < size; ++c)
                {
                    GameObject go = new GameObject($"B{r}_{c}");
                    Block b = go.AddComponent<Block>();
                    b.row = r; b.col = c; b.index = r * size + c;
                    BlockStateField.SetValue(b, states[r, c]);
                    arr[r, c] = b;
                }
            return arr;
        }

        public static void Destroy(Block[,] arr)
        {
            foreach (Block b in arr)
                if (b != null) Object.DestroyImmediate(b.gameObject);
        }
    }
}
```

- [ ] **Step 2: 스파이크 테스트 작성**

```csharp
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimMatchGoldenTests
    {
        [Test]
        public void 실_GPMatchChecker_직호출로_가로3매치를_판정한다()
        {
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };
            Block[,] arr = RealBlockFactory.CreateBoard(states);

            GPBoard board = new GPBoard();
            board.Init(arr, 3, new Dictionary<EBlockState, Sprite>(), 0f, 0, default);
            GPMatchChecker matcher = new GPMatchChecker();
            matcher.Init(board, 5);

            matcher.CheckMap(test: false);

            Assert.IsTrue(arr[0, 0].IsMatch());
            Assert.IsTrue(arr[0, 1].IsMatch());
            Assert.IsTrue(arr[0, 2].IsMatch());
            Assert.IsFalse(arr[1, 1].IsMatch());

            RealBlockFactory.Destroy(arr);
        }
    }
}
```

- [ ] **Step 3: 실행 → 통과 확인 (전제 검증 핵심)**

Run: Unity Test Runner(EditMode) → `실_GPMatchChecker_직호출로_가로3매치를_판정한다`.
Expected: PASS.
실패(NRE 등)면 RealBlockFactory 세팅 필드를 늘리거나, 그래도 안되면 spec 의 "수작업 골든 폴백" 으로 전환 후 **사용자에게 보고하고 멈춤**.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Tests/EditMode/Sim/RealBlockFactory.cs Assets/Tests/EditMode/Sim/RealBlockFactory.cs.meta Assets/Tests/EditMode/Sim/SimMatchGoldenTests.cs Assets/Tests/EditMode/Sim/SimMatchGoldenTests.cs.meta
git commit -m "# [test] - 시뮬 골든 전제 스파이크: 실 GPMatchChecker EditMode 직호출 검증"
```

---

## Task 2: SimBlock + SimBoard

**Files:**
- Create: `Assets/Scripts/Sim/SimBlock.cs`, `Assets/Scripts/Sim/SimBoard.cs`
- Test: `Assets/Tests/EditMode/Sim/SimGravityTests.cs`

- [ ] **Step 1: 실패 테스트 작성**

```csharp
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimGravityTests
    {
        [Test]
        public void SimBoard_Swap_은_두칸_state를_교환한다()
        {
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Cat1);
            board.SetState(0, 1, EBlockState.Cat2);
            board.Swap(0, 0, 0, 1);
            Assert.AreEqual(EBlockState.Cat2, board.GetState(0, 0));
            Assert.AreEqual(EBlockState.Cat1, board.GetState(0, 1));
        }

        [Test]
        public void SimBoard_IsValid_는_범위밖을_거른다()
        {
            SimBoard board = new SimBoard(3);
            Assert.IsTrue(board.IsValid(0, 0));
            Assert.IsTrue(board.IsValid(2, 2));
            Assert.IsFalse(board.IsValid(-1, 0));
            Assert.IsFalse(board.IsValid(3, 0));
        }
    }
}
```

- [ ] **Step 2: 실행 → 실패 확인**

Run: EditMode `SimGravityTests`. Expected: FAIL (컴파일 에러 — SimBoard 없음).

- [ ] **Step 3: SimBlock 구현**

```csharp
using static Defines;

namespace CatPang.Sim
{
    //# 순수 데이터 블록. 실게임 Block 의 매치/낙하 관련 필드만 미러. Unity 비종속.
    public class SimBlock
    {
        public EBlockState State;
        public int Row;
        public int Col;
        public bool Match;
        public bool SquareMatch;

        //# 일반 고양이 블록 여부. M1 은 Cat1~7 만 사용(스킨/특수블록 비대상).
        public bool IsNormal() => State >= EBlockState.Cat1 && State <= EBlockState.Cat7;

        public void ResetMatch() { Match = false; SquareMatch = false; }
    }
}
```

- [ ] **Step 4: SimBoard 구현**

```csharp
using static Defines;

namespace CatPang.Sim
{
    //# SimBlock[,] 그리드. 실게임 GPBoard 의 좌표/스왑 역할만 순수 재구현.
    public class SimBoard
    {
        public readonly int Size;
        public readonly SimBlock[,] Grid;

        public SimBoard(int size)
        {
            Size = size;
            Grid = new SimBlock[size, size];
            for (int r = 0; r < size; ++r)
                for (int c = 0; c < size; ++c)
                    Grid[r, c] = new SimBlock { Row = r, Col = c, State = EBlockState.None };
        }

        public bool IsValid(int r, int c) => r >= 0 && r < Size && c >= 0 && c < Size;
        public EBlockState GetState(int r, int c) => Grid[r, c].State;
        public void SetState(int r, int c, EBlockState s) => Grid[r, c].State = s;

        //# 두 칸 state 만 교환(좌표 고정 그리드). 실게임 ChangeBlock 의 시뮬 등가.
        public void Swap(int r1, int c1, int r2, int c2)
        {
            EBlockState tmp = Grid[r1, c1].State;
            Grid[r1, c1].State = Grid[r2, c2].State;
            Grid[r2, c2].State = tmp;
        }

        public void ResetAllMatch()
        {
            foreach (SimBlock b in Grid) b.ResetMatch();
        }
    }
}
```

- [ ] **Step 5: 실행 → 통과 확인**

Run: EditMode `SimGravityTests`. Expected: PASS (2 테스트).

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Sim/SimBlock.cs Assets/Scripts/Sim/SimBlock.cs.meta Assets/Scripts/Sim/SimBoard.cs Assets/Scripts/Sim/SimBoard.cs.meta Assets/Tests/EditMode/Sim/SimGravityTests.cs Assets/Tests/EditMode/Sim/SimGravityTests.cs.meta
git commit -m "# [feat] - 시뮬 코어 데이터 모델 SimBlock/SimBoard 추가"
```

---

## Task 3: SimMatchChecker + 골든 동등성

**Files:**
- Create: `Assets/Scripts/Sim/SimMatchChecker.cs`
- Test: `Assets/Tests/EditMode/Sim/SimMatchGoldenTests.cs` (골든 동등성 추가)

- [ ] **Step 1: 골든 동등성 테스트 추가 (`SimMatchGoldenTests` 클래스 안)**

```csharp
private static bool[,] RealMatchMask(EBlockState[,] states, int blockTypeCount)
{
    int size = states.GetLength(0);
    Block[,] arr = RealBlockFactory.CreateBoard(states);
    GPBoard board = new GPBoard();
    board.Init(arr, size, new Dictionary<EBlockState, Sprite>(), 0f, 0, default);
    GPMatchChecker matcher = new GPMatchChecker();
    matcher.Init(board, blockTypeCount);
    matcher.CheckMap(test: false);
    bool[,] mask = new bool[size, size];
    for (int r = 0; r < size; ++r)
        for (int c = 0; c < size; ++c) mask[r, c] = arr[r, c].IsMatch();
    RealBlockFactory.Destroy(arr);
    return mask;
}

private static bool[,] SimMatchMask(EBlockState[,] states)
{
    int size = states.GetLength(0);
    SimBoard board = new SimBoard(size);
    for (int r = 0; r < size; ++r)
        for (int c = 0; c < size; ++c) board.SetState(r, c, states[r, c]);
    new SimMatchChecker().CheckMap(board);
    bool[,] mask = new bool[size, size];
    for (int r = 0; r < size; ++r)
        for (int c = 0; c < size; ++c) mask[r, c] = board.Grid[r, c].Match;
    return mask;
}

private static void AssertSameMask(EBlockState[,] states, int blockTypeCount)
{
    bool[,] real = RealMatchMask(states, blockTypeCount);
    bool[,] sim = SimMatchMask(states);
    int size = states.GetLength(0);
    for (int r = 0; r < size; ++r)
        for (int c = 0; c < size; ++c)
            Assert.AreEqual(real[r, c], sim[r, c], $"({r},{c}) 매치 마스크 불일치");
}

[Test] public void 골든_가로3매치_동등() => AssertSameMask(new EBlockState[,]{
    { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
    { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
    { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1 },
    { EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
}, 5);

[Test] public void 골든_세로3매치_동등() => AssertSameMask(new EBlockState[,]{
    { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
    { EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
    { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1 },
    { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
}, 5);

[Test] public void 골든_사각매치_동등() => AssertSameMask(new EBlockState[,]{
    { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat2 },
    { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat3 },
    { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1 },
    { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat1, EBlockState.Cat2 },
}, 5);

[Test] public void 골든_매치없음_동등() => AssertSameMask(new EBlockState[,]{
    { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2 },
    { EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1 },
    { EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2 },
    { EBlockState.Cat2, EBlockState.Cat1, EBlockState.Cat2, EBlockState.Cat1 },
}, 5);
```

- [ ] **Step 2: 실행 → 실패 확인**

Run: EditMode `SimMatchGoldenTests`. Expected: FAIL (SimMatchChecker 없음).

- [ ] **Step 3: SimMatchChecker 구현 (GPMatchChecker 규칙 포팅)**

```csharp
using System.Collections.Generic;
using static Defines;

namespace CatPang.Sim
{
    //# GPMatchChecker 의 3·사각 매치 + CanPlay 를 순수 재구현. M1: 일반블록만, 폭탄/특수 미고려.
    public class SimMatchChecker
    {
        public const int MinMatchCount = 3;
        public bool IsMatch;

        public void CheckMap(SimBoard board)
        {
            IsMatch = false;
            board.ResetAllMatch();
            int size = board.Size;

            for (int r = 0; r < size; ++r)
                for (int c = 0; c < size; ++c)
                    CheckSquare(board, r, c);

            for (int r = 0; r < size; ++r)
            {
                List<SimBlock> line = new List<SimBlock>();
                for (int c = 0; c < size; ++c) line.Add(board.Grid[r, c]);
                Check3(line);
            }
            for (int c = 0; c < size; ++c)
            {
                List<SimBlock> line = new List<SimBlock>();
                for (int r = 0; r < size; ++r) line.Add(board.Grid[r, c]);
                Check3(line);
            }
        }

        private void CheckSquare(SimBoard board, int r, int c)
        {
            if (!board.IsValid(r + 1, c + 1)) return;
            SimBlock a = board.Grid[r, c], b = board.Grid[r + 1, c],
                     d = board.Grid[r, c + 1], e = board.Grid[r + 1, c + 1];
            if (!a.IsNormal() || !b.IsNormal() || !d.IsNormal() || !e.IsNormal()) return;
            if (a.Match || b.Match || d.Match || e.Match) return;
            if (a.State != b.State || a.State != d.State || a.State != e.State) return;
            a.Match = b.Match = d.Match = e.Match = true;
            a.SquareMatch = true; //# 이동관여 칸 추적은 M2. M1 은 좌상단 고정.
            IsMatch = true;
        }

        private void Check3(List<SimBlock> line)
        {
            EBlockState state = EBlockState.None;
            int count = 0;
            for (int i = 0; i < line.Count; ++i)
            {
                SimBlock b = line[i];
                if (!b.IsNormal()) { state = EBlockState.None; count = 0; continue; }
                if (state == EBlockState.None) { state = b.State; count = 1; }
                else if (state == b.State)
                {
                    ++count;
                    if (count >= MinMatchCount)
                    {
                        int t = i;
                        for (int j = 0; j < count; ++j) line[t--].Match = true;
                        IsMatch = true;
                    }
                }
                else { state = b.State; count = 1; }
            }
        }

        //# 가능한 수 존재 여부: 모든 칸 상/하/좌/우 인접 swap 후 매치 검사.
        public bool CanPlay(SimBoard board)
        {
            int size = board.Size;
            (int dr, int dc)[] dirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
            for (int r = 0; r < size; ++r)
                for (int c = 0; c < size; ++c)
                    foreach (var d in dirs)
                    {
                        int nr = r + d.dr, nc = c + d.dc;
                        if (!board.IsValid(nr, nc)) continue;
                        board.Swap(r, c, nr, nc);
                        CheckMap(board);
                        bool matched = IsMatch;
                        board.Swap(r, c, nr, nc);
                        if (matched) { board.ResetAllMatch(); return true; }
                    }
            board.ResetAllMatch();
            return false;
        }
    }
}
```

- [ ] **Step 4: 실행 → 통과 확인**

Run: EditMode `SimMatchGoldenTests`. Expected: PASS (스파이크 1 + 골든 4 = 5). 불일치 시 GPMatchChecker 원문과 라인 대조해 수정.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Sim/SimMatchChecker.cs Assets/Scripts/Sim/SimMatchChecker.cs.meta Assets/Tests/EditMode/Sim/SimMatchGoldenTests.cs
git commit -m "# [feat] - SimMatchChecker 추가 및 실게임 매치 골든 동등성 검증"
```

---

## Task 4: SimGravity — 낙하 + 시드 리필

**Files:**
- Create: `Assets/Scripts/Sim/SimGravity.cs`
- Test: `Assets/Tests/EditMode/Sim/SimGravityTests.cs`

- [ ] **Step 1: 실패 테스트 추가**

```csharp
[Test]
public void 낙하_매치제거칸_위블록이_바닥으로_떨어진다()
{
    SimBoard board = new SimBoard(3);
    board.SetState(0, 0, EBlockState.Cat1);
    board.Grid[1, 0].Match = true; board.SetState(1, 0, EBlockState.Cat2);
    board.Grid[2, 0].Match = true; board.SetState(2, 0, EBlockState.Cat3);

    new SimGravity(seed: 1).Apply(board, blockTypeCount: 5);

    Assert.AreEqual(EBlockState.Cat1, board.GetState(2, 0));
    Assert.IsTrue(board.Grid[0, 0].IsNormal());
    Assert.IsTrue(board.Grid[1, 0].IsNormal());
    Assert.IsFalse(board.Grid[2, 0].Match, "낙하 후 match 해제 기대");
}

[Test]
public void 리필_시드가_같으면_결과가_같다()
{
    SimBoard a = new SimBoard(5);
    SimBoard b = new SimBoard(5);
    foreach (SimBlock bl in a.Grid) bl.Match = true;
    foreach (SimBlock bl in b.Grid) bl.Match = true;
    new SimGravity(seed: 42).Apply(a, 5);
    new SimGravity(seed: 42).Apply(b, 5);
    for (int r = 0; r < 5; ++r)
        for (int c = 0; c < 5; ++c)
            Assert.AreEqual(a.GetState(r, c), b.GetState(r, c), $"({r},{c}) 시드 재현 실패");
}
```

- [ ] **Step 2: 실행 → 실패 확인**

Run: EditMode `SimGravityTests`. Expected: FAIL (SimGravity 없음).

- [ ] **Step 3: SimGravity 구현**

```csharp
using System;
using static Defines;

namespace CatPang.Sim
{
    //# 낙하 + 리필. M1: Wall 없음 → 각 열에서 비매치 블록을 바닥으로 모으고 빈칸을 위에 랜덤 리필.
    //# RNG 는 시드 주입(System.Random)으로 결정적 — UnityEngine.Random 미사용.
    //# 리필 범위 0..blockTypeCount-1 = Cat1(0)..  (GPGameScene.UpdateMap 의 Random(0, blockTypeCount) 와 동일).
    public class SimGravity
    {
        private readonly Random _rng;
        public SimGravity(int seed) { _rng = new Random(seed); }

        public void Apply(SimBoard board, int blockTypeCount)
        {
            int size = board.Size;
            for (int c = 0; c < size; ++c)
            {
                int writeRow = size - 1;
                for (int r = size - 1; r >= 0; --r)
                {
                    if (!board.Grid[r, c].Match)
                    {
                        board.Grid[writeRow, c].State = board.Grid[r, c].State;
                        --writeRow;
                    }
                }
                for (int r = writeRow; r >= 0; --r)
                    board.Grid[r, c].State = (EBlockState)_rng.Next(0, blockTypeCount);
            }
            board.ResetAllMatch();
        }
    }
}
```

- [ ] **Step 4: 실행 → 통과 확인**

Run: EditMode `SimGravityTests` (4 테스트). Expected: PASS.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Sim/SimGravity.cs Assets/Scripts/Sim/SimGravity.cs.meta Assets/Tests/EditMode/Sim/SimGravityTests.cs
git commit -m "# [feat] - SimGravity 낙하+시드 리필 추가"
```

---

## Task 5: SimStageData + SimStageLoader (실제 스키마)

**Files:**
- Create: `Assets/Scripts/Sim/SimStageData.cs`, `Assets/Scripts/Sim/SimStageLoader.cs`
- Test: `Assets/Tests/EditMode/Sim/SimGameTests.cs`

- [ ] **Step 1: 실패 테스트 작성 (실제 JSON 파싱)**

```csharp
using System.IO;
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimGameTests
    {
        private static string StageJson => File.ReadAllText("Assets/AssetBundleResources/json/Stage.json");
        private static string StageBlockJson => File.ReadAllText("Assets/AssetBundleResources/json/StageBlock.json");

        [Test]
        public void 로더_스테이지1_메타와_블록배치를_읽는다()
        {
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, stage: 1, normalMode: false);
            Assert.AreEqual(1, s.Stage);
            Assert.AreEqual(3, s.BoardSize);           //# 실제 stage1 boardSize=3
            Assert.AreEqual(100, s.TargetScore);       //# 실제 stage1 targetScore=100
            Assert.AreEqual(3, s.BlockTypeCount);      //# 실제 stage1 blockTypeCount=3
            Assert.AreEqual(s.BoardSize * s.BoardSize, s.InitialStates.Length);
        }

        [Test]
        public void 로더_노멀모드는_시간제거_무제한이면_기준이동치_적용()
        {
            //# stage1: time=10(시간모드), moveCount=-1. 노멀변형 → time=-1, moveCount=30(NormalModeBaseMoveCount)
            SimStageData n = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
            Assert.IsTrue(n.Time <= 0, "노멀은 시간제한 제거");
            Assert.AreEqual(30, n.MoveCount);
        }

        [Test]
        public void 로더_노멀모드_이동스테이지는_이동2배()
        {
            //# stage2: time=-1, moveCount=5. 노멀변형 → moveCount=10
            SimStageData n = SimStageLoader.Load(StageJson, StageBlockJson, 2, normalMode: true);
            Assert.AreEqual(10, n.MoveCount);
        }
    }
}
```

- [ ] **Step 2: 실행 → 실패 확인**

Run: EditMode `SimGameTests`. Expected: FAIL (SimStageData/SimStageLoader 없음).

- [ ] **Step 3: SimStageData 구현 (실제 스키마 DTO)**

```csharp
using System;
using static Defines;

namespace CatPang.Sim
{
    //# 시뮬용 스테이지 1건. 보드는 9x9 등 BoardSize^2 의 EBlockState 배열(row-major).
    public class SimStageData
    {
        public int Stage;
        public int Group;
        public int BoardSize;
        public float Time;          //# >0 시간제한, <=0 무제한
        public int MoveCount;       //# >0 이동제한, <=0 무제한
        public int TargetScore;     //# >0 점수목표
        public int BlockTypeCount;
        public EBlockState[] InitialStates; //# 길이 BoardSize^2. None=레코드 없음(런타임 랜덤 채움)

        public bool IsTimeMode => Time > 0;
        public bool IsMoveMode => MoveCount > 0;
    }

    //# Stage.json 실제 스키마: 루트가 StageInfo 배열. JsonUtility 는 루트 배열을 못 읽으므로 래핑 파싱(로더에서 처리).
    [Serializable] public class StageDto
    {
        public int group; public int stage; public int tutorialID;
        public int blockTypeCount; public int boardSize;
        public float time; public int targetScore; public int moveCount;
    }
    //# StageBlock.json 실제 스키마: 칸별 평면 레코드 배열.
    [Serializable] public class StageBlockDto
    {
        public int stage; public int blockState; public int hp;
        public int row; public int col; public bool tutorialBlock;
    }
}
```

- [ ] **Step 4: SimStageLoader 구현 (루트 배열 래핑 + 병합 + 노멀변형)**

```csharp
using System.Linq;
using UnityEngine;
using static Defines;

namespace CatPang.Sim
{
    //# Stage.json + StageBlock.json(둘 다 루트가 배열) 을 합쳐 SimStageData 로.
    //# JsonUtility 는 최상위 배열을 직접 파싱 못 하므로 "{\"items\":<json>}" 로 감싸 파싱한다.
    public static class SimStageLoader
    {
        [System.Serializable] private class StageWrap { public StageDto[] items; }
        [System.Serializable] private class BlockWrap { public StageBlockDto[] items; }

        public static SimStageData Load(string stageJson, string stageBlockJson, int stage, bool normalMode)
        {
            StageDto[] stages = JsonUtility.FromJson<StageWrap>("{\"items\":" + stageJson + "}").items;
            StageBlockDto[] blocks = JsonUtility.FromJson<BlockWrap>("{\"items\":" + stageBlockJson + "}").items;

            StageDto meta = stages.First(d => d.stage == stage);
            int size = meta.boardSize;

            float time = meta.time;
            int moveCount = meta.moveCount;
            if (normalMode)
            {
                //# Infomation.StageInfo.ApplyNormalModifiers 와 동일: time=-1, moveCount = >0 ? *2 : 30
                time = -1;
                moveCount = moveCount > 0 ? moveCount * 2 : 30;
            }

            EBlockState[] states = new EBlockState[size * size];
            for (int i = 0; i < states.Length; ++i) states[i] = EBlockState.None;
            foreach (StageBlockDto b in blocks.Where(b => b.stage == stage))
                if (b.row >= 0 && b.row < size && b.col >= 0 && b.col < size)
                    states[b.row * size + b.col] = (EBlockState)b.blockState;

            return new SimStageData
            {
                Stage = meta.stage, Group = meta.group, BoardSize = size,
                Time = time, MoveCount = moveCount, TargetScore = meta.targetScore,
                BlockTypeCount = meta.blockTypeCount, InitialStates = states,
            };
        }
    }
}
```

> 검증: `NormalModeBaseMoveCount(30)` 는 `Infomation.StageInfo` 상수와 동일. 값이 바뀌면 둘을 함께 본다.

- [ ] **Step 5: 실행 → 통과 확인**

Run: EditMode `SimGameTests` (3 테스트). Expected: PASS.

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Sim/SimStageData.cs Assets/Scripts/Sim/SimStageData.cs.meta Assets/Scripts/Sim/SimStageLoader.cs Assets/Scripts/Sim/SimStageLoader.cs.meta Assets/Tests/EditMode/Sim/SimGameTests.cs Assets/Tests/EditMode/Sim/SimGameTests.cs.meta
git commit -m "# [feat] - SimStageData/SimStageLoader 추가 (실제 스키마+노멀변형)"
```

---

## Task 6: ISimAiPolicy — AI 수 선택 (랜덤/탐욕)

**Files:**
- Create: `Assets/Scripts/Sim/ISimAiPolicy.cs`
- Test: `Assets/Tests/EditMode/Sim/SimGameTests.cs`

- [ ] **Step 1: 실패 테스트 추가**

```csharp
[Test]
public void AI_랜덤정책은_유효한_매치수를_고른다()
{
    SimBoard board = new SimBoard(3);
    board.SetState(0,0,EBlockState.Cat1); board.SetState(0,1,EBlockState.Cat1); board.SetState(0,2,EBlockState.Cat2);
    board.SetState(1,0,EBlockState.Cat3); board.SetState(1,1,EBlockState.Cat2); board.SetState(1,2,EBlockState.Cat1);
    board.SetState(2,0,EBlockState.Cat2); board.SetState(2,1,EBlockState.Cat3); board.SetState(2,2,EBlockState.Cat3);

    SimMove? mv = new RandomAiPolicy(seed: 1).ChooseMove(board, new SimMatchChecker());
    Assert.IsTrue(mv.HasValue);

    SimMove m = mv.Value;
    board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
    SimMatchChecker checker = new SimMatchChecker();
    checker.CheckMap(board);
    Assert.IsTrue(checker.IsMatch, "AI 가 고른 수는 매치를 만들어야");
}
```

- [ ] **Step 2: 실행 → 실패 확인**

Run: EditMode `SimGameTests`. Expected: FAIL.

- [ ] **Step 3: 구현 (SimMove + 인터페이스 + 두 정책 + 헬퍼)**

```csharp
using System;
using System.Collections.Generic;

namespace CatPang.Sim
{
    //# 한 수 = (Row,Col) 칸을 (Dr,Dc) 방향 인접칸과 swap.
    public struct SimMove
    {
        public int Row, Col, Dr, Dc;
        public SimMove(int r, int c, int dr, int dc) { Row = r; Col = c; Dr = dr; Dc = dc; }
    }

    public interface ISimAiPolicy
    {
        string Name { get; }
        SimMove? ChooseMove(SimBoard board, SimMatchChecker checker);
    }

    internal static class SimMoveFinder
    {
        private static readonly (int dr, int dc)[] Dirs = { (1, 0), (0, 1) }; //# 우/하만(좌/상은 대칭 중복)

        public static List<SimMove> FindValidMoves(SimBoard board, SimMatchChecker checker)
        {
            List<SimMove> moves = new List<SimMove>();
            int size = board.Size;
            for (int r = 0; r < size; ++r)
                for (int c = 0; c < size; ++c)
                    foreach (var d in Dirs)
                    {
                        int nr = r + d.dr, nc = c + d.dc;
                        if (!board.IsValid(nr, nc)) continue;
                        board.Swap(r, c, nr, nc);
                        checker.CheckMap(board);
                        bool matched = checker.IsMatch;
                        board.Swap(r, c, nr, nc);
                        board.ResetAllMatch();
                        if (matched) moves.Add(new SimMove(r, c, d.dr, d.dc));
                    }
            return moves;
        }

        public static int CountMatches(SimBoard board, SimMatchChecker checker, SimMove m)
        {
            board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
            checker.CheckMap(board);
            int n = 0;
            foreach (SimBlock b in board.Grid) if (b.Match) ++n;
            board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
            board.ResetAllMatch();
            return n;
        }
    }

    //# 전략 A: 유효 수 중 랜덤. 시드 결정적.
    public class RandomAiPolicy : ISimAiPolicy
    {
        public string Name => "Random";
        private readonly Random _rng;
        public RandomAiPolicy(int seed) { _rng = new Random(seed); }
        public SimMove? ChooseMove(SimBoard board, SimMatchChecker checker)
        {
            List<SimMove> moves = SimMoveFinder.FindValidMoves(board, checker);
            if (moves.Count == 0) return null;
            return moves[_rng.Next(moves.Count)];
        }
    }

    //# 전략 B: 매치 블록 수 최대. 동점은 시드 랜덤.
    public class GreedyAiPolicy : ISimAiPolicy
    {
        public string Name => "Greedy";
        private readonly Random _rng;
        public GreedyAiPolicy(int seed) { _rng = new Random(seed); }
        public SimMove? ChooseMove(SimBoard board, SimMatchChecker checker)
        {
            List<SimMove> moves = SimMoveFinder.FindValidMoves(board, checker);
            if (moves.Count == 0) return null;
            int best = -1; List<SimMove> bestMoves = new List<SimMove>();
            foreach (SimMove m in moves)
            {
                int cnt = SimMoveFinder.CountMatches(board, checker, m);
                if (cnt > best) { best = cnt; bestMoves.Clear(); bestMoves.Add(m); }
                else if (cnt == best) bestMoves.Add(m);
            }
            return bestMoves[_rng.Next(bestMoves.Count)];
        }
    }
}
```

- [ ] **Step 4: 실행 → 통과 확인**

Run: EditMode `SimGameTests`. Expected: PASS.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Sim/ISimAiPolicy.cs Assets/Scripts/Sim/ISimAiPolicy.cs.meta Assets/Tests/EditMode/Sim/SimGameTests.cs
git commit -m "# [feat] - 시뮬 AI 정책(Random/Greedy) 추가"
```

---

## Task 7: SimGame — 한 판 턴 루프 + 승패 (실게임 규칙)

**Files:**
- Create: `Assets/Scripts/Sim/SimGame.cs`
- Test: `Assets/Tests/EditMode/Sim/SimGameTests.cs`

- [ ] **Step 1: 실패 테스트 추가**

```csharp
[Test]
public void 한판_이동소진_점수미달이면_MoveOver()
{
    SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
    s.MoveCount = 1;            //# 이동 1회로 강제
    s.TargetScore = int.MaxValue; //# 달성 불가
    SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
    Assert.IsFalse(r.Clear);
    Assert.AreEqual(EFailReason.MoveOver, r.FailReason);
}

[Test]
public void 한판_목표0이면_즉시_클리어()
{
    SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
    s.TargetScore = 0;
    SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
    Assert.IsTrue(r.Clear);
    Assert.AreEqual(EFailReason.None, r.FailReason);
}

[Test]
public void 한판_시간모드는_Unsupported()
{
    //# 하드 stage1 원본: time=10 (시간모드) → M1 실시간 불가 → Unsupported
    SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: false);
    Assert.IsTrue(s.IsTimeMode, "stage1 하드는 시간모드여야(전제 확인)");
    SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
    Assert.IsTrue(r.Unsupported);
}

[Test]
public void 한판_특수블록스테이지는_Unsupported()
{
    SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 1, normalMode: true);
    s.InitialStates[0] = EBlockState.Wall; //# 특수블록 강제 삽입
    SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
    Assert.IsTrue(r.Unsupported);
}
```

- [ ] **Step 2: 실행 → 실패 확인**

Run: EditMode `SimGameTests`. Expected: FAIL (SimGame 없음).

- [ ] **Step 3: SimGame + SimResult 구현 (AfterDrag/Update 규칙 포팅)**

```csharp
using static Defines;

namespace CatPang.Sim
{
    public struct SimResult
    {
        public int Stage;
        public bool Clear;
        public EFailReason FailReason;
        public int MovesUsed;
        public int Turns;        //# 연쇄 포함 총 해소 횟수
        public int FinalScore;
        public bool Unsupported; //# 시간모드/특수블록/폭탄 등 M1 미지원
        public string PolicyName;
    }

    //# 한 스테이지를 끝까지 자동 플레이. AfterDrag 턴 루프 + Update 승패를 순수 포팅.
    //# 지원 범위: 이동제한 또는 무제한 + 일반블록만. 시간모드/특수블록은 Unsupported.
    public class SimGame
    {
        //# 무제한 점수게임 무한루프 방지 캡(턴 수). 9x9 점수목표 스테이지도 충분히 도달 가능한 여유값.
        private const int MaxTurns = 100000;

        //# 지원 = 시간모드 아님 + 모든 칸이 None/일반(Cat1~7).
        private static bool IsSupported(SimStageData s)
        {
            if (s.IsTimeMode) return false;
            foreach (EBlockState st in s.InitialStates)
            {
                if (st == EBlockState.None) continue;
                if (st >= EBlockState.Cat1 && st <= EBlockState.Cat7) continue;
                return false;
            }
            return true;
        }

        public SimResult Run(SimStageData s, ISimAiPolicy policy, int seed)
        {
            SimResult result = new SimResult { Stage = s.Stage, PolicyName = policy.Name, FailReason = EFailReason.None };
            if (!IsSupported(s)) { result.Unsupported = true; return result; }

            SimMatchChecker checker = new SimMatchChecker();
            SimGravity gravity = new SimGravity(seed);

            //# 보드 구성: 레코드(None 아님)면 그 state, None 이면 랜덤 일반(시드 RNG).
            SimBoard board = BuildInitialBoard(s, seed);
            //# 시작 보드의 기존 매치/무수(無手) 정리 (CreateMap 과 동일 의도)
            StabilizeStartBoard(board, checker, gravity, s);

            //# 시작 직후 점수목표 0 등 즉시 클리어 케이스
            if (CheckClear(s, result.FinalScore)) { result.Clear = true; return result; }

            int moveBudget = s.IsMoveMode ? s.MoveCount : int.MaxValue;

            while (true)
            {
                if (result.Turns > MaxTurns) { result.FailReason = EFailReason.MoveOver; return result; } //# 안전장치

                if (!checker.CanPlay(board))
                {
                    //# 무수 → 전체 리필(실게임 셔플 대용)
                    foreach (SimBlock b in board.Grid) b.Match = true;
                    gravity.Apply(board, s.BlockTypeCount);
                    ResolveCascades(board, checker, gravity, s, ref result);
                    if (CheckClear(s, result.FinalScore)) { result.Clear = true; return result; }
                    continue;
                }

                SimMove? mv = policy.ChooseMove(board, checker);
                if (mv == null) continue; //# CanPlay true 와 모순 방지(다음 루프서 재셔플)

                SimMove m = mv.Value;
                board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc);
                checker.CheckMap(board);
                if (!checker.IsMatch)
                {
                    board.Swap(m.Row, m.Col, m.Row + m.Dr, m.Col + m.Dc); //# 되돌림(이동 차감 없음)
                    board.ResetAllMatch();
                    continue;
                }

                result.MovesUsed += 1;
                ResolveCascades(board, checker, gravity, s, ref result);

                if (CheckClear(s, result.FinalScore)) { result.Clear = true; return result; }

                if (s.IsMoveMode)
                {
                    moveBudget -= 1;
                    if (moveBudget <= 0) { result.FailReason = EFailReason.MoveOver; return result; }
                }
            }
        }

        private SimBoard BuildInitialBoard(SimStageData s, int seed)
        {
            System.Random rng = new System.Random(seed);
            SimBoard board = new SimBoard(s.BoardSize);
            for (int r = 0; r < s.BoardSize; ++r)
                for (int c = 0; c < s.BoardSize; ++c)
                {
                    EBlockState st = s.InitialStates[r * s.BoardSize + c];
                    if (st == EBlockState.None) st = (EBlockState)rng.Next(0, s.BlockTypeCount);
                    board.SetState(r, c, st);
                }
            return board;
        }

        //# 시작 보드에 이미 매치가 있으면 점수 없이 정리(실게임 CreateMap 은 매치 없는 상태로 시작).
        private void StabilizeStartBoard(SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s)
        {
            checker.CheckMap(board);
            int guard = 0;
            while (checker.IsMatch && guard++ < 1000)
            {
                gravity.Apply(board, s.BlockTypeCount); //# 매치 제거+리필 (시작 정리라 점수 미가산)
                checker.CheckMap(board);
            }
        }

        //# 매치 제거→점수(블록당 1점)→낙하/리필→재검사 를 매치 없을 때까지(연쇄).
        private void ResolveCascades(SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s, ref SimResult result)
        {
            checker.CheckMap(board);
            while (checker.IsMatch)
            {
                int cleared = 0;
                foreach (SimBlock b in board.Grid) if (b.Match) ++cleared;
                result.FinalScore += cleared; //# 실게임 RemoveMatchBlock: 제거 블록당 +1
                result.Turns += 1;
                gravity.Apply(board, s.BlockTypeCount);
                checker.CheckMap(board);
            }
        }

        private static bool CheckClear(SimStageData s, int score)
        {
            //# M1 = 목표블록 없음(일반블록만). targetScore>0 면 점수 도달이 클리어. targetScore<=0 면 즉시(목표 없음).
            if (s.TargetScore > 0) return score >= s.TargetScore;
            return true;
        }
    }
}
```

> 주의: `StabilizeStartBoard` 의 무점수 정리는 실게임 CreateMap(매치 없는 시작 보장)을 근사한 것이다. 실게임은 재생성이지만 시뮬은 낙하/리필로 대체 — 시작 상태 분포가 미세하게 다를 수 있으므로, 메트릭 절대값보다 **전략·스테이지 간 상대 비교**에 무게를 둔다(리포트에 명시).

- [ ] **Step 4: 실행 → 통과 확인**

Run: EditMode `SimGameTests`. Expected: PASS (전체 누적).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Sim/SimGame.cs Assets/Scripts/Sim/SimGame.cs.meta Assets/Tests/EditMode/Sim/SimGameTests.cs
git commit -m "# [feat] - SimGame 한 판 턴 루프 + 승패(실게임 규칙) 추가"
```

---

## Task 8: SimMetrics + SimBatchRunner (배치 + JSON 출력)

**Files:**
- Create: `Assets/Scripts/Sim/SimMetrics.cs`, `Assets/Scripts/Sim/Editor/SimBatchRunner.cs`
- Test: `Assets/Tests/EditMode/Sim/SimGameTests.cs`

- [ ] **Step 1: 실패 테스트 추가**

```csharp
[Test]
public void 메트릭_클리어율을_집계한다()
{
    SimMetrics m = new SimMetrics();
    m.Add(new SimResult { Stage = 1, PolicyName = "Random", Clear = true,  MovesUsed = 5 });
    m.Add(new SimResult { Stage = 1, PolicyName = "Random", Clear = false, MovesUsed = 8, FailReason = EFailReason.MoveOver });
    m.Add(new SimResult { Stage = 1, PolicyName = "Random", Clear = true,  MovesUsed = 7 });

    StageMetric sm = m.Get(1, "Random");
    Assert.AreEqual(3, sm.Plays);
    Assert.AreEqual(2, sm.Clears);
    Assert.AreEqual(2f / 3f, sm.ClearRate, 0.001f);
}

[Test]
public void 메트릭_미지원판은_집계제외_별도카운트()
{
    SimMetrics m = new SimMetrics();
    m.Add(new SimResult { Stage = 2, PolicyName = "Random", Unsupported = true });
    StageMetric sm = m.Get(2, "Random");
    Assert.AreEqual(0, sm.Plays);
    Assert.AreEqual(1, sm.Unsupported);
}
```

- [ ] **Step 2: 실행 → 실패 확인**

Run: EditMode `SimGameTests`. Expected: FAIL.

- [ ] **Step 3: SimMetrics 구현**

```csharp
using System.Collections.Generic;
using static Defines;

namespace CatPang.Sim
{
    public class StageMetric
    {
        public int Stage; public string Policy;
        public int Plays; public int Clears; public int Unsupported;
        public int MoveOver; public int TimeOver;
        public long MovesUsedSum;
        public float ClearRate => Plays == 0 ? 0f : (float)Clears / Plays;
        public float AvgMoves => Plays == 0 ? 0f : (float)MovesUsedSum / Plays;
    }

    //# 판 결과 누적 → (stage, policy) 별 집계.
    public class SimMetrics
    {
        private readonly Dictionary<string, StageMetric> _map = new Dictionary<string, StageMetric>();
        private static string Key(int stage, string policy) => $"{stage}|{policy}";

        public void Add(SimResult r)
        {
            StageMetric sm = Get(r.Stage, r.PolicyName);
            if (r.Unsupported) { sm.Unsupported += 1; return; }
            sm.Plays += 1;
            sm.MovesUsedSum += r.MovesUsed;
            if (r.Clear) sm.Clears += 1;
            else if (r.FailReason == EFailReason.MoveOver) sm.MoveOver += 1;
            else if (r.FailReason == EFailReason.TimeOver) sm.TimeOver += 1;
        }

        public StageMetric Get(int stage, string policy)
        {
            string k = Key(stage, policy);
            if (!_map.TryGetValue(k, out StageMetric sm))
            {
                sm = new StageMetric { Stage = stage, Policy = policy };
                _map[k] = sm;
            }
            return sm;
        }

        public IEnumerable<StageMetric> All() => _map.Values;
    }
}
```

- [ ] **Step 4: 실행 → 통과 확인**

Run: EditMode `SimGameTests`. Expected: PASS.

- [ ] **Step 5: SimBatchRunner (에디터 메뉴) 구현**

```csharp
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using CatPang.Sim;

namespace CatPang.Sim.EditorTools
{
    //# N판 배치 시뮬 → JSON 저장. qa-simulator 가 이 JSON 을 읽어 docs/qa-reports/ 에 해석 리포트 작성.
    //# M1: 노멀 모드(항상 이동제한)만 정확 → 노멀 우선. 하드는 시간모드면 Unsupported 로 빠진다.
    public static class SimBatchRunner
    {
        private const int Seed = 42;
        private const int PlaysPerStageStrategy = 30;
        private const int StageFrom = 1;
        private const int StageTo = 150;  //# group 1~15 = 노멀/하드 공유 150 스테이지

        [MenuItem("CatPang/Sim/Run Batch (M1 Normal)")]
        public static void RunBatch()
        {
            string stageJson = File.ReadAllText("Assets/AssetBundleResources/json/Stage.json");
            string stageBlockJson = File.ReadAllText("Assets/AssetBundleResources/json/StageBlock.json");

            SimMetrics metrics = new SimMetrics();
            string[] strategies = { "Random", "Greedy" };

            for (int stage = StageFrom; stage <= StageTo; ++stage)
            {
                SimStageData data;
                try { data = SimStageLoader.Load(stageJson, stageBlockJson, stage, normalMode: true); }
                catch { continue; } //# 해당 stage 없음

                foreach (string strat in strategies)
                    for (int i = 0; i < PlaysPerStageStrategy; ++i)
                    {
                        int seed = Seed + i;
                        ISimAiPolicy policy = strat == "Greedy"
                            ? new GreedyAiPolicy(seed)
                            : (ISimAiPolicy)new RandomAiPolicy(seed);
                        SimResult r = new SimGame().Run(data, policy, seed);
                        metrics.Add(r);
                    }
            }

            string outPath = WriteJson(metrics);
            Debug.Log($"[Sim] 배치 완료 → {outPath}");
            AssetDatabase.Refresh();
        }

        private static string WriteJson(SimMetrics metrics)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n  \"note\": \"M1 노멀모드(이동제한). 일반매치만. 특수블록 포함 스테이지는 unsupported\",\n");
            sb.Append($"  \"seed\": {Seed}, \"playsPerStageStrategy\": {PlaysPerStageStrategy},\n  \"results\": [\n");
            bool first = true;
            foreach (StageMetric m in metrics.All())
            {
                if (!first) sb.Append(",\n");
                first = false;
                sb.Append("    {");
                sb.Append($"\"stage\": {m.Stage}, \"policy\": \"{m.Policy}\", ");
                sb.Append($"\"plays\": {m.Plays}, \"clears\": {m.Clears}, \"clearRate\": {m.ClearRate:F3}, ");
                sb.Append($"\"avgMoves\": {m.AvgMoves:F2}, \"moveOver\": {m.MoveOver}, ");
                sb.Append($"\"unsupported\": {m.Unsupported}");
                sb.Append("}");
            }
            sb.Append("\n  ]\n}\n");

            string dir = "docs/qa-reports/sim-output";
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, $"m1-batch-{System.DateTime.Now:yyyyMMdd-HHmmss}.json");
            File.WriteAllText(path, sb.ToString());
            return path;
        }
    }
}
```

- [ ] **Step 6: 배치 실행 검증 (수동)**

Unity 에디터 → `CatPang/Sim/Run Batch (M1 Normal)`.
Expected: 콘솔 `[Sim] 배치 완료 → docs/qa-reports/sim-output/m1-batch-*.json`, 에러 0. JSON 에 stage 1~150 × (Random/Greedy) 의 clearRate 채워짐. (특수블록 포함 스테이지는 `unsupported>0`, `plays=0`)

- [ ] **Step 7: 커밋**

```bash
git add Assets/Scripts/Sim/SimMetrics.cs Assets/Scripts/Sim/SimMetrics.cs.meta Assets/Scripts/Sim/Editor/SimBatchRunner.cs Assets/Scripts/Sim/Editor/SimBatchRunner.cs.meta Assets/Tests/EditMode/Sim/SimGameTests.cs
git commit -m "# [feat] - 시뮬 배치 러너+메트릭 JSON 출력 추가 (M1 완성)"
```

> 산출물 `sim-output/*.json` 은 커밋하지 않는다(런타임 생성물). 필요시 `.gitignore` 추가는 별도 커밋.

---

## Self-Review (작성자 점검 — 실제 코드 대조 완료)

**1. Spec 커버리지:**
- 순수 C# 병행 재구현 → Task 2~7 ✅ (기존 게임 코드 read-only)
- 실 GPMatchChecker 직호출 골든 → Task 1(전제)+Task 3(동등성) ✅
- 범위 M1(일반매치+낙하/리필+승패), 특수블록/시간모드 skip → Task 7 IsSupported ✅
- AI 2전략(랜덤/탐욕) → Task 6 ✅
- 메트릭(클리어율·이동수·실패사유)+JSON+qa-reports → Task 8 ✅
- 배치 진입점 `CatPang/Sim/...` → Task 8 ✅
- 노멀=시간제거+이동(>0?×2:30) → Task 5 (실제 ApplyNormalModifiers 반영) ✅

**2. 실제 코드 대조로 정정한 것 (이전 초안의 추측 → 실제):**
- Stage 스키마: `{group,stage,tutorialID,blockTypeCount,boardSize,time,targetScore,moveCount}` (mode enum 없음).
- StageBlock 스키마: 칸별 평면 레코드 `{stage,blockState,hp,row,col,tutorialBlock}` (blockData[81] 아님).
- 점수: 제거 블록당 **1점** (10점 아님 — RemoveMatchBlock:631 `curScore += 1`).
- 노멀변형: `moveCount>0 ? *2 : 30` (단순 2배 아님 — ApplyNormalModifiers).
- 승패: M1 은 목표블록 없어 점수도달=클리어. 시간모드는 실시간 의존이라 Unsupported(중요 경계).
- JsonUtility 루트 배열 미지원 → `{"items":...}` 래핑 파싱.

**3. 한계(리포트 명시 대상):**
- 시작 보드 정리(StabilizeStartBoard)는 실게임 재생성의 근사 → 절대값보다 상대 비교 중심.
- 시간모드 스테이지는 M1 측정 불가(Unsupported) → M2/M3 이후 또는 별도 시간모델 필요.

**4. 타입 일관성 확인:** `SimBoard.{Grid,Size,SetState,GetState,Swap,IsValid,ResetAllMatch}`, `SimBlock.{State,Row,Col,Match,SquareMatch,IsNormal,ResetMatch}`, `SimMatchChecker.{CheckMap(board),CanPlay(board),IsMatch}`, `SimGravity.Apply(board,blockTypeCount)`, `SimStageData.{Stage,Group,BoardSize,Time,MoveCount,TargetScore,BlockTypeCount,InitialStates,IsTimeMode,IsMoveMode}`, `SimMove{Row,Col,Dr,Dc}`, `SimResult{Stage,Clear,FailReason,MovesUsed,Turns,FinalScore,Unsupported,PolicyName}`, `SimMetrics.{Add,Get,All}`/`StageMetric{...}` — 태스크 간 시그니처 일치 확인 완료.
