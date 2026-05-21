# UIGameEnd 실패 사유 표시 — 설계 문서

- 작성일: 2026-05-21
- 대상: `UIGameEnd` 게임 결과 화면
- 목표: 게임 실패 시 "왜 실패했는지"를 화면에 표시한다. 현재는 `"Failed..."` 문구만 나온다.

## 1. 배경

### 현재 동작

`UIGameEnd`는 `UIGameEndArg`로 `clearState`, `result`, `gold` 세 필드만 받는다
(`UIGameEnd.cs:10-15`). `GameEnd(false)`는 `result = GameOver`만 넘기고 실패 원인은
전혀 전달하지 않으므로, 실패 화면은 항상 `"Failed..."`만 출력한다.

### 현재 게임 실패 조건 (`GPGameScene.Update()`)

게임은 종료 트리거가 걸린 시점에 `clear == false`이면 실패(`GameOver`)다.

**일반/하드 스테이지** (`GPGameScene.cs:177-207`)

- 종료 트리거 (둘 중 하나)
  - 시간 초과 — `_stageInfo.time > 0`이고 `timerImg.fillAmount >= 1`
  - 이동 횟수 소진 — `_stageInfo.moveCount > 0`이고 `moveCount <= 0`
- `clear`가 `false`가 되는 조건 (= 실패 사유)
  - 보드 목표 미달성 — `checkHp == true`이고 (`GetHp() > 0` 또는 Fish 또는 Ball)인
    블록이 하나라도 남음 (RainbowPang 제외). 깨야 하는 블록은 **전부** 깨야 한다.
  - 목표 점수 미달 — `targetScore > 0`인데 `curScore < targetScore`

**보스 스테이지** (`GPGameScene.cs:208-212`)

- 플레이어 HP 소진 — `_boss.hp <= 0`인데 보스 HP가 남아있음 → 실패
- 보스 HP를 다 깎으면 → 클리어

### 목표 블록의 정의

클리어 판정(`GPGameScene.cs:188-191`)이 "남았다"고 보는 블록:

- HP 블록 — `checkHp == true`이고 `GetHp() > 0` (스테이지 데이터의 `info.hp`로 양수
  HP를 받은 Wall·CatBox·Locker 등)
- Fish 블록 (`EBlockState.Fish`)
- Ball 블록 (`EBlockState.Ball`)

판정에서 빠지는 블록: 일반 고양이 블록(HP `-1`), RainbowPang(명시 제외),
WallCreator/PotalCreator(`CheckHpBlock()`이 false → `checkHp == false`),
폭탄 인접 변환 블록(`checkHp`가 false로 꺼짐, `GPBombResolver.cs:483-486`).

## 2. 설계 결정 사항

| 항목 | 결정 |
|------|------|
| 표시 정보 수준 | 종료 트리거 + 미달 목표 (점수, 남은 목표 블록) |
| 문구 관리 | CHMString 로컬라이제이션 (한/영, ID 174~179 신규) |
| 목표 블록 표현 | 종류별 + 블록 스프라이트 아이콘 |
| 아키텍처 | `GameEnd`가 사유 데이터만 계산, `UIGameEnd`가 문자열 조립·표시 |

게임 로직(GP)과 표현·로컬라이제이션(UI)을 분리한다. 문구·레이아웃 변경은
`UIGameEnd`에서만 일어난다 — UIBase 하위 클래스가 자기 화면 표시를 책임지는
기존 구조와 일치한다.

## 3. 데이터 모델

### enum (`Defines.cs`)

```csharp
public enum EFailReason
{
    None,
    TimeOver,   // 시간 초과
    MoveOver,   // 이동 횟수 소진
    HpOver,     // 체력 소진 (보스전)
}
```

### 실패 사유 데이터 (`UIGameEnd.cs`)

```csharp
// 종류별 남은 목표 블록 1건
public class BlockTypeCount
{
    public Defines.EBlockState state;
    public int count;
}

public class GameEndFailInfo
{
    public Defines.EFailReason reason;
    public int curScore;                  // 일반/하드
    public int targetScore;               // 일반/하드
    public List<BlockTypeCount> remainBlocks;  // 일반/하드, EBlockState별 그룹
    public float bossHpRatio;             // 보스전, 0~1
}
```

`UIGameEndArg`에 `public GameEndFailInfo failInfo;` 한 필드를 추가한다.
클리어 시에는 `null`.

## 4. 데이터 흐름

### 계산 — `GPGameScene`

