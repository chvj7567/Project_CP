#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ChvjUnityInfra;

// 일일 미션 도입 시 UIMission.prefab의 신규 SerializeField (dailyTapBtn / resetTimerText / offlineLockObj)
// 및 잘못 매칭된 specialTapBtn 참조를 자동 연결하는 일회성 Editor 유틸.
// 메뉴: CatPang > Daily Mission > UIMission 프리팹 자동 연결
// 또는 정적 메서드 DailyMissionPrefabSetup.WireUp()을 직접 호출.
public static class DailyMissionPrefabSetup
{
    const string PrefabPath = "Assets/AssetBundleResources/ui/UIMission.prefab";

    [MenuItem("CatPang/Daily Mission/UIMission 프리팹 자동 연결")]
    public static void WireUp()
    {
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[DailyMissionPrefabSetup] 프리팹 로드 실패: {PrefabPath}");
            return;
        }

        try
        {
            var ui = prefab.GetComponent<UIMission>();
            if (ui == null)
            {
                Debug.LogError("[DailyMissionPrefabSetup] UIMission 컴포넌트 없음");
                return;
            }

            var so = new SerializedObject(ui);

            // 1) specialTapBtn 복구 — btnSpecialMission으로
            var btnSpecial = FindChild(prefab.transform, "btnSpecialMission");
            var btnDaily = FindChild(prefab.transform, "btnDailyMission");
            var resetTimer = FindChild(prefab.transform, "ResetTimerText");
            var offlineLock = FindChild(prefab.transform, "OfflineLockObj");

            if (btnSpecial == null) { Debug.LogError("[DailyMissionPrefabSetup] btnSpecialMission 찾을 수 없음"); return; }
            if (btnDaily == null) { Debug.LogError("[DailyMissionPrefabSetup] btnDailyMission 찾을 수 없음"); return; }
            if (resetTimer == null) { Debug.LogError("[DailyMissionPrefabSetup] ResetTimerText 찾을 수 없음"); return; }
            if (offlineLock == null) { Debug.LogError("[DailyMissionPrefabSetup] OfflineLockObj 찾을 수 없음"); return; }

            so.FindProperty("specialTapBtn").objectReferenceValue = btnSpecial.GetComponent<Button>();
            so.FindProperty("dailyTapBtn").objectReferenceValue = btnDaily.GetComponent<Button>();
            so.FindProperty("resetTimerText").objectReferenceValue = resetTimer.GetComponent<CHText>();
            so.FindProperty("offlineLockObj").objectReferenceValue = offlineLock.gameObject;

            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
            Debug.Log("[DailyMissionPrefabSetup] UIMission 프리팹 필드 연결 완료");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }
    }

    static Transform FindChild(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; ++i)
        {
            var found = FindChild(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    // PlayMode 검증용 — UIMission을 직접 ShowUI로 열기.
    public static void ShowUIMissionInPlay()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[DailyMissionPrefabSetup] PlayMode가 아님");
            return;
        }
        CHMMain.UI.ShowUI(Defines.EUI.UIMission, new UIMissionArg());
        Debug.Log("[DailyMissionPrefabSetup] UIMission ShowUI 요청 완료");
    }

    // PlayMode 검증용 — 일일 미션 카운터 강제 증가 (스테이지 클리어 시뮬레이션)
    public static void SimulateStageClear()
    {
        if (!Application.isPlaying) return;
        DailyMissionService.OnStageClear();
        Debug.Log($"[DailyMissionPrefabSetup] OnStageClear 호출 완료. stageClearCountToday={CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).stageClearCountToday}");
    }

    public static void SimulateBlockDestroyed10()
    {
        if (!Application.isPlaying) return;
        DailyMissionService.OnBlockDestroyed(10);
        Debug.Log($"[DailyMissionPrefabSetup] OnBlockDestroyed(10) 호출 완료. blockDestroyCountToday={CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).blockDestroyCountToday}");
    }

    public static void PrintLoginState()
    {
        if (!Application.isPlaying) return;
        var login = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        Debug.Log($"[DailyMissionPrefabSetup] Login State: lastDailyResetDateKey={login.lastDailyResetDateKey}, attendanceTodayDone={login.attendanceTodayDone}, stageClearCountToday={login.stageClearCountToday}, blockDestroyCountToday={login.blockDestroyCountToday}, adWatchCountToday={login.adWatchCountToday}");
    }
}
#endif
