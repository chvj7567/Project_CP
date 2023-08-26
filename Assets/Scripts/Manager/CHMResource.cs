using UnityEngine;
using System;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif



public class CHMResource
{
    private void LoadAsset<T>(string _bundleName, string _assetName, Action<T> _callback) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        CHMAssetBundle.LoadAssetOnEditor<T>(_bundleName, _assetName, _callback);
#else
        CHMAssetBundle.LoadAsset<T>(_bundleName, _assetName, _callback);
#endif
    }

    public void InstantiateAsObservable<T>(string _bundleName, string _assetName, Action<T> _callback = null) where T : UnityEngine.Object
    {
        Action<T> _callbackTemp = original =>
        {
            if (original == null)
            {
                _callback(null);
            }
            else
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject go = original as GameObject;
                    T t = Instantiate(go) as T;
                    if (_callback != null) _callback(t);
                }
                else
                {
                    if (_callback != null) _callback(GameObject.Instantiate(original));
                }
            }
        };

        LoadAsset<T>(_bundleName, _assetName, _callbackTemp);
    }

    public void LoadJson(Defines.EJsonType _jsonType, Action<TextAsset> _callback)
    {
        LoadAsset<TextAsset>($"{Defines.EResourceType.Json.ToString()}", $"{_jsonType.ToString()}", _callback);
    }

    public void LoadSprite(Defines.ENormalBlockType _spriteType, Action<Sprite> _callback)
    {
        LoadAsset<Sprite>($"{Defines.EResourceType.Sprite.ToString()}", $"{_spriteType.ToString()}", _callback);
    }

    public void LoadSprite(Defines.ESpecailBlockType _spriteType, Action<Sprite> _callback)
    {
        LoadAsset<Sprite>($"{Defines.EResourceType.Sprite.ToString()}", $"{_spriteType.ToString()}", _callback);
    }

    public void LoadSprite(Defines.EWallBlockType _spriteType, Action<Sprite> _callback)
    {
        LoadAsset<Sprite>($"{Defines.EResourceType.Sprite.ToString()}", $"{_spriteType.ToString()}", _callback);
    }

    public void LoadSound(Defines.ESound _soundType, Action<AudioClip> _callback)
    {
        LoadAsset<AudioClip>($"{Defines.EResourceType.Sound.ToString()}", $"{_soundType.ToString()}", _callback);
    }

    public void InstantiateUI(Defines.EUI _ui, Action<GameObject> _callback = null)
    {
        InstantiateAsObservable<GameObject>($"{Defines.EResourceType.UI.ToString()}", $"{_ui.ToString()}", _callback);
    }

    public GameObject Instantiate(GameObject _object, Transform _parent = null)
    {
        if (_object == null) return null;

        CHPoolable poolable = _object.GetComponent<CHPoolable>();
        if (poolable != null)
        {
            return CHMMain.Pool.Pop(_object, _parent).gameObject;
        }
        else
        {
            GameObject go = GameObject.Instantiate(_object, _parent);
            return go;
        }
    }

    public async void Destroy(GameObject _object, float _time = 0)
    {
        if (_object == null) return;

        CHPoolable poolable = _object.GetComponent<CHPoolable>();
        if (poolable != null)
        {
            await Task.Delay((int)(_time * 1000f));
            CHMMain.Pool.Push(poolable);
        }
        else
        {
            UnityEngine.Object.Destroy(_object, _time);
        }
    }
}
