using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHMString
{
    public string CatPang = "CatPang";
    public string Login = "Login";
    public string SelectStage = "SelectStage";
    public string HardStage = "HardStage";
    public string NormalStage = "NormalStage";
    public string BossStage = "BossStage";
    public string Background = "Background";
    public string Gold = "Gold";

    public string BGMVolume = "BGMVolume";
    public string EffectVolume = "EffectVolume";
    public string Red = "Red";
    public string Green = "Green";
    public string Blue = "Blue";
    public string Alpha = "Alpha";

    public string Product_Name_RemoveAD = "RemoveAD";
    public string Product_ID_RemoveAD = "com.catpang.product1";
    public string Product_Name_AddTime = "AddTime";
    public string Product_ID_AddTime = "com.catpang.product2";
    public string Product_Name_AddMove = "AddMove";
    public string Product_ID_AddMove = "com.catpang.product3";

    public string GetString(int stringID)
    {
        var loginData = CHMData.Instance.GetLoginData(CatPang);
        return CHMMain.Json.GetStringInfo(stringID, loginData.languageType);
    }
}
