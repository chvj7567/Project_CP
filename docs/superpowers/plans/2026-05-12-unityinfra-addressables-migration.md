# UnityInfra 패키지 적용 + Addressables 마이그레이션 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `com.chvj.unityinfra` 패키지로 인프라 전 영역 전환 + AssetBundle 시스템을 Addressables로 마이그레이션.

**Architecture:** 10단계(P0~P9) 점진적 마이그레이션. 각 Phase 종료 시 Unity Editor Play 모드 작동 보장. 기존 `Assets/Scripts/Manager/CHM*.cs`는 P2~P8 동안 패키지 위임 어댑터로 유지 후 P9에서 삭제. 게임 전용 매니저(`CHMJson`/`CHMData`/`CHMString`)는 `CHSingletonStatic<T>` 패턴으로 재작성. 자동화 도구는 에디터 메뉴로 추가.

**Tech Stack:** Unity 6000.0.68f1, com.unity.addressables 2.8.1, com.chvj.unityinfra (embedded), UniRx, DOTween, TMP, GPGS, Google Mobile Ads, Unity IAP.

**Spec 참조:** `docs/superpowers/specs/2026-05-12-unityinfra-addressables-migration-design.md`

---

## 파일 구조 — 변경 요약

### 생성

| 경로 | 책임 |
|---|---|
| `Assets/Scripts/Editor/CHToolMigrateToAddressables.cs` | "Resource" 라벨 부여 + 그룹 8개 자동 생성 메뉴 |
| `Assets/Scripts/Editor/CHToolMigratePrefabs.cs` | 프리합 컴포넌트 자동 재바인딩 메뉴 (dry-run + 백업) |
| `Assets/Scripts/Manager/GameStringProvider.cs` | 패키지 `IStringProvider` 구현 (CHMString/CHMJson 위임) |
| `Assets/Scripts/Manager/GameFontProvider.cs` | 패키지 `IFontProvider` 구현 (Gaegu-Bold SDF 캐싱) |
| `Assets/Scripts/Lobby/LBStageButton.cs` | 기존 `Function/CHButton` 게임 특화 필드 분리 |
| `Assets/Tests/EditMode/CHMJsonTests.cs` | CHMJson Init 동작 EditMode 테스트 |

### 수정 (P2~P8 어댑터, P6 재작성)

- `Assets/Scripts/Manager/CHMResource.cs` (P2 어댑터 → P9 삭제)
- `Assets/Scripts/Manager/CHMPool.cs` (P3 어댑터 → P9 삭제)
- `Assets/Scripts/Manager/CHMSound.cs` (P3 어댑터 → P9 삭제)
- `Assets/Scripts/Manager/CHMUI.cs` + `Assets/Scripts/UI/UIBase.cs` (P4 어댑터 → P9 삭제)
- `Assets/Scripts/Manager/CHMJson.cs` (P6 재작성)
- `Assets/Scripts/Manager/CHMString.cs` (P6 재작성)
- `Assets/Scripts/Manager/CHMData.cs` (P6 베이스 변경)
- `Assets/Scripts/Manager/CHMAdmob.cs` (P7 어댑터 → P9 삭제)
- `Assets/Scripts/Manager/CHMIAP.cs` (P7 어댑터 → P9 삭제)
- `Assets/Scripts/Manager/CHMGPGS.cs` (P7 어댑터 → P9 삭제)
- `Assets/Scripts/Manager/CHMMain.cs` (P9 삭제)
- `Assets/Scripts/Scenes/ResourceDownload.cs` (P7~P9 단순화)
- 게임 코드 36개 파일 (P8 일괄 치환)
- UI 14개 스크립트 (P4 시그니처 변경)

### 삭제 (P9)

- `Assets/Scripts/Manager/CHMAssetBundle.cs`
- `Assets/Scripts/Function/CHLoadingBarFromAssetBundle.cs`
- `Assets/Scripts/Function/CHButton.cs`
- `Assets/Scripts/Function/CHTMPro.cs`
- `Assets/Scripts/Function/CHToggle.cs`
- `Assets/Scripts/Editor/AssetBundleMenuItem.cs`
- `Assets/Scripts/Manager/CHMMain.cs` (위 어댑터들과 함께)

---

## Phase 0 — 작업 기준선

### Task 0.1: 현재 상태 커밋 (사용자 수동)

**Files:**
- Modify: `Packages/manifest.json` (이미 com.chvj.unityinfra 추가됨)
- Modify: `Packages/packages-lock.json`
- 신규: `Packages/com.chvj.unityinfra/` (이미 untracked)

- [ ] **Step 1: 현재 상태 확인**

Run:
```powershell
git status
```
Expected: `Packages/com.chvj.unityinfra/`가 untracked, manifest 2종 modified.

- [ ] **Step 2: 사용자에게 커밋 요청**

CLAUDE.md memory 정책상 Claude는 commit 직접 실행 X. 다음 명령을 사용자가 수동 실행:

```powershell
git add Packages/com.chvj.unityinfra Packages/manifest.json Packages/packages-lock.json
git commit -m "[Package] com.chvj.unityinfra 임베디드 패키지 추가 및 manifest 갱신"
```

- [ ] **Step 3: 커밋 확인**

Run:
```powershell
git log -1 --oneline
```
Expected: 방금 만든 커밋이 HEAD.

---

## Phase 1 — Addressables 마이그레이션

### Task 1.1: Addressables 마이그레이션 에디터 툴 작성

**Files:**
- Create: `Assets/Scripts/Editor/CHToolMigrateToAddressables.cs`

- [ ] **Step 1: 툴 파일 작성**

```csharp
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class CHToolMigrateToAddressables
{
    private const string LabelName = "Resource";
    private const string SourceRoot = "Assets/AssetBundleResources";
    private static readonly string[] BundleFolders = { "ui", "unit", "effect", "sprite", "sound", "font", "data", "json" };

    [MenuItem("CatPang/Migrate To Addressables", priority = 100)]
    public static void Migrate()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AddressableAssetSettingsDefaultObject.GetSettings(create: true);
        }

        if (!settings.GetLabels().Contains(LabelName))
        {
            settings.AddLabel(LabelName);
        }

        var conflicts = ScanBasenameConflicts();
        if (conflicts.Count > 0)
        {
            foreach (var (key, paths) in conflicts)
            {
                Debug.LogError($"[Migrate] Basename 충돌: '{key}' ← {string.Join(", ", paths)}");
            }
            EditorUtility.DisplayDialog("Migrate", $"Basename 충돌 {conflicts.Count}건. 콘솔 확인 후 해소 필요.", "OK");
            return;
        }

        int totalAdded = 0;
        foreach (var folder in BundleFolders)
        {
            string groupName = char.ToUpperInvariant(folder[0]) + folder.Substring(1);
            var group = settings.FindGroup(groupName) ?? settings.CreateGroup(
                groupName, false, false, true, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

            string folderPath = Path.Combine(SourceRoot, folder);
            if (!AssetDatabase.IsValidFolder(folderPath)) continue;

            var assetPaths = AssetDatabase.FindAssets("", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !AssetDatabase.IsValidFolder(p))
                .Where(p => Path.GetExtension(p).ToLower() != ".meta")
                .ToList();

            foreach (var path in assetPaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = Path.GetFileNameWithoutExtension(path);
                entry.SetLabel(LabelName, true, true);

                var importer = AssetImporter.GetAtPath(path);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    importer.assetBundleName = "";
                }
                totalAdded++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Migrate] 완료. 총 {totalAdded}개 에셋 등록, 그룹 {BundleFolders.Length}개.");
    }

    private static List<(string key, List<string> paths)> ScanBasenameConflicts()
    {
        var map = new Dictionary<string, List<string>>();
        foreach (var folder in BundleFolders)
        {
            string folderPath = Path.Combine(SourceRoot, folder);
            if (!AssetDatabase.IsValidFolder(folderPath)) continue;
            foreach (var guid in AssetDatabase.FindAssets("", new[] { folderPath }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue;
                if (Path.GetExtension(path).ToLower() == ".meta") continue;
                string key = Path.GetFileNameWithoutExtension(path);
                if (!map.ContainsKey(key)) map[key] = new List<string>();
                map[key].Add(path);
            }
        }
        return map.Where(kv => kv.Value.Count > 1).Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
```

- [ ] **Step 2: 컴파일 확인**

Unity 에디터로 돌아가 자동 컴파일 대기. Console에 에러 없는지 확인.

