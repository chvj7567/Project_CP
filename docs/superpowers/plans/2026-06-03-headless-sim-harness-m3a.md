# 헤드리스 시뮬 하니스 M3a (폭탄 시스템) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** CatPang 폭탄 계열 블록(Arrow1~6·CatPang·5색폭탄·Rainbow)의 생성·발동·연쇄·조합을 헤드리스 Sim 에서 측정 가능하게 한다.

**Architecture:** M2 토대(`Assets/Scripts/Sim/`) 확장. 실게임 `GPMatchChecker`/`GPBombResolver` 의 순수 로직을 미러하는 Sim 클래스를 추가하고, 각 순수 조각(매치추적·발동기하·생성)을 실게임 직호출 골든으로 검증. 연쇄·조합·턴루프 통합은 수작업+단위 테스트(M1/M2 하이브리드).

**Tech Stack:** Unity 6, C#, NUnit EditMode, 자작 `SimTestRunner`(MCP `editor_invoke_method` 로 실행).

**규약:** Rule 01(자동 `git commit` 금지 — staging + 한글 커밋 메시지 제안만). Rule 02(`//#` 주석, `var` 금지, `!` 금지 → `== false`/`== null`, 가드절 중괄호 없이 개행). 테스트는 plain 무인자 `[Test]`. `.meta` 는 Unity 가 생성하므로 만들지 않음.

**참조 파일(실게임 — 골든 원천):**
- `Assets/Scripts/GamePlay/GPMatchChecker.cs` — CheckMap(L34), CheckSquareMatch(L69), Check3Match(L110, `SetScore(matchCount, direction)` 로 hScore/vScore 설정), SetMoveIndices(L28)
- `Assets/Scripts/GamePlay/GPBombResolver.cs` — Bomb1~12(L95~310 고정 기하), BoomAll(L86), Boom3(L125), RainbowPang(L312), CreateBombBlock(L346)
- `Assets/Scripts/Scenes/GPGameScene.cs` — AfterDrag 조합 디스패치(L785~804)
- `Assets/Scripts/Block.cs` — Damage(L409), IsFixdBlock(L516), CanNotDragBlock(L553), 분류 헬퍼

**기존 Sim 패턴(따를 것):**
- 골든 헬퍼: `Assets/Tests/EditMode/Sim/RealBlockFactory.cs` — `CreateBoard(states)` / `CreateBoard(states, hps)` (리플렉션으로 private `blockState`/`hp` 세팅, UI NRE 회피), `Destroy(arr)`
- 자동 러너: `Assets/Scripts/Editor/SimTestRunner.cs::RunSimTests()` — `CatPang.Sim.Tests` 네임스페이스 전체 실행, "PASS n / FAIL m" 반환
- 실행 루프: 파일 편집 → MCP `editor_refresh_assets {}` → `editor_wait_ready`(phases 에 `compiling` 확인) → `editor_invoke_method`(className `CatPang.Sim.EditorTools.SimTestRunner`, methodName `RunSimTests`)
- 기존 회귀 기준선: **PASS 33 / FAIL 0** (각 Task 후 유지 확인)

---

## File Structure

| 파일 | 신규/수정 | 책임 |
|---|---|---|
| `Assets/Scripts/Sim/SimBlock.cs` | 수정 | `HScore`/`VScore`/`SquareMatch` 필드, 폭탄 분류(`IsBomb`/`IsSpecialBomb`/`IsArrow`/`IsCatPang`), `SetScore`, `ResetScore`, `CanNotDrag` 에 RainbowPang 추가 |
| `Assets/Scripts/Sim/SimMatchChecker.cs` | 수정 | CheckMap 에서 hScore/vScore/squareMatch 추적, moveIndex 추적, `ChangeMatchState(r,c)` |
| `Assets/Scripts/Sim/SimBombResolver.cs` | 신규 | Bomb1~12 / BoomAll / Boom3 / RainbowPang 발동 기하 (ChangeMatchState 셀 집합) |
| `Assets/Scripts/Sim/SimBombFactory.cs` | 신규 | 매치→특수블록 생성 (실 CreateBombBlock 포팅) |
| `Assets/Scripts/Sim/SimComboResolver.cs` | 신규 | 두 블록 스왑 조합 디스패치 (실 AfterDrag 787~804) |
| `Assets/Scripts/Sim/SimGame.cs` | 수정 | 턴루프에 생성·발동·연쇄·조합 통합, IsSupported 에 M3a 블록 |
| `Assets/Scripts/Sim/ISimAiPolicy.cs` | 수정 | 폭탄 스왑/조합 수를 탐색에 포함 |
| `Assets/Tests/EditMode/Sim/RealBlockFactory.cs` | 수정 | 골든 스파이크용 `CreateGPBoard`/`CreateGPMatchChecker`/`CreateGPBombResolver` 헬퍼 추가 |
| `Assets/Tests/EditMode/Sim/SimBombSpikeTests.cs` | 신규 | Task1 스파이크 |
| `Assets/Tests/EditMode/Sim/SimMatchScoreGoldenTests.cs` | 신규 | Task2 골든 |
| `Assets/Tests/EditMode/Sim/SimBombResolverGoldenTests.cs` | 신규 | Task4 골든 |
| `Assets/Tests/EditMode/Sim/SimBombFactoryGoldenTests.cs` | 신규 | Task5 골든 |
| `Assets/Tests/EditMode/Sim/SimBombIntegrationTests.cs` | 신규 | Task6~8 단위 |

---

## Task 1: 골든 스파이크 — 실 GPBombResolver/GPMatchChecker EditMode 호출성 검증

**이 Task 의 통과 여부가 나머지 골든 접근 전체를 좌우한다(M1 선례). 반드시 결과 확인 후 진행.**

리스크: 실 `GPBombResolver.BombN` 은 `SaveBombCollectionData` → `CHMData.Instance.GetCollectionData(...)` 접근(L59~64). bare EditMode 에서 `CHMData.Instance` 가 null/미초기화면 NRE(M2 hpText 와 동형). 또한 `_createEffect`/`_playSound`/`_onBoomTrigger` 콜백 미주입 시 NRE.

**Files:**
- Modify: `Assets/Tests/EditMode/Sim/RealBlockFactory.cs`
- Create: `Assets/Tests/EditMode/Sim/SimBombSpikeTests.cs`

- [ ] **Step 1: RealBlockFactory 에 GP 객체 빌더 추가**

`RealBlockFactory.cs` 에 메서드 추가(기존 `using` 유지). `GPBoard.Init` 시그니처는 `Assets/Scripts/GamePlay/GPBoard.cs` 에서 확인 후 맞춘다(M2 `SimSpecialBlockGoldenTests` 가 이미 `board.Init(arr, size, new Dictionary<EBlockState,Sprite>(), 0f, 0, default)` 형태로 호출 — 그 시그니처 재사용).