`GameEnd` 내부의 `ShowUI(EUI.UIGameEnd, ...)` 호출 시점(`GPGameScene.cs:898`)에
`!clear`이면 `GameEndFailInfo`를 생성해 `UIGameEndArg.failInfo`에 담는다.
이 시점은 잔여 폭탄 자동 폭발(아래 참고)이 끝난 뒤이므로 보드 상태가 확정적이고
stale 누수가 없다.

- 보스전 → `reason = HpOver`, `bossHpRatio = bossHpImage.fillAmount`
- 일반/하드 →
  - 트리거 재유도: `_stageInfo.time > 0 && timerImg.fillAmount >= 1f`이면 `TimeOver`,
    아니면 `MoveOver`
  - `curScore = curScore.Value`, `targetScore = _stageInfo.targetScore`
  - `remainBlocks` = 새 헬퍼 `CollectRemainingObjectiveBlocks()`

새 헬퍼 `CollectRemainingObjectiveBlocks()`:

- `Update()`의 클리어 술어(`GPGameScene.cs:188-191`)와 **동일 조건**으로 보드 전체를
  훑어, `GetBlockState()` 기준으로 그룹핑해 `List<BlockTypeCount>`를 반환한다.
- `Update()`의 조기 break 로직은 그대로 두고, 이 헬퍼는 게임 종료 시 1회만 실행한다.

### 잔여 폭탄 자동 폭발 경로 (참고)

`GameEnd`는 결과 화면을 띄우기 전에 `_bombResolver.CatPang(true)`로 보드에 안 터진
폭탄 블록(`IsBombBlock()`, 단 PinkBomb·RainbowPang 제외)이 남았는지 확인한다
(`GPGameScene.cs:887-896`). 남아 있으면 `EGameState.CatPang` 상태로 전환하고 남은
폭탄을 전부 자동 폭발시킨 뒤 게임을 플레이 상태로 되돌리고 `return`한다 —
이 경로에서는 `UIGameEnd`를 띄우지 않는다. 폭발 결과로 실패가 클리어로 뒤집힐 수
있으므로, `failInfo` 계산은 반드시 이 경로를 통과한 뒤(`:898`)에 한다.

### 표시 — `UIGameEnd`

`Start()`의 `GameOver` 분기에서 `failedObj` 하위 위젯에 렌더한다.

- 트리거 줄 → `failReasonText`(TMP_Text)에 `EFailReason`별 문자열
  (TimeOver→174, MoveOver→175, HpOver→176)
- 목표 블록 → `failInfo.remainBlocks`가 비어있지 않으면 `failBlockHeaderText`(헤더,
  ID 179)를 켜고 `failBlockIconContainer`에 종류마다 아이콘 아이템을 복제
- 점수/보스 줄 → `failDetailText`(TMP_Text)
  - 일반/하드: `targetScore > 0 && curScore < targetScore`이면 ID 178을
    `string.Format`으로 `curScore`/`targetScore` 치환. 수치는 `ToString("N0")`로
    천 단위 콤마. 점수 미달이 아니면 비활성.
  - 보스전: ID 177을 `Mathf.RoundToInt(bossHpRatio * 100)`로 치환
- 클리어 시 `failedObj` 자체가 비활성이므로 `GameClear` 분기는 수정 불필요

### 아이콘 아이템 위젯 — `FailBlockIconItem` (신규)

`UIBase`가 아닌 일반 `MonoBehaviour`다 (화면 최상위가 아닌 위젯).

- `[SerializeField] Image icon`, `[SerializeField] CHText countText`
- `Setup(EBlockState state, int count)`
  - `countText`에 `x{count}` 표시 (언어 무관 기호, 하드코딩)
  - `icon.enabled = false`로 두고 `CHMResource.Instance.LoadSprite(state, sprite => …)`
    호출, 콜백 도착 시 `icon.sprite` 설정 후 `icon.enabled = true`
    (잘못된 스프라이트가 1프레임 보이는 것 방지)
- 그룹 키는 정확한 `EBlockState` — Wall/Locker 계열을 병합하지 않는다 (아이콘이
  시각적으로 구분함)
- `failBlockIconContainer`는 `HorizontalLayoutGroup` 단일 행. 실사용상 목표 블록
  종류는 **5개 이하**로 가정한다 (그 이상이면 행이 넘침 — 알려진 한계).

## 5. 문자열 ID 목록

`StringKorea.json` / `StringEnglish.json`에 ID 174~179 6건 추가 (현재 마지막 ID 173).

