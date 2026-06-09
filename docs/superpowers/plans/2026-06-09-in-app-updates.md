# Google Play In-App Updates Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **프로젝트 규칙(Rule 01) 주의:** 각 Task의 마지막 "스테이징" 스텝은 `git add`까지만 한다. **`git commit`은 절대 자동 실행하지 않는다** — 최종 커밋은 파이프라인 마무리에서 사용자에게 메시지(안)로 전달한다.

**Goal:** 부팅 시 Google Play 최신 버전을 감지해 우선순위에 따라 강제(Immediate)/권장(Flexible) 인앱 업데이트 흐름을 띄운다.

**Architecture:** ChvjPackage 옵트인 모듈 패턴(`CHMAdmob`/`CHMGPGS`와 동일). 순수 정책 판정(`AppUpdatePolicy`)은 게이팅 없는 메인 어셈블리에 두어 항상 테스트 가능하게 하고, Google Play 플러그인에 의존하는 네이티브 래퍼(`CHMAppUpdate`)만 `UNITY_INFRA_APPUPDATE` 심볼로 게이팅된 별도 어셈블리에 둔다. UI(UIConfirm)는 역방향 의존 금지를 위해 게임 측 부팅 코드에서 처리하고, 모듈은 콜백/결과 enum으로만 통지한다.

**Tech Stack:** Unity 6 (6000.0.68f1), C#, Google Play In-App Updates Unity 플러그인(`Google.Play.AppUpdate`/`Common`/`Core`), Unity Test Framework(NUnit, EditMode), UniRx/DOTween(기존).

---

## 사전 조건 (사용자 수동, 코드 자동화 불가)

In-App Updates 네이티브 동작 검증은 아래가 갖춰진 뒤에만 가능하다. **플랜의 Task 1·2·6(순수 정책 + 테스트 + 문구)은 이 조건 없이도 컴파일·테스트된다.** Task 3~5(네이티브/게이팅 코드)는 심볼 OFF 기본값에서 어셈블리가 비컴파일되므로 빌드를 깨지 않지만, 실동작 확인은 아래 이후.

1. Google "In-App Updates" `.unitypackage` 임포트 (`play-unity-plugins`)
2. `Tools/ChvjUnityInfra/Settings` → "App Update" 탭 → "Use App Update" 체크 (`UNITY_INFRA_APPUPDATE` 심볼 추가)
3. 내부 테스트 트랙에 구버전→신버전 업로드 (에디터/사이드로드는 항상 "업데이트 없음")

---

## 파일 구조

| 파일 | 책임 | 어셈블리 | 게이팅 |
|---|---|---|---|
| `Packages/com.chvj.unityinfra/Runtime/Core/AppUpdatePolicy.cs` (생성) | `EAppUpdateAction` enum + 우선순위→액션 순수 판정 | `com.chvj.unityinfra` (메인) | 없음 (항상 컴파일) |
| `Packages/com.chvj.unityinfra/Runtime/AppUpdate/CHMAppUpdate.cs` (생성) | In-App Updates API 래퍼 (조회/Immediate/Flexible/Complete) | `com.chvj.unityinfra.appupdate` (신규) | `UNITY_INFRA_APPUPDATE` |
| `Packages/com.chvj.unityinfra/Runtime/AppUpdate/com.chvj.unityinfra.appupdate.asmdef` (생성) | 게이팅 어셈블리 정의 | — | `defineConstraints` |
| `Packages/com.chvj.unityinfra/Editor/ChvjUnityInfraSettingsWindow.cs` (수정) | "App Update" 탭 + 토글 + 가이드 | Editor | — |
| `Assets/Scripts/Scenes/ResourceDownload.cs` (수정) | 부팅 시 업데이트 흐름 오케스트레이션 + UIConfirm 표시 | 게임 | 호출부 `#if UNITY_INFRA_APPUPDATE` |
| `Assets/Scripts/Manager/CHMString.cs` 외 문구 (수정) | UIConfirm 문구 키 + 한/영 사전 | 게임 | — |
| `Assets/Tests/EditMode/AppUpdatePolicyTests.cs` (생성) | `AppUpdatePolicy.Decide` 경계값 테스트 | EditMode 테스트 | 없음 |

---

## Task 1: 순수 정책 판정 — `AppUpdatePolicy` + 테스트