```csharp
//# 실 GPBoard + GPMatchChecker 를 EditMode 에서 구성(렌더/UI 없이).
public static GPBoard CreateGPBoard(Block[,] arr, int blockTypeCount)
{
    int size = arr.GetLength(0);
    GPBoard board = new GPBoard();
    board.Init(arr, size, new System.Collections.Generic.Dictionary<EBlockState, Sprite>(), 0f, blockTypeCount, default);
    return board;
}

public static GPMatchChecker CreateGPMatchChecker(GPBoard board, int blockTypeCount)
{
    GPMatchChecker matcher = new GPMatchChecker();
    matcher.Init(board, blockTypeCount);
    return matcher;
}
```

- [ ] **Step 2: 스파이크 테스트 작성 — GPMatchChecker.CheckMap 호출성**

`SimBombSpikeTests.cs`:

```csharp
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimBombSpikeTests
    {
        //# 스파이크 A: 실 GPMatchChecker.CheckMap 이 bare EditMode 에서 NRE 없이 돌고
        //# 가로 4매치 블록의 hScore 가 SetScore 로 4가 되는지(폭탄생성 골든 전제).
        [Test]
        public void 스파이크_실매치체커_가로4매치_hScore설정()
        {
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard board = RealBlockFactory.CreateGPBoard(arr, 5);
            GPMatchChecker matcher = RealBlockFactory.CreateGPMatchChecker(board, 5);

            matcher.CheckMap(test: false);

            //# (0,0)~(0,3) 가로 4매치 → 각 hScore=4
            int hScore00 = arr[0, 0].hScore;
            RealBlockFactory.Destroy(arr);
            Assert.AreEqual(4, hScore00, "가로 4매치 블록 hScore=4 (실게임 SetScore)");
        }
    }
}
```

`arr[0,0].hScore` 가 public 인지 확인(`Block.cs`). private 이면 RealBlockFactory 에 리플렉션 getter 추가하거나 `Block` 의 접근자를 쓴다. (`Block` 에 `hScore`/`vScore`/`squareMatch` 는 public 필드로 존재 — `GPBombResolver` 가 직접 읽으므로.)

- [ ] **Step 3: 스파이크 테스트 작성 — GPBombResolver.Bomb1 호출성 + CHMData 우회**

같은 파일에 추가. 목표: 실 `Bomb1` 을 stub 콜백으로 호출했을 때 NRE 지점을 특정. `_matcher.ChangeMatchState` 가 match 를 칠 셀을 캡처.

```csharp
        //# 스파이크 B: 실 GPBombResolver.Bomb1(3x3) 을 stub 으로 호출 가능한지 + CHMData NRE 여부.
        //# CHMData.Instance 가 NRE 면 이 테스트가 그 예외로 FAIL → 우회법(EnsureInit or 캡처 후 SaveBombCollectionData 회피) 확정.
        [Test]
        public void 스파이크_실밤리졸버_Bomb1_3x3_셀캡처()
        {
            EBlockState[,] states = Fill5x5(EBlockState.Cat1);
            states[2, 2] = EBlockState.CatPang; //# 중앙에 CatPang(Bomb1 대상)
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard board = RealBlockFactory.CreateGPBoard(arr, 5);
            GPMatchChecker matcher = RealBlockFactory.CreateGPMatchChecker(board, 5);

            GPBombResolver resolver = new GPBombResolver();
            //# stub: effect=null 반환, sound/onBoom no-op, pangEffectList 는 더미.
            resolver.Init(board, matcher,
                onBoomTrigger: () => System.Threading.Tasks.Task.CompletedTask,
                addBonusScore: _ => { },
                createEffect: (ps, pos) => null,
                playSound: _ => { },
                pangEffectList: new System.Collections.Generic.List<ParticleSystem>(),
                bombEffectPS: null,
                token: default);

            //# CHMData NRE 가능 지점. 통과하면 우회 불필요, FAIL 이면 메시지로 NRE 라인 확인.
            resolver.Bomb1(arr[2, 2], ani: false).GetAwaiter().GetResult();

            int matchCount = 0;
            foreach (Block b in arr)
                if (b.match) ++matchCount;
            RealBlockFactory.Destroy(arr);

            //# Bomb1 = 자기 포함 3x3 = 9칸
            Assert.AreEqual(9, matchCount, "Bomb1 은 3x3 9칸 match");
        }

        private static EBlockState[,] Fill5x5(EBlockState s)
        {
            EBlockState[,] a = new EBlockState[5, 5];
            for (int r = 0; r < 5; ++r)
                for (int c = 0; c < 5; ++c)
                    a[r, c] = s;
            return a;
        }
```

- [ ] **Step 4: 컴파일 + 실행, 결과 판정**

MCP: `editor_refresh_assets {}` → `editor_wait_ready`(compiling 확인) → `editor_invoke_method` RunSimTests.

- **PASS 35 (33 + 2)**: 골든 접근 유효 → Task 2 진행.
- **스파이크 B FAIL with NullReferenceException(CHMData)**: 우회 확정 — 다음 중 하나를 RealBlockFactory/테스트 setup 에 적용 후 재실행:
  1. `CHMData.EnsureInit()` 또는 `CHMData.Instance` 초기화를 `[SetUp]` 대신 테스트 본문 첫 줄에서 호출(가능하면).
  2. 불가하면 `SaveBombCollectionData` 를 회피: `match` 셀 캡처만 필요하므로, `Bomb1` 대신 그 내부의 `ChangeMatchState` 호출부와 동일한 결과를 주는 `matcher.ChangeMatchState` 직접 호출 골든으로 전환(geometry 만 골든, collection 부수효과 제외). 이 경우 Task4 골든도 "matcher.ChangeMatchState 직접" 방식으로 통일.
- 우회법 확정 후 이 Task 의 스파이크 테스트가 GREEN 이 될 때까지 반복.

- [ ] **Step 5: 스테이징 + 커밋 메시지 제안 (Rule 01)**

```
git add Assets/Tests/EditMode/Sim/RealBlockFactory.cs Assets/Tests/EditMode/Sim/SimBombSpikeTests.cs Assets/Tests/EditMode/Sim/SimBombSpikeTests.cs.meta
```
제안: `# [test] - Sim M3a 골든 스파이크: 실 GPMatchChecker/GPBombResolver EditMode 호출성 검증`

---

## Task 2: SimMatchChecker — hScore/vScore/squareMatch 추적 + 골든

**Files:**
- Modify: `Assets/Scripts/Sim/SimBlock.cs`
- Modify: `Assets/Scripts/Sim/SimMatchChecker.cs`
- Create: `Assets/Tests/EditMode/Sim/SimMatchScoreGoldenTests.cs`

- [ ] **Step 1: SimBlock 에 점수 필드 + SetScore/ResetScore 추가**

`SimBlock.cs` 에 추가(기존 필드 아래):

```csharp
//# M3a: 매치 런렝스. 실 Block.hScore/vScore/squareMatch 미러. CreateBombBlock 이 읽는다.
public int HScore;
public int VScore;
public bool SquareMatch;

//# 실 Block.SetScore(count, direction): 가로/세로 매치 길이 기록.
public void SetScore(int count, bool horizontal)
{
    if (horizontal)
        HScore = count;
    else
        VScore = count;
}

public void ResetScore()
{
    HScore = 0;
    VScore = 0;
    SquareMatch = false;
}
```

`ResetMatch()`(기존) 에 점수 리셋도 포함하도록 수정:

