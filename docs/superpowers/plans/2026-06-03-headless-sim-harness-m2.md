# 헤드리스 시뮬 하니스 M2 (정적 특수블록) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** M1 시뮬 코어에 폭탄 무관 정적 특수블록(Wall/Potal/Creator/CatBox)을 추가해 stage 1~150 측정 범위를 4개→59개로 넓힌다.

**Architecture:** M1 의 `Assets/Scripts/Sim/` 순수 C# 코어를 확장. SimBlock 에 HP/CheckHp 필드 추가, 매치 인접 데미지·CatBox 수집·Creator 변환·상태전이를 턴 루프에 통합, 승패를 "목표블록 제거 AND 점수"로 일반화. 기존 게임 코드(`GamePlay/`·`Block.cs`·`GPGameScene.cs`)는 읽기만, 수정 안 함. 매치/데미지/수집은 실게임 직호출 골든으로, 랜덤 의존(Creator)은 수작업 골든으로 검증.

**Tech Stack:** C# (Unity 6 / Assembly-CSharp), Unity Test Framework (NUnit, EditMode), 자체 러너 `SimTestRunner`(MCP editor_invoke_method), 기존 `Defines.EBlockState` 재사용.

---

## 검증 방법 (M1 과 동일 — 매 태스크)

각 태스크 구현 후 메인이 실행:
1. `editor_refresh_assets` → `editor_wait_ready` → `editor_read_log`(에러 0 확인)
2. `editor_invoke_method` 로 `CatPang.Sim.EditorTools.SimTestRunner.RunSimTests()` 호출 → `PASS n / FAIL 0` 확인
3. 테스트는 **plain 파라미터 없는 `[Test]` 만** (자체 러너 제약: `[TestCase]`/`[Values]`/`Assert.Multiple`/`[UnityTest]` 금지)

> M1 현재 베이스라인: **19 PASS**. M2 는 여기에 누적된다.

---

## 배경 — 실게임 정독으로 확정한 M2 규칙 (그대로 포팅)

> 출처: `Block.cs`(Damage 409-429 / CatInTheBox·CheckInBoxBlock 636-682 / CheckHpBlock 684-694 / IsWallBlock 535-551), `GPMatchChecker.cs`(CheckArround·DamageBlock 223-241 / ResetCheckWallDamage 42-44), `GPGameScene.cs`(RemoveMatchBlock 619-689 의 CheckArround 호출 / CatInTheBox 727-754 / UpdateMap 565-617 / Update 승패 177-206), `GPBombResolver.cs`(BlockCreatorBlock 462-492), `GPBoard.cs`(DownBlock 82-136 의 Wall 차단).

### HP / 데미지 (Block.Damage 409-429)
- 각 블록에 `hp`. `Damage()`: `checkDamage==false && hp>=1 && 박스아님` 일 때만 `hp-=1`. hp→0 이면 `changeBlockState = Random(0, blockTypeCount)`(일반블록 전환). 박스(CatBox)는 Damage 무시.
- `checkDamage` 는 턴당 1회 가드. 매 CheckMap 시작 시 전 칸 리셋(`ResetCheckWallDamage`, GPMatchChecker.CheckMap 42-44).

### 인접 데미지 (GPMatchChecker.CheckArround/DamageBlock 223-241, RemoveMatchBlock 630 에서 호출)
- 매치 제거되는 각 블록에 대해 **상하좌우 4칸** `DamageBlock`. (RainbowPang 은 changeNormalBlock=false — M2 엔 없음.)
- 즉 Wall/Potal 은 "직접 매치"가 아니라 **인접 칸이 매치 제거될 때** 데미지로 깎인다.

### CatBox 수집 (Block.CatInTheBox/CheckInBoxBlock 636-682, GPGameScene.CatInTheBox 727-754)
- 박스 **위 칸**(row-1) 블록을 `GetBaseCat` 으로 환산 → 박스가 받는 고양이면: CatBox1=Cat1/Cat6, CatBox2=Cat2/Cat7, CatBox3=Cat3, CatBox4=Cat4, CatBox5=Cat5.
- 받는 고양이면 박스 `hp-=1`(hp>0일 때) + **위 블록 제거(match=true)**. 매 턴 스캔, 수집 발생 시 추가 연쇄.

### Creator (GPBombResolver.BlockCreatorBlock 462-492)
- WallCreator→Wall, PotalCreator→Potal. 매 턴 1회: creator 블록마다 랜덤 4방향 중 인접 일반블록 1개를 찾아 `changeBlockState`=대상블록 + `changeHp=1` + `checkHp=false` 세팅, creator 는 `Damage()`. creator hp==0 이면 스킵.
- **랜덤 방향 의존 → 결정적 실게임 골든 불가 → 수작업 골든.**

### 상태 전이 (GPGameScene.UpdateMap 565-617)
- 매 턴 `changeBlockState != None` 인 칸을 그 상태로 전환 + `SetHp(changeHp)` + changeBlockState=None.

### 낙하 차단 (GPBoard.DownBlock 100-106)
- 각 열에서 떨어지는 블록은 자기보다 아래의 **가장 가까운 Wall** 아래로는 못 내려감. `IsWallBlock`=Wall/CatBox1~5/WallCreator/PotalCreator (Block.cs 535-551). Potal 은 IsWallBlock 아님(낙하 안 막음).

### 승패 (GPGameScene.Update 177-206 — 목표블록 제거 AND 점수)
```
clear = true
foreach 칸:
  if state == RainbowPang: continue
  if CheckHp == false: continue            // Creator 는 목표 아님
  if hp > 0 or IsFish or IsBall: clear=false; break
if TargetScore>0 and score < TargetScore: clear=false
//# 둘 다(AND) 만족해야 clear
```
- `CheckHp`(CheckHpBlock 684-694): WallCreator/PotalCreator 만 false, 나머지 true.
- M2 엔 Fish/Ball 없음 → 1단계는 hp>0 검사만 실동작(Fish/Ball 검사는 M3 대비 코드만 둠).

### IsSupported 갱신
- M2 supported = 시간모드 아님 + 모든 칸이 {None, Cat1~7, Wall, Potal, WallCreator, PotalCreator, CatBox1~5}. 그 외(Fish/Ball/Arrow/특수폭탄/Rainbow/CatPang) 있으면 Unsupported(→M3).

---

## 파일 구조

