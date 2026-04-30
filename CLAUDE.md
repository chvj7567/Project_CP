# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**CatPang** — Unity로 제작된 Android용 모바일 매치-3 퍼즐 게임. 개발 환경은 Windows, 빌드 타겟은 Android (Google Play).

## 빌드 및 개발

Unity CLI 빌드 워크플로우는 별도로 구성되어 있지 않으며, 모든 빌드는 Unity 에디터에서 수행한다.

- **에셋 번들 빌드** — `Assets/Scripts/Editor/AssetBundleMenuItem.cs`에 등록된 에디터 메뉴 항목 사용
- **스테이지/맵 제작** — `Assets/Scripts/Editor/CHToolCreateMap.cs` (EditorWindow, 9×9 그리드 시각 편집)
- **게임뷰 해상도** — `Assets/Scripts/Editor/CHToolGameView.cs` → CatPang > 게임뷰 720×1280 설정
- **APK 출력** — 프로젝트 루트의 `CatPang.apk` (릴리즈 빌드, 약 90MB)

## 씬 흐름

세 개의 씬이 순서대로 실행된다:

1. **ResourceDownloadScene** (`Assets/Scripts/Scenes/ResourceDownload.cs`) — CHMMain 초기화, 에셋 번들 다운로드, CHMData.LoadLocalData
2. **FirstScene** (`Assets/Scripts/Scenes/LBLobbyScene.cs`) — GPGS 로그인(LBLoginHandler), 튜토리얼(LBTutorial), 스테이지 선택 → GameScene 로드
3. **GameScene** (`Assets/Scripts/Scenes/GPGameScene.cs`) — InitUI/BindUI/LoadImage(비동기) 후 보드 초기화

## 아키텍처

### 매니저 허브 패턴

`CHMMain` (싱글톤, DontDestroyOnLoad)이 중앙 서비스 로케이터 역할을 하며 `Update()`에서 `CHMUI.UpdateUI()`를 호출한다. 모든 매니저는 정적 프로퍼티로 접근한다:

```
CHMMain.Pool        → CHMPool        (오브젝트 풀링)
CHMMain.Resource    → CHMResource    (에셋 로딩 추상화)
CHMMain.AssetBundle → CHMAssetBundle (번들 캐시, CHSingleton<T>)
CHMMain.Json        → CHMJson        (게임 설정 데이터)
CHMMain.Data        → CHMData        (플레이어 저장 데이터, CHSingleton<T>)
CHMMain.UI          → CHMUI          (UI 스택 매니저)
CHMMain.Sound       → CHMSound       (오디오)
CHMMain.String      → CHMString      (로컬라이제이션 키 / IAP ID)
CHMMain.Admob       → CHMAdmob       (Google Mobile Ads)
CHMMain.GPGS        → CHMGPGS        (Google Play Games Services)
CHMMain.IAP         → CHMIAP         (Unity Purchasing)
```

**초기화 순서**: CHMJson.Init() (비동기 완료 대기) → CHMPool.Init() → CHMSound.Init()

모든 매니저는 `TaskCompletionSource`를 사용한 비동기 초기화 패턴을 따른다.

### 리소스 로딩

`CHMResource`가 두 가지 모드를 자동으로 추상화한다:
- **에디터:** `Assets/AssetBundleResources/{bundleName}/{assetName}`에서 직접 로드
- **빌드:** `CHMAssetBundle`을 통해 미리 빌드된 번들에서 로드

항상 `CHMMain.Resource.Load<T>()` 또는 `CHMMain.Resource.Instantiate()`를 사용하고, `Resources.Load`를 직접 호출하지 않는다.

**에셋 번들 폴더 구성** (`Assets/AssetBundleResources/`):

| 번들 | 내용 |
|------|------|
| `ui/` | UI 프리팹 14개 (EUI별 1:1 대응) |
| `unit/` | Block.prefab, GoldImg.prefab |
| `effect/` | FireCracker, Damage, BlueBall 프리팹 |
| `sprite/` | EBlockState별 스프라이트 이미지 |
| `sound/` | ESound별 오디오 클립 |
| `font/` | NotoSansKR_SemiBold, Habo 폰트 에셋 |
| `data/` | ConstValue, StringKorea, StringEnglish JSON |
| `json/` | Stage, StageBlock, Mission, Shop, Guide, Tutorial JSON |

### UI 시스템

`CHMUI`는 지연 큐를 통해 ShowUI/CloseUI를 다음 프레임에 처리한다:
- `waitActiveUI` 큐 → `ShowUI(waitData)`: Instantiate → `InitUI(CHUIArg)` → SetActive
- `waitCloseUI` 큐 → `CloseWaitUI(waitData)`: `CloseUI()` 가상 메서드 호출 → Destroy
- `ShowUI()` 반환값은 고유 UID이며, 이를 이용해 특정 인스턴스를 `CloseUI(uid)`로 닫을 수 있다
- `showCurrentBackground = false`로 열면 UICamera + 별도 UICanvas가 생성되며 해당 UI 닫힐 때 자동 정리

**EUI 목록** (총 14개):
```
EventSystem, UICamera, UICanvas, UIChoice, UIAlarm
UIMission, UIShop, UIGameStart, UIGameEnd, UISetting
UIStageSelect, UINickname, UIRank, UIDataDelete
```

**UIBase 상속 패턴**: `UIBase`가 `Awake()`에서 배경/뒤로가기 버튼 닫기 리스너를 자동 등록한다. 하위 클래스 구현 방식:

```csharp
public class UIGameStart : UIBase
{
    public override void InitUI(CHUIArg _uiArg)  // ShowUI 직후 호출, 인자 캐스팅
    {
        var arg = _uiArg as UIGameStartArg;
    }

    private void Start()  // InitUI 이후, UniRx Subscribe 등록
    {
        btn.OnClickAsObservable().Subscribe(_ => { ... });
    }

    public override void CloseUI() { }  // 선택적 오버라이드
}
```

### 오브젝트 풀링

`CHMPool`이 GameObject를 재활용한다. 풀링 대상 오브젝트는 `CHPoolable` 마커 컴포넌트를 가진다. `Destroy()` 대신 항상 `CHMMain.Pool.Push()`로 오브젝트를 반환한다.

스크롤뷰 항목은 `CHPoolingScrollView` + `*ScrollViewItem` 패턴으로 풀링된다 (MissionScrollView, ShopScrollView, RankScrollView).

### 데이터 저장

- **로컬:** `Application.persistentDataPath/{key}.json` — `CHMData`가 관리
- **클라우드:** `"CatPang"` 슬롯명으로 Google Play Games Services 저장 — `connectGPGS=true`일 때 동기화
- **설정값:** 볼륨, 언어, 색상 설정은 `PlayerPrefs` 사용 (키는 `CHMString`에 정의)

**주요 저장 데이터 구조** (`Assets/Scripts/Data.cs`):

```
Data.Login      → 스테이지 진행도, 닉네임, 선택 고양이, 아이템 수량
Data.Collection → 수집품 개수 (key-value)
Data.Mission    → 미션별 clearState(EClearState), repeatCount
Data.Shop       → 상품 구매 여부 (key-bool)
```

### 게임 로직

스크립트는 `Assets/Scripts/GamePlay/`에 있다:
- **GPBoard.cs** — `Block[,]` 9×9 그리드 상태 관리
- **GPMatchChecker.cs** — 3-매치 및 정사각형 매치 판정
- **GPBombResolver.cs** — 폭탄 이펙트 및 연쇄 처리
- **GPBossController.cs** — 보스 스테이지 AI
- **GPTutorial.cs** — 튜토리얼 시퀀스

`Block.cs` (`Assets/Scripts/`): 개별 블록 컴포넌트. 드래그(OnBeginDrag/OnEndDrag로 4방향 EDrag 계산), `IsNormalBlock()` / `IsBombBlock()` / `IsFixdBlock()` / `IsSpecialBombBlock()` 분류 메서드 제공.

게임 상태는 `ReactiveProperty<EGameState>` (UniRx)로 추적하며, 게임 모드는 시간 제한/이동 횟수 제한 두 가지다.

## 네이밍 규칙

| 접두사 | 의미 |
|--------|------|
| `CHM`  | 매니저 클래스 (예: `CHMMain`, `CHMPool`) |
| `CH`   | 유틸리티/컴포넌트 클래스 (예: `CHSingleton`, `CHUtil`, `CHTMPro`) |
| `UI`   | UI 패널 클래스 (예: `UIShop`, `UIMission`) |
| `GP`   | 게임플레이 클래스 (예: `GPBoard`, `GPMatchChecker`) |
| `LB`   | 로비 클래스 (예: `LBLobbyScene`, `LBLoginHandler`) |
| `E`    | 열거형 타입 (예: `EUI`, `EBlockState`, `EGameState`) |

## 주요 열거형 (`Assets/Scripts/Defines.cs`)

- **EBlockState** (54개): Cat1~7, Arrow1~6, Wall, Portal, CatPang, 5색 특수폭탄, CatHat/CatSkin/Locker/CatBox/LockerBox(각 5종), WallCreator, PortalCreator, RainbowPang, Ball, Fish
- **EUI** (14개): 위 UI 시스템 섹션 참조
- **EGameState**: CatPang, GameOver, GameClear, GameOverWait, GameClearWait, NormalOrHardStagePlay, BossStagePlay
- **EJsonType**: StringKorea/English, Stage, StageBlock, Mission, Shop, Guide, Tutorial, ConstValue
- **EClearState**: NotDoing, Doing, Clear

## 주요 외부 의존성

| 패키지 | 용도 |
|--------|------|
| UniRx | 리액티브 확장 — `ReactiveProperty`, `Subject`, 버튼 클릭 옵저버블 |
| DOTween | 트윈 애니메이션 |
| TextMesh Pro | 한국어/영어 로컬라이제이션을 포함한 텍스트 렌더링 (`CHTMPro` 래퍼 사용) |
| Google Play Games SDK | 인증 + 클라우드 저장 |
| Google Mobile Ads SDK | 배너, 전면, 보상형 광고 |
| Unity Purchasing | IAP (소모성 상품 3종: 광고 제거, 시간 추가, 이동 횟수 추가) |

## 로컬라이제이션

`CHMJson`이 로드하는 두 개의 언어 사전 (한국어 기본, 영어 폴백). 앱 시작 시 언어 자동 감지. 문자열 키는 `CHMString`에 상수로 정의. 텍스트 표시는 `CHTMPro.SetStringID(key)` 사용.

## 에디터 툴 (`Assets/Scripts/Editor/`)

- `CHToolCreateMap.cs` — 스테이지/레벨 디자인 (EditorWindow, 9×9 그리드, JSON 저장)
- `CHToolGameView.cs` — 게임뷰 720×1280 해상도 설정
- `CHToolString.cs` — 로컬라이제이션 문자열 관리
- `AssetBundleMenuItem.cs` — 에셋 번들 빌드
- `CHTool.cs` — JSON 직렬화 헬퍼 구조체
- `CustomReadOnly.cs` — 인스펙터용 `[ReadOnly]` 어트리뷰트