게이팅 없는 메인 어셈블리에 둬서 플러그인 미설치여도 항상 컴파일·테스트되게 한다.

**Files:**
- Create: `Packages/com.chvj.unityinfra/Runtime/Core/AppUpdatePolicy.cs`
- Test: `Assets/Tests/EditMode/AppUpdatePolicyTests.cs`

- [ ] **Step 1: 실패 테스트 작성**

`Assets/Tests/EditMode/AppUpdatePolicyTests.cs`:
```csharp
using NUnit.Framework;
using ChvjUnityInfra;

public class AppUpdatePolicyTests
{
    [Test]
    public void 업데이트없음시_None반환()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(updateAvailable: false, priority: 5, immediateAllowed: true, flexibleAllowed: true);
        Assert.AreEqual(EAppUpdateAction.None, r);
    }

    [Test]
    public void 우선순위4_Immediate허용시_Immediate반환()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(true, 4, true, true);
        Assert.AreEqual(EAppUpdateAction.Immediate, r);
    }

    [Test]
    public void 우선순위5_Immediate허용시_Immediate반환()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(true, 5, true, false);
        Assert.AreEqual(EAppUpdateAction.Immediate, r);
    }

    [Test]
    public void 우선순위3_Flexible허용시_Flexible반환()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(true, 3, true, true);
        Assert.AreEqual(EAppUpdateAction.Flexible, r);
    }

    [Test]
    public void 우선순위0_Flexible허용시_Flexible반환()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(true, 0, false, true);
        Assert.AreEqual(EAppUpdateAction.Flexible, r);
    }

    [Test]
    public void 우선순위4_Immediate불가_Flexible허용시_Flexible로폴백()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(true, 4, false, true);
        Assert.AreEqual(EAppUpdateAction.Flexible, r);
    }

    [Test]
    public void 우선순위4_둘다불가시_None반환()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(true, 4, false, false);
        Assert.AreEqual(EAppUpdateAction.None, r);
    }

    [Test]
    public void 우선순위2_Flexible불가시_None반환()
    {
        EAppUpdateAction r = AppUpdatePolicy.Decide(true, 2, true, false);
        Assert.AreEqual(EAppUpdateAction.None, r);
    }
}
```

- [ ] **Step 2: 테스트 실패 확인**

Unity Test Runner(EditMode) 실행. 기대: `AppUpdatePolicy` 타입 없음으로 컴파일/실행 실패.

- [ ] **Step 3: 최소 구현 작성**

`Packages/com.chvj.unityinfra/Runtime/Core/AppUpdatePolicy.cs`:
```csharp
namespace ChvjUnityInfra
{
    //# 부팅 시 업데이트 처리 액션
    public enum EAppUpdateAction
    {
        None,
        Immediate,
        Flexible,
    }

    //# In-App Update 우선순위 → 처리 액션 판정. Google.Play 타입에 의존하지 않는 순수 로직.
    public static class AppUpdatePolicy
    {
        //# 이 값 이상 우선순위면 강제(Immediate), 미만이면 권장(Flexible)
        public const int ImmediateThreshold = 4;

        public static EAppUpdateAction Decide(bool updateAvailable, int priority, bool immediateAllowed, bool flexibleAllowed)
        {
            if (updateAvailable == false)
                return EAppUpdateAction.None;

            if (priority >= ImmediateThreshold && immediateAllowed)
                return EAppUpdateAction.Immediate;

            if (flexibleAllowed)
                return EAppUpdateAction.Flexible;

            return EAppUpdateAction.None;
        }
    }
}
```

- [ ] **Step 4: 테스트 통과 확인**

Unity Test Runner(EditMode) 재실행. 기대: 8개 전부 PASS.

- [ ] **Step 5: 스테이징** (commit 금지 — Rule 01)

```bash
git add "Packages/com.chvj.unityinfra/Runtime/Core/AppUpdatePolicy.cs" "Packages/com.chvj.unityinfra/Runtime/Core/AppUpdatePolicy.cs.meta" "Assets/Tests/EditMode/AppUpdatePolicyTests.cs" "Assets/Tests/EditMode/AppUpdatePolicyTests.cs.meta"
```

---

## Task 2: 게이팅 어셈블리 정의 — `com.chvj.unityinfra.appupdate.asmdef`

