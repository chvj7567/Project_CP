# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**CatPang** — Unity로 제작된 Android용 모바일 매치-3 퍼즐 게임. 개발 환경은 Windows, 빌드 타겟은 Android (Google Play). Unity 6000.0.68f1 (Unity 6).

게임 코드는 `Assets/Scripts/`에, 재사용 가능한 인프라는 임베디드 UPM 패키지 `Packages/com.chvj.unityinfra/`에 분리되어 있다.

## 빌드 및 개발

Unity CLI 빌드 워크플로우는 별도로 구성되어 있지 않으며, 모든 빌드는 Unity 에디터에서 수행한다.

- **리소스 시스템** — Unity **Addressables** 사용. `Assets/AssetBundleResources/` 하위 전 에셋에 `"Resource"` Addressables 라벨이 부여되어 있고 Local 그룹으로 APK에 포함된다 (원격 다운로드 없음). 라벨/그룹 재정비는 `Assets/Scripts/Editor/CHToolMigrateToAddressables.cs` 사용. APK 빌드 전 `Window > Asset Management > Addressables > Groups`에서 빌드 필요.
- **스테이지/맵 제작** — `Assets/Scripts/Editor/CHToolCreateMap.cs` (EditorWindow, 9×9 그리드 시각 편집, JSON 저장)
- **게임뷰 해상도** — `Assets/Scripts/Editor/CHToolGameView.cs` → `CatPang` 메뉴
- **옵트인 모듈 설정** — `Tools/ChvjUnityInfra/Settings` 에디터 창에서 Ads/IAP/Social 토글 (아래 패키지 섹션 참조)

## 씬 흐름

세 개의 씬이 빌드 인덱스 순서대로 실행된다 (`Assets/Scenes/*.unity`):

1. **ResourceDownloadScene** (`Assets/Scripts/Scenes/ResourceDownload.cs`) — `CHMResource.EnsureInit()` → `PreloadAsync()` (라벨 `"Resource"` 프리로드) → `CHMMain.EnsureInitialized()` → `CHMData.LoadLocalData` → FirstScene 로드
2. **FirstScene** (`Assets/Scripts/Scenes/LBLobbyScene.cs`) — GPGS 로그인(`LBLoginHandler`), 튜토리얼(`LBTutorial`), 스테이지 선택 → GameScene 로드
3. **GameScene** (`Assets/Scripts/Scenes/GPGameScene.cs`) — UI 초기화/바인딩/이미지 로드(비동기) 후 보드 초기화

`EScene` enum 이름은 `.unity` 파일명과 1:1 일치하므로 `SceneManager.GetActiveScene().name == nameof(Defines.EScene.X)`로 비교한다.

## 아키텍처

### com.chvj.unityinfra 패키지

`Packages/com.chvj.unityinfra/`는 게임에 독립적인 인프라를 담은 임베디드 로컬 UPM 패키지다. 모든 타입은 `ChvjUnityInfra` 네임스페이스에 있다. 패키지 자체 문서는 `Packages/com.chvj.unityinfra/README.md` 참조 — **인프라를 건드릴 때 먼저 읽을 것.**

패키지가 제공하는 것:
- **Core** — `CHSingletonStatic<T>` (일반 클래스 싱글톤, `T.Instance` 접근), `CHSingleton<T>` (MonoBehaviour 싱글톤), `CHUtil`, `CompositeDisposable`, `JsonArrayUtility`, `ReadOnlyAttribute`
- **Resource** — `CHMResource` (Addressables 래퍼, enum-이름 = 에셋 파일명 규칙)
- **Pool/Audio/UI** — `CHMPool`, `CHMSound`, `CHMUI`, `UIBase`, `CHButton`, `CHText`, `CHToggle`, `CHPoolingScrollView`, `CHDebugLog`
- **옵트인 모듈** — `CHMAdmob`(Ads), `CHMIAP`(IAP), `CHMGPGS`(Social). 각각 `UNITY_INFRA_ADS` / `UNITY_INFRA_IAP` / `UNITY_INFRA_SOCIAL` 스크립팅 심볼로 컴파일 게이팅된다. `Tools/ChvjUnityInfra/Settings` 창에서 토글하면 심볼이 추가/제거된다.