| 파일 | 변경 | 책임 |
|---|---|---|
| `Assets/Scripts/Sim/SimBlock.cs` | 수정 | `int Hp; bool CheckHp; bool CheckDamage;` 추가 + 분류 헬퍼(IsWall/IsBox/IsCreator/IsCatBox) + Damage/ResetCheckDamage |
| `Assets/Scripts/Sim/SimStageData.cs` | 수정 | `int[] InitialHps` 추가 (StageBlock hp 동반 로드) |
| `Assets/Scripts/Sim/SimStageLoader.cs` | 수정 | hp 배열 채우기 |
| `Assets/Scripts/Sim/SimMatchChecker.cs` | 수정 | CheckMap 시작 시 ResetCheckDamage + 인접데미지(CheckArround/DamageBlock) |
| `Assets/Scripts/Sim/SimGravity.cs` | 수정 | Wall 낙하 차단 |
| `Assets/Scripts/Sim/SimSpecialBlocks.cs` | 신규 | CatBox 수집 + Creator 변환 + 상태전이(ApplyChanges) 로직 응집 |
| `Assets/Scripts/Sim/SimGame.cs` | 수정 | IsSupported 확장, 턴 루프에 인접데미지·CatBox·Creator·상태전이 통합, CheckClear AND 로직, BuildInitialBoard 가 HP 세팅 |
| `Assets/Tests/EditMode/Sim/SimSpecialBlockGoldenTests.cs` | 신규 | 실게임 Damage/CatInTheBox 골든 + Sim 동등성 |
| `Assets/Tests/EditMode/Sim/SimM2Tests.cs` | 신규 | 낙하차단/Creator/승패AND/로더HP/IsSupported 단위 테스트 |
| `Assets/Tests/EditMode/Sim/RealBlockFactory.cs` | 수정 | HP 세팅 가능하게 확장(Damage 골든 전제) |

---

## Task 1: SimBlock 에 HP/분류 헬퍼 추가

**Files:**
- Modify: `Assets/Scripts/Sim/SimBlock.cs`
- Test: `Assets/Tests/EditMode/Sim/SimM2Tests.cs` (신규)

- [ ] **Step 1: 실패 테스트 작성**

`SimM2Tests.cs` 신규:

```csharp
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimM2Tests
    {
        [Test]
        public void SimBlock_분류헬퍼_특수블록을_구분한다()
        {
            SimBlock wall = new SimBlock { State = EBlockState.Wall };
            SimBlock potal = new SimBlock { State = EBlockState.Potal };
            SimBlock box = new SimBlock { State = EBlockState.CatBox1 };
            SimBlock creator = new SimBlock { State = EBlockState.WallCreator };
            SimBlock cat = new SimBlock { State = EBlockState.Cat1 };

            Assert.IsTrue(wall.IsWall(), "Wall.IsWall");
            Assert.IsTrue(box.IsCatBox(), "CatBox.IsCatBox");
            Assert.IsTrue(creator.IsCreator(), "Creator.IsCreator");
            Assert.IsFalse(cat.IsWall(), "Cat 은 Wall 아님");
            //# CheckHp: Creator 만 false(목표블록 아님)
            Assert.IsTrue(wall.CheckHp(), "Wall 은 목표블록");
            Assert.IsTrue(potal.CheckHp(), "Potal 은 목표블록");
            Assert.IsTrue(box.CheckHp(), "CatBox 는 목표블록");
            Assert.IsFalse(creator.CheckHp(), "Creator 는 목표블록 아님");
        }

        [Test]
        public void SimBlock_Damage_는_HP를_깎고_0이면_일반블록전환_플래그를_세운다()
        {
            SimBlock wall = new SimBlock { State = EBlockState.Wall, Hp = 2 };
            wall.Damage(blockTypeCount: 3);
            Assert.AreEqual(1, wall.Hp, "hp 2→1");
            Assert.AreEqual(EBlockState.None, wall.ChangeBlockState, "아직 전환 예약 없음");

            //# 같은 턴 재호출은 checkDamage 가드로 무시
            wall.Damage(blockTypeCount: 3);
            Assert.AreEqual(1, wall.Hp, "턴당 1회 — 변화 없음");

            //# 다음 턴: 리셋 후 한번 더 → hp 0 → 전환 예약
            wall.ResetCheckDamage();
            wall.Damage(blockTypeCount: 3);
            Assert.AreEqual(0, wall.Hp, "hp 1→0");
            Assert.IsTrue(wall.ChangeBlockState >= EBlockState.Cat1 && wall.ChangeBlockState <= EBlockState.Cat3,
                "hp 0 → 일반블록 전환 예약");
        }

        [Test]
        public void SimBlock_CatBox_는_Damage_무시()
        {
            SimBlock box = new SimBlock { State = EBlockState.CatBox1, Hp = 3 };
            box.ResetCheckDamage();
            box.Damage(blockTypeCount: 3);
            Assert.AreEqual(3, box.Hp, "박스는 Damage 로 안 깎임(CatInTheBox 로만)");
        }
    }
}
```

- [ ] **Step 2: 실행 → 실패 확인**

자체 러너 호출. Expected: 컴파일 에러(IsWall/Damage/Hp 등 없음) 또는 FAIL.

- [ ] **Step 3: SimBlock 확장 구현**

`SimBlock.cs` 를 다음으로 교체(기존 필드/메서드 보존 + 추가):

```csharp
using System;
using static Defines;

namespace CatPang.Sim
{
    //# 순수 데이터 블록. 실게임 Block 의 매치/낙하/HP 관련 필드만 미러. Unity 비종속.
    public class SimBlock
    {
        public EBlockState State;
        public int Row;
        public int Col;
        public bool Match;
        public bool SquareMatch;

        //# M2: HP/상태전이. Hp<=0 또는 -1 은 HP 없는 일반블록.
        public int Hp = -1;
        public EBlockState ChangeBlockState = EBlockState.None; //# 다음 상태전이 예약
        public int ChangeHp = -1;
        private bool _checkDamage; //# 턴당 1회 데미지 가드

        //# 일반 고양이 블록 여부. M1: Cat1~7.
        public bool IsNormal()
        {
            return State >= EBlockState.Cat1 && State <= EBlockState.Cat7;
        }

        public bool IsWall()
        {
            return State == EBlockState.Wall;
        }

        public bool IsCatBox()
        {
            return State >= EBlockState.CatBox1 && State <= EBlockState.CatBox5;
        }

        public bool IsCreator()
        {
            return State == EBlockState.WallCreator || State == EBlockState.PotalCreator;
        }

        //# 낙하를 막는 블록(실게임 Block.IsWallBlock): Wall/CatBox/Creator. Potal 은 안 막음.
        public bool IsWallLike()
        {
            return IsWall() || IsCatBox() || IsCreator();
        }

        //# 매치로 제거 가능한 블록인가(인접 데미지 대상 아님, 직접 매치 대상). 일반블록만.
        //# 목표블록 판정(실게임 CheckHpBlock): Creator 만 false.
        public bool CheckHp()
        {
            return IsCreator() == false;
        }

        //# 실게임 Block.Damage 포팅: 턴당 1회 + hp>=1 + 박스 아님. hp 0 이면 일반블록 전환 예약.
        public void Damage(int blockTypeCount, bool changeNormalBlock = true)
        {
            if (_checkDamage == false && Hp >= 1 && IsCatBox() == false)
            {
                _checkDamage = true;
                Hp -= 1;
                if (Hp <= 0 && changeNormalBlock)
                {
                    //# 시드 RNG 는 호출측에서 못 넘기므로 결정성은 호출 맥락(SimMatchChecker)이 책임.
                    //# 여기선 System.Random 정적 시드 없이 — 대신 SimMatchChecker 가 rng 주입 버전을 쓴다.
                    ChangeBlockState = (EBlockState)_damageRng.Next(0, blockTypeCount);
                }
            }
        }

        //# Damage 의 일반블록 전환 RNG. SetDamageRng 로 시드 주입(결정성).
        private static System.Random _damageRng = new System.Random(0);
        public static void SetDamageRng(System.Random rng) { _damageRng = rng; }

        public void ResetCheckDamage()
        {
            _checkDamage = false;
        }

        public void ResetMatch()
        {
            Match = false;
            SquareMatch = false;
        }
    }
}
```