- [ ] **Step 3: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Editor/CHToolMigrateToAddressables.cs Assets/Scripts/Editor/CHToolMigrateToAddressables.cs.meta
git commit -m "[Editor] Addressables 마이그레이션 자동화 툴 추가"
```

### Task 1.2: 툴 실행 + 결과 확인

- [ ] **Step 1: 메뉴 실행**

Unity 메뉴: `CatPang > Migrate To Addressables`.
Expected: Console에 `[Migrate] 완료. 총 N개 에셋 등록, 그룹 8개.` 출력. 충돌이 있으면 빨간 로그 후 중단됨 — 그 경우 충돌 파일명 확인 후 사용자에게 보고.

- [ ] **Step 2: Addressables Groups 창 확인**

Unity 메뉴: `Window > Asset Management > Addressables > Groups`.
Expected: UI, Unit, Effect, Sprite, Sound, Font, Data, Json 그룹 존재. 각 에셋이 "Resource" 라벨로 표시됨.

- [ ] **Step 3: Addressables 빌드**

Groups 창의 `Build > New Build > Default Build Script` 클릭.
Expected: 빌드 완료, 에러 없음. `Library/com.unity.addressables/aa/` 산출물 생성.

- [ ] **Step 4: 사용자 커밋 요청**

```powershell
git add Assets/AssetBundleResources Assets/AddressableAssetsData
git commit -m "[Addressables] AssetBundleResources 전 에셋에 Resource 라벨 + 그룹 8개 부여"
```

### Task 1.3: Addressables 동작 검증

- [ ] **Step 1: 임시 검증 스크립트로 패키지 CHMResource 작동 확인**

작업 전 임시 검증 코드를 `Assets/Scripts/Editor/CHToolMigrateToAddressables.cs`에 추가:

```csharp
[MenuItem("CatPang/Test/Load UIShop via Package CHMResource", priority = 200)]
public static async void TestLoad()
{
    await ChvjUnityInfra.CHMResource.Instance.Init();
    ChvjUnityInfra.CHMResource.Instance.Load<GameObject>("UIShop", obj =>
    {
        Debug.Log($"[Test] UIShop 로드 결과: {(obj != null ? obj.name : "null")}");
    });
}
```

- [ ] **Step 2: 메뉴 실행**

Unity 메뉴: `CatPang > Test > Load UIShop via Package CHMResource`. Editor가 Play 모드 아니어도 Addressables는 작동.
Expected: Console에 `[Test] UIShop 로드 결과: UIShop` 출력.

- [ ] **Step 3: 임시 검증 코드 제거**

위에서 추가한 `TestLoad` 메서드와 그 attribute 제거. 파일 저장.

- [ ] **Step 4: 사용자 커밋 요청 (검증만이라 커밋 X)**

이 단계는 검증만이므로 커밋 없음. 다음 Phase로.

---

## Phase 2 — CHMResource 어댑터 전환

### Task 2.1: CHMResource를 패키지 위임 어댑터로 교체

**Files:**
- Modify: `Assets/Scripts/Manager/CHMResource.cs`

- [ ] **Step 1: 현재 파일 백업 확인**

Run:
```powershell
git log --oneline Assets/Scripts/Manager/CHMResource.cs | Select-Object -First 1
```
Expected: 최근 커밋 SHA 출력. 롤백 안전.

- [ ] **Step 2: 어댑터로 재작성**

`Assets/Scripts/Manager/CHMResource.cs` 전체 교체:

```csharp
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CHMResource
{
    private bool _initialized = false;

    public async Task EnsureInit()
    {
        if (_initialized) return;
        _initialized = true;
        await ChvjUnityInfra.CHMResource.Instance.Init();
    }

    public void LoadData(string name, Action<TextAsset> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<TextAsset>(name, _callback);
    }

    public void LoadFont(Defines.ELanguageType languageType, Action<TMP_FontAsset> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<TMP_FontAsset>("Gaegu-Bold SDF", _callback);
    }

    public void LoadJson(Defines.EJsonType _jsonType, Action<TextAsset> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<TextAsset>(_jsonType.ToString(), _callback);
    }

    public void LoadSprite(Defines.EBlockState _spriteType, Action<Sprite> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<Sprite>(_spriteType.ToString(), _callback);
    }

    public void LoadSound(Defines.ESound _soundType, Action<AudioClip> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<AudioClip>(_soundType.ToString(), _callback);
    }

    public void InstantiateUI(Defines.EUI _ui, Action<GameObject> _callback = null)
    {
        ChvjUnityInfra.CHMResource.Instance.Instantiate<GameObject>(_ui.ToString(), _callback);
    }

    public void InstantiateEffect(Defines.EEffect _effectType, Action<GameObject> _callback = null)
    {
        ChvjUnityInfra.CHMResource.Instance.Instantiate<GameObject>(_effectType.ToString(), _callback);
    }

    public GameObject Instantiate(GameObject _object, Transform _parent = null)
    {
        if (_object == null) return null;
        CHPoolable poolable = _object.GetComponent<CHPoolable>();
        if (poolable != null)
        {
            return CHMMain.Pool.Pop(_object, _parent).gameObject;
        }
        return GameObject.Instantiate(_object, _parent);
    }

    public async void Destroy(GameObject _object, float _time = 0)
    {
        if (_object == null) return;
        CHPoolable poolable = _object.GetComponent<CHPoolable>();
        if (poolable != null)
        {
            await Task.Delay((int)(_time * 1000f));
            CHMMain.Pool.Push(poolable);
        }
        else
        {
            UnityEngine.Object.Destroy(_object, _time);
        }
    }
}
```

핵심 변경: `LoadAsset<T>(_bundleName, _assetName, cb)` 내부가 사라지고, 각 헬퍼가 직접 `ChvjUnityInfra.CHMResource.Instance.Load<T>(key, cb)`로 호출.

- [ ] **Step 3: CHPoolable 참조 확인**

`Assets/Scripts/Manager/CHMResource.cs:34` 등에서 사용하는 `CHPoolable` 타입이 게임 측 글로벌인지 패키지인지 확인:

Run:
```powershell
Get-ChildItem -Recurse -Path Assets/Scripts -Filter CHPoolable.cs
```
Expected: `Assets/Scripts/Function/CHPoolable.cs` 또는 게임 측 위치 확인. 만약 게임 측에 없고 패키지(ChvjUnityInfra)에만 있으면 위 코드의 `CHPoolable` → `ChvjUnityInfra.CHPoolable`로 변경.

- [ ] **Step 4: Init 호출 추가**

`Assets/Scripts/Manager/CHMMain.cs`의 `Init()` 메서드에 `CHMResource` 초기화 await 추가:

```csharp
static async void Init()
{
    if (m_instance == null)
    {
        // ... 기존 m_instance 세팅 코드 ...

        await m_instance.m_resource.EnsureInit();   // 추가
        await m_instance.m_json.Init();
        m_instance.m_pool.Init();
        m_instance.m_sound.Init();
    }
}
```

- [ ] **Step 5: Unity 컴파일 + Play 모드 검증**

Unity 에디터로 돌아가서 컴파일 대기. Console 에러 없음 확인.
Play 모드 진입(ResourceDownloadScene). Expected: 다음 씬으로 정상 진입, 로비 정상.

다만 ResourceDownloadScene이 아직 `CHLoadingBarFromAssetBundle`을 호출하므로 그 시점에 멈출 수 있음. **Editor에서 직접 FirstScene으로 진입하여 검증** (Hierarchy에서 ResourceDownloadScene 건너뛰고 다음 씬 열기).

- [ ] **Step 6: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/CHMResource.cs Assets/Scripts/Manager/CHMMain.cs
git commit -m "[Manager] CHMResource를 패키지 ChvjUnityInfra.CHMResource 위임 어댑터로 전환"
```

---

## Phase 3 — CHMPool, CHMSound 어댑터 전환

### Task 3.1: CHMPool 어댑터로 전환

**Files:**
- Modify: `Assets/Scripts/Manager/CHMPool.cs`

- [ ] **Step 1: 파일 전체 교체**

```csharp
using UnityEngine;

public class CHMPool
{
    public void Init()
    {
        ChvjUnityInfra.CHMPool.Instance.Init();
    }

    public void CreatePool(GameObject _original, int _count = 5)
    {
        ChvjUnityInfra.CHMPool.Instance.CreatePool(_original, _count);
    }

    public void Push(CHPoolable poolable)
    {
        ChvjUnityInfra.CHMPool.Instance.Push(poolable as ChvjUnityInfra.CHPoolable);
    }

    public ChvjUnityInfra.CHPoolable Pop(GameObject _original, Transform _parent = null)
    {
        return ChvjUnityInfra.CHMPool.Instance.Pop(_original, _parent);
    }

    public GameObject GetOriginal(string _name)
    {
        return ChvjUnityInfra.CHMPool.Instance.GetOriginal(_name);
    }

    public void Clear()
    {
        ChvjUnityInfra.CHMPool.Instance.Clear();
    }
}
```

