# UIGameEnd 실패 사유 표시 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 게임 실패 시 `UIGameEnd` 화면에 종료 사유(시간/이동/HP)와 미달 목표(점수, 남은 목표 블록 아이콘)를 표시한다.

**Architecture:** `GPGameScene.GameEnd`가 실패 사유 데이터(`GameEndFailInfo`)만 계산해 `UIGameEndArg`로 전달하고, `UIGameEnd`가 CHMString 로컬라이제이션으로 문자열을 조립·표시한다. 남은 목표 블록은 종류별로 `FailBlockIconItem` 위젯을 복제해 아이콘으로 보여준다.

**Tech Stack:** Unity / C#, UniRx, DOTween, TextMesh Pro, ChvjUnityInfra 패키지(`CHText`, `CHMResource`, `CHMUI`).

**관련 설계 문서:** `docs/superpowers/specs/2026-05-21-uigameend-fail-reason-design.md`

---

## 커밋 정책

이 프로젝트는 git 커밋을 **사용자가 직접 실행**한다. 각 태스크의 "Commit" 스텝은 staging 명령과 커밋 메시지 **제안**이다. 실행자(서브에이전트/세션)는 커밋을 자동 실행하지 말고, 제안된 명령을 사용자에게 제시하고 확인을 받는다.

## 테스트 방식

이 Unity 프로젝트에는 자동화 테스트 프레임워크가 없고(설계 문서 §7), `CollectRemainingObjectiveBlocks`는 `Block` MonoBehaviour 배열에 의존해 단위 테스트가 불가능하다. 따라서:

- **코드 태스크(1~6)** — Unity 에디터에서 컴파일 에러 0건으로 검증한다. UnityMCP 사용 시 `editor_recompile` 호출 후 `editor_read_log`(error 필터)로 확인, 미사용 시 Unity Console을 육안 확인한다.
- **전체 동작** — Task 8의 수동 플레이테스트(설계 문서 §7 시나리오)로 검증한다.

## 파일 구조

| 파일 | 책임 | 변경 |
|------|------|------|
| `Assets/Scripts/Defines.cs` | 전역 enum 정의 | `EFailReason` enum 추가 |
| `Assets/Scripts/UI/UIGameEnd.cs` | 게임 결과 화면 | 데이터 모델 3종 + 실패 사유 렌더링 |
| `Assets/Scripts/UI/FailBlockIconItem.cs` | 남은 목표 블록 아이콘 1개 위젯 | 신규 |
| `Assets/Scripts/Scenes/GPGameScene.cs` | 게임 진행/종료 로직 | 실패 사유 데이터 계산·전달 |
| `Assets/AssetBundleResources/json/StringKorea.json` | 한국어 문자열 | ID 174~179 추가 |
| `Assets/AssetBundleResources/json/StringEnglish.json` | 영어 문자열 | ID 174~179 추가 |
| `Assets/AssetBundleResources/ui/UIGameEnd.prefab` | UIGameEnd 프리팹 | 실패 사유 위젯 추가·와이어링 |

태스크 순서는 의존성 순이다: 1(enum) → 2(문자열) → 3(데이터 모델) → 4(위젯) → 5(게임 로직) → 6(렌더링) → 7(프리팹) → 8(검증). Task 7까지 완료해야 실패 화면이 정상 동작한다.

---

## Task 1: EFailReason enum 추가

**Files:**
- Modify: `Assets/Scripts/Defines.cs` (EGameState enum 직후)

- [ ] **Step 1: enum 추가**

`Defines.cs`에서 아래 `old`를 찾아 `new`로 교체한다.

old:
```csharp
        BossStagePlay,

        Max
    }

    public enum EClearState
```

new:
```csharp
        BossStagePlay,

        Max
    }

    public enum EFailReason
    {
        // 0 = 기본값. 명시적으로 -1을 두지 않아 default(EFailReason)이 None이 되도록 한다.
        None,

        TimeOver,   // 시간 초과
        MoveOver,   // 이동 횟수 소진
        HpOver,     // 체력 소진 (보스전)
    }

    public enum EClearState
```