`com.chvj.unityinfra.social.asmdef`를 본떠 작성한다. 심볼 OFF면 어셈블리 자체가 비컴파일되어 플러그인 미설치에도 안전.

**Files:**
- Create: `Packages/com.chvj.unityinfra/Runtime/AppUpdate/com.chvj.unityinfra.appupdate.asmdef`

- [ ] **Step 1: asmdef 작성**

```json
{
  "name": "com.chvj.unityinfra.appupdate",
  "rootNamespace": "ChvjUnityInfra",
  "references": [
    "com.chvj.unityinfra",
    "Google.Play.AppUpdate",
    "Google.Play.Common",
    "Google.Play.Core"
  ],
  "includePlatforms": [
    "Android",
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [
    "UNITY_INFRA_APPUPDATE"
  ],
  "versionDefines": [],
  "noEngineReferences": false
}
```

> 참고: 심볼 OFF(기본)면 `defineConstraints` 때문에 이 어셈블리는 빌드에서 제외되고, 존재하지 않는 `Google.Play.*` 참조도 평가되지 않는다. 플러그인 임포트 + 심볼 ON 후에만 참조가 해석된다.

- [ ] **Step 2: 컴파일 확인**

심볼 OFF 상태에서 Unity 재컴파일. 기대: 에러/경고 없음(어셈블리 비활성).

- [ ] **Step 3: 스테이징**

```bash
git add "Packages/com.chvj.unityinfra/Runtime/AppUpdate/com.chvj.unityinfra.appupdate.asmdef" "Packages/com.chvj.unityinfra/Runtime/AppUpdate/com.chvj.unityinfra.appupdate.asmdef.meta"
```

---

## Task 3: 네이티브 래퍼 — `CHMAppUpdate`

`#if UNITY_INFRA_APPUPDATE`로 전체를 감싼다(`CHMGPGS`가 `#if UNITY_ANDROID`로 감싸는 형태와 동일). 게임 UI를 모르며, 강제 취소/Flexible 완료는 콜백·결과로만 통지한다.

> **검증 한계:** 이 파일은 플러그인 미설치 상태에서 컴파일 불가(어셈블리 게이팅됨). 아래 코드는 In-App Updates 공식 API 기준 참조 구현이며, **플러그인 임포트 + 심볼 ON 후 컴파일/실동작을 확인**한다. gameplay-programmer는 임포트된 플러그인의 실제 API 시그니처(`AppUpdateManager`, `PlayAsyncOperation`, `AppUpdateRequest`, `AppUpdateOptions`)와 대조해 미세 조정한다.

**Files:**
- Create: `Packages/com.chvj.unityinfra/Runtime/AppUpdate/CHMAppUpdate.cs`

- [ ] **Step 1: 공개 API 골격 작성**

공개 메서드(이후 게임 측이 호출):
```csharp
//# 부팅 시 호출. 업데이트 정보 조회 + 정책 판정만. 네이티브 플로우는 시작 안 함.
public Task<EAppUpdateAction> CheckAsync();

//# Immediate 강제 업데이트 시작. 완료(성공 시 앱 재시작)/취소까지 await. 취소·실패면 false.
public Task<bool> StartImmediateAsync();

//# Flexible 백그라운드 다운로드 시작. 다운로드 완료 시 onDownloaded 1회 호출.
public void StartFlexible(Action onDownloaded);

//# 다운로드 완료분 설치(앱 재시작).
public void CompleteFlexibleUpdate();
```

- [ ] **Step 2: 참조 구현 작성**

