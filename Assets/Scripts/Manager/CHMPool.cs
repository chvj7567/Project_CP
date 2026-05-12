using UnityEngine;

public class CHMPool : ChvjUnityInfra.CHSingletonStatic<CHMPool>
{
    public void Init()
    {
        ChvjUnityInfra.CHMPool.Instance.Init();
    }

    public void CreatePool(GameObject _original, int _count = 5)
    {
        ChvjUnityInfra.CHMPool.Instance.CreatePool(_original, _count);
    }

    public void Push(CHPoolable poolable)
    {
        ChvjUnityInfra.CHMPool.Instance.Push(poolable);
    }

    public ChvjUnityInfra.CHPoolable Pop(GameObject _original, Transform _parent = null)
    {
        return ChvjUnityInfra.CHMPool.Instance.Pop(_original, _parent);
    }

    public GameObject GetOriginal(string _name)
    {
        return ChvjUnityInfra.CHMPool.Instance.GetOriginal(_name);
    }

    public void Clear()
    {
        ChvjUnityInfra.CHMPool.Instance.Clear();
    }
}
