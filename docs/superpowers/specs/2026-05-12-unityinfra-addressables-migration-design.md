# CatPang — `com.chvj.unityinfra` 패키지 적용 + Addressables 마이그레이션 설계

- 작성일: 2026-05-12
- 작업 브랜치: `Package` (현재)
- 작업자: chvj7567
- 범위: 기존 매니저/AssetBundle 시스템 전면 교체

---

## 1. 목표와 최종 상태

`com.chvj.unityinfra` 임베디드 패키지를 100% 사용하도록 인프라 전 영역을 전환하고, AssetBundle 기반 리소스 시스템을 Addressables로 마이그레이션한다.

### 최종 상태

- `CHMMain` 허브 **제거**. 모든 매니저는 `ChvjUnityInfra.CHMX.Instance`로 직접 접근.
- `Assets/AssetBundleResources/` 하위 모든 에셋이 **"Resource" Addressables 라벨**, Local 그룹으로 통합. 원격 다운로드 단계 없음 — APK에 전부 포함.
- `CHMAssetBundle`, `CHLoadingBarFromAssetBundle`, `AssetBundleMenuItem` **삭제**.
- `Function/CHButton`, `Function/CHTMPro`, `Function/CHToggle` 삭제 → 패키지의 `ChvjUnityInfra.CHButton/CHText/CHToggle`로 교체. 게임 특화 필드(`clearObj/lockObj/unlockObj`)는 신규 게임 컴포넌트 `LBStageButton`으로 분리.
- 게임 전용 매니저(`CHMJson`, `CHMData`, `CHMString`)는 `CHSingletonStatic<T>` 패턴으로 재작성.
- 옵트인 모듈(Admob/IAP/GPGS) 활성화: `UNITY_INFRA_ADS`, `UNITY_INFRA_IAP`, `UNITY_INFRA_SOCIAL` 심볼 정의. `AdConfig`/`IAPProductConfig` 에셋 생성.
- `ResourceDownloadScene`은 `ChvjUnityInfraSDK.Initialize` 한 번 호출하는 부트스트랩 씬으로 단순화.

---

## 2. 단계별 Phase (점진적 마이그레이션 — 매 단계 빌드 가능)

| Phase | 작업 | 빌드 가능 |
|---|---|---|
| **P0** | 작업 브랜치 기준선 커밋. `Packages/com.chvj.unityinfra`, 변경된 `manifest.json`/`packages-lock.json` 포함 | ✅ |
| **P1** | Addressables 마이그레이션 — 에디터 툴로 `AssetBundleResources/` 하위 전 파일에 "Resource" 라벨 부여, 그룹 8개 자동 생성. 명칭 충돌 검출 | ✅ 기존 AssetBundle 시스템과 공존 |
| **P2** | `Assets/Scripts/Manager/CHMResource.cs`(글로벌 namespace, 어댑터)의 메서드 내부만 `ChvjUnityInfra.CHMResource.Instance.Load<T>(...)`로 변환. `CHMMain.Resource` 프로퍼티 시그니처/게임 코드 변경 X. `CHMAssetBundle` 호출 우회 | ✅ 검증 포인트 |
| **P3** | `Assets/Scripts/Manager/CHMPool.cs`, `CHMSound.cs`도 같은 어댑터 패턴으로 패키지 위임. `CHMSound.Init<Defines.ESound>(Defines.ESound.Bgm)` 호출 (부팅 시점) | ✅ |
| **P4** | `CHMMain.UI` + `UIBase` 교체. 모든 UI 스크립트 `InitUI(CHUIArg)` → `InitUI(UIArg)`, arg 베이스 변경, `Close()` 호출 변경 | ⚠️ UI 회귀 테스트 |
| **P5** | `Function/CHTMPro` → 패키지 `CHText` 교체. 게임 측 `LBStageButton` 분리. 에디터 툴로 프리합 컴포넌트 자동 재바인딩 | ⚠️ 프리합 다수 변경 |
| **P6** | 게임 매니저 재작성 — `CHMJson`/`CHMData`/`CHMString`을 `CHSingletonStatic<T>` 패턴으로. `CHMMain` 의존성 제거. JSON 로딩은 `AfterResourceInit` 콜백에서 | ✅ |
| **P7** | 옵트인 모듈 활성화 — `Tools/ChvjUnityInfra/Settings`에서 ADS/IAP/SOCIAL 토글. Config 에셋 생성. 호출 사이트 → 패키지 싱글턴 | ✅ |
| **P8** | 247개 `CHMMain.X` 호출 일괄 치환 → `CHMX.Instance`. `using ChvjUnityInfra;` 추가 | ⚠️ 컴파일 검증 |
| **P9** | 정리 — `CHMMain.cs`, `CHMAssetBundle.cs`, `Assets/Scripts/Manager/`의 기존 매니저 8개(`CHMResource/Pool/UI/Sound/Admob/IAP/GPGS`+ `UIBase`), `Function/CHButton/CHTMPro/CHToggle.cs`, `CHLoadingBarFromAssetBundle.cs`, `Editor/AssetBundleMenuItem.cs` 삭제. AssetBundle 라벨 제거 | ✅ 최종 |