주의: 게임 측 `CHPoolable`과 패키지 `ChvjUnityInfra.CHPoolable` 둘 다 존재 가능. Step 2에서 확인 후 결정.

- [ ] **Step 2: CHPoolable 위치 확인**

Run:
```powershell
Get-ChildItem -Recurse -Path Assets/Scripts -Filter CHPoolable.cs
```

게임 측 `Assets/Scripts/Function/CHPoolable.cs`가 있으면 그것이 글로벌 클래스. 패키지에도 동명 클래스가 있어서 사용 시 fully-qualified가 필요. **Step 1의 코드에서 `CHPoolable poolable` 인자는 글로벌 게임 측 타입을 받아 패키지 타입으로 변환하는 캐스팅이 필요.**

만약 게임 측 CHPoolable이 패키지 것과 시그니처 동일하면 **게임 측 CHPoolable을 글로벌 namespace에서 제거하고 `using ChvjUnityInfra;` 추가가 간단**. 단, 글로벌 alias 깨짐. 영향 확인:

Run:
```powershell
$matches = Select-String -Path "Assets/Scripts/**/*.cs" -Pattern "\bCHPoolable\b" -Exclude "*.meta"
$matches | Measure-Object -Line
```

영향이 크면 어댑터 유지, 작으면 글로벌 CHPoolable 삭제 + `using ChvjUnityInfra;` 추가.

- [ ] **Step 3: 결정에 따라 적용**

게임 측 CHPoolable 유지 시 Step 1 코드 그대로. 삭제 시:
```powershell
git rm Assets/Scripts/Function/CHPoolable.cs Assets/Scripts/Function/CHPoolable.cs.meta
```
그리고 CHPoolable 사용 파일에 `using ChvjUnityInfra;` 추가 (sed/Edit).

- [ ] **Step 4: 컴파일 + Play 모드 검증**

Console 에러 없음 확인. Play 모드에서 GameScene 진입 후 블록 풀링 정상 작동 확인 (블록이 매치되어 사라지고 새 블록이 생성되는 사이클).

- [ ] **Step 5: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/CHMPool.cs
git commit -m "[Manager] CHMPool을 패키지 ChvjUnityInfra.CHMPool 위임 어댑터로 전환"
```

### Task 3.2: CHMSound 어댑터로 전환 + Init 호출

**Files:**
- Modify: `Assets/Scripts/Manager/CHMSound.cs`
- Modify: `Assets/Scripts/Manager/CHMMain.cs`

- [ ] **Step 1: CHMSound.cs 전체 교체**

```csharp
using UnityEngine;

public class CHMSound
{
    public float bgmVolume { get { return ChvjUnityInfra.CHMSound.Instance.BgmVolume; } }
    public float effectVolume { get { return ChvjUnityInfra.CHMSound.Instance.EffectVolume; } }
    public float Ratio { get { return ChvjUnityInfra.CHMSound.Instance.Ratio; } }

    public void Init()
    {
        ChvjUnityInfra.CHMSound.Instance.Init<Defines.ESound>(Defines.ESound.Bgm);
    }

    public void SetBGMVolume(float volume)
    {
        ChvjUnityInfra.CHMSound.Instance.SetBGMVolume(volume);
    }

    public void SetEffectVolume(float volume)
    {
        ChvjUnityInfra.CHMSound.Instance.SetEffectVolume(volume);
    }

    public void Play(Defines.ESound type, float pitch = 1.0f)
    {
        ChvjUnityInfra.CHMSound.Instance.Play(type, pitch);
    }
}
```

- [ ] **Step 2: CHMMain의 Init에 sound init 순서 확인**

`Assets/Scripts/Manager/CHMMain.cs:50` `m_instance.m_sound.Init();`이 이미 존재. 그대로 둠. (어댑터의 Init이 패키지 Init<TAudio> 호출하므로 작동.)

- [ ] **Step 3: 컴파일 + Play 모드 검증**

ResourceDownloadScene 우회로 직접 FirstScene 진입. 로비에서 버튼 클릭 → Cat 사운드 출력 확인. Bgm 루프 확인.

- [ ] **Step 4: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/CHMSound.cs
git commit -m "[Manager] CHMSound을 패키지 ChvjUnityInfra.CHMSound 위임 어댑터로 전환"
```

---

## Phase 4 — UI 시스템 교체

### Task 4.1: ShowUI 호출 사이트 사전 점검

- [ ] **Step 1: false 모드 호출 grep**

Run (Grep 툴):
```
pattern: \.ShowUI\([^)]*,\s*false\s*\)
path: Assets/Scripts
```
Expected: 일치 라인을 사용자에게 보고. 발견 시 각 호출처를 단일 캔버스 모드(`true` 또는 기본값)로 변경할 항목으로 메모.

- [ ] **Step 2: uid 반환값 사용 grep**

Run:
```
pattern: \bint\b[^=;]*=\s*\w+\.ShowUI\(
path: Assets/Scripts
```
Expected: uid를 변수에 저장하는 패턴 찾기. 발견 시 호출처에서 UIBase 참조 보관 방식으로 변경할 항목으로 메모.

- [ ] **Step 3: 발견 항목 리팩터**

Step 1, 2에서 발견된 호출처들을 수정. 호출처가 많으면 각 호출처를 별도 Edit으로 처리. 변경 없으면 다음 Task로.

- [ ] **Step 4: 사용자 커밋 요청 (변경 있을 때만)**

```powershell
git add Assets/Scripts
git commit -m "[UI] ShowUI 다중 인스턴스/별도 캔버스 호출 사이트 단일 캔버스로 통합"
```

### Task 4.2: UIBase 어댑터 작성

**Files:**
- Modify: `Assets/Scripts/UI/UIBase.cs`
- Modify: `Assets/Scripts/Manager/CHMUI.cs`

- [ ] **Step 1: 게임 측 UIBase를 패키지 UIBase 상속으로 교체**

`Assets/Scripts/UI/UIBase.cs`:

```csharp
using UnityEngine;

public class CHUIArg : ChvjUnityInfra.UIArg
{
    public static readonly CHUIArg empty = new CHUIArg();
}

public abstract class UIBase : ChvjUnityInfra.UIBase
{
    [HideInInspector] public Defines.EUI eUIType
    {
        get => UIType as Defines.EUI? ?? Defines.EUI.None;
    }
    [HideInInspector] public int uid = 0;

    public override void InitUI(ChvjUnityInfra.UIArg arg)
    {
        InitUI(arg as CHUIArg ?? CHUIArg.empty);
    }

    public virtual void InitUI(CHUIArg _uiArg) { }

    public virtual void CloseUI() { }

    public override void Close(bool reuse = true)
    {
        CloseUI();
        base.Close(reuse);
    }
}
```

이 변경으로 기존 14개 UI 스크립트는 `InitUI(CHUIArg)`/`CloseUI()` 시그니처 유지 가능. 베이스만 패키지로 바뀜.

주의: `eUIType` 변환에서 `UIType as Defines.EUI?`는 boxing/unboxing 필요. C# enum 캐스팅 패턴은 `(Defines.EUI)UIType`이 적절하나 nullable과 충돌. 대안:

```csharp
public Defines.EUI eUIType
{
    get => UIType is Defines.EUI e ? e : Defines.EUI.None;
}
```

위 코드로 교체.

- [ ] **Step 2: CHMUI 어댑터로 전환**

`Assets/Scripts/Manager/CHMUI.cs` 전체 교체:

```csharp
using UnityEngine;

public class CHUIWaitData { /* deprecated, 호환 placeholder */ }

public class CHMUI
{
    public int ShowUI(Defines.EUI _uiType, CHUIArg _uiArg, bool _bCurrentBackground = true)
    {
        ChvjUnityInfra.CHMUI.Instance.ShowUI(_uiType, _uiArg, ui =>
        {
            if (ui is UIBase b) b.uid = 0;
        });
        return 0; // uid 반환 더 이상 의미 없음
    }

    public void CloseUI(GameObject _uiObj)
    {
        if (_uiObj == null) return;
        var ui = _uiObj.GetComponent<UIBase>();
        if (ui != null) ui.Close(false);
    }

    public void CloseUI(Defines.EUI _uiType)
    {
        ChvjUnityInfra.CHMUI.Instance.CloseUI(_uiType, false);
    }

    public void UpdateUI() { /* 패키지는 즉시 처리, 큐 없음 */ }

    public void CreateEventSystemObject()
    {
        ChvjUnityInfra.CHMResource.Instance.Instantiate<GameObject>(Defines.EUI.EventSystem, _ => { });
    }
}
```

- [ ] **Step 3: UICanvas 태그/씬 배치 확인**