> 주의: Damage 의 일반블록 전환에 RNG 가 필요한데 SimBlock 은 시드를 모른다. **정적 `_damageRng` + `SetDamageRng` 주입**으로 결정성 확보 — SimGame.Run 시작에서 `SimBlock.SetDamageRng(new System.Random(seed))` 호출(Task 7). 테스트는 전환 *범위*(Cat1~Cat3)만 검사하므로 시드 무관.

- [ ] **Step 4: 실행 → 통과 확인**

자체 러너. Expected: `PASS 22 / FAIL 0` (M1 19 + M2 3).

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Sim/SimBlock.cs Assets/Scripts/Sim/SimBlock.cs.meta Assets/Tests/EditMode/Sim/SimM2Tests.cs Assets/Tests/EditMode/Sim/SimM2Tests.cs.meta
git commit -m "# [feat] - SimBlock M2 HP/데미지/분류 헬퍼 추가"
```

---

## Task 2: SimStageData/Loader 에 HP 동반 로드

**Files:**
- Modify: `Assets/Scripts/Sim/SimStageData.cs`, `Assets/Scripts/Sim/SimStageLoader.cs`
- Test: `Assets/Tests/EditMode/Sim/SimM2Tests.cs`

- [ ] **Step 1: 실패 테스트 추가**

`SimM2Tests` 에 추가 (`using System.IO;` 필요 — 파일 상단에 추가):

```csharp
        private static string StageJson => System.IO.File.ReadAllText("Assets/AssetBundleResources/json/Stage.json");
        private static string StageBlockJson => System.IO.File.ReadAllText("Assets/AssetBundleResources/json/StageBlock.json");

        [Test]
        public void 로더_특수블록_HP를_InitialHps에_싣는다()
        {
            //# Wall/Potal/CatBox 가 있는 스테이지면 InitialHps 가 InitialStates 와 같은 길이로 채워진다.
            //# stage 6 은 M2 대상(Wall/Potal/CatBox 류) — InitialStates 와 InitialHps 길이 동일 확인.
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 6, normalMode: true);
            Assert.AreEqual(s.InitialStates.Length, s.InitialHps.Length, "HP 배열 길이 = 상태 배열 길이");
            //# 레코드 없는 칸(None)은 hp -1
            for (int i = 0; i < s.InitialStates.Length; ++i)
            {
                if (s.InitialStates[i] == EBlockState.None)
                {
                    Assert.AreEqual(-1, s.InitialHps[i], $"None 칸 {i} 은 hp -1");
                }
            }
        }
```

- [ ] **Step 2: 실행 → 실패 확인** (InitialHps 없음 — 컴파일 에러)

- [ ] **Step 3: SimStageData 에 InitialHps 추가**

`SimStageData.cs` 의 `EBlockState[] InitialStates;` 아래에 추가:

```csharp
        public int[] InitialHps; //# 길이 BoardSize^2. 각 칸 hp(StageBlock hp). 레코드 없으면 -1.
```

- [ ] **Step 4: SimStageLoader 가 InitialHps 채우게 수정**

`SimStageLoader.cs` 의 states 채우는 블록을 다음으로 교체:

```csharp
            EBlockState[] states = new EBlockState[size * size];
            int[] hps = new int[size * size];
            for (int i = 0; i < states.Length; ++i)
            {
                states[i] = EBlockState.None;
                hps[i] = -1;
            }
            foreach (StageBlockDto b in blocks.Where(b => b.stage == stage))
            {
                if (b.row >= 0 && b.row < size && b.col >= 0 && b.col < size)
                {
                    int idx = b.row * size + b.col;
                    states[idx] = (EBlockState)b.blockState;
                    hps[idx] = b.hp;
                }
            }
```

그리고 return 의 `InitialStates = states,` 아래에 `InitialHps = hps,` 추가.

- [ ] **Step 5: 실행 → 통과 확인**

자체 러너. Expected: `PASS 23 / FAIL 0`.

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Sim/SimStageData.cs Assets/Scripts/Sim/SimStageLoader.cs Assets/Tests/EditMode/Sim/SimM2Tests.cs
git commit -m "# [feat] - StageBlock HP 를 SimStageData.InitialHps 로 로드"
```

---

## Task 3: RealBlockFactory HP 세팅 확장 + Damage 골든

**Files:**
- Modify: `Assets/Tests/EditMode/Sim/RealBlockFactory.cs`
- Test: `Assets/Tests/EditMode/Sim/SimSpecialBlockGoldenTests.cs` (신규)

- [ ] **Step 1: RealBlockFactory 에 HP 세팅 추가**

`RealBlockFactory.cs` 의 `CreateBoard` 를, HP 도 세팅하는 오버로드로 확장. 기존 `CreateBoard(EBlockState[,])` 보존하고 추가:

```csharp
        private static readonly FieldInfo HpField =
            typeof(Block).GetField("hp", BindingFlags.NonPublic | BindingFlags.Instance);

        //# state + hp 를 함께 세팅. hps[r,c] = -1 이면 hp 미설정(일반블록).
        public static Block[,] CreateBoard(EBlockState[,] states, int[,] hps)
        {
            int size = states.GetLength(0);
            Block[,] arr = new Block[size, size];
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    GameObject go = new GameObject($"B{r}_{c}");
                    Block b = go.AddComponent<Block>();
                    b.row = r; b.col = c; b.index = r * size + c;
                    BlockStateField.SetValue(b, states[r, c]);
                    HpField.SetValue(b, hps[r, c]);
                    arr[r, c] = b;
                }
            }
            return arr;
        }
```

> 전제: `Block` 의 hp 필드명이 `hp`(private int). Block.cs:57 `[SerializeField, ReadOnly] int hp = 0;` 확인됨. 실패 시(필드명 다름) RealBlockFactory 의 리플렉션 대상명을 맞추고 보고.

- [ ] **Step 2: Damage 골든 테스트 작성**

`SimSpecialBlockGoldenTests.cs` 신규:

```csharp
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Defines;

namespace CatPang.Sim.Tests
{
    //# 실게임 GPMatchChecker 인접 데미지 ↔ SimMatchChecker 동등성 골든.
    public class SimSpecialBlockGoldenTests
    {
        //# 실게임: 3x3, 가운데 행 Cat1 3개(매치) + (0,1) Wall(hp2). 매치 제거 시 CheckArround 로 Wall 데미지.
        //# 실 GPMatchChecker.CheckMap → RemoveMatchBlock 경로 대신, 직접 CheckArround 를 호출해 인접데미지만 골든.
        [Test]
        public void 골든_매치인접_Wall이_데미지받는다()
        {
            //# 보드: row1 = Cat1 Cat1 Cat1 (가로3매치), (0,1)=Wall hp2 (매치블록 (1,1) 위)
            EBlockState[,] states =
            {
                { EBlockState.Cat2, EBlockState.Wall, EBlockState.Cat2 },
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };
            int[,] hps = { {-1,2,-1}, {-1,-1,-1}, {-1,-1,-1} };

            //# 실게임 골든: Wall 의 hp 가 매치블록 인접 데미지로 줄어드는지
            Block[,] arr = RealBlockFactory.CreateBoard(states, hps);
            GPBoard board = new GPBoard();
            board.Init(arr, 3, new Dictionary<EBlockState, Sprite>(), 0f, 0, default);
            GPMatchChecker matcher = new GPMatchChecker();
            matcher.Init(board, 5);
            matcher.CheckMap(test: false); //# 매치 표시 + ResetCheckWallDamage
            //# 매치된 (1,1) 의 위 칸 (0,1) Wall 에 인접 데미지
            matcher.CheckArround(1, 1);
            int realWallHp = arr[0, 1].GetHp();
            RealBlockFactory.Destroy(arr);

            //# Sim: 동일 보드 구성
            SimBoard sb = new SimBoard(3);
            for (int r = 0; r < 3; ++r)
                for (int c = 0; c < 3; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                    sb.Grid[r, c].Hp = hps[r, c];
                }
            SimMatchChecker sc = new SimMatchChecker();
            sc.CheckMap(sb);
            sc.CheckArround(sb, 1, 1);
            int simWallHp = sb.Grid[0, 1].Hp;

            Assert.AreEqual(realWallHp, simWallHp, "실게임 Wall hp 와 Sim Wall hp 일치");
        }
    }
}
```

> 주의: 이 테스트는 Task 4(SimMatchChecker.CheckArround) 가 있어야 컴파일된다. Task 3 Step 2~4 는 Task 4 와 한 묶음으로 진행 — Step 2 작성 후 바로 Task 4 구현, 그 뒤 함께 실행. (TDD: 실패 테스트 먼저, 구현은 Task 4)

- [ ] **Step 3: 실행 → 실패 확인** (SimMatchChecker.CheckArround 없음 — 컴파일 에러). Task 4 로 진행.

---

## Task 4: SimMatchChecker 인접 데미지 + checkDamage 리셋

**Files:**
- Modify: `Assets/Scripts/Sim/SimMatchChecker.cs`

- [ ] **Step 1: SimMatchChecker.CheckMap 에 ResetCheckDamage 추가 + CheckArround/DamageBlock 구현**

`SimMatchChecker.cs` 의 `CheckMap` 맨 앞 `board.ResetAllMatch();` 다음 줄에 추가:

```csharp
            //# 실게임 GPMatchChecker.CheckMap 42-44: 매 검사 시작 시 데미지 가드 리셋.
            foreach (SimBlock b in board.Grid)
            {
                b.ResetCheckDamage();
            }
```

그리고 클래스 끝(마지막 `}` 앞)에 메서드 추가:

```csharp
        //# 실게임 GPMatchChecker.CheckArround 223-241: (row,col) 의 상하좌우를 데미지.
        public void CheckArround(SimBoard board, int row, int col, int blockTypeCount = 5)
        {
            if (board.IsValid(row, col) == false)
                return;
            DamageBlock(board, row - 1, col, blockTypeCount);
            DamageBlock(board, row, col + 1, blockTypeCount);
            DamageBlock(board, row, col - 1, blockTypeCount);
            DamageBlock(board, row + 1, col, blockTypeCount);
        }

        //# 실게임 DamageBlock 232-241. M2 엔 RainbowPang 없으므로 일반 Damage.
        public void DamageBlock(SimBoard board, int row, int col, int blockTypeCount)
        {
            if (board.IsValid(row, col))
            {
                board.Grid[row, col].Damage(blockTypeCount);
            }
        }
```

- [ ] **Step 2: 실행 → 통과 확인 (Task 3 골든 + 본 태스크)**

자체 러너. Expected: `PASS 24 / FAIL 0` (23 + Damage 골든 1).

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Sim/SimMatchChecker.cs Assets/Tests/EditMode/Sim/RealBlockFactory.cs Assets/Tests/EditMode/Sim/SimSpecialBlockGoldenTests.cs Assets/Tests/EditMode/Sim/SimSpecialBlockGoldenTests.cs.meta
git commit -m "# [feat] - SimMatchChecker 인접 데미지 + 실게임 골든 동등성"
```

---

## Task 5: SimGravity Wall 낙하 차단

**Files:**
- Modify: `Assets/Scripts/Sim/SimGravity.cs`
- Test: `Assets/Tests/EditMode/Sim/SimM2Tests.cs`

- [ ] **Step 1: 실패 테스트 추가**

`SimM2Tests` 에 추가:

```csharp
        [Test]
        public void 낙하_Wall아래로는_위블록이_안떨어진다()
        {
            //# 열0: (0,0)Cat1, (1,0)Wall, (2,0)매치제거(빈칸).
            //# Wall 이 낙하 차단 → Cat1 은 Wall 위(0,0)에 머물고, 빈칸(2,0)은 리필.
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Cat1);
            board.SetState(1, 0, EBlockState.Wall); board.Grid[1,0].Hp = 1;
            board.SetState(2, 0, EBlockState.Cat2); board.Grid[2, 0].Match = true;

            new SimGravity(seed: 1).Apply(board, blockTypeCount: 3);

            Assert.AreEqual(EBlockState.Cat1, board.GetState(0, 0), "Cat1 은 Wall 위에 머묾");
            Assert.AreEqual(EBlockState.Wall, board.GetState(1, 0), "Wall 고정");
            Assert.IsTrue(board.Grid[2, 0].IsNormal(), "Wall 아래 빈칸은 리필");
        }
