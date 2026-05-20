using UnityEditor;
using UnityEngine;
using ChvjUnityInfra;

// 미션 보상 버튼(btnAcquire)의 라벨 텍스트에 CHText를 붙이고
// MissionScrollViewItem.rewardBtnText 필드에 연결하는 일회성 마이그레이션 도구.
// 광고 시청 미션의 "보기 → 받기" 라벨 전환에 필요.
// 두 번 실행해도 안전(idempotent).
public static class CHToolWireMissionAdButton
{
    const string UIMissionPath = "Assets/AssetBundleResources/ui/UIMission.prefab";
    const string TextPath = "Scroll View/Viewport/Origin/btnAcquire/Text (TMP)";
    const string ItemPath = "Scroll View/Viewport/Origin";

    [MenuItem("CatPang/Wire Mission Ad Button", priority = 102)]
    public static void Wire()
    {
        var root = PrefabUtility.LoadPrefabContents(UIMissionPath);
        try
        {
            var textTr = root.transform.Find(TextPath);
            if (textTr == null)
                throw new System.Exception($"[WireMissionAdButton] 경로 없음: {TextPath}");

            var ch = textTr.GetComponent<CHText>();
            if (ch == null)
            {
                ch = textTr.gameObject.AddComponent<CHText>();
                Debug.Log("[WireMissionAdButton] btnAcquire 라벨에 CHText 추가");
            }

            var itemTr = root.transform.Find(ItemPath);
            var item = itemTr != null ? itemTr.GetComponent<MissionScrollViewItem>() : null;
            if (item == null)
                throw new System.Exception($"[WireMissionAdButton] MissionScrollViewItem 없음: {ItemPath}");

            var so = new SerializedObject(item);
            var prop = so.FindProperty("rewardBtnText");
            if (prop == null)
                throw new System.Exception("[WireMissionAdButton] rewardBtnText 필드 없음");

            prop.objectReferenceValue = ch;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, UIMissionPath);
            Debug.Log("[WireMissionAdButton] 완료 — rewardBtnText 연결됨");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