핵심 규칙: **enum 항목 이름 = Addressables 에셋 파일명** (예: `EUI.UIShop` → `UIShop.prefab`). `None`/`Max` 항목은 자동 skip.

### 게임 측 매니저 어댑터

`Assets/Scripts/Manager/`의 동명 매니저(`CHMResource`, `CHMSound`, `CHMUI`, `CHMPool`)는 **글로벌 네임스페이스의 얇은 어댑터**로, 내부에서 패키지의 `ChvjUnityInfra.CHMX.Instance`에 위임하며 게임 전용 enum(`Defines.EUI` 등) 시그니처를 노출한다. 어댑터 파일 안에서는 패키지 타입을 항상 fully-qualified(`ChvjUnityInfra.CHMResource.Instance...`)로 호출한다 — 동명 클래스 모호성 회피.

게임 전용 매니저 `CHMJson`, `CHMData`, `CHMString`, `CHMTime`은 모두 `CHSingletonStatic<T>` 패턴이며 `XXX.Instance`로 접근한다.

### 매니저 허브 — CHMMain

`CHMMain` (MonoBehaviour, `@CHMMain` GameObject, DontDestroyOnLoad)은 부팅 초기화 오케스트레이터다.

- `CHMMain.EnsureInitialized()` — 모든 매니저를 순서대로 초기화하는 idempotent awaitable (Task 캐싱). 부팅 시 await.
- 정적 접근자: `CHMMain.Pool / Resource / UI / Json / String / Sound / Time` — 접근만으로 초기화를 fire-and-forget 트리거한다.
- `LateUpdate()`에서 ESC 키 처리 (GameScene 외 씬에서 종료 확인 팝업).

**초기화 순서** (`CHMMain.InitAsync`):
```
CHMResource.EnsureInit → CHMJson.Init → GameFontProvider.PreloadAsync
→ CHMPool.Init → CHMSound.Init → CHMTime.Init → CHMUI.Init
→ CHText.StringProvider/FontProvider · CHButton/CHToggle SoundHook 등록
→ CHMAdmob.AcquireReward += DailyMissionService.OnAdWatched
```

`CHMData`, 그리고 옵트인 매니저 `CHMAdmob`/`CHMIAP`/`CHMGPGS`는 `CHMMain` 접근자에 없다 — 각각 `CHMData.Instance`, `ChvjUnityInfra.CHMAdmob.Instance` 등으로 직접 접근한다.

### 리소스 로딩

항상 `CHMResource`를 통해 로드하며 `Resources.Load`를 직접 호출하지 않는다. 게임 어댑터의 타입별 헬퍼:

```csharp
CHMResource.Instance.LoadJson(EJsonType, cb);      // TextAsset
CHMResource.Instance.LoadSprite(EBlockState, cb);  // Sprite (string 오버로드도 있음)
CHMResource.Instance.LoadSound(ESound, cb);        // AudioClip
CHMResource.Instance.InstantiateUI(EUI, cb);       // GameObject
CHMResource.Instance.InstantiateEffect(EEffect, cb);
CHMResource.Instance.Instantiate(go, parent);      // CHPoolable 있으면 풀에서 Pop
CHMResource.Instance.Destroy(go, time);            // CHPoolable 있으면 풀로 Push
```

에셋은 `Assets/AssetBundleResources/` 하위 8개 폴더에 분류된다: `ui/` `unit/` `effect/` `sprite/` `sound/` `font/` `data/` `json/`.

### 오브젝트 풀링

