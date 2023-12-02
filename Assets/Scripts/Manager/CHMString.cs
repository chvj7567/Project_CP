using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHMString
{
    public string CatPang = "CatPang";
    public string Login = "Login";
    public string Stage = "Stage";
    public string Background = "Background";
    public string Gold = "Gold";

    public string Product_Name_RemoveAD = "RemoveAD";
    public string Product_ID_RemoveAD = "com.catpang.product1";
    public string Product_Name_AddTime = "AddTime";
    public string Product_ID_AddTime = "com.catpang.product2";
    public string Product_Name_AddMove = "AddMove";
    public string Product_ID_AddMove = "com.catpang.product3";

    public string GetString(int stringID)
    {
        var laungageType = Application.systemLanguage == SystemLanguage.Korean ? Defines.ELanguageType.Korea : Defines.ELanguageType.English;
        return CHMJson.Instance.GetStringInfo(stringID, laungageType);
    }
}