- [ ] **Step 2: 컴파일 확인**

Unity 에디터로 전환해 자동 컴파일 → Console에 컴파일 에러 0건 확인.
(UnityMCP: `editor_recompile` 후 `editor_read_log`)

- [ ] **Step 3: Commit (제안)**

```bash
git add Assets/Scripts/Defines.cs
git commit -m "[Defines] 게임 실패 사유 enum EFailReason 추가

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 2: 로컬라이제이션 문자열 추가

**Files:**
- Modify: `Assets/AssetBundleResources/json/StringKorea.json` (배열 끝)
- Modify: `Assets/AssetBundleResources/json/StringEnglish.json` (배열 끝)

- [ ] **Step 1: 두 JSON 파일의 끝부분 확인**

각 파일의 마지막 약 8줄을 Read로 확인한다. 마지막 항목은 `stringID: 173`이고, 들여쓰기는 `{`가 8칸·필드가 12칸·`}`가 8칸·닫는 `]`가 4칸이다. 아래 `old_string`의 들여쓰기가 실제 파일과 다르면 실제에 맞춰 조정한다.

- [ ] **Step 2: StringKorea.json에 174~179 추가**

old:
```json
        {
            "stringID": 173,
            "value": "받기"
        }
    ]
```

new:
```json
        {
            "stringID": 173,
            "value": "받기"
        },
        {
            "stringID": 174,
            "value": "시간 초과"
        },
        {
            "stringID": 175,
            "value": "이동 횟수 소진"
        },
        {
            "stringID": 176,
            "value": "체력 소진"
        },
        {
            "stringID": 177,
            "value": "보스 HP {0}% 남음"
        },
        {
            "stringID": 178,
            "value": "점수 {0} / {1}"
        },
        {
            "stringID": 179,
            "value": "남은 목표 블록"
        }
    ]
```

- [ ] **Step 3: StringEnglish.json에 174~179 추가**

old:
```json
        {
            "stringID": 173,
            "value": "Get"
        }
    ]
```

new:
```json
        {
            "stringID": 173,
            "value": "Get"
        },
        {
            "stringID": 174,
            "value": "Time's Up"
        },
        {
            "stringID": 175,
            "value": "Out of Moves"
        },
        {
            "stringID": 176,
            "value": "Out of HP"
        },
        {
            "stringID": 177,
            "value": "Boss HP {0}% Left"
        },
        {
            "stringID": 178,
            "value": "Score {0} / {1}"
        },
        {
            "stringID": 179,
            "value": "Blocks Remaining"
        }
    ]
```

- [ ] **Step 4: JSON 유효성 확인**

두 파일이 유효한 JSON인지 확인한다(콤마 누락 없음, 배열 닫힘). 한글이 깨지지 않았는지(UTF-8) 육안 확인.

- [ ] **Step 5: Commit (제안)**

```bash
git add Assets/AssetBundleResources/json/StringKorea.json Assets/AssetBundleResources/json/StringEnglish.json
git commit -m "[String] 실패 사유 로컬라이제이션 문자열 174~179 추가

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 3: 실패 사유 데이터 모델 추가

**Files:**
- Modify: `Assets/Scripts/UI/UIGameEnd.cs` (using 추가, `UIGameEndArg` 위/안)

- [ ] **Step 1: using 추가**

old:
```csharp
using UnityEngine.UI;
using static Defines;
```

new:
```csharp
using UnityEngine.UI;
using System.Collections.Generic;
using static Defines;
```

- [ ] **Step 2: 데이터 모델 클래스 + failInfo 필드 추가**

old:
```csharp
public class UIGameEndArg : CHUIArg
{
    public Defines.EClearState clearState = Defines.EClearState.None;
    public Defines.EGameState result = Defines.EGameState.None;
    public int gold;
}
```