`CHMPool`이 GameObject를 재활용한다. 풀링 대상은 `CHPoolable` 마커 컴포넌트를 가진다. `Destroy()` 대신 `CHMResource.Instance.Destroy()` (또는 `CHMPool.Instance.Push()`)로 반환한다. `CHMResource.Instance.Instantiate()`는 `CHPoolable`이 붙은 프리팹이면 자동으로 풀에서 Pop한다.

스크롤뷰 항목은 `CHPoolingScrollView` + `*ScrollViewItem` 패턴으로 풀링된다 (`MissionScrollView`, `ShopScrollView`, `RankScrollView`).

### UI 시스템

`ChvjUnityInfra.CHMUI`가 UI 캐싱·재사용·ESC 자동닫기를 담당한다 (게임 측 `CHMUI`는 어댑터).

- `ShowUI(EUI, UIArg, callback)` — 캐시에 있으면 재사용, 없으면 Addressables로 instantiate → `Init()` → `InitUI(arg)` → 활성화. `ShowUIAsync`는 await 버전.
- 캔버스는 씬의 `UICanvas` 태그 GameObject를 찾거나, 없으면 `UICanvas` 프리팹을 Addressables로 instantiate한다.
- `CloseUI(EUI, reuse)` — `reuse=true`면 캐시에 남겨 다음 ShowUI에서 재사용, `false`면 Destroy.
- ESC 키 → 최상위 UI 자동 닫힘 (`BlockEscClose=true`로 오버라이드 시 차단).
- 씬 전환 시 캔버스/캐시 전부 무효화.

**EUI 목록** (`Defines.EUI`, 13개): `EventSystem, UICamera, UICanvas, UIAlarm, UIMission, UIShop, UIGameStart, UIGameEnd, UISetting, UIStageSelect, UINickname, UIRank, UIConfirm`

**UIBase 상속 패턴**: 게임의 `UIBase`(`Assets/Scripts/UI/UIBase.cs`)는 패키지 `ChvjUnityInfra.UIBase`를 상속하며 게임 전용 API를 추가한다. 패키지 `UIBase`는 `Init()`에서 배경/뒤로가기 버튼의 닫기 리스너를 자동 등록한다.

```csharp
public class UIShop : UIBase
{
    public override void InitUI(CHUIArg _uiArg)  // ShowUI 직후 호출, 인자 캐스팅
    {
        var arg = _uiArg as UIShopArg;            // UIShopArg : CHUIArg
    }

    private void Start() { /* UniRx Subscribe 등록 */ }

    public override void CloseUI() { }            // 선택적 — Close() 경로에서 호출됨
}
```

게임 `UIBase`가 추가하는 것: `InitUI(CHUIArg)` 오버라이드 지점 (패키지의 `InitUI(UIArg)`는 `sealed`), `CloseUI()` 가상 메서드, `actBack` 콜백 (모든 닫기 경로에서 fire), `eUIType` 프로퍼티.

### 데이터 저장

- **로컬:** `Application.persistentDataPath/{key}.json` — `CHMData`가 관리. `CHMData.LoadLocalData()` / `SaveData()`.
- **클라우드:** `"CatPang"` 슬롯명으로 Google Play Games Services 저장 — GPGS 연결 시 동기화 (`CHMData`에 local/cloud 데이터 dict 쌍 존재).
- **설정값:** 볼륨, 언어 등은 `PlayerPrefs` 사용.

**주요 저장 데이터 구조** (`Assets/Scripts/Data.cs`):
```
Data.Login      → 스테이지 진행도, 닉네임, 선택 고양이, 아이템 수량,
                  일일 미션 카운터(stageClearCountToday 등), lastDailyResetDateKey
Data.Collection → 수집품 개수 (key-value)
Data.Mission    → 미션별 clearState(EClearState), startValue, repeatCount
Data.Shop       → 상품 구매 여부 (key-bool)
```

### 시간 / 일일 미션

