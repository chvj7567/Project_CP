using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CHTool
{
    [Serializable]
    public class StageInfoJson
    {
        public List<Infomation.StageInfo> stageList = new List<Infomation.StageInfo>();
    }

    [Serializable]
    public class StageBlockInfoJson
    {
        public List<Infomation.StageBlockInfo> stageBlockList = new List<Infomation.StageBlockInfo>();
    }

    [Serializable]
    public class StringKoreaInfoJson
    {
        public List<Infomation.StringInfo> stringList = new List<Infomation.StringInfo>();
    }

    [Serializable]
    public class StringEnglishInfoJson
    {
        public List<Infomation.StringInfo> stringList = new List<Infomation.StringInfo>();
    }

    public static void LoadAssetOnEditor<T>(string _bundleName, string _assetName, Action<T> _callback) where T : UnityEngine.Object
    {
        string path = null;

        if (typeof(T) == typeof(GameObject))
        {
            path = $"Assets/AssetBundleResources/{_bundleName.ToLower()}/{_assetName}.prefab";
        }
        else if (typeof(T) == typeof(TextAsset))
        {
            path = $"Assets/AssetBundleResources/{_bundleName.ToLower()}/{_assetName}.json";
        }
        else if (typeof(T) == typeof(Sprite))
        {
            path = $"Assets/AssetBundleResources/{_bundleName.ToLower()}/{_assetName}.jpg";

            T temp = AssetDatabase.LoadAssetAtPath<T>(path);

            if (temp == null)
            {
                path = $"Assets/AssetBundleResources/{_bundleName.ToLower()}/{_assetName}.png";
            }
        }
        else if (typeof(T) == typeof(AudioClip))
        {
            path = $"Assets/AssetBundleResources/{_bundleName.ToLower()}/{_assetName}.wav";

            T temp = AssetDatabase.LoadAssetAtPath<T>(path);

            if (temp == null)
            {
                path = $"Assets/AssetBundleResources/{_bundleName.ToLower()}/{_assetName}.mp3";
            }
        }

        T original = AssetDatabase.LoadAssetAtPath<T>(path);

        if (original == null)
        {
            Debug.Log($"Null : {_assetName}\n{path}");
        }

        _callback(original);
    }
}