| ID | StringKorea | StringEnglish | 용도 |
|----|-------------|---------------|------|
| 174 | 시간 초과 | Time's Up | 트리거 |
| 175 | 이동 횟수 소진 | Out of Moves | 트리거 |
| 176 | 체력 소진 | Out of HP | 트리거(보스) |
| 177 | 보스 HP {0}% 남음 | Boss HP {0}% Left | 보스 미달 목표 |
| 178 | 점수 {0} / {1} | Score {0} / {1} | 점수 미달 목표 |
| 179 | 남은 목표 블록 | Blocks Remaining | 블록 섹션 헤더 |

`{0}`·`{1}`은 `UIGameEnd`에서 `string.Format`으로 치환한다. 아이콘 개수 `x{count}`는
언어 무관 기호이므로 문자열 ID를 두지 않고 하드코딩한다.

## 6. 프리팹 수동 작업 & 번들 리빌드

### `UIGameEnd.prefab` 편집

`failedObj`("Failed") 하위에 다음을 추가한다:

- `failReasonText` — TMP_Text, 트리거 줄
- `failBlockHeaderText` — TMP_Text, "남은 목표 블록" 헤더
- `failBlockIconContainer` — Transform + `HorizontalLayoutGroup`, 아이콘 행
- `failBlockIconTemplate` — 비활성 자식, `FailBlockIconItem` 컴포넌트 부착
  (자식에 Image + CHText)
- `failDetailText` — TMP_Text, 점수 줄 (보스전에선 보스 HP 줄로 재사용)
- 수직 배치는 `failedObj`에 `VerticalLayoutGroup`을 두거나 수동 배치

### SerializeField 와이어링 (Unity 에디터)

- `UIGameEnd` 5개: `failReasonText`, `failBlockHeaderText`, `failBlockIconContainer`,
  `failBlockIconTemplate`, `failDetailText`
- `FailBlockIconItem` 템플릿 2개: `icon`(Image), `countText`(CHText)
- 참조형 연결이므로 UnityMCP `component_set`으로 가능하나 연결 후 검증 필요
  (Sprite 참조는 `component_set`이 못 다루지만, 여기 연결 대상은 컴포넌트/Transform
  참조라 가능)

### 번들 리빌드 (`AssetBundleMenuItem` 메뉴)

- `json` 번들 — StringKorea/English JSON 수정 반영
- `ui` 번들 — UIGameEnd.prefab 수정 반영

## 7. 테스트 시나리오 (수동)

UI 자동 테스트 프레임워크가 없으므로 에디터에서 수동 확인한다.

1. 하드 스테이지 시간 초과, 점수 달성·블록 잔존 → "시간 초과" + 블록 아이콘들
   (점수 줄 없음)
2. 노멀 스테이지 이동 소진, 점수 미달·블록 클리어 → "이동 횟수 소진" + "점수 X/Y"
   (블록 섹션 없음)
3. 시간/이동 소진 + 점수·블록 둘 다 미달 → 세 섹션 모두 표시
4. 보스 스테이지 체력 소진 → "체력 소진" + "보스 HP X% 남음"
5. 잔여 폭탄 자동 폭발 경로(`GPGameScene.cs:887-896`) — 폭탄이 남으면 게임 속행,
   `UIGameEnd` 미표시 확인
6. 클리어 시 `failedObj` 비활성 → 실패 UI 노출 안 됨
7. 한국어/영어 양쪽 언어로 1~4 재확인

## 8. 영향받는 파일

| 파일 | 변경 |
|------|------|
| `Assets/Scripts/Defines.cs` | `EFailReason` enum 추가 |
| `Assets/Scripts/UI/UIGameEnd.cs` | `BlockTypeCount`·`GameEndFailInfo` 추가, `UIGameEndArg.failInfo` 추가, `GameOver` 분기 렌더링 |
| `Assets/Scripts/UI/FailBlockIconItem.cs` | 신규 위젯 |
| `Assets/Scripts/Scenes/GPGameScene.cs` | `CollectRemainingObjectiveBlocks()` 추가, `GameEnd`에서 `failInfo` 계산·전달 |
| `Assets/AssetBundleResources/json/StringKorea.json` | ID 174~179 추가 |
| `Assets/AssetBundleResources/json/StringEnglish.json` | ID 174~179 추가 |
| `Assets/AssetBundleResources/ui/UIGameEnd.prefab` | 위 위젯 추가·와이어링 |
