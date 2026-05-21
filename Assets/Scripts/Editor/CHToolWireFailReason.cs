using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChvjUnityInfra;

// UIGameEnd 프리팹의 실패 사유 위젯 참조를 재와이어링하는 에디터 툴.
// 계층을 재구성하면 SerializeField 참조가 끊기므로, 메뉴를 다시 실행해 복구한다.
// Modal/Failed 하위에서 이름으로 재귀 탐색하므로 위치를 옮겨도 동작한다.
public static class CHToolWireFailReason
{
    const string PrefabPath = "Assets/AssetBundleResources/ui/UIGameEnd.prefab";

    [MenuItem("CatPang/Wire FailReason Prefab")]
    public static void Wire()
    {
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Transform failed = root.transform.Find("Modal/Failed");
            if (failed == null) { Debug.LogError("[WireFailReason] Modal/Failed 없음"); return; }

            Transform failReason2 = FindByName(failed, "FailReason2");
            Transform reasonText  = FindByName(failed, "FailReasonText");
            Transform detailText  = FindByName(failed, "FailDetailText");
            Transform headerText  = FindByName(failed, "FailBlockHeaderText");
            Transform container   = FindByName(failed, "FailBlockIconContainer");
            Transform template    = FindByName(failed, "FailBlockIconTemplate");
            Transform icon        = template != null ? FindByName(template, "Icon") : null;
            Transform countText   = template != null ? FindByName(template, "CountText") : null;

            if (failReason2 == null || reasonText == null || detailText == null || headerText == null ||
                container == null || template == null || icon == null || countText == null)
            {
                Debug.LogError($"[WireFailReason] 오브젝트 누락: FailReason2={failReason2} " +
                               $"reason={reasonText} detail={detailText} header={headerText} " +
                               $"container={container} template={template} icon={icon} count={countText}");
                return;
            }

            // 실패 사유 텍스트 4개 CHText 보강 (없으면 추가)
            CHText reasonCH = EnsureCHText(reasonText);
            CHText headerCH = EnsureCHText(headerText);
            CHText detailCH = EnsureCHText(detailText);
            CHText countCH  = EnsureCHText(countText);

            var ui = root.GetComponent<UIGameEnd>();
            if (ui == null) { Debug.LogError("[WireFailReason] UIGameEnd 컴포넌트 없음"); return; }

            var soUI = new SerializedObject(ui);
            SetRef(soUI, "failReasonText",         reasonCH);
            SetRef(soUI, "failBlockRoot",          failReason2.gameObject);
            SetRef(soUI, "failBlockHeaderText",    headerCH);
            SetRef(soUI, "failBlockIconContainer", container);
            SetRef(soUI, "failBlockIconTemplate",  template.GetComponent<FailBlockIconItem>());
            SetRef(soUI, "failDetailText",         detailCH);
            soUI.ApplyModifiedProperties();

            var item = template.GetComponent<FailBlockIconItem>();
            if (item == null) { Debug.LogError("[WireFailReason] FailBlockIconItem 컴포넌트 없음"); return; }

            var soItem = new SerializedObject(item);
            SetRef(soItem, "icon",      icon.GetComponent<Image>());
            SetRef(soItem, "countText", countCH);
            soItem.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("[WireFailReason] 완료 — CHText 보강 + 참조 8개 연결");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // 하위 트리에서 이름으로 첫 매치를 재귀 탐색
    static Transform FindByName(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; ++i)
        {
            var found = FindByName(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    static CHText EnsureCHText(Transform t)
    {
        var ch = t.GetComponent<CHText>();
        if (ch == null) ch = t.gameObject.AddComponent<CHText>();
        return ch;
    }

    static void SetRef(SerializedObject so, string prop, Object val)
    {
        var p = so.FindProperty(prop);
        if (p == null) { Debug.LogError($"[WireFailReason] 프로퍼티 '{prop}' 없음"); return; }
        if (val == null) { Debug.LogError($"[WireFailReason] '{prop}' 값이 null"); return; }
        p.objectReferenceValue = val;
    }
}