각 Phase 종료 시 Unity Editor Play 모드로 핵심 시나리오 검증: ResourceDownload → Lobby → 스테이지 선택 → GameScene → 매치 1회 → 클리어.

### Namespace 충돌 처리 (P2~P8 중요)

기존 매니저(`Assets/Scripts/Manager/CHMResource.cs` 등)는 글로벌 namespace, 패키지의 동명 클래스(`ChvjUnityInfra.CHMResource` 등)는 `ChvjUnityInfra` namespace. P2~P8 동안 두 그룹이 공존하므로:

- **글로벌 어댑터 파일 내부**: 패키지 타입은 항상 fully-qualified로 호출 (`ChvjUnityInfra.CHMResource.Instance.Load<...>(...)`). `using ChvjUnityInfra;`는 추가하지 않음 (이름 충돌로 모호해짐)
- **P8에서 일괄 치환**: 게임 코드의 `CHMMain.Resource` → `CHMResource.Instance` 변경 시점에 해당 파일 상단에 `using ChvjUnityInfra;` 추가. 글로벌 동명 클래스는 이 시점에 아직 존재하지만 어댑터 역할만 하므로 호출 충돌은 없음
- **P9 삭제 후**: 글로벌 동명 클래스가 사라지면 `using ChvjUnityInfra;` + 단순 이름 호출이 깔끔하게 작동

이 순서를 어기면(예: P5에 너무 일찍 `using` 추가) `'CHMResource' is ambiguous` 컴파일 에러 발생.

---

## 3. 핵심 API 매핑 테이블

| 영역 | 기존 (CHMMain 기반) | 신 (`ChvjUnityInfra`) |
|---|---|---|
| Resource: JSON | `CHMMain.Resource.LoadJson(EJsonType.X, cb)` | `CHMResource.Instance.Load<TextAsset>(EJsonType.X, cb)` |
| Resource: Sprite | `CHMMain.Resource.LoadSprite(EBlockState.X, cb)` | `CHMResource.Instance.Load<Sprite>(EBlockState.X, cb)` |
| Resource: Sound | `CHMMain.Resource.LoadSound(ESound.X, cb)` | `CHMResource.Instance.Load<AudioClip>(ESound.X, cb)` |
| Resource: UI 인스턴스 | `CHMMain.Resource.InstantiateUI(EUI.X, cb)` | `CHMUI.Instance.ShowUI(EUI.X, arg, cb)` (UI 매니저 경유) |
| Resource: Effect | `CHMMain.Resource.InstantiateEffect(EEffect.X, cb)` | `CHMResource.Instance.Instantiate<GameObject>(EEffect.X, cb)` |
| Resource: Font | `CHMMain.Resource.LoadFont(ELang.X, cb)` | `CHMResource.Instance.Load<TMP_FontAsset>("Gaegu-Bold SDF", cb)` *현재 한/영 동일 폰트 — ELang 매개변수 제거* |
| Resource: Data | `CHMMain.Resource.LoadData(name, cb)` | `CHMResource.Instance.Load<TextAsset>(name, cb)` |
| Pool | `CHMMain.Pool.Pop(prefab, parent)` / `Push(p)` | `CHMPool.Instance.Pop(prefab, parent)` / `Push(p)` |
| Sound: 재생 | `CHMMain.Sound.Play(ESound.X)` | `CHMSound.Instance.Play(ESound.X)` |
| Sound: 초기화 | `CHMMain.Sound.Init()` (자동) | `CHMSound.Instance.Init<Defines.ESound>(Defines.ESound.Bgm)` |
| UI: 열기 | `CHMMain.UI.ShowUI(EUI.X, arg, true)` → uid 반환 | `CHMUI.Instance.ShowUI(EUI.X, arg, cb)` — **uid 반환 X**, 캐시는 type별 1개 |
| UI: 닫기 (GameObject) | `CHMMain.UI.CloseUI(gameObject)` | `uiBase.Close()` (UIBase 안에서) |
| UI: 닫기 (enum) | `CHMMain.UI.CloseUI(EUI.X)` | `CHMUI.Instance.CloseUI(EUI.X)` |
| UIBase: InitUI | `public override void InitUI(CHUIArg arg)` | `public override void InitUI(UIArg arg)` (**abstract**) |
| UIBase: 닫기 콜백 | `public override void CloseUI() {}` | `public override void Close(bool reuse) { /* cleanup */; base.Close(reuse); }` |
| UIArg | `class UIShopArg : CHUIArg { ... }` | `class UIShopArg : UIArg { ... }` |
| Json | `CHMMain.Json.GetJson(EJsonType.X)` | `CHMJson.Instance.GetJson(EJsonType.X)` (재작성) |
| Data | `CHMData.Instance.X` | 그대로 유지 (베이스만 `CHSingletonStatic<CHMData>`로 변경) |
| String | `CHMMain.String.X` | `CHMString.Instance.X` (재작성) |
| Ads | `CHMMain.Admob.X` | `CHMAdmob.Instance.X` |
| IAP | `CHMMain.IAP.X` | `CHMIAP.Instance.X` |
| GPGS | `CHMMain.GPGS.X` | `CHMGPGS.Instance.X` |

