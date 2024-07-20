using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Infomation;

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
        string directory = "Assets/StreamingAssets";

        if (Directory.Exists(directory) == false)
        {
            Directory.CreateDirectory(directory);
        }

        BuildPipeline.BuildAssetBundles(directory, BuildAssetBundleOptions.None, BuildTarget.Android);
    }

    /*[Serializable]
    public class Temp
    {
        public List<StageInfo> stageList = new List<StageInfo>();
    }

    public static Temp stageData = new Temp();

    [MenuItem("CHTools/StageJson")]
    public static void CreateStageJson()
    {
        GenerateStageData();
        SaveStageDataToJson();
    }

    public static void GenerateStageData()
    {
        int group = 1;
        for (int stage = 1; stage <= 100; stage++)
        {
            StageInfo data = new StageInfo
            {
                group = group,
                stage = stage,
                blockTypeCount = 5,
                boardSize = 9,
                time = 180,
                targetScore = 100,
                moveCount = -1
            };

            stageData.stageList.Add(data);

            if (stage % 9 == 0)
                group++;
        }
    }

    public static void SaveStageDataToJson()
    {
        string jsonData = JsonUtility.ToJson(stageData, true);

        // JSON 파일로 저장 (Assets 폴더 내에 저장됨)
        string filePath = Path.Combine(Application.dataPath + "/AssetBundleResources/json", "stages.json");
        File.WriteAllText(filePath, jsonData);

        Debug.Log("JSON 데이터 생성이 완료되었습니다.");
    }*/
}