- **CHMTime** — NTP 기반 서버 시각 매니저. 디바이스 시각 위변조 방지를 위해 부팅 시 NTP를 1회 받고(`time.google.com` → `pool.ntp.org` 폴백, 실패 시 백그라운드 재시도) 이후 `realtimeSinceStartup` 경과로 보정. `IsAvailable`, `UtcNow`, `GetUtcDateKey()` 제공.
- **DailyMissionService** — 일일 미션 자정 리셋 판정 + 카운터 hook(`OnStageClear`/`OnBlockDestroyed`/`OnAdWatched`/`MarkAttendance`) + 진행도 조회. NTP 미수신 상태에서는 리셋하지 않는다(위변조 안전). 모든 진입점에서 `CheckAndResetIfNeeded()`를 먼저 호출한다.

### 게임 로직

스크립트는 `Assets/Scripts/GamePlay/`에 있다:
- **GPBoard.cs** — `Block[,]` 9×9 그리드 상태 관리
- **GPMatchChecker.cs** — 3-매치 및 정사각형 매치 판정
- **GPBombResolver.cs** — 폭탄 이펙트 및 연쇄 처리
- **GPBossController.cs** — 보스 스테이지 AI (`EBossSkillType`: Wall/Creator/CatBox)
- **GPTutorial.cs** — 튜토리얼 시퀀스

`Block.cs` (`Assets/Scripts/`): 개별 블록 컴포넌트. 드래그(`OnBeginDrag`/`OnEndDrag`로 4방향 `EDrag` 계산), `IsNormalBlock()` / `IsBombBlock()` / `IsFixdBlock()` / `IsSpecialBombBlock()` 분류 메서드 제공.

게임 상태는 `ReactiveProperty<EGameState>` (UniRx)로 추적하며, 게임 모드는 시간 제한/이동 횟수 제한 두 가지다. 실패 시 `EFailReason`(TimeOver/MoveOver/HpOver)이 `UIGameEnd`에 표시된다.

**스테이지 구조** (`Assets/AssetBundleResources/json/Stage.json`):
- Stage.json 행 수는 250개이지만, 플레이 모드 기준 실제 스테이지 수는 **노멀 150 + 하드 150 + 보스 100 = 400**이다.
- `group < 100000` (group 1~15) → 하드·노멀 공유 스테이지 150개. 노멀 모드는 별도 JSON 없이 하드 데이터 재사용, 실행 시 **시간제한 제거 + 이동횟수 2배** 적용.
- `group >= 100000` (group 100001~100010) → 보스 스테이지 100개.

## 네이밍 규칙

| 접두사 | 의미 |
|--------|------|
| `CHM`  | 매니저 클래스 (예: `CHMMain`, `CHMPool`, `CHMTime`) |
| `CH`   | 유틸리티/컴포넌트 클래스 (예: `CHSingletonStatic`, `CHUtil`, `CHText`) |
| `UI`   | UI 패널 클래스 (예: `UIShop`, `UIMission`) |
| `GP`   | 게임플레이 클래스 (예: `GPBoard`, `GPMatchChecker`) |
| `LB`   | 로비 클래스 (예: `LBLobbyScene`, `LBLoginHandler`) |
| `E`    | 열거형 타입 (예: `EUI`, `EBlockState`, `EGameState`) |

## 주요 열거형 (`Assets/Scripts/Defines.cs`)