```

- [ ] **Step 2: 실행 → 실패 확인** (현재 SimGravity 는 Wall 무시하고 Cat1 을 바닥으로 떨어뜨림)

- [ ] **Step 3: SimGravity 에 Wall 차단 구현**

`SimGravity.cs` 의 `Apply` 안 열 순회를, Wall 을 경계로 **세그먼트별 압축**하도록 교체:

```csharp
        public void Apply(SimBoard board, int blockTypeCount)
        {
            int size = board.Size;
            for (int c = 0; c < size; ++c)
            {
                //# Wall 류(낙하 차단)를 경계로 열을 세그먼트로 나눠 각 세그먼트 안에서만 압축+리필.
                int segmentBottom = size - 1;
                for (int r = size - 1; r >= 0; --r)
                {
                    if (board.Grid[r, c].IsWallLike())
                    {
                        //# Wall 위 세그먼트 [r+1 .. segmentBottom] 압축+리필
                        CompactSegment(board, c, r + 1, segmentBottom, blockTypeCount);
                        segmentBottom = r - 1; //# Wall 위쪽이 다음 세그먼트 바닥
                    }
                }
                CompactSegment(board, c, 0, segmentBottom, blockTypeCount);
            }
            board.ResetAllMatch();
        }

        //# 한 세그먼트 [top..bottom] 에서 비매치 블록을 bottom 으로 압축, 남은 위 칸 랜덤 리필.
        private void CompactSegment(SimBoard board, int c, int top, int bottom, int blockTypeCount)
        {
            if (bottom < top)
                return;
            int writeRow = bottom;
            for (int r = bottom; r >= top; --r)
            {
                if (board.Grid[r, c].Match == false)
                {
                    board.Grid[writeRow, c].State = board.Grid[r, c].State;
                    board.Grid[writeRow, c].Hp = board.Grid[r, c].State == board.Grid[writeRow, c].State ? board.Grid[writeRow, c].Hp : board.Grid[r, c].Hp;
                    --writeRow;
                }
            }
            for (int r = writeRow; r >= top; --r)
            {
                board.Grid[r, c].State = (EBlockState)_rng.Next(0, blockTypeCount);
                board.Grid[r, c].Hp = -1;
            }
        }
```

> 주의: 기존 단순 `Apply` 의 State-만-이동 방식은 HP 가 칸에 매여 어긋난다(블록이 이동하면 HP 도 따라가야). 위 CompactSegment 는 State 이동 시 HP 도 함께 이동시키되, 리필 칸은 hp=-1. **HP 동행 이동 로직 정확성**은 Step 4 의 낙하 테스트 + Task 8 골든으로 검증. (M1 의 리필 시드 재현 테스트도 여전히 통과해야 함 — Wall 없으면 전체가 한 세그먼트라 동작 동일.)

- [ ] **Step 4: 실행 → 통과 확인**

자체 러너. Expected: `PASS 25 / FAIL 0` (24 + 낙하차단 1). **M1 의 `리필_시드가_같으면_결과가_같다` 가 여전히 PASS 인지 반드시 확인**(회귀) — Wall 없는 보드는 단일 세그먼트라 결과 동일해야.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Sim/SimGravity.cs Assets/Tests/EditMode/Sim/SimM2Tests.cs
git commit -m "# [feat] - SimGravity Wall 낙하 차단(세그먼트 압축)"
```

---

## Task 6: SimSpecialBlocks — CatBox 수집 + Creator + 상태전이

**Files:**
- Create: `Assets/Scripts/Sim/SimSpecialBlocks.cs`
- Test: `Assets/Tests/EditMode/Sim/SimSpecialBlockGoldenTests.cs`(CatBox 골든), `SimM2Tests.cs`(Creator/상태전이)

- [ ] **Step 1: CatBox 골든 테스트 추가** (`SimSpecialBlockGoldenTests` 에)

```csharp
        //# 실게임 Block.CatInTheBox ↔ Sim 동등성. CatBox1 위에 Cat1 → 박스 hp 감소 + 위블록 제거.
        [Test]
        public void 골든_CatBox1_위의_Cat1을_수집한다()
        {
            //# 실게임: CatBox1(hp2) 바로 위에 Cat1. CatInTheBox(Cat1) → true, 박스 hp 2→1.
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.CatBox1, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat1 },
            };
            int[,] hps = { {-1,-1,-1}, {2,-1,-1}, {-1,-1,-1} };

            Block[,] arr = RealBlockFactory.CreateBoard(states, hps);
            bool realCollected = arr[1, 0].CatInTheBox(arr[0, 0].GetBlockState());
            int realBoxHp = arr[1, 0].GetHp();
            RealBlockFactory.Destroy(arr);

            SimBoard sb = new SimBoard(3);
            for (int r = 0; r < 3; ++r)
                for (int c = 0; c < 3; ++c)
                {
                    sb.SetState(r, c, states[r, c]);
                    sb.Grid[r, c].Hp = hps[r, c];
                }
            bool simCollected = SimSpecialBlocks.CatInTheBox(sb.Grid[1, 0], sb.Grid[0, 0].State);
            int simBoxHp = sb.Grid[1, 0].Hp;

            Assert.AreEqual(realCollected, simCollected, "수집 판정 일치");
            Assert.AreEqual(realBoxHp, simBoxHp, "박스 hp 일치");
        }
```

- [ ] **Step 2: Creator/상태전이 단위 테스트 추가** (`SimM2Tests` 에)

