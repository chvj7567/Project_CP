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
        var assembly = typeof(Editor).Assembly;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes = singletonType.GetProperty("instance").GetValue(null);

        var group = sizesType.GetMethod("GetGroup")
            .Invoke(sizes, new object[] { (int)GameViewSizeGroupType.Android });

        var groupType = group.GetType();
        var sizeType = assembly.GetType("UnityEditor.GameViewSize");
        var wProp = sizeType.GetProperty("width");
        var hProp = sizeType.GetProperty("height");
        int total = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);

        int targetIndex = -1;
        for (int i = 0; i < total; i++)
        {
            var s = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
            if ((int)wProp.GetValue(s) == WIDTH && (int)hProp.GetValue(s) == HEIGHT)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
        {
            var gvstType = assembly.GetType("UnityEditor.GameViewSizeType");
            var ctor = sizeType.GetConstructor(new[] { gvstType, typeof(int), typeof(int), typeof(string) });
            var newSize = ctor.Invoke(new object[] { Enum.Parse(gvstType, "FixedResolution"), WIDTH, HEIGHT, LABEL });
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
            targetIndex = total;
        }

        var gameViewType = assembly.GetType("UnityEditor.GameView");
        var gv = EditorWindow.GetWindow(gameViewType);
        gameViewType.GetMethod("SizeSelectionCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(gv, new object[] { targetIndex, null });

        Debug.Log($"[CatPang] Game View → {WIDTH}×{HEIGHT}");
    }
}