### 일괄 치환용 정규식

```
CHMMain\.(Resource|Pool|UI|Sound|Json|Data|String|Admob|IAP|GPGS)\b
→ CHM$1.Instance
```

`Resource.LoadJson/LoadSprite/LoadSound/InstantiateUI/InstantiateEffect/LoadFont/LoadData/Destroy` 같은 변경된 메서드는 별도 sed 패턴으로 처리.

---

## 4. UI 시스템 변경의 깊은 영향

### 4-1. 사라지는 기능 vs 대안

| 기존 기능 | 패키지 대응 | 영향 |
|---|---|---|
| `ShowUI` 반환 uid + `CloseUI(uid)` | 없음 — type별 캐시 1개 | 같은 EUI 다중 인스턴스 불가. 호출처 확인 필요 |
| `_bCurrentBackground=false` 모드 (별도 UICamera+UICanvas) | 없음 — 단일 캔버스만 | 호출처 확인 후 단일 캔버스로 통합 |
| 매 프레임 `UpdateUI()` 지연 큐 | 없음 — `ShowUI`/`CloseUI` 즉시 처리 (instantiate만 async) | `CHMMain.Update`에서 호출하던 라인 제거 |
| `Awake()`에서 backgroundBtn/backBtn 자동 바인딩 (UniRx) | `Init(Enum)`에서 `_backgroundButton`/`_backButton` 자동 바인딩 (CompositeDisposable) | 필드명 다름 — 프리합 재바인딩 필요 |
| `InitUI(CHUIArg)` virtual | `InitUI(UIArg)` **abstract** | 모든 UIBase 하위 클래스 반드시 구현 |
| `CloseUI()` virtual 빈 메서드 | `Close(bool reuse)` virtual — `base.Close(reuse)` 호출하면 `CHMUI.CloseUI` 트리거 | 닫기 시 `base.Close(false)` 명시 |

### 4-2. UIBase 마이그레이션 패턴

```csharp
// 기존
public class UIShop : UIBase
{
    public override void InitUI(CHUIArg _uiArg)
    {
        var arg = _uiArg as UIShopArg;
    }
    public override void CloseUI() { /* cleanup */ }
}
public class UIShopArg : CHUIArg { public int category; }
```

```csharp
// 신
using ChvjUnityInfra;
public class UIShop : UIBase
{
    public override void InitUI(UIArg arg)
    {
        var a = arg as UIShopArg;
    }
    public override void Close(bool reuse = true)
    {
        // cleanup
        base.Close(reuse);
    }
}
public class UIShopArg : UIArg { public int category; }
```

### 4-3. 프리합 backgroundBtn/backBtn 재바인딩

- 기존 UIBase: `[SerializeField] Button backgroundBtn; Button backBtn;`
- 신 UIBase: `[SerializeField] Button _backgroundButton; Button _backButton;`