new:
```csharp
// 실패 화면에 표시할 '남은 목표 블록' 1종류
public class BlockTypeCount
{
    public Defines.EBlockState state;
    public int count;
}

// 게임 실패 사유 데이터. 클리어 시에는 생성하지 않는다(null).
public class GameEndFailInfo
{
    public Defines.EFailReason reason;
    public int curScore;                       // 일반/하드
    public int targetScore;                    // 일반/하드
    public List<BlockTypeCount> remainBlocks;  // 일반/하드, EBlockState별 그룹
    public float bossHpRatio;                  // 보스전, 0~1
}

public class UIGameEndArg : CHUIArg
{
    public Defines.EClearState clearState = Defines.EClearState.None;
    public Defines.EGameState result = Defines.EGameState.None;
    public int gold;
    public GameEndFailInfo failInfo;           // 실패 시에만, 클리어 시 null
}
```

- [ ] **Step 3: 컴파일 확인**

Unity Console에 컴파일 에러 0건 확인.

- [ ] **Step 4: Commit (제안)**

```bash
git add Assets/Scripts/UI/UIGameEnd.cs
git commit -m "[UIGameEnd] 실패 사유 데이터 모델(GameEndFailInfo) 추가

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 4: FailBlockIconItem 위젯 생성

**Files:**
- Create: `Assets/Scripts/UI/FailBlockIconItem.cs`

- [ ] **Step 1: 새 파일 작성**

`Assets/Scripts/UI/FailBlockIconItem.cs`를 아래 내용으로 생성한다.

```csharp
using ChvjUnityInfra;
using UnityEngine;
using UnityEngine.UI;

// 게임 실패 화면 '남은 목표 블록'의 아이콘 1개 위젯.
// 화면 최상위가 아닌 위젯이므로 UIBase가 아닌 일반 MonoBehaviour로 둔다.
public class FailBlockIconItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private CHText countText;

    // 블록 종류와 개수를 설정한다. 스프라이트는 비동기로 로드된다.
    public void Setup(Defines.EBlockState state, int count)
    {
        if (countText != null)
            countText.SetText($"x{count}");

        if (icon == null)
            return;

        // 스프라이트 로드 완료 전 잘못된 이미지가 1프레임 보이는 것을 방지
        icon.enabled = false;
        CHMResource.Instance.LoadSprite(state, sprite =>
        {
            // 콜백 도착 전 UI가 파괴됐을 수 있으므로 방어
            if (icon == null || sprite == null)
                return;

            icon.sprite = sprite;
            icon.enabled = true;
        });
    }
}
```

- [ ] **Step 2: 컴파일 확인**

Unity 에디터로 전환하면 새 스크립트를 import하고 `.meta`를 자동 생성한다. Console에 컴파일 에러 0건 확인.

- [ ] **Step 3: Commit (제안)**

```bash
git add Assets/Scripts/UI/FailBlockIconItem.cs Assets/Scripts/UI/FailBlockIconItem.cs.meta
git commit -m "[UIGameEnd] 남은 목표 블록 아이콘 위젯 FailBlockIconItem 추가

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 5: GPGameScene 실패 사유 계산·전달

**Files:**
- Modify: `Assets/Scripts/Scenes/GPGameScene.cs` (`GameEnd` 메서드, `GetClearState` 직후)

- [ ] **Step 1: ShowUI 호출에 failInfo 전달**

`GameEnd` 메서드 안의 `ShowUI(EUI.UIGameEnd, ...)` 호출을 수정한다.

old:
```csharp
        CHMUI.Instance.ShowUI(EUI.UIGameEnd, new UIGameEndArg
        {
            clearState = GetClearState(),
            result = gameResult.Value,
            gold = curScore.Value
        });
```

new:
```csharp
        CHMUI.Instance.ShowUI(EUI.UIGameEnd, new UIGameEndArg
        {
            clearState = GetClearState(),
            result = gameResult.Value,
            gold = curScore.Value,
            failInfo = clear ? null : BuildFailInfo(),
        });
```

- [ ] **Step 2: BuildFailInfo / CollectRemainingObjectiveBlocks 메서드 추가**

`GetClearState()` 메서드의 닫는 중괄호와 `// Block.cs 에서 호출하는 public 파사드` 주석 사이에 두 메서드를 추가한다.

old:
```csharp
        return EClearState.Doing;
    }

    // Block.cs 에서 호출하는 public 파사드
```