```csharp
public void ResetMatch()
{
    Match = false;
    SquareMatch = false;
    HScore = 0;
    VScore = 0;
}
```

- [ ] **Step 2: SimMatchChecker 에 moveIndex + 점수 추적 추가**

`SimMatchChecker.cs` 수정. 필드 추가:

```csharp
//# 이번 수에서 이동한 두 블록 index(실 SetMoveIndices). squareMatch 위치 결정에 사용.
private int _moveIndex1 = -1;
private int _moveIndex2 = -1;
public void SetMoveIndices(int idx1, int idx2) { _moveIndex1 = idx1; _moveIndex2 = idx2; }
```

`CheckMap` 의 `Check3` 호출을 방향 인자와 함께 점수 기록하도록, 그리고 `CheckSquare` 에 squareMatch 위치 로직(실 L88~104)을 반영한다. `Check3(line)` 시그니처를 `Check3(line, horizontal)` 로 바꾸고 매치 시 `b.SetScore(count, horizontal)` 호출. 가로 호출은 `Check3(rowLine, true)`, 세로는 `Check3(colLine, false)`.

`CheckSquare` 의 `a.SquareMatch = true` 부분을 실게임처럼 moveIndex 우선으로:

```csharp
//# 실 CheckSquareMatch L95~104: 이동한 블록을 squareMatch 우선, 없으면 좌상단.
if (_moveIndex1 == a.Index || _moveIndex2 == a.Index) a.SquareMatch = true;
else if (_moveIndex1 == b.Index || _moveIndex2 == b.Index) b.SquareMatch = true;
else if (_moveIndex1 == d.Index || _moveIndex2 == d.Index) d.SquareMatch = true;
else if (_moveIndex1 == e.Index || _moveIndex2 == e.Index) e.SquareMatch = true;
else a.SquareMatch = true;
```

`SimBlock` 에 `Index` 필드가 없으면 추가(`public int Index;`), `SimBoard` 생성 시 `Index = r*size+c` 세팅. CheckMap 시작의 ResetAllMatch 가 ResetMatch 를 부르므로 점수도 리셋됨(Step1 에서 반영).

- [ ] **Step 3: 골든 테스트 — 실 GPMatchChecker 와 hScore/vScore/squareMatch 동등**

`SimMatchScoreGoldenTests.cs`:

```csharp
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimMatchScoreGoldenTests
    {
        //# 가로 4매치 보드에서 실게임/Sim 의 hScore 가 일치하는지.
        [Test]
        public void 골든_가로4매치_hScore_실게임과_일치()
        {
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };

            //# 실게임
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard gb = RealBlockFactory.CreateGPBoard(arr, 5);
            GPMatchChecker gm = RealBlockFactory.CreateGPMatchChecker(gb, 5);
            gm.CheckMap(test: false);
            int[] realH = new int[5];
            for (int c = 0; c < 5; ++c) realH[c] = arr[0, c].hScore;
            RealBlockFactory.Destroy(arr);

            //# Sim
            SimBoard sb = new SimBoard(5);
            for (int r = 0; r < 5; ++r)
                for (int c = 0; c < 5; ++c)
                    sb.SetState(r, c, states[r, c]);
            SimMatchChecker sc = new SimMatchChecker();
            sc.CheckMap(sb);

            for (int c = 0; c < 5; ++c)
                Assert.AreEqual(realH[c], sb.Grid[0, c].HScore, $"(0,{c}) hScore 일치");
        }
    }
}
```

- [ ] **Step 4: 컴파일 + 실행 → PASS(기존+신규) 확인. 회귀 PASS 유지.**

MCP RunSimTests. 기대: 기존 + 스파이크 + 신규 골든 전부 GREEN.

- [ ] **Step 5: 스테이징 + 메시지 제안**

`# [feat] - Sim M3a: SimMatchChecker hScore/vScore/squareMatch 추적 + 실게임 골든`

---

## Task 3: SimBlock 폭탄 분류 + ChangeMatchState + Bomb() 디스패치

**Files:**
- Modify: `Assets/Scripts/Sim/SimBlock.cs`
- Modify: `Assets/Scripts/Sim/SimMatchChecker.cs`
- Test: `Assets/Tests/EditMode/Sim/SimBombIntegrationTests.cs` (신규, 이후 Task 에서 계속 확장)

- [ ] **Step 1: SimBlock 분류 헬퍼 추가 (실 Block 분류 미러)**

실 `Block.cs` 의 `IsBombBlock`/`IsSpecialBombBlock` 정의를 확인 후 미러. `IsBomb` = CatPang(18)+Arrow1~6(10~15). `IsSpecialBomb` = 5색폭탄(Pink23/Green22/Orange20/Blue21/Yellow19). (정확한 집합은 `Block.IsBombBlock`/`IsSpecialBombBlock` 소스로 검증.)

```csharp
public bool IsArrow() => State >= EBlockState.Arrow1 && State <= EBlockState.Arrow6;
public bool IsCatPang() => State == EBlockState.CatPang;
//# 실 Block.IsBombBlock 미러(검증 후 집합 확정): CatPang + Arrow1~6.
public bool IsBomb() => IsCatPang() || IsArrow();
//# 실 Block.IsSpecialBombBlock 미러: 5색폭탄.
public bool IsSpecialBomb()
{
    return State == EBlockState.PinkBomb || State == EBlockState.GreenBomb
        || State == EBlockState.OrangeBomb || State == EBlockState.BlueBomb
        || State == EBlockState.YellowBomb;
}
```

`CanNotDrag()` 에 RainbowPang 추가:

```csharp
public bool CanNotDrag()
{
    return IsWall() || State == EBlockState.Potal || IsCatBox() || IsCreator()
        || State == EBlockState.RainbowPang;
}
```

- [ ] **Step 2: SimMatchChecker.ChangeMatchState(r,c) 추가**

실 `GPMatchChecker.ChangeMatchState`(L248~252) 미러: 범위 안 + Fish/Fixd/PinkBomb 아니면 match=true. M3a 엔 Fish 없음(M3b). RainbowPang hp>0 가드 포함.

```csharp
//# 실 GPMatchChecker.ChangeMatchState 248~252: 폭탄 발동이 칸을 match 처리.
public void ChangeMatchState(SimBoard board, int row, int col)
{
    if (board.IsValid(row, col) == false)
        return;
    SimBlock b = board.Grid[row, col];
    if (b.State == EBlockState.RainbowPang && b.Hp > 0)
        return;
    //# 실게임: !IsFish && !IsFixd && != PinkBomb 이면 match. (M3a: Fish 없음 → Fixd/Pink 만 제외)
    if (b.IsCreator() || b.IsWall() || b.IsCatBox() || b.State == EBlockState.Potal)
        return;
    if (b.State == EBlockState.PinkBomb)
        return;
    b.Match = true;
}
```

- [ ] **Step 3: SimBlock.Bomb() 디스패치 추가(상태→어느 BombN)**