필드명 변경으로 프리합 14개의 인스펙터 참조가 해제됨. 에디터 툴로 prefab YAML 직접 패치 (`backgroundBtn:` → `_backgroundButton:`, `backBtn:` → `_backButton:`).

### 4-4. ShowUI 호출 사이트 점검 (P4 진입 전 수행)

P4 작업 시작 전 grep으로 확인:
- `\.ShowUI\(.*,\s*false\s*\)` — 별도 캔버스 호출처
- `int.*=.*\.ShowUI\(` — 반환값 uid 저장 패턴

발견되면 호출처 측에서 단일 캔버스로 변경 또는 UIBase 참조 저장 방식으로 리팩터.

---

## 5. Function/ 컴포넌트 교체 전략

### 5-1. CHTMPro → CHText 매핑

| 기존 API | 신 API | 비고 |
|---|---|---|
| `[SerializeField] int stringID` | `[SerializeField] private int _stringID` | 필드명 다름 — 프리합 YAML 패치 |
| `SetText(params object[])` | (P5 진입 전 `Packages/com.chvj.unityinfra/Runtime/UI/CHText.cs` 전체 API 확인 필수 — 위험 9-1 참조). 메서드 미존재 시 게임 측 확장 메서드 또는 `CHGameText : CHText` 서브클래스로 흡수 | |
| `SetStringID(int)` | 위와 동일 — 없으면 확장 메서드로 보충 | |
| `SetColor(Color)` | `text.color = ...` 직접 | 호출처 적음 |
| `SetPlusString(string)` | 없음 — 호출처에서 직접 문자열 조합 | 1~2곳 |
| `GetString()` | `text.text` 직접 | |

### 5-2. CHButton (Function) → CHButton (패키지) + LBStageButton 분리

기존 `Function/CHButton` 필드 4개(`text`, `clearObj`, `lockObj`, `unlockObj`)는 스테이지 버튼 전용. 게임 측 별도 컴포넌트로 분리.

```csharp
using ChvjUnityInfra;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CHButton))]
public class LBStageButton : MonoBehaviour
{
    public TMP_Text text;
    public GameObject clearObj;
    public GameObject lockObj;
    public GameObject unlockObj;
}
```

`StageSelect.cs`의 `btnList`를 `List<LBStageButton>`으로 변경. 기존 `btnList[i].text.text`, `.clearObj`, `.lockObj`, `.unlockObj` 호출은 변경 없음.

클릭 SFX(`CHMMain.Sound.Play(ESound.Cat)`)는 패키지의 `CHButton.ClickSoundHook`으로 부팅 시 한 번 등록:
```csharp
CHButton.ClickSoundHook = () => CHMSound.Instance.Play(Defines.ESound.Cat);
```

### 5-3. CHToggle (Function) → CHToggle (패키지)

단순 교체.

### 5-4. 자동 재바인딩 에디터 툴 — `Tools/CatPang/Migrate Prefabs`

순서:
1. `Assets/AssetBundleResources/ui/**.prefab`, `Assets/Prefabs/**.prefab` 등 스캔
2. 각 프리합 YAML 열어서:
   - `Function/CHTMPro` (script GUID A) → `ChvjUnityInfra/CHText` (script GUID B)
   - `stringID:` 필드 값 보존 → `_stringID:`로 키 rename
   - `Function/CHButton` → `ChvjUnityInfra/CHButton`. 같은 GameObject에 `LBStageButton` 컴포넌트 append, `text/clearObj/lockObj/unlockObj` 필드 이관
   - `Function/CHToggle` → `ChvjUnityInfra/CHToggle`
   - UIBase 하위 프리합의 `backgroundBtn:` → `_backgroundButton:`, `backBtn:` → `_backButton:`
3. **Dry-run 모드** 제공 — 변경 요약 출력 후 사용자 확인
4. **백업 모드** — `Library/CatPangPrefabBackup/{timestamp}/`에 원본 보관

---

## 6. Addressables 마이그레이션 절차

### 6-1. 그룹 구성

기존 8개 번들 폴더(ui/unit/effect/sprite/sound/font/data/json) 구조를 유지해 그룹 8개로 분리.

| Addressables 그룹 | 포함 에셋 | 빌드 패스 |
|---|---|---|
| `UI` | `AssetBundleResources/ui/*.prefab` | Local |
| `Unit` | `AssetBundleResources/unit/*.prefab` | Local |
| `Effect` | `AssetBundleResources/effect/*.prefab` | Local |
| `Sprite` | `AssetBundleResources/sprite/*.png` | Local |
| `Sound` | `AssetBundleResources/sound/*.{mp3,wav,ogg}` | Local |
| `Font` | `AssetBundleResources/font/*.asset` | Local |
| `Data` | `AssetBundleResources/data/*.json` | Local |
| `Json` | `AssetBundleResources/json/*.json` | Local |