```csharp
        [Test]
        public void 상태전이_ChangeBlockState가_다음턴_적용된다()
        {
            SimBoard board = new SimBoard(3);
            board.SetState(0, 0, EBlockState.Wall);
            board.Grid[0, 0].Hp = 0;
            board.Grid[0, 0].ChangeBlockState = EBlockState.Cat1;
            board.Grid[0, 0].ChangeHp = -1;

            SimSpecialBlocks.ApplyChanges(board);

            Assert.AreEqual(EBlockState.Cat1, board.GetState(0, 0), "예약된 상태로 전환");
            Assert.AreEqual(-1, board.Grid[0, 0].Hp, "전환 후 hp 적용");
            Assert.AreEqual(EBlockState.None, board.Grid[0, 0].ChangeBlockState, "예약 소비됨");
        }

        [Test]
        public void Creator_는_주변_일반블록을_변환예약한다()
        {
            //# WallCreator(hp2) 중앙, 사방 일반블록 → 한 칸이 Wall 변환 예약 + creator 데미지.
            SimBoard board = new SimBoard(3);
            for (int r = 0; r < 3; ++r)
                for (int c = 0; c < 3; ++c)
                {
                    board.SetState(r, c, EBlockState.Cat1);
                    board.Grid[r, c].Hp = -1;
                }
            board.SetState(1, 1, EBlockState.WallCreator);
            board.Grid[1, 1].Hp = 2;

            SimSpecialBlocks.RunCreators(board, new System.Random(1));

            //# creator 주변에 Wall 변환 예약(ChangeBlockState==Wall)된 칸이 정확히 1개
            int reserved = 0;
            foreach (SimBlock b in board.Grid)
            {
                if (b.ChangeBlockState == EBlockState.Wall) ++reserved;
            }
            Assert.AreEqual(1, reserved, "creator 가 일반블록 1개를 Wall 로 변환 예약");
            Assert.AreEqual(1, board.Grid[1, 1].Hp, "creator 자신 데미지 2→1");
        }
```

- [ ] **Step 3: 실행 → 실패 확인** (SimSpecialBlocks 없음)

- [ ] **Step 4: SimSpecialBlocks 구현**

`SimSpecialBlocks.cs` 신규:

```csharp
using System;
using static Defines;

namespace CatPang.Sim
{
    //# M2 정적 특수블록 로직: CatBox 수집, Creator 변환, 상태전이 적용.
    //# 실게임: Block.CatInTheBox / GPBombResolver.BlockCreatorBlock / GPGameScene.UpdateMap.
    public static class SimSpecialBlocks
    {
        //# 실게임 Block.GetBaseCat: 스킨 고양이를 원본 Cat1~5 로 환산. M2 보드엔 스킨 없으므로 Cat1~7 그대로.
        private static EBlockState BaseCat(EBlockState s)
        {
            if (s >= EBlockState.Cat1 && s <= EBlockState.Cat7)
                return s;
            return EBlockState.None;
        }

        //# 실게임 Block.CheckInBoxBlock: 박스가 받는 고양이인지.
        private static bool BoxAccepts(EBlockState boxState, EBlockState upState)
        {
            EBlockState cat = BaseCat(upState);
            switch (boxState)
            {
                case EBlockState.CatBox1: return cat == EBlockState.Cat1 || cat == EBlockState.Cat6;
                case EBlockState.CatBox2: return cat == EBlockState.Cat2 || cat == EBlockState.Cat7;
                case EBlockState.CatBox3: return cat == EBlockState.Cat3;
                case EBlockState.CatBox4: return cat == EBlockState.Cat4;
                case EBlockState.CatBox5: return cat == EBlockState.Cat5;
                default: return false;
            }
        }

        //# 실게임 Block.CatInTheBox 662-682: 받는 고양이면 박스 hp 감소(hp>0일 때), 항상 true 반환.
        public static bool CatInTheBox(SimBlock box, EBlockState upState)
        {
            if (box.IsCatBox() == false)
                return false;
            if (BoxAccepts(box.State, upState) == false)
                return false;
            if (box.Hp <= 0)
                return true;
            box.Hp -= 1;
            return true;
        }

        //# 보드 전체 CatBox 스캔: 박스 위 칸이 받는 고양이면 위 블록 제거(Match=true). 수집 발생 여부 반환.
        public static bool CollectCatBoxes(SimBoard board)
        {
            bool any = false;
            for (int r = 0; r < board.Size; ++r)
            {
                for (int c = 0; c < board.Size; ++c)
                {
                    SimBlock box = board.Grid[r, c];
                    if (box.IsCatBox() == false)
                        continue;
                    if (board.IsValid(r - 1, c) == false)
                        continue;
                    SimBlock up = board.Grid[r - 1, c];
                    if (CatInTheBox(box, up.State))
                    {
                        if (up.IsNormal())
                        {
                            up.Match = true;
                            any = true;
                        }
                    }
                }
            }
            return any;
        }

        //# 실게임 GPBombResolver.BlockCreatorBlock 462-492: creator 가 랜덤 방향 인접 일반블록 1개를 변환 예약.
        public static void RunCreators(SimBoard board, Random rng)
        {
            int size = board.Size;
            (int dr, int dc)[] dirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    SimBlock creator = board.Grid[r, c];
                    if (creator.IsCreator() == false)
                        continue;
                    if (creator.Hp == 0)
                        continue;

                    EBlockState target = creator.State == EBlockState.WallCreator
                        ? EBlockState.Wall
                        : EBlockState.Potal;

                    //# 실게임은 랜덤 시작 방향에서 인접 일반블록을 찾을 때까지 회전. 여기선 랜덤 시작 + 4방향 순회.
                    int start = rng.Next(0, 4);
                    for (int k = 0; k < 4; ++k)
                    {
                        (int dr, int dc) d = dirs[(start + k) % 4];
                        int nr = r + d.dr, nc = c + d.dc;
                        if (board.IsValid(nr, nc) == false)
                            continue;
                        SimBlock nb = board.Grid[nr, nc];
                        if (nb.IsNormal())
                        {
                            nb.ChangeBlockState = target;
                            nb.ChangeHp = 1;
                            creator.Damage(0, changeNormalBlock: false); //# creator 자신 데미지(전환 안 함)
                            break;
                        }
                    }
                }
            }
        }

        //# 실게임 GPGameScene.UpdateMap 의 changeBlockState 적용: 예약된 상태전이를 보드에 반영.
        public static void ApplyChanges(SimBoard board)
        {
            foreach (SimBlock b in board.Grid)
            {
                if (b.ChangeBlockState != EBlockState.None)
                {
                    b.State = b.ChangeBlockState;
                    b.Hp = b.ChangeHp;
                    b.ChangeBlockState = EBlockState.None;
                    b.ChangeHp = -1;
                    b.Match = false;
                }
            }
        }
    }
}
```

