using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AssetBundleMenuItem
{
    [MenuItem("CHTools/CreateAssetBundleNameJson")]
    public static void CreateAssetBundleNameJson()
    {
        List<string> listAssetBundleName = AssetDatabase.GetAllAssetBundleNames().ToList();

        AssetBundleData assetBundleData = new AssetBundleData();

        foreach (string name in listAssetBundleName)
        {
            assetBundleData.listAssetBundleInfo.Add(new AssetBundleInfo(name));
        }

        string json = JsonUtility.ToJson(assetBundleData);

        File.WriteAllText($"{Application.dataPath}/Resources/AssetBundleName.json", json);
    }

    [MenuItem("CHTools/AssetBundleBuild Windows")]
    public static void AssetBundleBuildWindows()
    {
        string directory = "Assets/Bundle";

        if (Directory.Exists(directory) == false)
        {
            Directory.CreateDirectory(directory);
        }

        BuildPipeline.BuildAssetBundles(directory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    [MenuItem("CHTools/AssetBundleBuild Android")]
    public static void AssetBundleBuildAndroid()
    {
        string directory = "Assets/Bundle";

        if (Directory.Exists(directory) == false)
        {
            Directory.CreateDirectory(directory);
        }

        BuildPipeline.BuildAssetBundles(directory, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
