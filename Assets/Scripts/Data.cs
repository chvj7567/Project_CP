using System;
using System.Collections;
using System.Collections.Generic;
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
    public class ExtractData<T> : ILoader<string, T> where T : class
    {
        public List<Login> loginList = new List<Login>();
        public List<Stage> stageList = new List<Stage>();

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

            return dict;
        }

        public List<T> MakeList(Dictionary<string, T> dict)
        {
            List<T> list = new List<T>();

            if (typeof(T) == typeof(Stage))
            {
                foreach (T info in dict.Values)
                    list.Add(info);
            }

            return list;
        }
    }
}