# CLAUDE.md

이 파일은 Claude Code (claude.ai/code)가 이 저장소에서 작업할 때 참고하는 가이드입니다.

## 프로젝트 개요

**CatPang** — Unity로 제작된 Android용 모바일 매치-3 퍼즐 게임. 개발 환경은 Windows, 빌드 타겟은 Android (Google Play).

## 빌드 및 개발 명령

Unity CLI 빌드 워크플로우는 별도로 구성되어 있지 않으며, 모든 빌드는 Unity 에디터에서 수행한다.

**에셋 번들 빌드** — `Assets/Scripts/Editor/AssetBundleMenuItem.cs`에 추가된 Unity 에디터 메뉴 항목을 사용.

**스테이지/맵 제작** — Unity 에디터 메뉴에서 접근 가능한 `Assets/Scripts/Editor/CHToolCreateMap.cs` 커스텀 툴 사용.

**APK 출력** — 프로젝트 루트의 `CatPang.apk` (릴리즈 빌드, 약 90MB).

## 씬 흐름

세 개의 씬이 순서대로 실행된다:
1. **ResourceDownloadScene** (`Assets/Scripts/Scenes/ResourceDownload.cs`) — 에셋 번들 다운로드 및 데이터 초기화
2. **FirstScene** (`Assets/Scripts/Scenes/First.cs`) — 메인 로비: 스테이지 선택, 상점, 미션, GPGS 로그인
3. **GameScene** (`Assets/Scripts/Scenes/Game.cs`) — 매치-3 게임플레이 루프

## 아키텍처

### 매니저 허브 패턴

`CHMMain` (싱글톤, DontDestroyOnLoad)이 중앙 서비스 로케이터 역할을 한다. 모든 매니저는 정적 프로퍼티로 접근한다:

```
CHMMain.Pool        → CHMPool        (오브젝트 풀링)
CHMMain.Resource    → CHMResource    (에셋 로딩)
CHMMain.AssetBundle → CHMAssetBundle (번들 캐시)
CHMMain.Json        → CHMJson        (게임 설정 데이터)
CHMMain.Data        → CHMData        (플레이어 저장 데이터)
CHMMain.UI          → CHMUI          (UI 스택 매니저)
CHMMain.Sound       → CHMSound       (오디오)
CHMMain.String      → CHMString      (로컬라이제이션 키 / IAP ID)
CHMMain.Admob       → CHMAdmob       (Google Mobile Ads)
CHMMain.GPGS        → CHMGPGS        (Google Play Games Services)
CHMMain.IAP         → CHMIAP         (Unity Purchasing)
```

모든 매니저는 `TaskCompletionSource`를 사용한 비동기 초기화 패턴을 따른다.

### 리소스 로딩

`CHMResource`가 두 가지 모드를 자동으로 추상화한다:
- **에디터:** `AssetBundleResources` 폴더에서 직접 에셋 로드
- **빌드:** `CHMAssetBundle`을 통해 미리 빌드된 에셋 번들에서 로드

항상 `CHMMain.Resource.Load<T>()` 또는 `CHMMain.Resource.Instantiate()`를 사용하고, `Resources.Load`를 직접 호출하지 않는다.

### UI 시스템

`CHMUI`는 매 `Update`마다 처리되는 지연 큐를 유지한다. `ShowUI(EUI.X)` / `CloseUI(EUI.X)`를 사용하고, UI 프리팹을 직접 인스턴스화하지 않는다. 활성 UI는 고유 ID로 추적된다.

모든 UI 패널은 `UIBase`를 상속하며, `Start()`에서 UniRx 리액티브 버튼 구독을 연결한다.

### 오브젝트 풀링

`CHMPool`이 GameObject를 재활용한다. 풀링 대상 오브젝트는 `CHPoolable` 마커 컴포넌트를 가진다. `Destroy()` 대신 항상 `CHMMain.Pool.Push()`로 오브젝트를 반환한다.

### 데이터 저장

- **로컬:** `Application.persistentDataPath`에 JSON 직렬화 — `CHMData`가 관리
- **클라우드:** `"CatPang"` 슬롯명으로 Google Play Games Services 저장 — Android에서 `CHMData`가 동기화
- **설정값:** 볼륨, 언어, 색상 설정은 `PlayerPrefs` 사용 (키는 `CHMString`에 정의)

### 게임 로직

- 보드: `Block` 인스턴스 9×9 그리드 (`Assets/Scripts/Block.cs`)
- 게임 상태는 `ReactiveProperty<EGameState>` (UniRx)로 추적
- 블록 타입(고양이, 장애물, 폭탄, 파워업)은 `Defines.cs`의 `EBlockState` 열거형에 정의
- 게임 모드 두 가지: 시간 제한 모드, 이동 횟수 제한 모드

## 네이밍 규칙

| 접두사 | 의미 |
|--------|------|
| `CHM`  | 매니저 클래스 (예: `CHMMain`, `CHMPool`) |
| `CH`   | 유틸리티/컴포넌트 클래스 (예: `CHSingleton`, `CHUtil`) |
| `UI`   | UI 패널 클래스 (예: `UIShop`, `UIMission`) |
| `E`    | 열거형 타입 (예: `EUI`, `EBlockState`, `EGameState`) |

## 주요 외부 의존성

| 패키지 | 용도 |
|--------|------|
| UniRx | 리액티브 확장 — `ReactiveProperty`, `Subject`, 버튼 클릭 옵저버블 |
| DOTween | 트윈 애니메이션 |
| TextMesh Pro | 한국어/영어 로컬라이제이션을 포함한 텍스트 렌더링 |
| Google Play Games SDK | 인증 + 클라우드 저장 |
| Google Mobile Ads SDK | 배너, 전면, 보상형 광고 |
| Unity Purchasing | IAP (소모성 상품 3종: 광고 제거, 시간 추가, 이동 횟수 추가) |

## 로컬라이제이션

`CHMJson`이 로드하는 두 개의 언어 사전 (한국어 기본, 영어 폴백). 앱 시작 시 언어 자동 감지. 문자열 키는 `CHMString`에 상수로 정의되어 있다.

## 에디터 툴

`Assets/Scripts/Editor/`의 커스텀 Unity 에디터 스크립트:
- `CHToolCreateMap.cs` — 스테이지/레벨 디자인 툴
- `CHToolString.cs` — 로컬라이제이션 문자열 관리
- `AssetBundleMenuItem.cs` — 에셋 번들 빌드
- `CHTool.cs` — 범용 에디터 유틸리티