`Packages/com.chvj.unityinfra/Runtime/AppUpdate/CHMAppUpdate.cs`:
```csharp
#if UNITY_INFRA_APPUPDATE
using System;
using System.Threading.Tasks;
using Google.Play.AppUpdate;
using Google.Play.Common;
using UnityEngine;

namespace ChvjUnityInfra
{
    //# Google Play In-App Updates 매니저. Android 전용, 옵트인(UNITY_INFRA_APPUPDATE).
    public class CHMAppUpdate : CHSingletonStatic<CHMAppUpdate>
    {
        private AppUpdateManager _manager;
        private AppUpdateInfo _info;
        private Action _onFlexibleDownloaded;

        private AppUpdateManager Manager => _manager ??= new AppUpdateManager();

        //# 업데이트 정보 조회 + 정책 판정. 실패/없음/에디터는 None.
        public async Task<EAppUpdateAction> CheckAsync()
        {
#if UNITY_EDITOR
            return EAppUpdateAction.None;
#else
            try
            {
                PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> op = Manager.GetAppUpdateInfo();
                await ToTask(op);

                if (op.Error != AppUpdateErrorCode.NoError)
                    return EAppUpdateAction.None;

                _info = op.GetResult();

                bool available = _info.UpdateAvailability == UpdateAvailability.UpdateAvailable;
                bool immediateAllowed = _info.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions());
                bool flexibleAllowed = _info.IsUpdateTypeAllowed(AppUpdateOptions.FlexibleAppUpdateOptions());

                return AppUpdatePolicy.Decide(available, _info.UpdatePriority, immediateAllowed, flexibleAllowed);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CHMAppUpdate] CheckAsync 실패: {e.Message}");
                return EAppUpdateAction.None;
            }
#endif
        }

        //# Immediate 강제 업데이트. 성공 시 시스템이 앱 재시작. 취소/실패면 false.
        public async Task<bool> StartImmediateAsync()
        {
            if (_info == null)
                return false;

            try
            {
                AppUpdateRequest req = Manager.StartUpdate(_info, AppUpdateOptions.ImmediateAppUpdateOptions());
                await ToTask(req);
                return req.Error == AppUpdateErrorCode.NoError;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CHMAppUpdate] StartImmediate 실패: {e.Message}");
                return false;
            }
        }

        //# Flexible 백그라운드 다운로드. 완료 시 onDownloaded 1회.
        public void StartFlexible(Action onDownloaded)
        {
            if (_info == null)
                return;

            _onFlexibleDownloaded = onDownloaded;
            AppUpdateRequest req = Manager.StartUpdate(_info, AppUpdateOptions.FlexibleAppUpdateOptions());
            req.Completed += _ =>
            {
                if (req.Status == AppUpdateStatus.Downloaded)
                {
                    Action cb = _onFlexibleDownloaded;
                    _onFlexibleDownloaded = null;
                    cb?.Invoke();
                }
            };
        }

        //# 다운로드 완료분 설치(앱 재시작).
        public void CompleteFlexibleUpdate()
        {
            Manager.CompleteUpdate();
        }

        //# PlayAsyncOperation/AppUpdateRequest(Completed 이벤트) → Task 변환
        private static Task ToTask(PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> op)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            op.Completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }

        private static Task ToTask(AppUpdateRequest req)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            req.Completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
#endif
```

> Resume 처리(앱 재개 시 Immediate 진행 중이면 재개, Flexible Downloaded면 완료 콜백)는 게임 측 `OnApplicationFocus`에서 `CheckAsync` 재호출로 커버한다(부팅 흐름과 동일 경로 재사용). 별도 MonoBehaviour 도입은 YAGNI — 필요 시 후속.

- [ ] **Step 3: (플러그인 임포트 + 심볼 ON 후) 컴파일 확인**

기대: 컴파일 성공. API 시그니처 불일치 시 실제 플러그인 기준 조정.

- [ ] **Step 4: 스테이징**

```bash
git add "Packages/com.chvj.unityinfra/Runtime/AppUpdate/CHMAppUpdate.cs" "Packages/com.chvj.unityinfra/Runtime/AppUpdate/CHMAppUpdate.cs.meta"
```

---

## Task 4: 에디터 설정창 — "App Update" 탭

`ChvjUnityInfraSettingsWindow.cs`에 Ads/IAP/Social과 동일 톤의 탭을 추가한다.

**Files:**
- Modify: `Packages/com.chvj.unityinfra/Editor/ChvjUnityInfraSettingsWindow.cs`

- [ ] **Step 1: 상수 추가**

`SOCIAL_DEFINE` 아래에:
```csharp
        private const string APPUPDATE_DEFINE = "UNITY_INFRA_APPUPDATE";
```

- [ ] **Step 2: 탭 라벨/스위치 확장**

```csharp
        private static readonly string[] TabLabels = { "Ads", "IAP", "Social", "App Update" };
```
`OnGUI`의 switch에 추가:
```csharp
                case 3: DrawAppUpdateTab(); break;
```

- [ ] **Step 3: 탭 본문 추가**