new:
```csharp
        return EClearState.Doing;
    }

    // 게임 실패 시 사유 데이터 생성. ShowUI(UIGameEnd) 직전(부활 경로 통과 후)에만 호출된다.
    private GameEndFailInfo BuildFailInfo()
    {
        var info = new GameEndFailInfo();

        if (_selectStage == ESelectStage.Boss)
        {
            info.reason = EFailReason.HpOver;
            info.bossHpRatio = bossHpImage.fillAmount;
            return info;
        }

        // 일반/하드 — 종료 트리거 재유도 (Update의 시간 초과 판정과 동일 조건)
        bool timeOver = _stageInfo.time > 0 && timerImg.fillAmount >= 1f;
        info.reason = timeOver ? EFailReason.TimeOver : EFailReason.MoveOver;
        info.curScore = curScore.Value;
        info.targetScore = _stageInfo.targetScore;
        info.remainBlocks = CollectRemainingObjectiveBlocks();
        return info;
    }

    // 클리어 판정(Update의 보드 스캔)과 동일 조건으로 남은 목표 블록을 EBlockState별로 집계.
    private List<BlockTypeCount> CollectRemainingObjectiveBlocks()
    {
        var result = new List<BlockTypeCount>();

        for (int i = 0; i < boardSize; ++i)
        {
            for (int j = 0; j < boardSize; ++j)
            {
                var block = boardArr[i, j];
                if (block.GetBlockState() == EBlockState.RainbowPang) continue;
                if (!block.checkHp) continue;
                if (block.GetHp() > 0 || block.IsFishBlock() || block.IsBallBlock())
                {
                    var state = block.GetBlockState();
                    var entry = result.Find(e => e.state == state);
                    if (entry != null) entry.count++;
                    else result.Add(new BlockTypeCount { state = state, count = 1 });
                }
            }
        }

        return result;
    }

    // Block.cs 에서 호출하는 public 파사드
```

- [ ] **Step 3: 컴파일 확인**

Unity Console에 컴파일 에러 0건 확인. (이 시점에 `UIGameEnd`는 아직 `failInfo`를 읽지 않으므로 동작 변화 없음.)

- [ ] **Step 4: Commit (제안)**

```bash
git add Assets/Scripts/Scenes/GPGameScene.cs
git commit -m "[GPGameScene] 게임 종료 시 실패 사유 데이터 계산·전달

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 6: UIGameEnd 실패 사유 렌더링

**Files:**
- Modify: `Assets/Scripts/UI/UIGameEnd.cs` (SerializeField·상수·`Start` 분기·`ShowFailReason`)

- [ ] **Step 1: SerializeField 5개 추가**

old:
```csharp
    [SerializeField] private Button claimBtn;

    private bool received = false;
```

new:
```csharp
    [SerializeField] private Button claimBtn;

    [Header("실패 사유 표시")]
    [SerializeField] private TMP_Text failReasonText;
    [SerializeField] private TMP_Text failBlockHeaderText;
    [SerializeField] private Transform failBlockIconContainer;
    [SerializeField] private FailBlockIconItem failBlockIconTemplate;
    [SerializeField] private TMP_Text failDetailText;

    private bool received = false;
```

- [ ] **Step 2: 문자열 ID 상수 추가**

old:
```csharp
    // 광고 시청 보상으로 지급하는 골드 배수
    private const int AdRewardGoldMultiplier = 3;
```

new:
```csharp
    // 광고 시청 보상으로 지급하는 골드 배수
    private const int AdRewardGoldMultiplier = 3;

    // 실패 사유 로컬라이제이션 문자열 ID (StringKorea/StringEnglish.json)
    private const int FailTimeOverStringID = 174;
    private const int FailMoveOverStringID = 175;
    private const int FailHpOverStringID = 176;
    private const int FailBossHpStringID = 177;
    private const int FailScoreStringID = 178;
    private const int FailBlockHeaderStringID = 179;