패키지 `CHMUI.Init`는 `GameObject.FindGameObjectWithTag("UICanvas")`로 캔버스를 찾음. 씬에 UICanvas 태그가 부여된 GameObject가 있어야 함.

씬 LBLobbyScene, GPGameScene 등을 Hierarchy에서 열어 UICanvas 태그 부여 확인. 없으면 추가. (어댑터 모드에서도 패키지 CHMUI가 init되려면 필요.)

- [ ] **Step 4: 패키지 CHMUI.Init 호출 추가**

`Assets/Scripts/Manager/CHMMain.cs`의 Init에:

```csharp
static async void Init()
{
    if (m_instance == null)
    {
        // ... 기존 코드 ...

        await m_instance.m_resource.EnsureInit();
        await m_instance.m_json.Init();
        m_instance.m_pool.Init();
        m_instance.m_sound.Init();
        ChvjUnityInfra.CHMUI.Instance.Init();   // 추가
    }
}
```

`ChvjUnityInfra.CHMUI.Instance.Init()`은 UICanvas 태그가 있는 씬에서만 작동. ResourceDownloadScene에 UICanvas가 없으면 다음 씬으로 넘어간 후 init되도록 처리. 또는 ResourceDownloadScene에 UICanvas 추가.

- [ ] **Step 5: 컴파일 + Play 모드 검증**

각 UI 화면 한 번씩 열어서 정상 표시 + 닫기 확인:
- LBLobbyScene 진입 → 설정 UI 열기/닫기
- 스테이지 선택 UI 진입/닫기
- 미션, 상점 UI 진입/닫기

ESC 키로 최상위 UI 닫히는지 확인.

- [ ] **Step 6: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/UI/UIBase.cs Assets/Scripts/Manager/CHMUI.cs Assets/Scripts/Manager/CHMMain.cs
git commit -m "[UI] UIBase/CHMUI를 패키지 ChvjUnityInfra.UI 위임 어댑터로 전환"
```

### Task 4.3: 프리합 backgroundBtn/backBtn 필드 재바인딩 툴

**Files:**
- Create: `Assets/Scripts/Editor/CHToolMigrateUIPrefabs.cs`

- [ ] **Step 1: 툴 작성**

```csharp
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CHToolMigrateUIPrefabs
{
    private const string BackupDir = "Library/CatPangPrefabBackup";

    [MenuItem("CatPang/Migrate UI Prefabs (backgroundBtn → _backgroundButton)", priority = 110)]
    public static void Migrate()
    {
        string ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupRoot = Path.Combine(BackupDir, ts);
        Directory.CreateDirectory(backupRoot);

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/AssetBundleResources/ui" });
        int patched = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string yaml = File.ReadAllText(path);
            string backupPath = Path.Combine(backupRoot, Path.GetFileName(path));
            File.WriteAllText(backupPath, yaml);

            string newYaml = Regex.Replace(yaml, @"\bbackgroundBtn:", "_backgroundButton:");
            newYaml = Regex.Replace(newYaml, @"\bbackBtn:", "_backButton:");

            if (newYaml != yaml)
            {
                File.WriteAllText(path, newYaml);
                patched++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[Migrate UI Prefabs] 패치된 프리합 {patched}개. 백업: {backupRoot}");
    }
}
```

- [ ] **Step 2: Dry-run 변종 추가**

같은 파일에 dry-run 모드도 추가:

```csharp
[MenuItem("CatPang/Migrate UI Prefabs (Dry-run)", priority = 109)]
public static void MigrateDryRun()
{
    var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/AssetBundleResources/ui" });
    int wouldPatch = 0;
    foreach (var guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        string yaml = File.ReadAllText(path);
        if (Regex.IsMatch(yaml, @"\bbackgroundBtn:|\bbackBtn:"))
        {
            Debug.Log($"[Dry-run] 패치 대상: {path}");
            wouldPatch++;
        }
    }
    Debug.Log($"[Dry-run] 패치 대상 총 {wouldPatch}개.");
}
```

- [ ] **Step 3: Dry-run 실행**

Unity 메뉴: `CatPang > Migrate UI Prefabs (Dry-run)`. Console 출력 확인.
Expected: 14개 UI 프리합 중 backgroundBtn/backBtn 필드가 있는 N개가 나열됨.

- [ ] **Step 4: 실제 마이그레이션 실행**

Unity 메뉴: `CatPang > Migrate UI Prefabs (backgroundBtn → _backgroundButton)`. Console에 "패치된 프리합 N개" 확인.

- [ ] **Step 5: 프리합 인스펙터 확인**

`Assets/AssetBundleResources/ui/UIShop.prefab`을 열어 UIBase 컴포넌트의 `_backgroundButton`, `_backButton`이 정상 바인딩됐는지 확인.

- [ ] **Step 6: Play 모드 검증**

각 UI 열어서 배경/뒤로가기 버튼 클릭 시 닫히는지 확인.

- [ ] **Step 7: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Editor/CHToolMigrateUIPrefabs.cs Assets/Scripts/Editor/CHToolMigrateUIPrefabs.cs.meta Assets/AssetBundleResources/ui
git commit -m "[UI] UI 프리합 backgroundBtn/backBtn 필드명 패키지 명명규칙으로 재바인딩"
```

---

## Phase 5 — Function/ 컴포넌트 → 패키지

### Task 5.1: 패키지 CHText API 확인 + 필요 시 게임 측 헬퍼 작성

- [ ] **Step 1: 패키지 CHText 전체 코드 읽기**

Read: `Packages/com.chvj.unityinfra/Runtime/UI/CHText.cs` (전체 파일).

확인 사항:
- `SetText(params object[])` 시그니처 존재 여부
- `SetStringID(int)` 시그니처 존재 여부
- `SetColor(Color)` 존재 여부
- `_stringID` 필드 직렬화 형식 (private serialized인지)

- [ ] **Step 2: 누락된 API가 있으면 게임 측 헬퍼 작성**

누락 API가 있으면 `Assets/Scripts/Function/CHTextExtensions.cs` 생성:

```csharp
using ChvjUnityInfra;
using TMPro;
using UnityEngine;

public static class CHTextExtensions
{
    public static void SetStringID(this CHText self, int stringID)
    {
        // 패키지 CHText에 SetStringID가 없을 경우의 보충 구현
        // 필드 접근이 private이라 reflection 또는 패키지 PR 필요할 수 있음
        // 임시: 자체 구현으로 stringID + StringProvider 호출
        var text = self.GetComponent<TMP_Text>();
        if (text == null) return;
        if (CHText.StringProvider != null)
        {
            text.text = CHText.StringProvider.GetString(stringID);
        }
    }

    public static void SetColor(this CHText self, Color color)
    {
        var text = self.GetComponent<TMP_Text>();
        if (text != null) text.color = color;
    }

    public static string GetString(this CHText self)
    {
        var text = self.GetComponent<TMP_Text>();
        return text != null ? text.text : "";
    }
}
```

- [ ] **Step 3: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Function/CHTextExtensions.cs Assets/Scripts/Function/CHTextExtensions.cs.meta
git commit -m "[UI] CHText 누락 API 보충 확장 메서드"
```

### Task 5.2: LBStageButton 게임 컴포넌트 작성

**Files:**
- Create: `Assets/Scripts/Lobby/LBStageButton.cs`

- [ ] **Step 1: 컴포넌트 작성**

```csharp
using ChvjUnityInfra;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CHButton))]
public class LBStageButton : MonoBehaviour
{
    public TMP_Text text;
    public GameObject clearObj;
    public GameObject lockObj;
    public GameObject unlockObj;
}
```

- [ ] **Step 2: 컴파일 확인**

Unity Console 에러 없음.

- [ ] **Step 3: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Lobby/LBStageButton.cs Assets/Scripts/Lobby/LBStageButton.cs.meta
git commit -m "[UI] LBStageButton 게임 컴포넌트 추가 (기존 CHButton 게임 특화 필드 분리)"
```

### Task 5.3: 프리합 컴포넌트 자동 재바인딩 툴 작성

**Files:**
- Create: `Assets/Scripts/Editor/CHToolMigratePrefabComponents.cs`

- [ ] **Step 1: 툴 작성 (script GUID 매핑)**

먼저 GUID 수집:

Run (PowerShell):
```powershell
Get-Content Assets/Scripts/Function/CHButton.cs.meta
Get-Content Assets/Scripts/Function/CHTMPro.cs.meta
Get-Content Assets/Scripts/Function/CHToggle.cs.meta
Get-Content Assets/Scripts/Lobby/LBStageButton.cs.meta
Get-Content Packages/com.chvj.unityinfra/Runtime/UI/CHButton.cs.meta
Get-Content Packages/com.chvj.unityinfra/Runtime/UI/CHText.cs.meta
Get-Content Packages/com.chvj.unityinfra/Runtime/UI/CHToggle.cs.meta
```

