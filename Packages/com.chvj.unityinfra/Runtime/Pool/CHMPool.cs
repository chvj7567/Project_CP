using System.Collections.Generic;
using UnityEngine;

namespace ChvjUnityInfra
{
    public class CHMPool : CHSingletonStatic<CHMPool>
    {
        private class CHPool
        {
            public GameObject Original { get; private set; }
            public Transform Root { get; set; }

            private Stack<CHPoolable> _stPool = new Stack<CHPoolable>();

            public void Init(GameObject original, int count = 5)
            {
                Original = original;
                Root = new GameObject().transform;
                Root.name = $"{original.name}Root";

                for (int i = 0; i < count; ++i)
                {
                    Push(Create());
                }
            }

            private CHPoolable Create()
            {
                GameObject go = Object.Instantiate<GameObject>(Original);
                go.name = Original.name;
                return go.GetOrAddComponent<CHPoolable>();
            }

            public void Push(CHPoolable poolable)
            {
                if (poolable == null) return;

                poolable.transform.SetParent(Root, false);
                poolable.isUse = false;
                poolable.gameObject.SetActive(false);

                _stPool.Push(poolable);
            }

            public CHPoolable Pop(Transform parent)
            {
                CHPoolable poolable;

                if (_stPool.Count > 0)
                {
                    poolable = _stPool.Pop();
                }
                else
                {
                    poolable = Create();
                }

                poolable.transform.SetParent(parent, false);
                poolable.isUse = true;
                poolable.gameObject.SetActive(true);

                return poolable;
            }
        }

        private Dictionary<string, CHPool> _poolDic = new Dictionary<string, CHPool>();
        private GameObject _rootObject;

        public void Init()
        {
            _rootObject = GameObject.Find("@CHMPool");
            if (_rootObject == null)
            {
                _rootObject = new GameObject { name = "@CHMPool" };
            }

            Object.DontDestroyOnLoad(_rootObject);
        }

        public void CreatePool(GameObject original, int count = 5)
        {
            CHPool pool = new CHPool();
            pool.Init(original, count);
            pool.Root.parent = _rootObject.transform;

            _poolDic.Add(original.name, pool);
        }

        public void Push(CHPoolable poolable)
        {
            if (poolable == null) return;

            if (_poolDic.ContainsKey(poolable.gameObject.name) == false)
            {
                GameObject.Destroy(poolable.gameObject);
                return;
            }

            _poolDic[poolable.gameObject.name].Push(poolable);
        }

        public CHPoolable Pop(GameObject original, Transform parent = null)
        {
            if (_poolDic.ContainsKey(original.name) == false)
            {
                CreatePool(original);
            }

            return _poolDic[original.name].Pop(parent);
        }

        public GameObject GetOriginal(string name)
        {
            if (_poolDic.ContainsKey(name) == false)
                return null;
            return _poolDic[name].Original;
        }

        public void Clear()
        {
            foreach (Transform child in _rootObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            _poolDic.Clear();
        }
    }
}