```

- [ ] **Step 3: GameOver 분기에서 ShowFailReason 호출**

old:
```csharp
        if (arg.result == Defines.EGameState.GameOver)
        {
            if (successObj != null) successObj.SetActive(false);
            if (failedObj != null) failedObj.SetActive(true);

            resultText.DOText("Failed...", ResultTextRevealDuration);
            goldText.SetText(0);
            goldx2Text.SetText(0);
        }
```

new:
```csharp
        if (arg.result == Defines.EGameState.GameOver)
        {
            if (successObj != null) successObj.SetActive(false);
            if (failedObj != null) failedObj.SetActive(true);

            resultText.DOText("Failed...", ResultTextRevealDuration);
            goldText.SetText(0);
            goldx2Text.SetText(0);

            ShowFailReason(arg.failInfo);
        }
```

- [ ] **Step 4: ShowFailReason / FailReasonStringID 메서드 추가**

`Start()` 메서드의 닫는 중괄호와 `private void BindUI()` 사이에 추가한다.

old:
```csharp
        BindUI();
    }

    private void BindUI()
```

new:
```csharp
        BindUI();
    }

    // 실패 화면에 종료 사유와 미달 목표를 표시한다.
    private void ShowFailReason(GameEndFailInfo info)
    {
        // 템플릿 자체는 항상 숨김 — 복제본만 표시
        if (failBlockIconTemplate != null)
            failBlockIconTemplate.gameObject.SetActive(false);

        if (info == null)
        {
            if (failReasonText != null) failReasonText.gameObject.SetActive(false);
            if (failBlockHeaderText != null) failBlockHeaderText.gameObject.SetActive(false);
            if (failDetailText != null) failDetailText.gameObject.SetActive(false);
            return;
        }

        // 트리거 줄
        if (failReasonText != null)
        {
            failReasonText.gameObject.SetActive(true);
            failReasonText.text = CHMString.Instance.GetString(FailReasonStringID(info.reason));
        }

        // 남은 목표 블록 섹션 (일반/하드 전용)
        bool hasBlocks = info.remainBlocks != null && info.remainBlocks.Count > 0;
        if (failBlockHeaderText != null)
        {
            failBlockHeaderText.gameObject.SetActive(hasBlocks);
            if (hasBlocks)
                failBlockHeaderText.text = CHMString.Instance.GetString(FailBlockHeaderStringID);
        }
        if (hasBlocks && failBlockIconTemplate != null && failBlockIconContainer != null)
        {
            foreach (var entry in info.remainBlocks)
            {
                var item = Instantiate(failBlockIconTemplate, failBlockIconContainer);
                item.gameObject.SetActive(true);
                item.Setup(entry.state, entry.count);
            }
        }

        // 점수 줄(일반/하드) 또는 보스 HP 줄(보스전)
        if (failDetailText != null)
        {
            if (info.reason == Defines.EFailReason.HpOver)
            {
                failDetailText.gameObject.SetActive(true);
                failDetailText.text = string.Format(
                    CHMString.Instance.GetString(FailBossHpStringID),
                    Mathf.RoundToInt(info.bossHpRatio * 100f));
            }
            else if (info.targetScore > 0 && info.curScore < info.targetScore)
            {
                failDetailText.gameObject.SetActive(true);
                failDetailText.text = string.Format(
                    CHMString.Instance.GetString(FailScoreStringID),
                    info.curScore.ToString("N0"), info.targetScore.ToString("N0"));
            }
            else
            {
                failDetailText.gameObject.SetActive(false);
            }
        }
    }

    // EFailReason → 트리거 문자열 ID
    private static int FailReasonStringID(Defines.EFailReason reason)
    {
        switch (reason)
        {
            case Defines.EFailReason.TimeOver: return FailTimeOverStringID;
            case Defines.EFailReason.MoveOver: return FailMoveOverStringID;
            case Defines.EFailReason.HpOver:   return FailHpOverStringID;
            default:                           return FailTimeOverStringID;
        }
    }

    private void BindUI()