발동 매핑은 CLAUDE.md 표 기준: CatPang→Bomb1, Arrow1→Bomb4, Arrow3→Bomb5, Arrow5→Bomb2, Arrow6→Bomb6, Arrow2→Bomb7, Arrow4→Bomb8, YellowBomb→Bomb9, OrangeBomb→Bomb10, BlueBomb→Bomb11, GreenBomb→Bomb12, RainbowPang→RainbowPang. 디스패치는 `SimBombResolver`(Task4) 가 구현하므로, 여기선 enum 매핑 헬퍼만:

```csharp
//# 이 블록이 발동될 때 호출할 BombN 종류. SimBombResolver 가 분기에 사용.
public enum EBombKind { None, Bomb1, Bomb2, Bomb4, Bomb5, Bomb6, Bomb7, Bomb8, Bomb9, Bomb10, Bomb11, Bomb12, Rainbow }

public EBombKind BombKind()
{
    switch (State)
    {
        case EBlockState.CatPang: return EBombKind.Bomb1;
        case EBlockState.Arrow1: return EBombKind.Bomb4;
        case EBlockState.Arrow3: return EBombKind.Bomb5;
        case EBlockState.Arrow5: return EBombKind.Bomb2;
        case EBlockState.Arrow6: return EBombKind.Bomb6;
        case EBlockState.Arrow2: return EBombKind.Bomb7;
        case EBlockState.Arrow4: return EBombKind.Bomb8;
        case EBlockState.YellowBomb: return EBombKind.Bomb9;
        case EBlockState.OrangeBomb: return EBombKind.Bomb10;
        case EBlockState.BlueBomb: return EBombKind.Bomb11;
        case EBlockState.GreenBomb: return EBombKind.Bomb12;
        case EBlockState.RainbowPang: return EBombKind.Rainbow;
        default: return EBombKind.None;
    }
}
```

- [ ] **Step 4: 단위 테스트 — 분류·ChangeMatchState·BombKind**

`SimBombIntegrationTests.cs`:

```csharp
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimBombIntegrationTests
    {
        [Test]
        public void 분류헬퍼_폭탄종류를_구분한다()
        {
            Assert.IsTrue(new SimBlock { State = EBlockState.CatPang }.IsBomb(), "CatPang IsBomb");
            Assert.IsTrue(new SimBlock { State = EBlockState.Arrow1 }.IsArrow(), "Arrow1 IsArrow");
            Assert.IsTrue(new SimBlock { State = EBlockState.PinkBomb }.IsSpecialBomb(), "Pink IsSpecialBomb");
            Assert.IsTrue(new SimBlock { State = EBlockState.RainbowPang }.CanNotDrag(), "Rainbow 드래그불가");
            Assert.AreEqual(SimBlock.EBombKind.Bomb1, new SimBlock { State = EBlockState.CatPang }.BombKind());
        }

        [Test]
        public void ChangeMatchState_고정블록과_PinkBomb는_제외()
        {
            SimBoard b = new SimBoard(3);
            b.SetState(0, 0, EBlockState.Cat1);
            b.SetState(0, 1, EBlockState.Wall); b.Grid[0, 1].Hp = 1;
            b.SetState(0, 2, EBlockState.PinkBomb);
            SimMatchChecker m = new SimMatchChecker();
            m.ChangeMatchState(b, 0, 0);
            m.ChangeMatchState(b, 0, 1);
            m.ChangeMatchState(b, 0, 2);
            Assert.IsTrue(b.Grid[0, 0].Match, "일반블록 match");
            Assert.IsFalse(b.Grid[0, 1].Match, "Wall 제외");
            Assert.IsFalse(b.Grid[0, 2].Match, "PinkBomb 제외");
        }
    }
}
```

- [ ] **Step 5: 컴파일 + 실행 → PASS. 스테이징 + 메시지**

`# [feat] - Sim M3a: SimBlock 폭탄 분류·BombKind·ChangeMatchState`

---

## Task 4: SimBombResolver — Bomb1~12 / BoomAll / Boom3 / Rainbow 발동 기하 + 골든

**Files:**
- Create: `Assets/Scripts/Sim/SimBombResolver.cs`
- Create: `Assets/Tests/EditMode/Sim/SimBombResolverGoldenTests.cs`

- [ ] **Step 1: SimBombResolver 골격 + Bomb1(3x3) 부터**

각 BombN 은 실 `GPBombResolver.BombN`(L95~310) 의 `ChangeMatchState(row±, col±)` 패턴을 그대로 포팅. 시그니처: `void BombN(SimBoard board, SimMatchChecker checker, int row, int col)`. RainbowPang 만 시드 RNG 필요.

```csharp
using static Defines;

namespace CatPang.Sim
{
    //# 실 GPBombResolver 의 발동 기하(Bomb1~12/BoomAll/Boom3/Rainbow) 순수 포팅.
    //# 각 BombN 은 고정 셀 집합에 checker.ChangeMatchState. 이펙트/사운드/점수는 시뮬 무관.
    public class SimBombResolver
    {
        private readonly System.Random _rng;
        public SimBombResolver(System.Random rng) { _rng = rng; }

        //# 실 Bomb1(L95): 자기 포함 3x3 (8방향).
        public void Bomb1(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int dr = -1; dr <= 1; ++dr)
                for (int dc = -1; dc <= 1; ++dc)
                    checker.ChangeMatchState(board, row + dr, col + dc);
        }

        //# 실 Bomb4(L154): 가로 한 줄.
        public void Bomb4(SimBoard board, SimMatchChecker checker, int row, int col)
        {
            board.Grid[row, col].Match = true;
            for (int i = 0; i < board.Size; ++i)
                checker.ChangeMatchState(board, row, i);
        }
        //# ... Bomb2/5/6/7/8/9/10/11/12 동일 패턴으로 실게임 L113~310 포팅 ...
    }
}
```