전체에 **"Resource" 라벨** 부여. `CHMResource`가 이 라벨로 LocationInfo 로드.

### 6-2. 명칭 충돌 검사

`CHMResource`의 key 추출:
```csharp
string key = pathInfo.ToString().Split('/').Last().Split('.').First();
```

전 파일 basename unique 필요. 에디터 툴이 충돌 발견 시 빨간 로그로 알람 후 작업 중단. (사전 점검 결과 모든 enum 항목명이 폴더 간 unique — 큰 문제 없을 것으로 예상)

### 6-3. 에디터 자동화 툴 — `Tools/CatPang/Migrate To Addressables`

순서:
1. **Pre-check**: basename 충돌 검사 → 충돌 있으면 중단
2. **AddressableAssetSettings 확보** (없으면 생성)
3. **그룹 8개 생성** — BundledAssetGroupSchema, ContentUpdateGroupSchema, LocalLoadPath
4. **각 폴더 안 에셋 → 그룹 등록, "Resource" 라벨 부여**. address는 파일 basename (default)
5. **AssetBundle 라벨 제거** (기존 인스펙터의 AssetBundle name/variant 클리어)
6. **결과 리포트** 출력 — 총 N개, 그룹별 카운트

### 6-4. 빌드 파이프라인 변경

- 기존: `Editor/AssetBundleMenuItem.cs` 메뉴 → `BuildPipeline.BuildAssetBundles`
- 신: `Window/Asset Management/Addressables/Groups`에서 "Build → New Build → Default Build Script". Local 그룹 빌드 산출물은 자동으로 APK에 포함

P9에서 `AssetBundleMenuItem.cs`, `CHLoadingBarFromAssetBundle.cs` 삭제, ResourceDownload 씬의 다운로드 로직 제거.

---

## 7. 게임 매니저 재작성 (Json / Data / String)

### 7-1. CHMJson

```csharp
using ChvjUnityInfra;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CHMJson : CHSingletonStatic<CHMJson>
{
    private Dictionary<Defines.EJsonType, string> _jsonMap = new();

    public async Task Init()
    {
        var tasks = new List<Task>();
        foreach (Defines.EJsonType type in System.Enum.GetValues(typeof(Defines.EJsonType)))
        {
            if (type == Defines.EJsonType.None || type == Defines.EJsonType.Max) continue;
            tasks.Add(LoadOne(type));
        }
        await Task.WhenAll(tasks);
    }

    private Task LoadOne(Defines.EJsonType type)
    {
        var tcs = new TaskCompletionSource<bool>();
        CHMResource.Instance.Load<TextAsset>(type, asset =>
        {
            if (asset != null) _jsonMap[type] = asset.text;
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    public string GetJson(Defines.EJsonType type) =>
        _jsonMap.TryGetValue(type, out var v) ? v : null;

    // 기존 GetXxx 헬퍼들도 동일 패턴으로 이관
}
```

### 7-2. CHMData

이미 `CHSingleton` 패턴 — 베이스만 `CHSingletonStatic<CHMData>`로 교체. `CHMMain.Resource/String` 참조부 → 각 패키지 싱글턴으로 변경.

### 7-3. CHMString

const 모음 — 베이스만 `CHSingletonStatic<CHMString>`으로 변경. 기존 코드 거의 그대로.

---

## 8. 부팅 흐름 (ResourceDownloadScene 단순화)

```csharp
using ChvjUnityInfra;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System.Threading;
using System.Threading.Tasks;

public class ResourceDownload : MonoBehaviour
{
    [SerializeField] List<Image> backgroundList = new();
    CancellationTokenSource tokenSource;
    int backgroundIndex = 0;

    private async void Start()
    {
        tokenSource = new CancellationTokenSource();
        ChangeBackgroundLoop();

        await ChvjUnityInfraSDK.Initialize(new InfraInitConfig<Defines.ESound>
        {
            BGMKeys = new[] { Defines.ESound.Bgm },
            ClickSoundHook = () => CHMSound.Instance.Play(Defines.ESound.Cat),
            StringProvider = new GameStringProvider(),
            FontProvider = new GameFontProvider(),
            AfterResourceInit = async () =>
            {
                await CHMJson.Instance.Init();
                await GameFontProvider.PreloadAsync();
            }
        });

        await CHMData.Instance.LoadLocalData(CHMString.Instance.CatPang);
        SceneManager.LoadScene(1);
    }

    // 기존 ChangeBackgroundLoop / ChangeBackground 유지
}
```