`DrawSocialTab()` 아래에:
```csharp
        // ────────── App Update ──────────

        private void DrawAppUpdateTab()
        {
            EditorGUILayout.LabelField("Google Play In-App Updates (Android)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawToggle("Use App Update", APPUPDATE_DEFINE);

            EditorGUILayout.Space();

#if UNITY_INFRA_APPUPDATE
            EditorGUILayout.HelpBox(
                "사용 스텝:\n" +
                "1. 'Use App Update' 체크 (이미 켜져 있음)\n" +
                "2. Google 'In-App Updates' Unity 플러그인(.unitypackage) 임포트\n" +
                "3. 부팅 코드는 ResourceDownload.cs에 이미 통합됨 (Android 한정):\n" +
                "   #if UNITY_INFRA_APPUPDATE\n" +
                "   EAppUpdateAction a = await CHMAppUpdate.Instance.CheckAsync();\n" +
                "   #endif\n" +
                "4. 정책: 우선순위 >= 4 → Immediate(강제), 그 외 → Flexible(권장)\n" +
                "   우선순위는 Play Console/Developer API의 inAppUpdatePriority로 출시 시 지정\n" +
                "\n" +
                "주의: Android 전용. 에디터/사이드로드는 항상 '업데이트 없음'.\n" +
                "실동작 검증은 내부 테스트 트랙 업로드 후 구버전→신버전 시나리오.",
                MessageType.Info);
#else
            EditorGUILayout.HelpBox(
                "App Update 모듈이 꺼져 있습니다.\n" +
                "'Use App Update' 체크 → 컴파일 완료 후 사용 가이드가 표시됩니다.\n" +
                "전제: Google 'In-App Updates' Unity 플러그인 임포트 필요 + Android 플랫폼 빌드.",
                MessageType.Warning);
#endif
        }
```

- [ ] **Step 4: 컴파일 + 동작 확인**

`Tools/ChvjUnityInfra/Settings` 열어 4번째 탭 표시 + 토글로 심볼 추가/제거 확인.

- [ ] **Step 5: 스테이징**

```bash
git add "Packages/com.chvj.unityinfra/Editor/ChvjUnityInfraSettingsWindow.cs"
```

---

## Task 5: 부팅 통합 — `ResourceDownload`

`CHMMain.EnsureInitialized()` 직후, `FirstScene` 로드 직전에 업데이트 흐름을 끼운다. UIConfirm 표시(강제 취소 재안내 / Flexible 완료 재시작)는 여기서 처리.

**Files:**
- Modify: `Assets/Scripts/Scenes/ResourceDownload.cs`

- [ ] **Step 1: Start()에 흐름 호출 삽입**

`await CHMData.Instance.LoadLocalData(...)` 다음, `SetProgress(1f, ...)` 앞에:
```csharp
#if UNITY_INFRA_APPUPDATE
        await RunAppUpdateFlow();
#endif
```

- [ ] **Step 2: 흐름 메서드 추가**

`ResourceDownload` 클래스 내부에:
```csharp
#if UNITY_INFRA_APPUPDATE
    //# 부팅 시 인앱 업데이트 흐름. Immediate는 차단(강제), Flexible은 백그라운드.
    async Task RunAppUpdateFlow()
    {
        Defines.EAppUpdateAction action = (Defines.EAppUpdateAction)(int)await ChvjUnityInfra.CHMAppUpdate.Instance.CheckAsync();

        if (await IsImmediate())
        {
            bool ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            while (ok == false)
            {
                bool retry = await ShowForcedUpdateConfirm();
                if (retry == false)
                    break;
                ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            }
        }
        else if (action == ChvjUnityInfra.EAppUpdateAction.Flexible)
        {
            ChvjUnityInfra.CHMAppUpdate.Instance.StartFlexible(ShowFlexibleCompleteConfirm);
        }
    }
#endif
```

> 위 enum 캐스팅은 게임 전용 enum이 따로 없으므로 패키지 `ChvjUnityInfra.EAppUpdateAction`을 직접 쓰는 게 단순하다. gameplay-programmer는 게임 측 별도 enum을 만들지 말고 `ChvjUnityInfra.EAppUpdateAction`을 직접 비교하도록 정리한다(아래 정정판 사용).

- [ ] **Step 3: 정정판 — 패키지 enum 직접 사용**