> 주의: `RunCreators` 의 creator.Damage 는 changeNormalBlock=false 라 hp 만 깎고 전환 예약 안 함(creator 는 hp 0 돼도 사라지지 않고 멈춤 — 실게임 동일). creator 데미지에 `ResetCheckDamage` 가 필요한지: 실게임 BlockCreatorBlock 은 턴당 1회 호출이고 Damage 의 checkDamage 가드가 같은 턴 매치데미지와 충돌 가능 → **RunCreators 직전에 creator 칸만 ResetCheckDamage** 하거나, 매치 해소와 다른 단계에서 호출. plan Task 7 통합 시 순서로 해결.

- [ ] **Step 5: 실행 → 통과 확인**

자체 러너. Expected: `PASS 28 / FAIL 0` (25 + CatBox골든1 + 상태전이1 + Creator1).

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Sim/SimSpecialBlocks.cs Assets/Scripts/Sim/SimSpecialBlocks.cs.meta Assets/Tests/EditMode/Sim/SimSpecialBlockGoldenTests.cs Assets/Tests/EditMode/Sim/SimM2Tests.cs
git commit -m "# [feat] - SimSpecialBlocks: CatBox 수집/Creator/상태전이"
```

---

## Task 7: SimGame 통합 — IsSupported 확장 + 턴 루프 + 승패 AND

**Files:**
- Modify: `Assets/Scripts/Sim/SimGame.cs`
- Test: `Assets/Tests/EditMode/Sim/SimM2Tests.cs`

- [ ] **Step 1: 승패 AND + IsSupported 테스트 추가**

`SimM2Tests` 에 추가:

```csharp
        [Test]
        public void IsSupported_M2블록은_지원_폭탄은_미지원()
        {
            //# Wall/Potal/CatBox 만 있는 stage6 → 지원. (M3 블록 없다고 가정한 M2 대상 스테이지)
            SimStageData m2 = SimStageLoader.Load(StageJson, StageBlockJson, 6, normalMode: true);
            SimResult r6 = new SimGame().Run(m2, new RandomAiPolicy(1), seed: 1);
            Assert.IsFalse(r6.Unsupported, "stage6(M2 블록)은 지원되어야");
        }

        [Test]
        public void 승패_목표블록_남으면_점수도달해도_미클리어()
        {
            //# 인위적 stage: targetScore=0(점수조건 무시) 인데 Wall(hp1) 남아있으면 미클리어여야.
            //# Run 직접 구성이 어려우므로 CheckClear 를 간접 검증: 보드에 hp>0 Wall 있으면 false.
            SimStageData s = SimStageLoader.Load(StageJson, StageBlockJson, 6, normalMode: true);
            //# 강제로 목표점수 0(목표블록만 보게) + 이동 1 → 한 수로 못 깨면 목표블록 남아 미클리어
            s.TargetScore = 0;
            s.MoveCount = 1;
            SimResult r = new SimGame().Run(s, new RandomAiPolicy(1), seed: 1);
            //# stage6 은 Wall/Potal/CatBox 가 많아 1수로 전부 못 없앰 → 목표블록 남음 → MoveOver
            Assert.IsFalse(r.Clear, "목표블록 남으면 점수0이어도 미클리어");
            Assert.AreEqual(EFailReason.MoveOver, r.FailReason);
        }
```

> 주의: stage6 의 실제 블록 구성에 따라 두 번째 테스트가 1수로 우연히 클리어될 수도 있다. 구현 후 실패하면 **목표블록이 확실히 2개 이상 남는 스테이지로 교체**(StageBlock.json 에서 Wall/Potal hp 합이 큰 stage 선택)하고 보고.

- [ ] **Step 2: 실행 → 실패 확인** (IsSupported 가 아직 M2 블록 미지원 → r6.Unsupported=true)

- [ ] **Step 3: SimGame 수정 — IsSupported / BuildInitialBoard HP / 턴루프 / CheckClear**

`SimGame.cs` 를 다음 변경들로 수정:

(a) `IsSupported` 교체:

```csharp
        //# M2 지원 = 시간모드 아님 + 모든 칸이 {None, Cat1~7, Wall, Potal, Creator, CatBox}.
        //# Fish/Ball/Arrow/특수폭탄/Rainbow/CatPang 있으면 미지원(→M3).
        private static bool IsSupported(SimStageData s)
        {
            if (s.IsTimeMode)
                return false;

            foreach (EBlockState st in s.InitialStates)
            {
                if (st == EBlockState.None)
                    continue;
                if (st >= EBlockState.Cat1 && st <= EBlockState.Cat7)
                    continue;
                if (st == EBlockState.Wall || st == EBlockState.Potal)
                    continue;
                if (st == EBlockState.WallCreator || st == EBlockState.PotalCreator)
                    continue;
                if (st >= EBlockState.CatBox1 && st <= EBlockState.CatBox5)
                    continue;
                return false;
            }
            return true;
        }
```

(b) `Run` 시작에 데미지 RNG 시드 주입 — `SimGravity gravity = new SimGravity(seed);` 다음 줄에:

```csharp
            SimBlock.SetDamageRng(new System.Random(seed));
```

(c) `BuildInitialBoard` 가 HP 도 세팅 — `board.SetState(r, c, st);` 를 다음으로:

```csharp
                    board.SetState(r, c, st);
                    board.Grid[r, c].Hp = s.InitialStates[r * s.BoardSize + c] == EBlockState.None
                        ? -1
                        : s.InitialHps[r * s.BoardSize + c];
```

(d) `ResolveCascades` 를 M2 통합 버전으로 교체 (인접데미지 + CatBox + Creator + 상태전이):

```csharp
        //# 매치 제거→점수→인접데미지→낙하/리필→CatBox수집→상태전이→재검사. 매치/수집 없을 때까지(연쇄).
        private void ResolveCascades(SimBoard board, SimMatchChecker checker, SimGravity gravity, SimStageData s, ref SimResult result)
        {
            checker.CheckMap(board);
            while (checker.IsMatch)
            {
                int cleared = 0;
                //# 매치 제거 블록 점수 + 인접 데미지(Wall/Potal 깎기).
                for (int r = 0; r < board.Size; ++r)
                {
                    for (int c = 0; c < board.Size; ++c)
                    {
                        if (board.Grid[r, c].Match)
                        {
                            ++cleared;
                            checker.CheckArround(board, r, c, s.BlockTypeCount);
                        }
                    }
                }
                result.FinalScore += cleared;
                result.Turns += 1;

                gravity.Apply(board, s.BlockTypeCount);     //# 매치=true 제거+낙하(Wall 차단)+리필
                SimSpecialBlocks.ApplyChanges(board);        //# Damage 로 예약된 일반블록 전환 적용

                //# CatBox 수집(위 블록 제거) → 발생하면 다시 낙하/검사
                if (SimSpecialBlocks.CollectCatBoxes(board))
                {
                    gravity.Apply(board, s.BlockTypeCount);
                    SimSpecialBlocks.ApplyChanges(board);
                }

                checker.CheckMap(board);
            }
        }