나머지 BombN 의 정확한 셀 패턴은 `GPBombResolver.cs` 의 해당 메서드를 1:1 옮긴다(Bomb2=십자 L113, Bomb5=세로 L168, Bomb6=X대각 L182, Bomb7=`/`대각 L200, Bomb8=`\`대각 L218, Bomb9=마름모 L236, Bomb10=5x5테두리 L259, Bomb11=5x5모서리 L278, Bomb12=5x5변형 L295). **골든 테스트(Step 3)가 각 패턴의 정확성을 보증하므로, 포팅 오류는 골든에서 잡힌다.**

- [ ] **Step 2: BoomAll / Boom3 / RainbowPang**

```csharp
//# 실 BoomAll(L86): 전체 칸 ChangeMatchState.
public void BoomAll(SimBoard board, SimMatchChecker checker)
{
    for (int r = 0; r < board.Size; ++r)
        for (int c = 0; c < board.Size; ++c)
            checker.ChangeMatchState(board, r, c);
}

//# 실 Boom3(L125): 보드의 blockState 색 전부 match.
public void Boom3(SimBoard board, EBlockState targetColor)
{
    for (int r = 0; r < board.Size; ++r)
        for (int c = 0; c < board.Size; ++c)
            if (board.Grid[r, c].State == targetColor)
                board.Grid[r, c].Match = true;
}

//# 실 RainbowPang(L312): hp<=0 일 때 PinkBomb..BlueBomb 를 일반/제거대상 칸에 살포(시드 위치).
public void RainbowPang(SimBoard board, SimBlock self)
{
    if (self.Hp > 0)
        return;
    self.Match = true;
    for (EBlockState bs = EBlockState.PinkBomb; bs <= EBlockState.BlueBomb; ++bs)
    {
        while (true)
        {
            if (HasRainbowTarget(board) == false)
                break;
            int r = _rng.Next(0, board.Size);
            int c = _rng.Next(0, board.Size);
            SimBlock b = board.Grid[r, c];
            if (b.Match == false && b.IsNormal() == false)
                continue;
            if (b.ChangeBlockState != EBlockState.None)
                continue;
            b.ChangeBlockState = bs;
            break;
        }
    }
}

private static bool HasRainbowTarget(SimBoard board)
{
    foreach (SimBlock b in board.Grid)
        if (b.Match || (b.IsNormal() && b.ChangeBlockState == EBlockState.None))
            return true;
    return false;
}
```

(실 RainbowPang 의 `b.remove` 는 Sim 의 `Match` 에 대응 — 그 턴 제거 예정 칸.)

- [ ] **Step 3: 골든 테스트 — 각 BombN 셀 집합이 실게임과 동등**

`SimBombResolverGoldenTests.cs`. Task1 스파이크에서 확정한 방식(실 `resolver.BombN` 직접 호출 또는 CHMData 우회 시 `matcher.ChangeMatchState` 기준)으로 실게임 match 셀 집합을 캡처, Sim 과 비교. 헬퍼로 일반화:

```csharp
using NUnit.Framework;
using System.Collections.Generic;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimBombResolverGoldenTests
    {
        //# 실 Bomb1(3x3) match 셀 == Sim Bomb1 match 셀.
        [Test]
        public void 골든_Bomb1_3x3_셀일치()
        {
            AssertBombMatchesReal(EBlockState.CatPang, 2, 2, (sim, b, ch) => sim.Bomb1(b, ch, 2, 2));
        }
        //# 골든_Bomb4_가로줄, 골든_Bomb2_십자 ... 각 BombN 1개씩 동일 패턴 추가.

        //# 실게임에서 (br,bc)에 bombState 두고 해당 BombN 호출 → match 셀 집합 캡처.
        //# Sim 에서 simRun 으로 동일 BombN 호출 → 집합 비교.
        private static void AssertBombMatchesReal(
            EBlockState bombState, int br, int bc,
            System.Action<SimBombResolver, SimBlock, SimMatchChecker> simRun)
        {
            EBlockState[,] states = Fill(9, EBlockState.Cat1);
            states[br, bc] = bombState;

            //# 실게임 match 셀(Task1 스파이크 확정 방식)
            HashSet<(int, int)> real = CaptureRealBombMatches(states, bombState, br, bc);

            //# Sim
            SimBoard sb = new SimBoard(9);
            for (int r = 0; r < 9; ++r)
                for (int c = 0; c < 9; ++c)
                    sb.SetState(r, c, states[r, c]);
            SimMatchChecker sc = new SimMatchChecker();
            SimBombResolver sr = new SimBombResolver(new System.Random(1));
            simRun(sr, sb.Grid[br, bc], sc);
            HashSet<(int, int)> sim = new HashSet<(int, int)>();
            for (int r = 0; r < 9; ++r)
                for (int c = 0; c < 9; ++c)
                    if (sb.Grid[r, c].Match) sim.Add((r, c));

            Assert.That(sim, Is.EquivalentTo(real), $"{bombState} 발동 셀 집합 일치");
        }

        //# Task1 스파이크 결과에 맞춰 구현(실 resolver.BombN 호출 or matcher.ChangeMatchState 직접).
        private static HashSet<(int, int)> CaptureRealBombMatches(
            EBlockState[,] states, EBlockState bombState, int br, int bc)
        {
            Block[,] arr = RealBlockFactory.CreateBoard(states);
            GPBoard gb = RealBlockFactory.CreateGPBoard(arr, 5);
            GPMatchChecker gm = RealBlockFactory.CreateGPMatchChecker(gb, 5);
            GPBombResolver res = new GPBombResolver();
            res.Init(gb, gm,
                () => System.Threading.Tasks.Task.CompletedTask, _ => { },
                (ps, pos) => null, _ => { },
                new List<ParticleSystem>(), null, default);
            //# 발동(스파이크에서 NRE 우회 확정된 경로)
            InvokeRealBomb(res, arr[br, bc], bombState);
            HashSet<(int, int)> set = new HashSet<(int, int)>();
            for (int r = 0; r < arr.GetLength(0); ++r)
                for (int c = 0; c < arr.GetLength(1); ++c)
                    if (arr[r, c].match) set.Add((r, c));
            RealBlockFactory.Destroy(arr);
            return set;
        }

        private static void InvokeRealBomb(GPBombResolver res, Block b, EBlockState s)
        {
            //# CLAUDE.md 매핑대로 BombN 호출. (스파이크 확정 호출 형식)
            switch (s)
            {
                case EBlockState.CatPang: res.Bomb1(b, false).GetAwaiter().GetResult(); break;
                case EBlockState.Arrow1: res.Bomb4(b, false).GetAwaiter().GetResult(); break;
                //# ... 나머지 매핑 ...
            }
        }

        private static EBlockState[,] Fill(int n, EBlockState s)
        {
            EBlockState[,] a = new EBlockState[n, n];
            for (int r = 0; r < n; ++r) for (int c = 0; c < n; ++c) a[r, c] = s;
            return a;
        }
    }
}
```

각 BombN 마다 골든 메서드 1개씩(12 + BoomAll). Boom3/Rainbow 는 비기하라 별도 단위(Boom3=색 전부 match, Rainbow=시드 결정성)로 검증.

- [ ] **Step 4: 컴파일 + 실행 → 전 골든 GREEN. 포팅 오류는 여기서 잡고 수정 반복.**

- [ ] **Step 5: 스테이징 + 메시지**

`# [feat] - Sim M3a: SimBombResolver 발동기하(Bomb1~12/BoomAll/Boom3/Rainbow) + 실게임 골든`

---

## Task 5: SimBombFactory — 매치→특수블록 생성 + 골든

**Files:**
- Create: `Assets/Scripts/Sim/SimBombFactory.cs`
- Create: `Assets/Tests/EditMode/Sim/SimBombFactoryGoldenTests.cs`

- [ ] **Step 1: SimBombFactory 구현 (실 CreateBombBlock L346 포팅)**

전제: CheckMap 후 각 블록의 HScore/VScore/SquareMatch + moveIndex 가 세팅된 상태. 실게임 순서(square → 교차 → hScore>3 → vScore>3)를 그대로.

