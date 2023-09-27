using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHMString
{
    public string catPang = "CatPang";
    public string login = "Login";
    public string stage = "Stage";
    public string background = "Background";
    public string GetString(int _stringID)
    {
        return CHMMain.Json.GetStringInfo(_stringID);
    }

    public string GetString(int _stringID, params object[] _argArr)
    {
        return string.Format(GetString(_stringID), _argArr);
    }
}
