using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHMString : ChvjUnityInfra.CHSingletonStatic<CHMString>
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

    //# In-App Update UIConfirm 문구 stringID (String*.json 의 stringID 와 1:1)
    public int AppUpdateForcedTitle = 186;
    public int AppUpdateForcedDesc = 187;
    public int AppUpdateReadyTitle = 188;
    public int AppUpdateReadyDesc = 189;

    public string GetString(int stringID)
    {
        Data.Login loginData = CHMData.Instance.GetLoginData(CatPang);
        return CHMJson.Instance.GetStringInfo(stringID, loginData.languageType);
    }
}