```csharp
using static Defines;

namespace CatPang.Sim
{
    //# 실 GPBombResolver.CreateBombBlock(L346) 포팅: 매치 점수→특수블록 생성.
    //# arrowPangIndex: 어느 화살표 변형을 낼지 분기(실게임과 동일하게 호출측에서 주입).
    public static class SimBombFactory
    {
        public static void CreateBombs(SimBoard board, int arrowPangIndex, int moveIndex1, int moveIndex2)
        {
            int size = board.Size;
            //# 1) squareMatch → CatPang, 2) h&v 교차 → Arrow5/6
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    SimBlock b = board.Grid[r, c];
                    if (b.SquareMatch)
                        CreateAt(b, EBlockState.CatPang);
                    if (b.HScore >= SimMatchChecker.MinMatchCount && b.VScore >= SimMatchChecker.MinMatchCount)
                        CreateAt(b, arrowPangIndex == 1 ? EBlockState.Arrow5 : EBlockState.Arrow6);
                }
            }
            //# 3) hScore>3 → Arrow1/4 (이동칸 우선), 4) vScore>3 → Arrow3/2
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    SimBlock b = board.Grid[r, c];
                    if (b.HScore > SimMatchChecker.MinMatchCount)
                        CreateLine(board, r, c, true, arrowPangIndex, moveIndex1, moveIndex2);
                    else if (b.VScore > SimMatchChecker.MinMatchCount)
                        CreateLine(board, r, c, false, arrowPangIndex, moveIndex1, moveIndex2);
                }
            }
        }

        //# 생성 칸은 match 해제 + state 변경(낙하/제거에서 빠지고 특수블록으로 잔존).
        private static void CreateAt(SimBlock b, EBlockState bomb)
        {
            b.State = bomb;
            b.Match = false;
            b.Hp = -1;
            b.ResetScore();
        }

        //# 실 L378~432: 라인의 이동인덱스 칸에 화살표 생성, 없으면 시작 칸. 라인 점수 리셋.
        private static void CreateLine(SimBoard board, int r, int c, bool horizontal,
            int arrowPangIndex, int moveIndex1, int moveIndex2)
        {
            SimBlock start = board.Grid[r, c];
            int len = horizontal ? start.HScore : start.VScore;
            bool placed = false;
            for (int idx = 0; idx < len; ++idx)
            {
                int tr = horizontal ? r : r + idx;
                int tc = horizontal ? c + idx : c;
                if (board.IsValid(tr, tc) == false)
                    continue;
                SimBlock tb = board.Grid[tr, tc];
                if (tb.Index == moveIndex1 || tb.Index == moveIndex2)
                {
                    EBlockState bomb = horizontal
                        ? (arrowPangIndex == 1 ? EBlockState.Arrow1 : EBlockState.Arrow4)
                        : (arrowPangIndex == 1 ? EBlockState.Arrow3 : EBlockState.Arrow2);
                    CreateAt(tb, bomb);
                    placed = true;
                }
                else
                {
                    tb.ResetScore();
                }
            }
            if (placed == false)
            {
                EBlockState bomb = horizontal
                    ? (arrowPangIndex == 1 ? EBlockState.Arrow1 : EBlockState.Arrow4)
                    : (arrowPangIndex == 1 ? EBlockState.Arrow3 : EBlockState.Arrow2);
                CreateAt(start, bomb);
            }
        }
    }
}
```

- [ ] **Step 2: 골든 테스트 — 가로 4매치 → Arrow1/4 생성 위치·종류 실게임과 일치**

`SimBombFactoryGoldenTests.cs`. 실게임은 `CreateBombBlock` 이 `_board.CreateNewBlock`(UI 동반) 을 호출 → bare EditMode NRE 위험. **따라서 생성 골든은 "어느 칸이 어떤 특수블록이 되는가"를 비교하되, 실게임 캡처가 NRE 면 hScore/vScore 골든(Task2)으로 입력 동등성을 보증하고 생성 규칙은 단위 테스트로 검증**(실 CreateBombBlock 분기 로직을 소스 대조). Task1 스파이크에서 `CreateNewBlock` 호출성도 함께 확인:

```csharp
using NUnit.Framework;
using static Defines;

namespace CatPang.Sim.Tests
{
    public class SimBombFactoryGoldenTests
    {
        //# 가로 4매치(arrowPangIndex=1, 이동칸=index of (0,1)) → (0,1) 이 Arrow1.
        [Test]
        public void 생성_가로4매치_이동칸에_Arrow1()
        {
            SimBoard sb = new SimBoard(5);
            EBlockState[,] states =
            {
                { EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat1, EBlockState.Cat2 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
                { EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2 },
                { EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3, EBlockState.Cat2, EBlockState.Cat3 },
            };
            for (int r = 0; r < 5; ++r) for (int c = 0; c < 5; ++c) sb.SetState(r, c, states[r, c]);
            SimMatchChecker sc = new SimMatchChecker();
            int moveIdx = sb.Grid[0, 1].Index;
            sc.SetMoveIndices(moveIdx, -1);
            sc.CheckMap(sb);

            SimBombFactory.CreateBombs(sb, arrowPangIndex: 1, moveIndex1: moveIdx, moveIndex2: -1);

            Assert.AreEqual(EBlockState.Arrow1, sb.GetState(0, 1), "이동칸에 Arrow1 생성");
        }
    }
}
```

(실게임 직접 골든이 `CreateNewBlock` NRE 로 막히면, 이 단위 테스트 + Task2 의 hScore 골든 조합으로 등가 보증 — 입력(점수)은 골든, 분기 로직은 소스 1:1 단위. 스파이크 결과에 따라 실 골든 추가.)

- [ ] **Step 3: 컴파일 + 실행 → PASS. 스테이징 + 메시지**

`# [feat] - Sim M3a: SimBombFactory 매치→특수블록 생성`

---

## Task 6: SimComboResolver — 두 블록 스왑 조합

**Files:**
- Create: `Assets/Scripts/Sim/SimComboResolver.cs`
- Test: `Assets/Tests/EditMode/Sim/SimBombIntegrationTests.cs` (확장)

- [ ] **Step 1: 조합 디스패치 구현 (실 AfterDrag L787~804)**

결과를 enum 으로 반환해 SimGame 이 발동/생성을 수행하게 한다(순수 분류).

```csharp
using static Defines;

namespace CatPang.Sim
{
    //# 실 GPGameScene.AfterDrag 787~804 조합 분기 포팅(순수 판정).
    public static class SimComboResolver
    {
        public enum EComboResult { None, BoomAll, Boom3, DetonateA, DetonateB, MergeToColorBomb }

        //# a,b: 스왑된 두 블록. 결과만 반환(부수효과는 SimGame 이 수행).
        public static EComboResult Resolve(SimBlock a, SimBlock b)
        {
            if (a.IsSpecialBomb() && b.IsSpecialBomb())
                return EComboResult.BoomAll;
            if (a.State == EBlockState.PinkBomb || b.State == EBlockState.PinkBomb)
                return EComboResult.Boom3;
            if (a.IsSpecialBomb())
                return EComboResult.DetonateA;
            if (b.IsSpecialBomb())
                return EComboResult.DetonateB;
            if (a.IsBomb() && b.IsBomb())
                return EComboResult.MergeToColorBomb;
            if (a.IsBomb())
                return EComboResult.DetonateA;
            if (b.IsBomb())
                return EComboResult.DetonateB;
            return EComboResult.None;
        }
    }
}
```

- [ ] **Step 2: 단위 테스트 — 각 조합 분기**

`SimBombIntegrationTests.cs` 에 추가:

