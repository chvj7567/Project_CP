using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class AssetBundleInfo
{
    public string name;

    public AssetBundleInfo(string _name)
    {
        name = _name;
    }
}

[Serializable]
public class AssetBundleData
{
    public List<AssetBundleInfo> listAssetBundleInfo = new List<AssetBundleInfo>();
}

public partial class AssetBundlePool
{
    [Serializable]
    class Item
    {
        public string key;
        public AssetBundle obj;
    }

    Dictionary<string, Item> dicItem = new Dictionary<string, Item>();

    public void LoadAssetBundle(string _bundleName, AssetBundle _assetBundle)
    {
        if (dicItem.TryGetValue(_bundleName, out Item item) == false && _assetBundle != null)
        {
            dicItem.Add(_bundleName, new Item
            {
                key = _bundleName,
                obj = _assetBundle,
            });
        }
    }

    public AssetBundle GetItem(string _bundleName)
    {
        if (dicItem.TryGetValue(_bundleName.ToLower(), out Item ret))
        {
            return ret.obj;
        }
        else
        {
            return null;
        }
    }
}

public partial class ObjectPool
{
    [Serializable]
    class Item
    {
        public string key;
        public UnityEngine.Object obj;
    }

    Dictionary<string, Item> dicItem = new Dictionary<string, Item>();

    public void Load<T>(string _bundleName, string _assetName, T _object)
    {
        string key = $"{_bundleName.ToLower()}/{_assetName.ToLower()}";
        if (dicItem.TryGetValue(key, out Item item) == false && _object != null)
        {
            dicItem.Add(key, new Item
            {
                key = _bundleName,
                obj = _object as UnityEngine.Object,
            });
        }
    }

    public UnityEngine.Object GetItem(string _bundleName, string _assetName)
    {
        string key = $"{_bundleName.ToLower()}/{_assetName.ToLower()}";
        if (dicItem.TryGetValue(key, out Item ret))
        {
            return ret.obj;
        }
        else
        {
            return null;
        }
    }
}

public static class CHMAssetBundle
{
    public static bool firstDownload = true;
    static AssetBundlePool assetBundlePool = new AssetBundlePool();
    static ObjectPool objectPool = new ObjectPool();

    public static void LoadAssetBundle(string _bundleName, AssetBundle _assetBundle)
    {
        assetBundlePool.LoadAssetBundle(_bundleName, _assetBundle);
    }

    public static void LoadAsset<T>(string _bundleName, string _assetName, Action<T> _callback) where T : UnityEngine.Object
    {
        var obj = objectPool.GetItem(_bundleName, _assetName);
        if (obj == null)
        {
            AssetBundle assetBundle = assetBundlePool.GetItem(_bundleName);

            if (assetBundle != null)
            {
                var tempObj = assetBundle.LoadAsset<T>(_assetName);
                objectPool.Load<T>(_bundleName, _assetName, tempObj);

                _callback(assetBundle.LoadAsset<T>(_assetName));
            }
            else
            {
                _callback(null);
            }
        }
        else
        {
            _callback(obj as T);
        }
    }

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

        T original = AssetDatabase.LoadAssetAtPath<T>(path);

        _callback(original);
    }
#endif
}
