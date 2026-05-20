using UnityEditor;
using UnityEngine;
using ChvjUnityInfra;

// 미션 행에 보상 개수 텍스트(x10 형식)를 추가하고
// MissionScrollViewItem.rewardCountText 필드에 연결하는 일회성 도구.
// MissionValue(CHText)를 복제해 iconReward 아래에 배치한다. 두 번 실행해도 안전.
public static class CHToolWireMissionRewardCount
{
    const string UIMissionPath = "Assets/AssetBundleResources/ui/UIMission.prefab";
    const string OriginPath = "Scroll View/Viewport/Origin";

    [MenuItem("CatPang/Wire Mission Reward Count", priority = 103)]
    public static void Wire()
    {
        var root = PrefabUtility.LoadPrefabContents(UIMissionPath);
        try
        {
            var origin = root.transform.Find(OriginPath);
            if (origin == null)
                throw new System.Exception($"[WireRewardCount] 경로 없음: {OriginPath}");

            var iconReward = origin.Find("iconReward");
            var missionValue = origin.Find("MissionValue");
            if (iconReward == null || missionValue == null)
                throw new System.Exception("[WireRewardCount] iconReward 또는 MissionValue 없음");

            var existing = iconReward.Find("RewardCount");
            GameObject countGO;
            if (existing != null)
            {
                countGO = existing.gameObject;
            }
            else
            {
                // MissionValue(CHText)를 복제해 스타일 상속, iconReward 아래로 배치
                countGO = Object.Instantiate(missionValue.gameObject, iconReward);
                countGO.name = "RewardCount";

                var rt = countGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -34f);
                rt.sizeDelta = new Vector2(80f, 28f);

                Debug.Log("[WireRewardCount] RewardCount 텍스트 생성");
            }

            var ch = countGO.GetComponent<CHText>();
            if (ch == null)
                throw new System.Exception("[WireRewardCount] 복제본에 CHText 없음");

            // SetText가 인자를 이어붙이도록 stringID 모드 해제 (_stringID = -1)
            var chSo = new SerializedObject(ch);
            chSo.FindProperty("_stringID").intValue = -1;
            chSo.ApplyModifiedProperties();

            var item = origin.GetComponent<MissionScrollViewItem>();
            if (item == null)
                throw new System.Exception("[WireRewardCount] MissionScrollViewItem 없음");

            var so = new SerializedObject(item);
            var prop = so.FindProperty("rewardCountText");
            if (prop == null)
                throw new System.Exception("[WireRewardCount] rewardCountText 필드 없음");

            prop.objectReferenceValue = ch;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, UIMissionPath);
            Debug.Log("[WireRewardCount] 완료 — rewardCountText 연결됨");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
