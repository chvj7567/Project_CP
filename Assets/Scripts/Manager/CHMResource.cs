using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CHMResource
{
    // 같은 Task를 캐싱해 동시 호출자가 모두 같은 Addressables Init을 await하도록 함
    private Task<bool> _initTask;

    public Task<bool> EnsureInit()
    {
        if (_initTask == null)
            _initTask = ChvjUnityInfra.CHMResource.Instance.Init();
        return _initTask;
    }

    public void LoadData(string name, Action<TextAsset> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<TextAsset>(name, _callback);
    }

    public void LoadFont(Defines.ELanguageType languageType, Action<TMP_FontAsset> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<TMP_FontAsset>("Gaegu-Bold SDF", _callback);
    }

    public void LoadJson(Defines.EJsonType _jsonType, Action<TextAsset> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<TextAsset>(_jsonType.ToString(), _callback);
    }

    public void LoadSprite(Defines.EBlockState _spriteType, Action<Sprite> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<Sprite>(_spriteType.ToString(), _callback);
    }

    public void LoadSound(Defines.ESound _soundType, Action<AudioClip> _callback)
    {
        ChvjUnityInfra.CHMResource.Instance.Load<AudioClip>(_soundType.ToString(), _callback);
    }

    public void InstantiateUI(Defines.EUI _ui, Action<GameObject> _callback = null)
    {
        ChvjUnityInfra.CHMResource.Instance.Instantiate<GameObject>(_ui, _callback);
    }

    public void InstantiateEffect(Defines.EEffect _effectType, Action<GameObject> _callback = null)
    {
        ChvjUnityInfra.CHMResource.Instance.Instantiate<GameObject>(_effectType, _callback);
    }

    public GameObject Instantiate(GameObject _object, Transform _parent = null)
    {
        if (_object == null) return null;
        CHPoolable poolable = _object.GetComponent<CHPoolable>();
        if (poolable != null)
        {
            return CHMMain.Pool.Pop(_object, _parent).gameObject;
        }
        return GameObject.Instantiate(_object, _parent);
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