```csharp
        [Test]
        public void 조합_특수x특수는_BoomAll_화살표x화살표는_색폭탄승급()
        {
            SimBlock pink = new SimBlock { State = EBlockState.PinkBomb };
            SimBlock blue = new SimBlock { State = EBlockState.BlueBomb };
            SimBlock cat = new SimBlock { State = EBlockState.Cat1 };
            SimBlock a1 = new SimBlock { State = EBlockState.Arrow1 };
            SimBlock a3 = new SimBlock { State = EBlockState.Arrow3 };

            Assert.AreEqual(SimComboResolver.EComboResult.BoomAll, SimComboResolver.Resolve(blue, blue));
            Assert.AreEqual(SimComboResolver.EComboResult.Boom3, SimComboResolver.Resolve(pink, cat));
            Assert.AreEqual(SimComboResolver.EComboResult.MergeToColorBomb, SimComboResolver.Resolve(a1, a3));
            Assert.AreEqual(SimComboResolver.EComboResult.DetonateA, SimComboResolver.Resolve(a1, cat));
        }
```

- [ ] **Step 3: 컴파일 + 실행 → PASS. 스테이징 + 메시지**

`# [feat] - Sim M3a: SimComboResolver 스왑 조합 디스패치`

---

## Task 7: SimGame 턴루프 통합 — 생성·발동·연쇄·조합

**Files:**
- Modify: `Assets/Scripts/Sim/SimGame.cs`
- Test: `Assets/Tests/EditMode/Sim/SimBombIntegrationTests.cs` (확장)

- [ ] **Step 1: 발동(연쇄) 헬퍼 — match 된 폭탄을 BombN 으로 전개**

`SimGame.cs` 에 `SimBombResolver _bombResolver` 필드 추가(Run 시작에서 `new SimBombResolver(new System.Random(seed + 2))` 주입 — 기존 _damageRng(seed)/_creatorRng(seed+1) 와 분리). ResolveCascades 의 매치 처리 직후, **match=true 인 폭탄 블록을 찾아 BombKind 로 발동**하는 단계 추가. 발동은 더 많은 match 를 칠 수 있어(연쇄) 기존 cascade 루프가 이를 흡수. 무한방지는 기존 `MaxCascadeDepth` 캡 재사용.

```csharp
//# ResolveCascades 매치 iteration 안, gravity 직전에 호출.
//# match 된 폭탄을 전부 발동(BombN). 발동이 친 새 match 는 다음 iteration 에서 흡수(연쇄).
private void DetonateMatchedBombs(SimBoard board, SimMatchChecker checker)
{
    for (int r = 0; r < board.Size; ++r)
    {
        for (int c = 0; c < board.Size; ++c)
        {
            SimBlock b = board.Grid[r, c];
            if (b.Match == false)
                continue;
            switch (b.BombKind())
            {
                case SimBlock.EBombKind.Bomb1: _bombResolver.Bomb1(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb2: _bombResolver.Bomb2(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb4: _bombResolver.Bomb4(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb5: _bombResolver.Bomb5(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb6: _bombResolver.Bomb6(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb7: _bombResolver.Bomb7(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb8: _bombResolver.Bomb8(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb9: _bombResolver.Bomb9(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb10: _bombResolver.Bomb10(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb11: _bombResolver.Bomb11(board, checker, r, c); break;
                case SimBlock.EBombKind.Bomb12: _bombResolver.Bomb12(board, checker, r, c); break;
                case SimBlock.EBombKind.Rainbow: _bombResolver.RainbowPang(board, b); break;
            }
        }
    }
}
```

- [ ] **Step 2: 매치→생성 호출 + 발동 통합 in ResolveCascades**

ResolveCascades 매치 branch 에서: CheckMap 후 → `DetonateMatchedBombs` → `SimBombFactory.CreateBombs(board, arrowPangIndex, mi1, mi2)`(생성은 match 제거 전, 실게임 AfterDrag 순서) → 점수/제거/gravity. arrowPangIndex 는 실게임처럼 토글(간단히 `(result.Turns % 2 == 0) ? 1 : 2` 등 결정적 값) — 정확한 토글원은 `_arrowPangIndex` 설정부(GPGameScene) 확인해 맞춘다. moveIndex 는 그 수의 두 블록 index(메인 루프에서 SetMoveIndices 로 전달).

- [ ] **Step 3: 조합 통합 in 메인 루프 swap 처리**

메인 루프에서 AI 가 고른 수가 **폭탄 조합 수**이면(두 블록 중 폭탄 계열), swap 후 매치 검사 대신 `SimComboResolver.Resolve(a,b)` 분기 수행:
- BoomAll → `_bombResolver.BoomAll`
- Boom3 → `_bombResolver.Boom3(board, otherColor)`
- DetonateA/B → 해당 칸 match=true 후 `DetonateMatchedBombs`
- MergeToColorBomb → 한 칸 `ChangeBlockState = 색폭탄(_rng)`, 다른 칸 match
그 후 ResolveCascades 로 연쇄 흡수. `result.MovesUsed += 1`.

- [ ] **Step 4: 단위 테스트 — 발동·연쇄 1판**

```csharp
        [Test]
        public void 턴루프_CatPang이_매치되면_3x3발동()
        {
            //# 3x3 중앙 CatPang, 한 행을 Cat1 3매치로 만들어 CatPang 이 match→발동되면 주변 제거.
            //# 결정적 보드로 Run 1수 후 CatPang 주변이 비는지(리필 전 상태는 검증 어려우니 점수 증가로 확인).
            //# 간이: ResolveCascades 직접 호출 대신 SimBombResolver.Bomb1 단위는 Task4 가 커버.
            //# 여기선 DetonateMatchedBombs 가 match 된 CatPang 을 전개하는지 화이트박스 검증.
            SimBoard b = new SimBoard(5);
            for (int r = 0; r < 5; ++r) for (int c = 0; c < 5; ++c) b.SetState(r, c, EBlockState.Cat1);
            b.SetState(2, 2, EBlockState.CatPang);
            b.Grid[2, 2].Match = true;
            SimMatchChecker m = new SimMatchChecker();
            new SimGame().TestDetonate(b, m); //# 테스트 전용 노출 래퍼(internal)
            int matched = 0;
            foreach (SimBlock x in b.Grid) if (x.Match) ++matched;
            Assert.GreaterOrEqual(matched, 9, "CatPang 발동으로 3x3 이상 match");
        }
```

`SimGame` 에 `internal void TestDetonate(SimBoard, SimMatchChecker)` 래퍼 추가(테스트 전용, `_bombResolver` 미주입 시 `new SimBombResolver(new System.Random(0))` 로컬 생성).

- [ ] **Step 5: 컴파일 + 실행 → PASS. 회귀 PASS 유지 확인. 스테이징 + 메시지**

`# [feat] - Sim M3a: SimGame 턴루프에 폭탄 생성·발동·연쇄·조합 통합`

---

## Task 8: 능동 AI — 폭탄 스왑/조합 수 탐색

**Files:**
- Modify: `Assets/Scripts/Sim/ISimAiPolicy.cs`
- Test: `Assets/Tests/EditMode/Sim/SimBombIntegrationTests.cs` (확장)

- [ ] **Step 1: FindValidMoves 에 폭탄 조합 수 포함**

