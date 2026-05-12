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

    [MenuItem("CatPang/Build Addressables", priority = 99)]
    public static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log("[Addressables] Build completed.");
    }

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