각 파일의 `guid: XXXX` 16진 32자 값을 수집.

- [ ] **Step 2: 툴 작성**

GUID를 상수로 박은 툴 작성. 예시 (실제 GUID는 Step 1 결과로 치환):

```csharp
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CHToolMigratePrefabComponents
{
    private const string BackupDir = "Library/CatPangPrefabBackup";

    // Step 1에서 수집한 실제 GUID로 치환
    private const string OldCHButtonGUID = "GUID_HERE_OLD_CHBUTTON";
    private const string OldCHTMProGUID  = "GUID_HERE_OLD_CHTMPRO";
    private const string OldCHToggleGUID = "GUID_HERE_OLD_CHTOGGLE";
    private const string NewCHButtonGUID = "GUID_HERE_NEW_CHBUTTON";
    private const string NewCHTextGUID   = "GUID_HERE_NEW_CHTEXT";
    private const string NewCHToggleGUID = "GUID_HERE_NEW_CHTOGGLE";
    private const string LBStageButtonGUID = "GUID_HERE_LBSTAGEBUTTON";

    [MenuItem("CatPang/Migrate Prefab Components (Dry-run)", priority = 120)]
    public static void DryRun() => Run(dryRun: true);

    [MenuItem("CatPang/Migrate Prefab Components", priority = 121)]
    public static void Migrate() => Run(dryRun: false);

    private static void Run(bool dryRun)
    {
        string ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupRoot = Path.Combine(BackupDir, ts);
        if (!dryRun) Directory.CreateDirectory(backupRoot);

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int patched = 0;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string yaml = File.ReadAllText(path);
            string newYaml = yaml;

            // CHTMPro → CHText, stringID → _stringID
            newYaml = Regex.Replace(newYaml, @"guid:\s*" + OldCHTMProGUID, $"guid: {NewCHTextGUID}");
            newYaml = Regex.Replace(newYaml, @"^(\s+)stringID:", "$1_stringID:", RegexOptions.Multiline);

            // CHToggle (Function) → CHToggle (package)
            newYaml = Regex.Replace(newYaml, @"guid:\s*" + OldCHToggleGUID, $"guid: {NewCHToggleGUID}");

            // CHButton (Function) → CHButton (package). 추가 컴포넌트(LBStageButton) append는 별도 처리 필요.
            // 단순 GUID 치환은 게임 특화 필드(text/clearObj/lockObj/unlockObj)를 잃게 함.
            // 안전을 위해: 기존 CHButton이 있는 프리합은 사용자에게 보고하고 수동 처리 안내.
            if (Regex.IsMatch(yaml, @"guid:\s*" + OldCHButtonGUID))
            {
                Debug.LogWarning($"[Migrate] CHButton 사용 프리합 (수동 LBStageButton 분리 필요): {path}");
            }

            if (newYaml != yaml)
            {
                if (!dryRun)
                {
                    File.WriteAllText(Path.Combine(backupRoot, Path.GetFileName(path)), yaml);
                    File.WriteAllText(path, newYaml);
                }
                patched++;
                Debug.Log($"[{(dryRun ? "Dry-run" : "Migrate")}] {path}");
            }
        }

        if (!dryRun) AssetDatabase.Refresh();
        Debug.Log($"[Migrate Prefab Components {(dryRun ? "Dry-run" : "")}] 변경 대상 {patched}개. 백업: {(dryRun ? "skipped" : backupRoot)}");
    }
}
```

- [ ] **Step 3: Dry-run 실행**

Unity 메뉴: `CatPang > Migrate Prefab Components (Dry-run)`.
Expected: 변경 대상 프리합 N개 목록 + CHButton 사용 프리합 경고 별도 출력.

- [ ] **Step 4: CHButton 사용 프리합 수동 처리 안내 출력 확인**

`StageButton.prefab` 등 CHButton 사용 프리합 리스트를 사용자에게 보고. 각 프리합에 대해:
1. 기존 `Function/CHButton` 컴포넌트의 text/clearObj/lockObj/unlockObj 필드를 `LBStageButton`에 옮김
2. 기존 CHButton 컴포넌트 제거
3. 패키지 CHButton 컴포넌트 추가

이 단계는 사용자가 Unity 인스펙터에서 수동 수행 (script GUID 치환 + LBStageButton 추가 + 필드 이관은 YAML 직접 편집으로도 가능하나 위험).

- [ ] **Step 5: 사용자 커밋 요청 (툴만)**

```powershell
git add Assets/Scripts/Editor/CHToolMigratePrefabComponents.cs Assets/Scripts/Editor/CHToolMigratePrefabComponents.cs.meta
git commit -m "[Editor] 프리합 컴포넌트 자동 재바인딩 툴 추가"
```

### Task 5.4: 프리합 재바인딩 실행 + 수동 수정

- [ ] **Step 1: 자동 마이그레이션 실행**

Unity 메뉴: `CatPang > Migrate Prefab Components`. Console 결과 확인.

- [ ] **Step 2: Unity Editor에서 컴포넌트 정상 표시 확인**

`Assets/AssetBundleResources/ui/UIShop.prefab` 등 열어서 `CHText`가 정상 표시 + `_stringID` 필드 값이 보존됐는지 확인.

- [ ] **Step 3: StageButton 프리합 수동 수정**

StageButton.prefab을 인스펙터에서 열고:
1. 기존 `Function/CHButton` 컴포넌트의 text/clearObj/lockObj/unlockObj 필드 메모
2. 컴포넌트 제거
3. `Add Component > CHButton (ChvjUnityInfra)` 추가
4. `Add Component > LBStageButton` 추가
5. LBStageButton의 text/clearObj/lockObj/unlockObj 필드에 메모한 GameObject 재바인딩
6. StageSelect.cs의 `btnList` 타입을 `List<CHButton>` 또는 `List<LBStageButton>`으로 변경 — 이건 P8 일괄 치환에서 처리

- [ ] **Step 4: Play 모드 검증**

LBLobbyScene 진입 → 스테이지 선택 UI → 스테이지 버튼 표시 확인 (잠금/해제/클리어 표시 정상).
설정 UI에서 텍스트 표시 확인 (한국어).

- [ ] **Step 5: 사용자 커밋 요청**

```powershell
git add Assets/AssetBundleResources/ui Assets/Prefabs
git commit -m "[UI] 프리합 컴포넌트 패키지 CHButton/CHText/CHToggle로 마이그레이션"
```

---

## Phase 6 — 게임 매니저 재작성

### Task 6.1: CHMJson 재작성 (CHSingletonStatic<T> 패턴)

**Files:**
- Modify: `Assets/Scripts/Manager/CHMJson.cs`
- Create: `Assets/Tests/EditMode/CHMJsonTests.cs` (선택)

- [ ] **Step 1: 기존 CHMJson API 확인**

Read: `Assets/Scripts/Manager/CHMJson.cs`. 게임 코드의 호출처 grep:

Pattern: `CHMMain\.Json\.\w+`. 사용되는 메서드/필드 리스트 작성.

- [ ] **Step 2: 재작성**

호출처 API를 보존하면서 베이스를 `CHSingletonStatic<CHMJson>`으로 변경:

```csharp
using ChvjUnityInfra;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CHMJson : CHSingletonStatic<CHMJson>
{
    private Dictionary<Defines.EJsonType, string> _jsonMap = new();
    // 기존 CHMJson의 다른 필드/메서드 보존 (예: GetStringInfo, GetStageInfo 등)

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
        ChvjUnityInfra.CHMResource.Instance.Load<TextAsset>(type, asset =>
        {
            if (asset != null) _jsonMap[type] = asset.text;
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    public string GetJson(Defines.EJsonType type) =>
        _jsonMap.TryGetValue(type, out var v) ? v : null;

    public void Clear() { _jsonMap.Clear(); }

    // 기존 GetXxx 헬퍼들도 이관 (Step 1에서 발견한 API)
}
```

기존 메서드(예: `GetStageInfo`, `GetMissionInfo` 등)를 동일 시그니처로 보존.

- [ ] **Step 3: CHMMain의 CHMJson 인스턴스 참조 제거**

`Assets/Scripts/Manager/CHMMain.cs`의:
```csharp
CHMJson m_json = new CHMJson();
public static CHMJson Json { get { return Instance.m_json; } }
```
→ 정적 프로퍼티가 `CHMJson.Instance`를 반환하도록 변경:

```csharp
public static CHMJson Json { get { return CHMJson.Instance; } }
```

`Init` 메서드에서 `m_json` 참조도 `CHMJson.Instance`로 변경. `m_json = new CHMJson()` 필드 자체 제거.