- **EBlockState** (`Max = 84`): `Cat1~7`(0~6), `Arrow1~6`(10~15), `Wall`(16), `Potal`(17), `CatPang`(18), 5색 특수폭탄(19~23), `Fish`(24), `CatBox1~5`(40~44), `WallCreator`/`PotalCreator`(45~46), `RainbowPang`(52), `Ball`(53), 고양이 스킨 테마 6종 × Cat1~5(54~83: CatCrown/CatFlowers/CatMushroom/CatParty/CatSanta/CatStrawberry). **슬롯 25~39, 47~51은 삭제된 옛 항목의 빈 슬롯** — `StageBlock.json`의 int 값 호환을 위해 보존하므로 새 항목은 끝에 append.
- **EUI** (13개): 위 UI 시스템 섹션 참조
- **EScene**: ResourceDownloadScene, FirstScene, GameScene — `.unity` 파일명과 1:1 일치
- **EJsonType**: StringKorea/English, Select, Monster, Stage, StageBlock, Mission, Shop, Guide, Tutorial, ConstValue
- **EGameState**: CatPang, GameOver, GameClear, GameOverWait, GameClearWait, NormalOrHardStagePlay, BossStagePlay
- **EFailReason**: None, TimeOver, MoveOver, HpOver (`default(EFailReason) == None`)
- **EClearState**: None, NotDoing, Doing, Clear
- **EDailyCounter**: None, Attendance, NormalStageClear, BlockDestroy, AdWatch — 일일 미션 카운터 종류

## 로컬라이제이션

`CHMJson`이 한국어/영어 문자열 사전을 로드한다 (한국어 기본, 영어 폴백, 앱 시작 시 언어 자동 감지). 문자열 키는 `CHMString`에 상수로 정의.

표시는 패키지 `CHText` 기반이다. 게임은 `CHText`를 상속한 `CHTMPro`(`Assets/Scripts/Function/CHTMPro.cs`)를 프리팹에서 사용 — `SetStringID(key)`로 텍스트 설정. i18n/폰트는 Provider 주입으로 동작하며, `GameStringProvider : IStringProvider`와 `GameFontProvider : IFontProvider`가 `CHMMain` 초기화 시 `CHText.StringProvider/FontProvider`에 등록된다. 본문 폰트는 `Gaegu-Bold SDF` / `Gaegu-Regular SDF`.

> 참고: `Function/CHButton`, `Function/CHTMPro`는 패키지의 `CHButton`/`CHText`를 상속해 프리팹의 스크립트 GUID와 게임 전용 필드를 보존하는 마이그레이션 잔존 클래스다. 신규 코드는 가능한 패키지 타입을 직접 쓴다.

## 에디터 툴 (`Assets/Scripts/Editor/`)

- `CHToolCreateMap.cs` — 스테이지/레벨 디자인 (EditorWindow, 9×9 그리드, JSON 저장)
- `CHToolGameView.cs` — 게임뷰 해상도 설정
- `CHToolString.cs` — 로컬라이제이션 문자열 관리
- `CHToolMigrateToAddressables.cs` — `AssetBundleResources/` 에셋에 `"Resource"` 라벨/그룹 일괄 부여
- `CHTool.cs` — JSON 직렬화 헬퍼 구조체
- `CustomReadOnly.cs` — 인스펙터용 `[ReadOnly]` 어트리뷰트

## 주요 외부 의존성

| 패키지 | 용도 |
|--------|------|
| Unity Addressables | 리소스 로딩 (모든 에셋, `"Resource"` 라벨) |
| UniRx | 리액티브 확장 — `ReactiveProperty`, `Subject`, 버튼 클릭 옵저버블 |
| DOTween | 트윈 애니메이션 |
| TextMesh Pro | 텍스트 렌더링 (`CHText`/`CHTMPro` 래퍼 사용) |
| Google Play Games SDK | 인증 + 클라우드 저장 (`UNITY_INFRA_SOCIAL`) |
| Google Mobile Ads SDK | 배너, 전면, 보상형 광고 (`UNITY_INFRA_ADS`) |
| Unity Purchasing | IAP — 소모성 상품 (`UNITY_INFRA_IAP`) |

## 설계 문서

진행 중·완료된 작업의 스펙/플랜은 `docs/superpowers/specs/`, `docs/superpowers/plans/`에 날짜별로 보관된다. 인프라 마이그레이션 배경은 `docs/superpowers/specs/2026-05-12-unityinfra-addressables-migration-design.md` 참조.