```

- [ ] **Step 5: 컴파일 확인**

Unity Console에 컴파일 에러 0건 확인. (프리팹 와이어링은 Task 7에서 — 이 시점에는 SerializeField가 모두 null이라 `ShowFailReason`이 null 가드로 아무것도 표시하지 않는다.)

- [ ] **Step 6: Commit (제안)**

```bash
git add Assets/Scripts/UI/UIGameEnd.cs
git commit -m "[UIGameEnd] 실패 화면에 사유 표시 렌더링 추가

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 7: UIGameEnd 프리팹 위젯 추가·와이어링

**Files:**
- Modify: `Assets/AssetBundleResources/ui/UIGameEnd.prefab`

이 태스크는 Unity 에디터의 프리팹 편집 모드에서 수행한다(레이아웃·RectTransform 조정이 시각 작업이므로). UnityMCP로 오브젝트 생성·컴포넌트 부착도 가능하나, 참조형 SerializeField 와이어링 후 반드시 검증한다(Sprite 참조는 런타임 로드라 와이어링 대상 아님).

- [ ] **Step 1: 프리팹 열기**

`Assets/AssetBundleResources/ui/UIGameEnd.prefab`을 더블클릭해 프리팹 편집 모드로 연다. Hierarchy에서 `UIGameEnd` 컴포넌트의 `failedObj`가 가리키는 `Failed` 게임오브젝트를 찾는다.

- [ ] **Step 2: FailReasonRoot 컨테이너 생성**

`Failed` 하위에 빈 게임오브젝트 `FailReasonRoot`를 만들고 `VerticalLayoutGroup` 컴포넌트를 추가한다(자식 정렬: 가운데, child force expand는 취향껏). 이 루트가 아래 5개 요소를 세로로 배치한다. 기존 `Failed` 하위 요소와 겹치지 않게 RectTransform 위치를 조정한다.

- [ ] **Step 3: FailReasonText 생성**

스타일 일관성을 위해 `Failed` 하위의 기존 TMP 텍스트(예: `GameResultText`)를 복제해 `FailReasonRoot` 안으로 옮기고 이름을 `FailReasonText`로 바꾼다. 텍스트 내용은 비운다(런타임에 채워짐). DOText 등 불필요한 컴포넌트가 붙어 있으면 제거하고 순수 `TextMeshPro - Text (UI)`만 남긴다.

- [ ] **Step 4: FailBlockHeaderText 생성**

Step 3과 동일 방식으로 TMP 텍스트를 복제해 `FailBlockHeaderText`로 만들어 `FailReasonRoot` 안에 둔다.

- [ ] **Step 5: FailBlockIconContainer 생성**

`FailReasonRoot` 하위에 빈 게임오브젝트 `FailBlockIconContainer`를 만들고 `HorizontalLayoutGroup`을 추가한다(아이콘 가로 정렬). 실사용상 목표 블록 종류는 5개 이하로 가정한다.

- [ ] **Step 6: FailBlockIconTemplate 생성·구성**

`FailBlockIconContainer` 하위에 빈 게임오브젝트 `FailBlockIconTemplate`을 만든다.
1. `FailBlockIconItem` 컴포넌트를 추가한다.
2. 하위에 `Image` 컴포넌트를 가진 자식 `Icon`을 만든다.
3. 하위에 `TextMeshPro - Text (UI)` + `CHText` 컴포넌트를 가진 자식 `CountText`를 만든다(`CHText`는 `TMP_Text`를 RequireComponent). `CHText`의 `_stringID`는 기본값 `-1`로 둔다(런타임에 `x{개수}` 문자열을 직접 설정).
4. `FailBlockIconItem` 컴포넌트의 `icon` 필드에 `Icon`의 Image를, `countText` 필드에 `CountText`의 CHText를 드래그해 연결한다.
5. `FailBlockIconTemplate` 게임오브젝트를 **비활성**(Inspector 좌상단 체크 해제)으로 둔다 — 복제 원본이므로.

- [ ] **Step 7: FailDetailText 생성**

Step 3과 동일 방식으로 TMP 텍스트를 복제해 `FailDetailText`로 만들어 `FailReasonRoot` 안 맨 아래에 둔다.

- [ ] **Step 8: UIGameEnd 컴포넌트 SerializeField 와이어링**