- [ ] **Step 4: 컴파일 + Play 모드 검증**

ResourceDownloadScene 우회 후 LBLobbyScene 진입. 미션/상점 UI 열어서 텍스트 표시 정상 확인 (JSON 데이터가 정상 로드됐다는 의미).

- [ ] **Step 5: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/CHMJson.cs Assets/Scripts/Manager/CHMMain.cs
git commit -m "[Manager] CHMJson을 CHSingletonStatic<CHMJson> 패턴으로 재작성"
```

### Task 6.2: CHMString 재작성

**Files:**
- Modify: `Assets/Scripts/Manager/CHMString.cs`

- [ ] **Step 1: 베이스 변경**

`Assets/Scripts/Manager/CHMString.cs` 첫 줄(클래스 선언) 변경:

```csharp
// 기존:
// public class CHMString { ... }

// 신:
using ChvjUnityInfra;
public class CHMString : CHSingletonStatic<CHMString>
{
    // 기존 const들 + 메서드들 그대로 유지
}
```

- [ ] **Step 2: CHMMain의 CHMString 참조 변경**

`Assets/Scripts/Manager/CHMMain.cs`:
```csharp
public static CHMString String { get { return CHMString.Instance; } }
```
`m_string` 필드 제거.

- [ ] **Step 3: 컴파일 + 검증**

UI에 표시되는 한국어 텍스트가 정상인지 Play 모드에서 확인.

- [ ] **Step 4: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/CHMString.cs Assets/Scripts/Manager/CHMMain.cs
git commit -m "[Manager] CHMString을 CHSingletonStatic<CHMString> 패턴으로 변경"
```

### Task 6.3: CHMData 베이스 변경

**Files:**
- Modify: `Assets/Scripts/Manager/CHMData.cs`

- [ ] **Step 1: 베이스 확인**

`CHMData`가 이미 `CHSingleton<CHMData>` 또는 유사한 패턴인지 확인. `CHSingletonStatic<CHMData>`로 변경 (Mono가 필요하면 `CHSingleton<CHMData>`).

```csharp
using ChvjUnityInfra;
public class CHMData : CHSingletonStatic<CHMData>
{
    // 기존 코드 유지
}
```

`CHMData.Instance.X` 호출 패턴이 이미 게임 코드 곳곳에 있으므로 호출처 변경 불요.

- [ ] **Step 2: 컴파일 + 검증**

게임 종료/재시작 시 LoginData 정상 로드 확인.

- [ ] **Step 3: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/CHMData.cs
git commit -m "[Manager] CHMData 베이스를 CHSingletonStatic<CHMData>로 변경"
```

### Task 6.4: GameStringProvider, GameFontProvider 작성

**Files:**
- Create: `Assets/Scripts/Manager/GameStringProvider.cs`
- Create: `Assets/Scripts/Manager/GameFontProvider.cs`

- [ ] **Step 1: GameStringProvider 작성**

```csharp
using ChvjUnityInfra;

public class GameStringProvider : IStringProvider
{
    public string GetString(int stringID)
    {
        return CHMString.Instance.GetString(stringID);
    }
}
```

`CHMString.Instance.GetString(int)` 메서드가 없으면 기존 `CHMMain.String.GetString(int)` 시그니처를 참고해 추가 (CHMString 안에 이미 있을 가능성 높음).

- [ ] **Step 2: GameFontProvider 작성**

```csharp
using System.Threading.Tasks;
using ChvjUnityInfra;
using TMPro;
using UnityEngine;

public class GameFontProvider : IFontProvider
{
    private static TMP_FontAsset _cachedFont;
    private static Material _cachedMaterial;

