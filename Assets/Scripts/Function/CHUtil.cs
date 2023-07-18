using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute {}

public static class CHUtil
{
    public static bool IsNullOrEmpty<T>(this List<T> _list)
    {
        if (_list == null)
        {
            return true;
        }

        if (_list.Count <= 0)
        {
            return true;
        }

        return false;
    }

    public static T GetOrAddComponent<T>(this GameObject _obj) where T : UnityEngine.Component
    {
        T component = _obj.GetComponent<T>();
        if (component == null)
            component = _obj.AddComponent<T>();
        return component;
    }

    // 자식 게임오브젝트 중에 T에 해당하는 컴포넌트를 가져온다.
    // 자식들의 그 아래 자식들까지 찾으려면 recursive에 true로 체크하여 재귀적으로 찾는다.
    public static T FindChild<T>(this GameObject _obj, string _name = null, bool _recursive = false) where T : UnityEngine.Object
    {
        if (_obj == null)
            return null;

        if (_recursive == false)
        {
            for (int i = 0; i < _obj.transform.childCount; i++)
            {
                Transform transform = _obj.transform.GetChild(i);
                if (string.IsNullOrEmpty(_name) || transform.name == _name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in _obj.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(_name) || component.name == _name)
                    return component;
            }
        }

        return null;
    }

    // 게임 오브젝트를 찾는 경우 제네릭 형식이 아닌 일반 형식으로 호출할 수 있게 오버라이딩한다.
    public static GameObject FindChild(this GameObject _obj, string _name = null, bool _recursive = false)
    {
        Transform transform = FindChild<Transform>(_obj, _name, _recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static float ReverseValue(float _value)
    {
        return -_value;
    }
}
