using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using TMPro;
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
        if (dicItem.TryGetValue(_bundleName, out Item ret))
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

public class CHMAssetBundle : CHSingleton<CHMAssetBundle>
{
    public bool firstDownload = true;
    public Dictionary<string, string> bundleDic = new Dictionary<string, string>();
    AssetBundlePool assetBundlePool = new AssetBundlePool();
    ObjectPool objectPool = new ObjectPool();

    public void LoadAssetBundle(string _bundleName, AssetBundle _assetBundle)
    {
        assetBundlePool.LoadAssetBundle(_bundleName, _assetBundle);
    }

    public void LoadAsset<T>(string _bundleName, string _assetName, Action<T> _callback) where T : UnityEngine.Object
    {
        var obj = objectPool.GetItem(_bundleName, _assetName);
        if (obj == null)
        {
            if (bundleDic.TryGetValue(_bundleName.ToLower(), out var name) == false)
            {
                _callback(null);
                return;
            }

            AssetBundle assetBundle = assetBundlePool.GetItem(name);

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
}