    public static Task PreloadAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        ChvjUnityInfra.CHMResource.Instance.Load<TMP_FontAsset>("Gaegu-Bold SDF", font =>
        {
            _cachedFont = font;
            _cachedMaterial = font != null ? font.material : null;
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    public TMP_FontAsset GetFont() => _cachedFont;
    public Material GetFontMaterial() => _cachedMaterial;
}
```

- [ ] **Step 3: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/GameStringProvider.cs Assets/Scripts/Manager/GameStringProvider.cs.meta Assets/Scripts/Manager/GameFontProvider.cs Assets/Scripts/Manager/GameFontProvider.cs.meta
git commit -m "[Manager] GameStringProvider, GameFontProvider 작성 (패키지 Provider 인터페이스 구현)"
```

---

## Phase 7 — 옵트인 모듈 활성화

### Task 7.1: 패키지 Settings 토글 활성화 + Config 에셋 생성

- [ ] **Step 1: Unity 메뉴에서 Settings 열기**

`Tools > ChvjUnityInfra > Settings` 메뉴 클릭. 창 열림.

- [ ] **Step 2: Ads 토글 + Config 에셋**

Ads 탭에서:
1. `Use Admob ✓` 체크 → ScriptingDefineSymbols에 `UNITY_INFRA_ADS` 추가됨
2. "AdConfig 에셋 편집" 클릭 → `Resources/ChvjUnityInfra/AdConfig.asset` 생성
3. 인스펙터에서 광고 ID 입력:
   - BannerAdUnitId / InterstitialAdUnitId / RewardedAdUnitId (기존 프로젝트의 광고 ID는 사용자에게 확인 — `Assets/Scripts/Manager/CHMAdmob.cs` 내부에 하드코딩되어 있으면 그걸 옮김)
   - UseTestAds = true (개발 단계)

- [ ] **Step 3: IAP 토글 + Config 에셋**

IAP 탭에서:
1. `Use IAP ✓` 체크 → `UNITY_INFRA_IAP` 추가
2. "IAPProductConfig 에셋 편집" → `Resources/ChvjUnityInfra/IAPProductConfig.asset` 생성
3. 인스펙터에서 Products 추가 (3종 소모성: RemoveAD, AddTime, AddMove)
   - productName / productID / productType=Consumable 설정 — 기존 `CHMString`의 IAP 상수 참고

- [ ] **Step 4: Social 토글**

Social 탭에서 `Use GPGS ✓` 체크 → `UNITY_INFRA_SOCIAL` 추가.

- [ ] **Step 5: 컴파일 대기**

Unity 자동 컴파일. Console 에러 확인 — 패키지의 Admob/IAP/GPGS 어셈블리가 컴파일됨.

- [ ] **Step 6: 사용자 커밋 요청**

```powershell
git add Assets/Resources/ChvjUnityInfra ProjectSettings/ProjectSettings.asset
git commit -m "[Settings] UnityInfra Ads/IAP/Social 모듈 활성화 + Config 에셋 생성"
```

### Task 7.2: 게임 측 CHMAdmob/CHMIAP/CHMGPGS 어댑터로 전환

**Files:**
- Modify: `Assets/Scripts/Manager/CHMAdmob.cs`
- Modify: `Assets/Scripts/Manager/CHMIAP.cs`
- Modify: `Assets/Scripts/Manager/CHMGPGS.cs`

- [ ] **Step 1: 기존 CHMAdmob API 확인**

Read: `Assets/Scripts/Manager/CHMAdmob.cs`. 게임 코드에서 호출되는 메서드 grep: `CHMMain\.Admob\.\w+`.

- [ ] **Step 2: CHMAdmob 어댑터로 전환**

```csharp
using GoogleMobileAds.Api;
using UnityEngine;

public class CHMAdmob
{
    public System.Action AcquireReward
    {
        get => ChvjUnityInfra.CHMAdmob.Instance.AcquireReward;
        set => ChvjUnityInfra.CHMAdmob.Instance.AcquireReward = value;
    }

    public System.Action CloseAD
    {
        get => ChvjUnityInfra.CHMAdmob.Instance.CloseAD;
        set => ChvjUnityInfra.CHMAdmob.Instance.CloseAD = value;
    }

    public void Init() => ChvjUnityInfra.CHMAdmob.Instance.Init();
    public void ShowBanner(AdPosition position) => ChvjUnityInfra.CHMAdmob.Instance.ShowBanner(position);
    public void ShowInterstitialAd() => ChvjUnityInfra.CHMAdmob.Instance.ShowInterstitialAd();
    public void ShowRewardedAd() => ChvjUnityInfra.CHMAdmob.Instance.ShowRewardedAd();
}
```

- [ ] **Step 3: CHMIAP 어댑터로 전환**

```csharp
using UnityEngine.Purchasing;

public class CHMIAP
{
    public System.Action<ChvjUnityInfra.CHMIAP.PurchaseState> purchaseState
    {
        get => ChvjUnityInfra.CHMIAP.Instance.purchaseState;
        set => ChvjUnityInfra.CHMIAP.Instance.purchaseState = value;
    }

    public bool IsInitialized => ChvjUnityInfra.CHMIAP.Instance.IsInitialized;
    public void Init() => ChvjUnityInfra.CHMIAP.Instance.Init();
    public void Purchase(string productName) => ChvjUnityInfra.CHMIAP.Instance.Purchase(productName);
    public void RestorePurchase() => ChvjUnityInfra.CHMIAP.Instance.RestorePurchase();
    public bool HadPurchased(string productName) => ChvjUnityInfra.CHMIAP.Instance.HadPurchased(productName);
    public Product GetProduct(string productName) => ChvjUnityInfra.CHMIAP.Instance.GetProduct(productName);
    public decimal GetPrice(string productID) => ChvjUnityInfra.CHMIAP.Instance.GetPrice(productID);
    public string GetPriceUnit(string productID) => ChvjUnityInfra.CHMIAP.Instance.GetPriceUnit(productID);
    public bool CanBuyFromID(string productID) => ChvjUnityInfra.CHMIAP.Instance.CanBuyFromID(productID);
    public bool CanBuyFromName(string productName) => ChvjUnityInfra.CHMIAP.Instance.CanBuyFromName(productName);
}
```

기존 `CHMIAP`이 PurchaseState 타입을 자체 정의했다면 그쪽도 매핑. 호출처 grep으로 확인 후 처리.

- [ ] **Step 4: CHMGPGS 어댑터로 전환**

```csharp
#if UNITY_ANDROID
using System;
using UnityEngine.SocialPlatforms;

public class CHMGPGS
{
    public void Login(Action<bool, ILocalUser> onLoginSuccess = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.Login(onLoginSuccess);

    public void Logout() => ChvjUnityInfra.CHMGPGS.Instance.Logout();

    public void SaveCloud(string fileName, string saveData, Action<bool> onCloudSaved = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.SaveCloud(fileName, saveData, onCloudSaved);

    public void LoadCloud(string fileName, Action<bool, string> onCloudLoaded = null) =>
        ChvjUnityInfra.CHMGPGS.Instance.LoadCloud(fileName, onCloudLoaded);

    // 게임 코드 호출처에 맞춰 나머지 메서드 위임 추가
}
#endif
```

- [ ] **Step 5: CHMMain에서 인스턴스 참조 정리**

`Assets/Scripts/Manager/CHMMain.cs`의 `m_admob`, `m_iap`, `m_gpgs` 필드 정의가 있으면 패키지 Instance를 반환하는 형태로 유지하거나 제거.

- [ ] **Step 6: Play 모드 검증**

GameEnd 화면에서 보상형 광고 호출 (테스트 광고 표시) 확인. 상점에서 IAP 구매 흐름 (실제 결제는 X, IAP 초기화 정상) 확인.

- [ ] **Step 7: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Manager/CHMAdmob.cs Assets/Scripts/Manager/CHMIAP.cs Assets/Scripts/Manager/CHMGPGS.cs Assets/Scripts/Manager/CHMMain.cs
git commit -m "[Manager] CHMAdmob/CHMIAP/CHMGPGS을 패키지 위임 어댑터로 전환"
```

---

## Phase 8 — 일괄 호출 사이트 치환

### Task 8.1: CHMMain.X → CHMX.Instance regex 치환

- [ ] **Step 1: 영향 범위 재확인**

Grep:
```
pattern: CHMMain\.(Resource|Pool|UI|Sound|Json|Data|String|Admob|IAP|GPGS)\b
path: Assets/Scripts
output_mode: files_with_matches
```
Expected: 36개 파일 리스트.

- [ ] **Step 2: 안전 치환 — 매니저 파일 자체는 제외**

`Assets/Scripts/Manager/CHM*.cs`와 `Assets/Scripts/Manager/CHMMain.cs`는 어댑터 내부 호출이라 P9 삭제 시 사라짐. 게임 코드만 치환.

대상 파일 목록 (Step 1 결과)에서 `Assets/Scripts/Manager/` 경로 제외하고 나머지 파일 일괄 처리.

- [ ] **Step 3: 한 파일씩 Edit으로 치환**

각 파일에 대해 Edit 툴로:
- `CHMMain.Resource` → `CHMResource.Instance`
- `CHMMain.Pool` → `CHMPool.Instance`
- `CHMMain.UI` → `CHMUI.Instance`
- `CHMMain.Sound` → `CHMSound.Instance`
- `CHMMain.Json` → `CHMJson.Instance`
- `CHMMain.Data` → `CHMData.Instance`
- `CHMMain.String` → `CHMString.Instance`
- `CHMMain.Admob` → `CHMAdmob.Instance`
- `CHMMain.IAP` → `CHMIAP.Instance`
- `CHMMain.GPGS` → `CHMGPGS.Instance`

각 파일마다 위 10개 패턴을 `replace_all: true`로 처리. 글로벌 어댑터 클래스 이름은 패키지 동명 클래스와 같지만 namespace로 구분되므로 `using ChvjUnityInfra;`는 추가하지 **않음** (P9에서 어댑터 삭제 후 추가).

- [ ] **Step 4: 컴파일 + Play 모드 검증**

전체 시나리오 한 번 돌려서 정상 작동 확인.

- [ ] **Step 5: 사용자 커밋 요청**

```powershell
git add Assets/Scripts
git commit -m "[Refactor] 게임 코드 247개 호출 사이트 CHMMain.X → CHMX.Instance 치환"
```

### Task 8.2: Resource 헬퍼 메서드 시그니처 변경

- [ ] **Step 1: 어댑터 변환 확인**

`Assets/Scripts/Manager/CHMResource.cs`는 P2에서 `LoadJson/LoadSprite/LoadSound/InstantiateUI/InstantiateEffect/LoadFont/LoadData` 헬퍼를 보존했으므로 호출처 변경 불요. **이 단계는 P9에서 어댑터 삭제 후 변경**.

따라서 이 Task는 **placeholder**이며 P9에 통합됨. 별도 커밋 없음.

### Task 8.3: ResourceDownload 씬 단순화

**Files:**
- Modify: `Assets/Scripts/Scenes/ResourceDownload.cs`

- [ ] **Step 1: 파일 재작성**

```csharp
using ChvjUnityInfra;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResourceDownload : MonoBehaviour
{
    [SerializeField] List<Image> backgroundList = new();
    CancellationTokenSource tokenSource;
    int backgroundIndex = 0;

    private async void Start()
    {
        tokenSource = new CancellationTokenSource();
        _ = ChangeBackgroundLoop();

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

    async Task ChangeBackgroundLoop()
    {
        // 기존 코드 그대로
    }

    int ChangeBackground()
    {
        // 기존 코드 그대로
    }
}
```

기존의 `CHLoadingBarFromAssetBundle` 의존성 완전 제거.

- [ ] **Step 2: 씬에서 CHLoadingBarFromAssetBundle GameObject 제거**

Unity Hierarchy에서 ResourceDownloadScene 열고 CHLoadingBarFromAssetBundle 컴포넌트가 붙은 GameObject 제거 (혹은 GameObject 자체 비활성화).

- [ ] **Step 3: Play 모드 검증**

ResourceDownloadScene부터 정상 시작 → 로비 진입 → 스테이지 선택 → 게임 진입까지 풀 사이클 확인.

- [ ] **Step 4: 사용자 커밋 요청**

```powershell
git add Assets/Scripts/Scenes/ResourceDownload.cs Assets/Scenes
git commit -m "[Scene] ResourceDownload를 ChvjUnityInfraSDK.Initialize 부트스트랩으로 단순화"
```

---

## Phase 9 — 정리

### Task 9.1: 어댑터 매니저 파일 삭제 + using 추가

- [ ] **Step 1: 게임 코드에 using ChvjUnityInfra; 추가**

P8에서 변경한 36개 파일 각각 상단에 `using ChvjUnityInfra;` 추가. 한 파일씩 Edit.

(글로벌 어댑터를 곧 삭제하므로 이 시점에 패키지 namespace를 import해야 컴파일 가능.)

- [ ] **Step 2: 어댑터 파일 삭제**

Run:
```powershell
git rm Assets/Scripts/Manager/CHMMain.cs Assets/Scripts/Manager/CHMMain.cs.meta
git rm Assets/Scripts/Manager/CHMResource.cs Assets/Scripts/Manager/CHMResource.cs.meta
git rm Assets/Scripts/Manager/CHMPool.cs Assets/Scripts/Manager/CHMPool.cs.meta
git rm Assets/Scripts/Manager/CHMSound.cs Assets/Scripts/Manager/CHMSound.cs.meta
git rm Assets/Scripts/Manager/CHMUI.cs Assets/Scripts/Manager/CHMUI.cs.meta
git rm Assets/Scripts/Manager/CHMAssetBundle.cs Assets/Scripts/Manager/CHMAssetBundle.cs.meta
git rm Assets/Scripts/Manager/CHMAdmob.cs Assets/Scripts/Manager/CHMAdmob.cs.meta
git rm Assets/Scripts/Manager/CHMIAP.cs Assets/Scripts/Manager/CHMIAP.cs.meta
git rm Assets/Scripts/Manager/CHMGPGS.cs Assets/Scripts/Manager/CHMGPGS.cs.meta
git rm Assets/Scripts/UI/UIBase.cs Assets/Scripts/UI/UIBase.cs.meta
```

CHMJson/CHMString/CHMData는 P6에서 재작성됐으므로 삭제 대상 X. UIBase는 패키지 ChvjUnityInfra.UIBase 사용으로 전환되므로 게임 측 어댑터 삭제.

- [ ] **Step 3: 호출처에서 UIBase 사용 부분 처리**

UIBase 어댑터가 삭제되면 `Defines.EUI eUIType`, `int uid` 같은 게임 측 속성도 사라짐. 게임 코드에서 이를 사용하는 곳:

Grep:
```
pattern: \.eUIType|\.uid
path: Assets/Scripts
```

발견된 곳에서:
- `.eUIType` → `(Defines.EUI)UIType` 또는 적절한 캐스팅
- `.uid` 사용처는 더 이상 의미 없음 (CHMUI는 uid 미반환) → 호출처 코드 리팩터

또는 P4에서 만든 글로벌 UIBase 어댑터를 유지하되 패키지 UIBase 상속만 변경하는 방식이 더 안전. 영향 크면 글로벌 UIBase 어댑터 보존 결정.

- [ ] **Step 4: Function/ 컴포넌트 삭제**

```powershell
git rm Assets/Scripts/Function/CHButton.cs Assets/Scripts/Function/CHButton.cs.meta
git rm Assets/Scripts/Function/CHTMPro.cs Assets/Scripts/Function/CHTMPro.cs.meta
git rm Assets/Scripts/Function/CHToggle.cs Assets/Scripts/Function/CHToggle.cs.meta
git rm Assets/Scripts/Function/CHLoadingBarFromAssetBundle.cs Assets/Scripts/Function/CHLoadingBarFromAssetBundle.cs.meta
```

CHPoolable은 게임/패키지 둘 다 있을 수 있어 P3에서 결정한 대로 유지/삭제. CHGaugeBar, CHPurchaseButton, CHAdvertise, CHDebugLog 등 게임 특화 컴포넌트는 유지.

- [ ] **Step 5: Editor 툴 삭제**

```powershell
git rm Assets/Scripts/Editor/AssetBundleMenuItem.cs Assets/Scripts/Editor/AssetBundleMenuItem.cs.meta
```

마이그레이션 툴(CHToolMigrate*)은 유지 — 향후 추가 마이그레이션 시 재사용 가능.

- [ ] **Step 6: 컴파일 + Play 모드 풀 회귀 검증**

전체 시나리오 한 번 더:
- ResourceDownload → LBLobby → 스테이지 선택 → GameScene → 매치 1회 → 클리어 화면
- 각 UI (설정, 미션, 상점, 랭크, 닉네임, 데이터 삭제) 진입/닫기
- 광고/IAP 흐름 확인

- [ ] **Step 7: 사용자 커밋 요청**

```powershell
git add Assets/Scripts
git commit -m "[Cleanup] 어댑터 매니저/Function 컴포넌트/AssetBundle 빌드툴 삭제 및 using ChvjUnityInfra 적용"
```

### Task 9.2: AssetBundle 라벨 잔재 제거

- [ ] **Step 1: AssetBundle 라벨 일괄 제거 (Task 1.1에서 이미 처리됐을 가능성)**

확인: Project 창에서 임의의 `AssetBundleResources/ui/*.prefab` 파일 우클릭 → Inspector 하단 AssetBundle 표기가 "None"인지. 만약 잔재가 있으면 다음 메뉴로 일괄 제거:

```csharp
// Assets/Scripts/Editor/CHToolMigrateToAddressables.cs에 추가
[MenuItem("CatPang/Clear AssetBundle Labels", priority = 130)]
public static void ClearAssetBundleLabels()
{
    var guids = AssetDatabase.FindAssets("", new[] { "Assets/AssetBundleResources" });
    int cleared = 0;
    foreach (var guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        var importer = AssetImporter.GetAtPath(path);
        if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
        {
            importer.assetBundleName = "";
            cleared++;
        }
    }
    AssetDatabase.SaveAssets();
    Debug.Log($"[Clear AB Labels] {cleared}개 제거.");
}
```

메뉴 실행.

- [ ] **Step 2: Build 디렉토리 정리**

```powershell
Remove-Item -Recurse -Force AssetBundles -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force Assets/StreamingAssets/AssetBundles -ErrorAction SilentlyContinue
```

`Assets/StreamingAssets/build_info`도 사용 안 하면 제거 — 그러나 다른 곳에서 참조하는지 확인 후 결정.

- [ ] **Step 3: 사용자 커밋 요청**

```powershell
git add Assets
git commit -m "[Cleanup] AssetBundle 라벨/산출물 잔재 제거"
```

### Task 9.3: 최종 Android Player 빌드 + 디바이스 검증

- [ ] **Step 1: Player Settings 확인**

`File > Build Settings`에서 Android 플랫폼 선택. Scenes In Build에 ResourceDownloadScene, FirstScene, GameScene 등 순서 확인.

- [ ] **Step 2: Build And Run**

`Build And Run` 또는 `Build`. 산출물 APK 확인.

- [ ] **Step 3: 디바이스 실기 테스트**

설치된 APK 실행:
- 첫 화면(ResourceDownload) 정상 로딩
- 로비 진입, GPGS 로그인 표시
- 스테이지 1 클리어
- 보상형 광고 표시 (테스트 광고)
- IAP 초기화 정상 (Google Play Console 테스트 계정 필요)
- 사운드/폰트 정상

- [ ] **Step 4: 사용자 커밋 요청 (최종)**

```powershell
git add .
git commit -m "[Migration] UnityInfra 패키지 적용 + Addressables 마이그레이션 완료"
```

(잔여 변경 있을 때만)

---

## 검증 체크리스트 — 마이그레이션 완료 기준

- [ ] Editor Play 모드 풀 사이클: ResourceDownload → LBLobby → 스테이지 선택 → GameScene → 매치 1회 → 클리어
- [ ] 모든 UI 14종 정상 표시 + 닫기 (배경/뒤로가기/ESC)
- [ ] 사운드 (Bgm 루프, Cat 효과음, 기타 효과음) 정상
- [ ] 폰트 표시 정상 (한국어 텍스트)
- [ ] 옵션 변경 후 PlayerPrefs 영구 저장 (BGM 볼륨 등)
- [ ] Android Player 빌드 성공
- [ ] 디바이스 실기 테스트 — 첫 실행부터 로비 진입까지 정상
- [ ] IAP 초기화 정상 (제품 목록 로드)
- [ ] 보상형 광고 1회 정상 (테스트)
- [ ] `Assets/Scripts/Manager/`에 `CHMJson.cs`, `CHMData.cs`, `CHMString.cs`, `GameStringProvider.cs`, `GameFontProvider.cs`만 남음 (기존 9개 매니저 + CHMAssetBundle 삭제됨)
- [ ] `Assets/Scripts/Function/`에 CHButton/CHTMPro/CHToggle/CHLoadingBarFromAssetBundle 없음
- [ ] `Assets/Scripts/Editor/`에 AssetBundleMenuItem.cs 없음
- [ ] Addressables Groups에 UI/Unit/Effect/Sprite/Sound/Font/Data/Json 그룹 + 모든 에셋이 "Resource" 라벨

---

## 리스크 대응

| 리스크 | 대응 |
|---|---|
| P2~P3 어댑터 작성 후 검증 실패 | P1 직후 커밋으로 롤백 가능. `git revert <P2-P3 SHA>` |
| P4 UI 시스템 차이로 인한 회귀 | Task 4.1의 사전 grep + 호출처 리팩터로 사전 처리. 별도 캔버스/uid 패턴 발견 시 P4 진입 전 해소 |
| P5 프리합 자동 재바인딩 실패 | `Library/CatPangPrefabBackup/{ts}/`에서 복원 |
| P8 일괄 치환 오류 | 한 파일씩 변경 후 컴파일 확인. `git diff` 검토 |
| 패키지 CHText API 부족 (SetStringID 등) | Task 5.1에서 게임 측 확장 메서드로 보충 |
| Addressables 그룹 빌드 누락 | P1 직후 Addressables Build 실행, Play 모드에서 에셋 로딩 검증 |