Step 2 메서드를 다음으로 대체(중간 캐스팅 제거):
```csharp
#if UNITY_INFRA_APPUPDATE
    async Task RunAppUpdateFlow()
    {
        ChvjUnityInfra.EAppUpdateAction action = await ChvjUnityInfra.CHMAppUpdate.Instance.CheckAsync();

        if (action == ChvjUnityInfra.EAppUpdateAction.Immediate)
        {
            bool ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            while (ok == false)
            {
                bool retry = await ShowForcedUpdateConfirm();
                if (retry == false)
                    break;
                ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            }
        }
        else if (action == ChvjUnityInfra.EAppUpdateAction.Flexible)
        {
            ChvjUnityInfra.CHMAppUpdate.Instance.StartFlexible(ShowFlexibleCompleteConfirm);
        }
    }

    //# 강제 업데이트 취소 시 재안내. 확인 누르면 true(재요청).
    Task<bool> ShowForcedUpdateConfirm()
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        UIConfirmArg arg = new UIConfirmArg
        {
            confirmType = EConfirmType.Confirm,
            txtTitle = CHMString.Instance.AppUpdateForcedTitle,
            txtDesc = CHMString.Instance.AppUpdateForcedDesc,
            onYes = () => tcs.TrySetResult(true),
            onClose = () => tcs.TrySetResult(true),
        };
        CHMUI.Instance.ShowUI(Defines.EUI.UIConfirm, arg);
        return tcs.Task;
    }

    //# Flexible 다운로드 완료 시 재시작 안내. Yes면 설치.
    void ShowFlexibleCompleteConfirm()
    {
        UIConfirmArg arg = new UIConfirmArg
        {
            confirmType = EConfirmType.YesNo,
            txtTitle = CHMString.Instance.AppUpdateReadyTitle,
            txtDesc = CHMString.Instance.AppUpdateReadyDesc,
            onYes = () => ChvjUnityInfra.CHMAppUpdate.Instance.CompleteFlexibleUpdate(),
        };
        CHMUI.Instance.ShowUI(Defines.EUI.UIConfirm, arg);
    }
#endif
```

> gameplay-programmer 확인 사항: (a) `CHMUI.Instance.ShowUI(Defines.EUI, CHUIArg)` 시그니처가 게임 어댑터에 존재하는지(없으면 `ShowUI`의 실제 오버로드에 맞춤). (b) ResourceDownloadScene에 UIConfirm을 띄울 캔버스/EventSystem이 확보되는지 — 패키지 CHMUI가 `UICanvas` 프리팹을 자동 instantiate하며 `EUI.EventSystem` 포함 여부 확인. 미포함이면 부팅 씬에 EventSystem 보장 필요. (c) 강제 업데이트 중 백그라운드 진입 후 재개 시 처리는 후속(설계서 §5.1) — MVP 범위에선 다음 부팅 재검사로 갈음.

- [ ] **Step 4: using 확인**

`ResourceDownload.cs` 상단에 `using System.Threading.Tasks;`는 이미 있음. `UIConfirmArg`/`EConfirmType`/`CHMString`/`CHMUI`는 글로벌 네임스페이스라 추가 using 불필요.

- [ ] **Step 5: 스테이징**

```bash
git add "Assets/Scripts/Scenes/ResourceDownload.cs"
```

---

## Task 6: 로컬라이즈 문구

UIConfirm 문구를 하드코딩하지 않고 `CHMString` 상수 + 한/영 사전에 추가(Rule 03 — 정적 텍스트도 로컬라이즈).

**Files:**
- Modify: `Assets/Scripts/Manager/CHMString.cs` (또는 문자열 상수 정의 위치)
- Modify: `Assets/AssetBundleResources/json/StringKorea.json`, `StringEnglish.json` (실제 파일명은 EJsonType 기준 확인)

- [ ] **Step 1: 문자열 키 4종 추가**

`CHMString`에 상수 추가(기존 패턴대로):
```csharp
//# In-App Update 문구
public string AppUpdateForcedTitle => "AppUpdateForcedTitle";
public string AppUpdateForcedDesc  => "AppUpdateForcedDesc";
public string AppUpdateReadyTitle  => "AppUpdateReadyTitle";
public string AppUpdateReadyDesc   => "AppUpdateReadyDesc";
```
> gameplay-programmer/game-designer는 기존 `CHMString` 상수 정의 형태(필드/프로퍼티/const)를 확인해 동일 형식으로 맞춘다.