```

(e) 턴 루프에 Creator 1회 호출 — `result.MovesUsed += 1;` 다음, `ResolveCascades` 호출 전에:

```csharp
                //# 유효 드래그마다 Creator 가 주변 블록 변환(실게임 AfterDrag 의 checkCreateBlock 1회).
                SimSpecialBlocks.RunCreators(board, _creatorRng);
                SimSpecialBlocks.ApplyChanges(board);
```

그리고 클래스 필드 추가(MaxTurns 아래):

```csharp
        private System.Random _creatorRng;
```

`Run` 의 `SimBlock.SetDamageRng(...)` 다음에:

```csharp
            _creatorRng = new System.Random(seed + 1);
```

(f) `CheckClear` 를 목표블록 AND 점수로 교체:

```csharp
        //# 승패: 목표블록(checkHp 이고 hp>0, 또는 Fish/Ball) 없음 AND 점수도달. (실게임 Update 177-206)
        private static bool CheckClear(SimBoard board, SimStageData s, int score)
        {
            //# 1. 목표블록 제거 검사
            for (int r = 0; r < board.Size; ++r)
            {
                for (int c = 0; c < board.Size; ++c)
                {
                    SimBlock b = board.Grid[r, c];
                    if (b.State == EBlockState.RainbowPang)
                        continue;
                    if (b.CheckHp() == false)
                        continue;
                    //# M3 대비 Fish/Ball 검사 자리(M2 엔 해당 블록 없음).
                    if (b.Hp > 0)
                        return false;
                }
            }
            //# 2. 점수
            if (s.TargetScore > 0 && score < s.TargetScore)
                return false;
            return true;
        }
```

그리고 **`CheckClear` 호출부 3곳**의 시그니처를 `CheckClear(s, result.FinalScore)` → `CheckClear(board, s, result.FinalScore)` 로 변경.

- [ ] **Step 4: 실행 → 통과 확인**

자체 러너. Expected: `PASS 30 / FAIL 0` (28 + IsSupported1 + 승패AND1). M1/M2 기존 테스트 전부 유지 확인.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Sim/SimGame.cs Assets/Tests/EditMode/Sim/SimM2Tests.cs
git commit -m "# [feat] - SimGame M2 통합: IsSupported 확장+턴루프+승패 AND"
```

---

## Task 8: 배치 러너 커버리지 검증 (59/150)

**Files:**
- Modify: `Assets/Scripts/Sim/Editor/SimBatchRunner.cs` (모드 분류에 M2 블록 반영 — 필요 시)
- Test: 수동 배치 실행

- [ ] **Step 1: 배치 러너 실행**

메인이 `editor_invoke_method` 로 `CatPang.Sim.EditorTools.SimBatchRunner.RunBatch()` 호출 → `docs/qa-reports/sim-output/m2-batch-*.json` 생성 확인. (러너 코드는 M1 그대로 — Run 이 M2 블록을 supported 처리하면 자동 반영)

- [ ] **Step 2: 커버리지 집계 (PowerShell)**

```powershell
Set-Location D:\Project_CP
$j = Get-Content (Get-ChildItem "docs/qa-reports/sim-output/m2-batch-*.json" | Sort-Object LastWriteTime | Select-Object -Last 1).FullName -Raw | ConvertFrom-Json
$r = $j.metrics | Where-Object {$_.policy -eq "Random"}
$played = ($r | Where-Object {$_.plays -gt 0}).Count
"M2 measurable stage: $played / 150 (목표 59)"
```

Expected: **measurable ≈ 59**. 크게 다르면(예: <50 또는 >70) IsSupported 로직 또는 stage 데이터 재확인 후 보고.

- [ ] **Step 3: 결과를 plan 헤더에 기록 + 커밋**

plan 파일 상단에 실측 커버리지(M2 측정 stage 수, 모드 분포) 한 줄 추가.

```bash
git add docs/superpowers/plans/2026-06-03-headless-sim-harness-m2.md
git commit -m "# [docs] - M2 배치 커버리지 실측 기록"
```

---

## Self-Review (작성자 점검 — spec 대조)

**1. Spec 커버리지:**
- HP/데미지 모델 → Task 1(SimBlock) + Task 4(인접데미지) ✅
- CatBox 수집 → Task 6 ✅
- Creator → Task 6 ✅
- 상태 전이(UpdateMap) → Task 6 ApplyChanges ✅
- 승패 목표블록 AND 점수 → Task 7 CheckClear ✅
- 낙하 차단 → Task 5 ✅
- StageBlock hp 로드 → Task 2 ✅
- IsSupported 갱신 → Task 7 ✅
- 실게임 골든(Damage/CatInTheBox) → Task 3/4(Damage), Task 6(CatBox) ✅
- 수작업 골든(Creator/승패/낙하) → Task 5/6/7 ✅
- 커버리지 59 검증 → Task 8 ✅

**2. 검증 기준선:** M1 19 → Task1 22 → T2 23 → T4 24 → T5 25 → T6 28 → T7 30. 누적 일관.

**3. 알려진 리스크 (실행 중 확인):**
- Task 3: `Block` hp 필드명 `hp` 가정 — 다르면 RealBlockFactory 리플렉션 수정.
- Task 5: HP 동행 낙하 — M1 시드 재현 회귀 반드시 확인.
- Task 6: Creator 데미지 checkDamage 가드 ↔ 매치데미지 충돌 — Task 7 통합 순서(RunCreators 를 ResolveCascades 와 분리된 단계로)로 회피.
- Task 7: 승패 테스트 stage6 가 1수 우연 클리어 가능 — 실패 시 목표블록 많은 stage 로 교체.
- Creator/승패 AND 는 실게임 골든이 아닌 수작업 골든 — M2 리포트에 self-referential 경계 명기.

**4. 타입 일관성:** `SimBlock.{Hp,ChangeBlockState,ChangeHp,IsWall,IsCatBox,IsCreator,IsWallLike,CheckHp,Damage,ResetCheckDamage,SetDamageRng}`, `SimMatchChecker.{CheckArround,DamageBlock}`, `SimSpecialBlocks.{CatInTheBox,CollectCatBoxes,RunCreators,ApplyChanges}`, `SimStageData.InitialHps`, `SimGame.CheckClear(board,s,score)` — 태스크 간 시그니처 일치 확인.
