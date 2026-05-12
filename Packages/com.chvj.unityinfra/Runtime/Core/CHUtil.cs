using System.Collections.Generic;
using UnityEngine;

namespace ChvjUnityInfra
{
    public static class CHUtil
    {
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            if (list == null)
            {
                return true;
            }

            if (list.Count <= 0)
            {
                return true;
            }

            return false;
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }
            return component;
        }

        public static T FindChild<T>(this GameObject obj, string name = null, bool recursive = false) where T : Object
        {
            if (obj == null)
            {
                return null;
            }

            if (recursive == false)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    Transform transform = obj.transform.GetChild(i);
                    if (string.IsNullOrEmpty(name) || transform.name == name)
                    {
                        T component = transform.GetComponent<T>();
                        if (component != null)
                        {
                            return component;
                        }
                    }
                }
            }
            else
            {
                foreach (T component in obj.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                    {
                        return component;
                    }
                }
            }

            return null;
        }

        public static GameObject FindChild(this GameObject obj, string name = null, bool recursive = false)
        {
            Transform transform = FindChild<Transform>(obj, name, recursive);
            if (transform == null)
            {
                return null;
            }

            return transform.gameObject;
        }
    }
}
