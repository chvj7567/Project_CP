using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 신규 고양이 스킨 6종(Crown/Flowers/Mushroom/Party/Santa/Strawberry)을
// 상점 UI 프리팹(UIShop.prefab)에 연결하는 마이그레이션 도구.
// MCP component_set이 Sprite 에셋 참조를 설정하지 못해 에디터 스크립트로 처리한다.
// 두 번 실행해도 안전(idempotent): 신규 오브젝트는 이름으로 찾아 없을 때만 생성한다.
public static class CHToolWireSkinShop
{
    const string UIShopPath = "Assets/AssetBundleResources/ui/UIShop.prefab";
    const string SpriteDir = "Assets/AssetBundleResources/sprite/";

    // 인덱스 = skinIndex(selectCatShop). 0=없음, 1~6=테마. 대표 스프라이트는 Cat1 기준
    static readonly string[] SkinSprite =
    {
        null, "CatCrown1", "CatFlowers1", "CatMushroom1",
        "CatParty1", "CatSanta1", "CatStrawberry1"
    };

    [MenuItem("CatPang/Wire Skin Shop", priority = 101)]
    public static void Wire()
    {
        var root = PrefabUtility.LoadPrefabContents(UIShopPath);
        try
        {
            WireSkinPreview(root);
            WireShopItems(root);
            PrefabUtility.SaveAsPrefabAsset(root, UIShopPath);
            Debug.Log("[WireSkinShop] 완료.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // UIShop.skinImgList: 스킨 미리보기 4개 → 7개
    static void WireSkinPreview(GameObject root)
    {
        var skins = RequireChild(root.transform, "SkinBox/Skins");
        var uiShop = root.GetComponent<UIShop>();

        // 기존 4개를 이름으로 고정 (순서 = skinIndex 0~3)
        var list = new List<GameObject>
        {
            RequireChild(skins, "Image (1)").gameObject,
            RequireChild(skins, "Image (3)").gameObject,
            RequireChild(skins, "Image (4)").gameObject,
            RequireChild(skins, "Image (7)").gameObject,
        };

        // 신규 3개 (skinIndex 4~6) — 없으면 Image (3) 복제
        var template = list[1];
        for (int i = 4; i <= 6; ++i)
            list.Add(GetOrClone(skins, $"Image (Skin{i})", template));

        // 스프라이트 설정 (skinIndex 1~6, 0은 기본 고양이라 유지)
        for (int i = 1; i < list.Count; ++i)
            SetSprite(list[i], SkinSprite[i]);

        WireList(uiShop, "skinImgList", list);
        Debug.Log($"[WireSkinShop] skinImgList {list.Count}개 연결");
    }

    // ShopScrollViewItem.shopImgList: 상품 이미지 9개 → 12개
    static void WireShopItems(GameObject root)
    {
        var origin = RequireChild(root.transform, "Scroll View/Viewport/Origin");
        var imgItems = RequireChild(origin, "imgItems");
        var item = origin.GetComponent<ShopScrollViewItem>();

        // 기존 9개 (인덱스 = shopID 0~8)
        var list = new List<GameObject>();
        foreach (var n in new[] { "RemoveAD", "NormalCat", "Skin1Cat", "Skin2Cat",
                                  "TimeItem", "MoveItem", "Skin3Cat", "HP", "Attack" })
            list.Add(RequireChild(imgItems, n).gameObject);

        // 신규 3개 (shopID 9~11) — Skin1Cat 복제
        var template = RequireChild(imgItems, "Skin1Cat").gameObject;
        foreach (var n in new[] { "Skin4Cat", "Skin5Cat", "Skin6Cat" })
            list.Add(GetOrClone(imgItems, n, template));

        // 스킨 상품 스프라이트 (shopID 인덱스 → skinIndex 매핑)
        SetSprite(list[2], SkinSprite[1]);   // Skin1Cat  - Crown
        SetSprite(list[3], SkinSprite[2]);   // Skin2Cat  - Flowers
        SetSprite(list[6], SkinSprite[3]);   // Skin3Cat  - Mushroom
        SetSprite(list[9], SkinSprite[4]);   // Skin4Cat  - Party
        SetSprite(list[10], SkinSprite[5]);  // Skin5Cat  - Santa
        SetSprite(list[11], SkinSprite[6]);  // Skin6Cat  - Strawberry

        WireList(item, "shopImgList", list);
        Debug.Log($"[WireSkinShop] shopImgList {list.Count}개 연결");
    }

    static Transform RequireChild(Transform parent, string path)
    {
        var t = parent.Find(path);
        if (t == null)
            throw new System.Exception($"[WireSkinShop] 경로를 찾을 수 없음: {parent.name}/{path}");
        return t;
    }

    static GameObject GetOrClone(Transform parent, string name, GameObject template)
    {
        var exist = parent.Find(name);
        if (exist != null)
            return exist.gameObject;

        var go = Object.Instantiate(template, parent);
        go.name = name;
        return go;
    }

    static Sprite Load(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + name + ".png");
        if (sp == null)
            Debug.LogError($"[WireSkinShop] 스프라이트 없음: {name}");
        return sp;
    }

    static void SetSprite(GameObject go, string spriteName)
    {
        var img = go.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError($"[WireSkinShop] Image 컴포넌트 없음: {go.name}");
            return;
        }

        var sp = Load(spriteName);
        if (sp == null)
            return;

        img.sprite = sp;
        Debug.Log($"[WireSkinShop] {go.name} ← {spriteName}");
    }

    static void WireList(Object component, string fieldName, List<GameObject> items)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        prop.arraySize = items.Count;
        for (int i = 0; i < items.Count; ++i)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        so.ApplyModifiedProperties();
    }
}
