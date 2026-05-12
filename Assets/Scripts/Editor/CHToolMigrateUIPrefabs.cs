using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CHToolMigrateUIPrefabs
{
    private const string BackupDir = "Library/CatPangPrefabBackup";

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
