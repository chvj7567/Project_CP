using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class Login
    {
        public string key = "";
        public bool connectGPGS = false;
    }

    [Serializable]
    public class Stage
    {
        public string key = "";
        public int stage = -1;
        public bool clear = false;
        public int boomAllCount = -1;
    }

    [Serializable]
    public class Collection
    {
        public string key = "";
        public int value = -1;
    }

    [Serializable]
    public class Mission
    {
        public string key = "";
        public int startValue = -1;
        public Defines.EClearState clearState = Defines.EClearState.None;
        public int repeatCount = -1;
    }

    [Serializable]
    public class ExtractData<T> : ILoader<string, T> where T : class
    {
        public List<Login> loginList = new List<Login>();
        public List<Stage> stageList = new List<Stage>();
        public List<Collection> collectionList = new List<Collection>();
        public List<Mission> missionList = new List<Mission>();

        public Dictionary<string, T> MakeDict()
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();

            if (typeof(T) == typeof(Login))
            {
                foreach (Login data in loginList)
                    dict.Add(data.key, data as T);
            }
            else if (typeof(T) == typeof(Stage))
            {
                foreach (Stage data in stageList)
                    dict.Add(data.key, data as T);
            }
            else if (typeof(T) == typeof(Collection))
            {
                foreach (Collection data in collectionList)
                    dict.Add(data.key, data as T);
            }
            else if (typeof(T) == typeof(Mission))
            {
                foreach (Mission data in missionList)
                    dict.Add(data.key, data as T);
            }

            return dict;
        }

        public List<T> MakeList()
        {
            List<T> list = new List<T>();

            if (typeof(T) == typeof(Login))
            {
                foreach (Login data in loginList)
                    list.Add(data as T);
            }
            else if (typeof(T) == typeof(Stage))
            {
                foreach (Stage data in stageList)
                    list.Add(data as T);
            }
            else if (typeof(T) == typeof(Collection))
            {
                foreach (Collection data in collectionList)
                    list.Add(data as T);
            }
            else if (typeof(T) == typeof(Collection))
            {
                foreach (Mission data in missionList)
                    list.Add(data as T);
            }

            return list;
        }

        public List<T> MakeList(Dictionary<string, T> dict)
        {
            List<T> list = new List<T>();

            foreach (var data in dict)
                list.Add(data.Value);

            return list;
        }
    }
}