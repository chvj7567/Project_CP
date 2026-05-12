using System;
using UnityEngine;

namespace ChvjUnityInfra
{
    public static class JsonArrayUtility
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        public static T[] FromJsonArray<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return Array.Empty<T>();
            }

            string wrapped = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper?.items ?? Array.Empty<T>();
        }
    }
}