프리팹 루트의 `UIGameEnd` 컴포넌트를 선택하고 `실패 사유 표시` 헤더 아래 5개 필드를 연결한다:
- `failReasonText` ← `FailReasonText`
- `failBlockHeaderText` ← `FailBlockHeaderText`
- `failBlockIconContainer` ← `FailBlockIconContainer` (Transform)
- `failBlockIconTemplate` ← `FailBlockIconTemplate` (FailBlockIconItem)
- `failDetailText` ← `FailDetailText`

- [ ] **Step 9: 프리팹 저장 및 검증**

프리팹을 저장한다(Ctrl+S). 검증:
- `UIGameEnd` 컴포넌트의 5개 필드, `FailBlockIconItem`의 2개 필드가 모두 `None`이 아님
- `FailBlockIconTemplate`이 비활성 상태
- Console에 missing-reference 경고 없음

- [ ] **Step 10: Commit (제안)**

```bash
git add Assets/AssetBundleResources/ui/UIGameEnd.prefab
git commit -m "[UIGameEnd] 프리팹에 실패 사유 위젯 추가·연결

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>"
```

---

## Task 8: 수동 플레이테스트

**Files:** 없음 (검증 전용)

Unity 에디터는 `CHMResource`가 `Assets/AssetBundleResources/`에서 직접 로드하므로 번들 리빌드 없이 플레이테스트가 가능하다.

- [ ] **Step 1: 에디터에서 게임 실행**

Unity 에디터에서 GameScene(또는 정상 씬 흐름)으로 진입해 플레이한다.

- [ ] **Step 2: 시나리오별 확인 (한국어)**

설계 문서 §7 시나리오를 수동 확인한다:
- [ ] 하드 스테이지 시간 초과, 점수 달성·블록 잔존 → "시간 초과" + 블록 아이콘들, 점수 줄 없음
- [ ] 노멀 스테이지 이동 소진, 점수 미달·블록 클리어 → "이동 횟수 소진" + "점수 X / Y", 블록 섹션 없음
- [ ] 시간/이동 소진 + 점수·블록 둘 다 미달 → 세 섹션 모두 표시
- [ ] 보스 스테이지 체력 소진 → "체력 소진" + "보스 HP X% 남음"
- [ ] 잔여 폭탄 자동 폭발 경로 — 폭탄이 남으면 게임 속행, UIGameEnd 미표시
- [ ] 클리어 시 `failedObj` 비활성 → 실패 사유 위젯 노출 안 됨

- [ ] **Step 3: 시나리오별 확인 (영어)**

설정에서 언어를 영어로 바꾼 뒤 Step 2의 1~4를 재확인한다(영어 문자열 노출, 아이콘 동일).

- [ ] **Step 4: 블록 아이콘 검증**

남은 목표 블록 아이콘이 올바른 블록 스프라이트로 표시되고 `x개수`가 정확한지, 로드 지연 시 깨진 이미지가 보이지 않는지 확인한다.

- [ ] **Step 5: 릴리스 빌드용 번들 리빌드 (APK 빌드 전에만)**

> 에디터 플레이테스트에는 불필요. 릴리스 APK를 빌드하기 전 `AssetBundleMenuItem` 메뉴로 `json` 번들(문자열)·`ui` 번들(프리팹)을 리빌드한다. 리빌드된 번들 파일의 커밋 여부는 프로젝트 관행에 따른다.

---

## 자체 검토 결과

- **스펙 커버리지** — 설계 §3(데이터 모델)→Task 1·3, §4(데이터 흐름)→Task 5·6, §5(문자열)→Task 2, §6(프리팹·번들)→Task 7·8, §7(테스트)→Task 8. 누락 없음.
- **플레이스홀더** — 없음. 모든 코드 스텝에 완전한 코드 포함.
- **타입 일관성** — `EFailReason`, `BlockTypeCount`, `GameEndFailInfo`, `UIGameEndArg.failInfo`, `FailBlockIconItem.Setup`, `ShowFailReason`의 시그니처가 태스크 전반에서 일치. 문자열 ID 174~179가 Task 2(JSON)와 Task 6(상수)에서 동일.