- [ ] **Step 2: 한/영 사전 항목 추가**

한국어:
```
AppUpdateForcedTitle : "업데이트 필요"
AppUpdateForcedDesc  : "원활한 플레이를 위해 최신 버전으로 업데이트해 주세요."
AppUpdateReadyTitle  : "업데이트 준비 완료"
AppUpdateReadyDesc   : "지금 재시작하여 업데이트를 적용할까요?"
```
영어:
```
AppUpdateForcedTitle : "Update Required"
AppUpdateForcedDesc  : "Please update to the latest version to continue."
AppUpdateReadyTitle  : "Update Ready"
AppUpdateReadyDesc   : "Restart now to apply the update?"
```

- [ ] **Step 3: 표시 확인**

심볼 ON + 실기기 시나리오에서 문구가 키가 아닌 번역문으로 출력되는지 확인(또는 게임 내 언어 전환으로 확인).

- [ ] **Step 4: 스테이징**

```bash
git add "Assets/Scripts/Manager/CHMString.cs" "Assets/AssetBundleResources/json/StringKorea.json" "Assets/AssetBundleResources/json/StringEnglish.json"
```

---

## Self-Review

**1. Spec coverage**
- §4.1 패키지 모듈 → Task 2(asmdef), Task 3(CHMAppUpdate) ✅
- §4.1 순수 정책 분리(테스트 가능성) → Task 1 ✅
- §4.2 에디터 탭 → Task 4 ✅
- §4.3 부팅 통합 → Task 5 ✅
- §5 동작 흐름(Immediate/Flexible/취소 재요청/완료 안내) → Task 3 + Task 5 ✅
- §5.2 UIConfirm 재사용 + 역방향 의존 금지(콜백 분리) → Task 3(콜백), Task 5(게임 측 UI) ✅
- §6 ImmediateThreshold=4 → Task 1 상수 ✅
- §7 테스트 전략(순수 함수 단위 테스트) → Task 1 테스트 ✅
- §8 엣지(에디터/네트워크 실패 무동작) → Task 3 `#if UNITY_EDITOR` + try/catch ✅
- §3 문구 로컬라이즈 → Task 6 ✅
- 미커버 갭: §5.1 Resume 정밀 처리는 MVP에서 "다음 부팅 재검사"로 의도적 축소(Task 3/5 주석에 명시). 후속 과제로 표기.

**2. Placeholder scan** — "TBD/TODO/적절히 처리" 류 없음. 모든 코드 스텝에 실제 코드 포함. 단, Task 3는 플러그인 미설치로 현 시점 컴파일 불가임을 명시(참조 구현 + 임포트 후 검증)했고, Task 6의 `CHMString`/json 실제 형식은 "기존 형태 확인" 지시로 위임(파일 형식 미상 부분).

**3. Type consistency** — `EAppUpdateAction`(Task1) ↔ `CHMAppUpdate.CheckAsync` 반환(Task3) ↔ ResourceDownload 비교(Task5) 일치. `AppUpdatePolicy.Decide` 시그니처(Task1) ↔ Task3 호출 인자 4개 일치. `StartImmediateAsync`/`StartFlexible`/`CompleteFlexibleUpdate`(Task3) ↔ Task5 호출 일치. `ImmediateThreshold=4`(Task1) ↔ Task4 가이드 문구 일치.

## 수동 QA 체크리스트 (네이티브 — 자동 테스트 불가)

1. 내부 테스트 트랙에 v1(낮은 versionCode) 설치 → v2(높은 versionCode, `inAppUpdatePriority=5`) 업로드 → v1 실행 시 **Immediate 전체화면** 표시
2. Immediate에서 뒤로가기 취소 → UIConfirm "업데이트 필요" → 확인 시 다시 전체화면
3. v2를 `inAppUpdatePriority=0`로 → v1 실행 시 게임 진입 + 백그라운드 다운로드 → 완료 시 "업데이트 준비 완료" 팝업 → Yes로 재시작·설치
4. 에디터/사이드로드 APK 실행 → 무동작(게임 정상 진입)
5. 비행기 모드 부팅 → 무동작(게임 정상 진입)
