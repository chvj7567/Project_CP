using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CHToolGameView
{
    const int WIDTH = 720;
    const int HEIGHT = 1280;
    const string LABEL = "720x1280";

    [MenuItem("CatPang/게임뷰 720×1280 설정")]
    public static void Apply()
    {
        Assembly assembly = typeof(Editor).Assembly;
        Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singletonType.GetProperty("instance").GetValue(null);

        object group = sizesType.GetMethod("GetGroup")
            .Invoke(sizes, new object[] { (int)GameViewSizeGroupType.Android });

        Type groupType = group.GetType();
        Type sizeType = assembly.GetType("UnityEditor.GameViewSize");
        PropertyInfo wProp = sizeType.GetProperty("width");
        PropertyInfo hProp = sizeType.GetProperty("height");
        int total = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);

        int targetIndex = -1;
        for (int i = 0; i < total; i++)
        {
            object s = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
            if ((int)wProp.GetValue(s) == WIDTH && (int)hProp.GetValue(s) == HEIGHT)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
        {
            Type gvstType = assembly.GetType("UnityEditor.GameViewSizeType");
            ConstructorInfo ctor = sizeType.GetConstructor(new[] { gvstType, typeof(int), typeof(int), typeof(string) });
            object newSize = ctor.Invoke(new object[] { Enum.Parse(gvstType, "FixedResolution"), WIDTH, HEIGHT, LABEL });
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
            targetIndex = total;
        }

        Type gameViewType = assembly.GetType("UnityEditor.GameView");
        EditorWindow gv = EditorWindow.GetWindow(gameViewType);
        gameViewType.GetMethod("SizeSelectionCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(gv, new object[] { targetIndex, null });

        Debug.Log($"[CatPang] Game View → {WIDTH}×{HEIGHT}");
    }
}