`GameStringProvider`(신규 게임 측 클래스)는 `CHMString`/`CHMJson` wrapping해서 `IStringProvider.GetString(int)` 구현. `GameFontProvider`(신규 게임 측 클래스)는 정적 `PreloadAsync()`로 부팅 시 `CHMResource.Instance.Load<TMP_FontAsset>("Gaegu-Bold SDF", ...)` 결과를 정적 필드로 캐싱하고, `IFontProvider.GetFont()`에서 그 캐시를 반환.

두 클래스 모두 `Assets/Scripts/Manager/` 또는 `Assets/Scripts/Function/`에 새로 작성.

---

## 9. 리스크 & 검증

### 9-1. 주요 리스크

| 리스크 | 완화책 |
|---|---|
| UI 다중 인스턴스 (uid 기반) 패턴 사용처 | P4 진입 전 grep으로 식별 후 단일 인스턴스로 리팩터 |
| `ShowUI(..., false)` 별도 캔버스 호출처 | P4 진입 전 grep, 통합 캔버스로 변경 |
| 프리합 자동 재바인딩 시 YAML 손상 | dry-run + `Library/CatPangPrefabBackup/` 백업 |
| Addressables 빌드 누락 에셋 | P1 후 Player 빌드 → 디바이스 실기 테스트(스테이지 1개 클리어까지) |
| `CHMSound.Init<ESound>` 호출 누락 시 무음 | P3에서 ResourceDownload Start에 명시 호출 추가 |
| TMP_FontAsset 로딩 타이밍 (CHText.Awake 시점) | `AfterResourceInit`에서 폰트 캐싱 후 FontProvider 즉시 반환 |
| 247개 호출 일괄 치환 오발화 | 안전 패턴 정규식만 매칭(`CHMMain\.(Resource\|Pool\|UI\|Sound\|Json\|Data\|String\|Admob\|IAP\|GPGS)\b`) |
| `CHText` 패키지 API가 기존 `CHTMPro`와 메서드 시그니처 불일치 | P5 진입 전 패키지 `CHText` 전체 코드 확인 후 게임 측 헬퍼 메서드로 흡수 |

### 9-2. Phase별 검증 체크리스트

각 Phase 종료 시 Unity Editor Play 모드에서:
- [ ] ResourceDownload 씬에서 다음 씬으로 정상 진입
- [ ] LBLobbyScene에서 로그인/튜토리얼/스테이지 선택 UI 정상
- [ ] GameScene 진입, 보드 생성, 매치 1회, 클리어 화면 정상
- [ ] 사운드 (Bgm 루프, Cat 효과음) 정상
- [ ] 폰트 표시 정상 (CHText 텍스트)
- [ ] 설정/상점/미션 UI 정상

P9 종료 시 추가로:
- [ ] Android Player 빌드 성공
- [ ] 디바이스에서 첫 실행 → 로비 진입까지 정상
- [ ] IAP 테스트 결제 1회 정상
- [ ] 보상형 광고 1회 정상

---

## 10. 결정 사항 요약 (확정)

| 결정 사항 | 선택 |
|---|---|
| 통합 방식 | 기존 매니저 전부 삭제, 패키지 100% 사용 |
| CHMMain 전략 | 완전 제거 → 패키지 싱글턴 직접 호출 |
| Function/ 컴포넌트 | 패키지의 컴포넌트로 완전 교체 |
| 게임 전용 매니저 | `Assets/Scripts/Manager/`에 `CHSingletonStatic<T>` 구현으로 재작성 |
| 자동화 범위 | Addressables 라벨 + 프리합 재바인딩 모두 에디터 툴로 자동화 |
| 에셋 배포 | 전부 로컬에 포함, 다운로드 단계 제거 |

---

## 11. 참고

- 패키지 위치: `Packages/com.chvj.unityinfra/`
- 패키지 README: `Packages/com.chvj.unityinfra/README.md`
- 패키지 의존성: `com.unity.addressables@2.8.1` (이미 manifest에 등록됨)
- Unity 버전: 6000.0.68f1
