using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CHStatic : MonoBehaviour
{
#if UNITY_EDITOR
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
        else if (typeof(T) == typeof(TMP_FontAsset))
        {
            path = $"Assets/AssetBundleResources/{_bundleName.ToLower()}/{_assetName}.asset";
        }

        T original = AssetDatabase.LoadAssetAtPath<T>(path);

        if (original == null)
        {
            Debug.Log($"Null : {_assetName}");
        }

        _callback(original);
    }
#endif
}