현재 `FindValidMoves` 는 양쪽 `CanNotDrag()==false` + swap 후 매치 생기는 수만. 확장: swap 대상 둘 중 하나라도 폭탄 계열(IsBomb/IsSpecialBomb)이면 **매치 없이도 유효 수**(조합/발동). 단 RainbowPang 은 CanNotDrag 라 제외(드래그 불가 — 실게임도 RainbowPang 은 매치/발동으로만).

```csharp
//# 폭탄 계열이 관여하면 매치 없이도 유효(조합/발동). 그 외엔 기존(매치 필요).
bool bombMove = board.Grid[r, c].IsBomb() || board.Grid[r, c].IsSpecialBomb()
    || board.Grid[nr, nc].IsBomb() || board.Grid[nr, nc].IsSpecialBomb();
if (bombMove)
{
    moves.Add(new SimMove(r, c, d.dr, d.dc));
    continue;
}
//# 기존 매치 검사 경로
```

- [ ] **Step 2: 단위 테스트 — 인접 두 폭탄 스왑이 유효 수로 탐색됨**

```csharp
        [Test]
        public void AI_인접_두폭탄_스왑이_유효수()
        {
            SimBoard b = new SimBoard(3);
            for (int r = 0; r < 3; ++r) for (int c = 0; c < 3; ++c) b.SetState(r, c, EBlockState.Cat1);
            b.SetState(1, 1, EBlockState.Arrow1);
            b.SetState(1, 2, EBlockState.Arrow3);
            SimMove? mv = new RandomAiPolicy(1).ChooseMove(b, new SimMatchChecker());
            Assert.IsTrue(mv.HasValue, "폭탄 인접 스왑이 유효 수로 잡힘");
        }
```

- [ ] **Step 3: 컴파일 + 실행 → PASS. 스테이징 + 메시지**

`# [feat] - Sim M3a: 능동 AI 폭탄 스왑/조합 수 탐색`

---

## Task 9: IsSupported 갱신 + 커버리지 재측정

**Files:**
- Modify: `Assets/Scripts/Sim/SimGame.cs`
- Modify: `Assets/Tests/EditMode/Sim/SimGameTests.cs` (기존 `한판_특수블록스테이지는_Unsupported` 가 Fish 사용 — 유지 OK, Fish 는 M3b 라 여전히 Unsupported)
- Test: `Assets/Tests/EditMode/Sim/SimBombIntegrationTests.cs`

- [ ] **Step 1: IsSupported 화이트리스트에 M3a 블록 추가**

`SimGame.IsSupported` 의 허용 블록 스캔에 추가: Arrow1~6, CatPang, 5색폭탄, RainbowPang. **Fish/Ball 은 추가하지 않는다(M3b).**

```csharp
if (st >= EBlockState.Arrow1 && st <= EBlockState.Arrow6) continue;
if (st == EBlockState.CatPang) continue;
if (st >= EBlockState.YellowBomb && st <= EBlockState.PinkBomb) continue; //# 19~23
if (st == EBlockState.RainbowPang) continue;
//# Fish(24)/Ball(53) 은 계속 미지원 → return false (M3b)
```

- [ ] **Step 2: 단위 테스트 — 초기 Arrow/색폭탄 stage 가 supported, Fish stage 는 미지원 유지**

```csharp
        [Test]
        public void IsSupported_초기폭탄stage는_지원_Fish는_미지원()
        {
            string sj = System.IO.File.ReadAllText("Assets/AssetBundleResources/json/Stage.json");
            string sbj = System.IO.File.ReadAllText("Assets/AssetBundleResources/json/StageBlock.json");
            //# stage3: 초기 YellowBomb 보유(분포표) → supported.
            SimStageData s3 = SimStageLoader.Load(sj, sbj, 3, normalMode: true);
            Assert.IsFalse(new SimGame().Run(s3, new RandomAiPolicy(1), 1).Unsupported, "초기 YellowBomb stage 지원");
            //# stage31: 초기 Fish 보유 → 여전히 미지원(M3b).
            SimStageData s31 = SimStageLoader.Load(sj, sbj, 31, normalMode: true);
            Assert.IsTrue(new SimGame().Run(s31, new RandomAiPolicy(1), 1).Unsupported, "Fish stage 는 M3b → 미지원");
        }
```

- [ ] **Step 3: 컴파일 + 실행 → 전체 PASS. 회귀 확인.**

- [ ] **Step 4: 커버리지 재측정 (MCP)**

`editor_invoke_method` → `CatPang.Sim.EditorTools.SimBatchRunner.CountSupported`. 결과 파일 `docs/qa-reports/sim-output/m2-coverage-count.txt`(또는 m3a 명으로 변경) 에서 supported 수 증가 확인. Fish/Ball **단독** 보유가 아닌, 폭탄만 가진 stage 가 supported 로 전환됐는지 점검.

- [ ] **Step 5: 측정 리포트 + 스테이징 + 메시지**

`docs/qa-reports/2026-06-03-headless-sim-m3a.md` 에 supported 증가분·기존 59 stage 메트릭 변화(폭탄 생성 반영) 기록. 
스테이징: 변경된 Sim 파일 + 신규 테스트(+meta) + 리포트.
제안: `# [feat] - Sim M3a 폭탄 시스템 완료: IsSupported 확장 + 커버리지 재측정`

---

## Self-Review (작성자 체크)

**Spec 커버리지:** §2 컴포넌트 표 → Task2~8 1:1. §3 생성 → Task5. §4 조합 → Task6+7. §5 발동/연쇄 → Task4+7. §6 능동AI → Task8. §7 비결정성 → Task4(Rainbow rng)/Task7(merge rng). §8 골든+스파이크 → Task1(스파이크)+Task2/4 골든. §9 IsSupported → Task9. 누락 없음.

**플레이스홀더:** Bomb2/5/6/7/8/9/10/11/12 의 정확한 셀 패턴과 InvokeRealBomb 의 나머지 매핑은 "실게임 L번호 1:1 포팅 + 골든이 보증"으로 위임 — 구현자가 소스를 직접 옮기고 골든이 검증하는 구조라 의도적(완전한 12개 패턴 나열은 GPBombResolver.cs 와 중복). 그 외 TODO 없음.

**타입 일관성:** `SimBlock.HScore/VScore/SquareMatch/Index/BombKind()/IsBomb()/IsSpecialBomb()/CanNotDrag()`, `SimMatchChecker.CheckMap/ChangeMatchState/SetMoveIndices/MinMatchCount`, `SimBombResolver.BombN(board,checker,r,c)/BoomAll/Boom3/RainbowPang`, `SimBombFactory.CreateBombs(board,arrowPangIndex,mi1,mi2)`, `SimComboResolver.Resolve(a,b)→EComboResult` — Task 간 시그니처 일치 확인.

**리스크:** Task1 스파이크가 CHMData/CreateNewBlock NRE 로 실 골든을 부분 차단할 수 있음 — 그 경우 Task4/5 는 "matcher.ChangeMatchState 직접 골든 + 분기 단위 테스트"로 대체(각 Task 에 그 분기 명시됨). 이 폴백이 골든 강도를 일부 낮추나 M1 의 "수작업 골든" 수준은 유지.
