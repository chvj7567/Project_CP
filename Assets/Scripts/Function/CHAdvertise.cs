using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHAdvertise : MonoBehaviour
{
    List<int> adStageList = new List<int> { 2, 5, 8, 0 };
    public bool GetAdvertise()
    {
        var lastPlayStage = 0;
        var selectStage = PlayerPrefs.GetInt(CHMMain.String.SelectStage);
        if (selectStage == (int)Defines.ESelectStage.Boss)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.BossStage) - CHMData.Instance.BossStageStartValue;
        }
        else if (selectStage == (int)Defines.ESelectStage.Normal)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.Stage);
        }
        else if (selectStage == (int)Defines.ESelectStage.Easy)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.EasyStage);
        }

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData == null)
            return false;

        if (loginData.buyRemoveAD)
        {
            return false;
        }

        var checkAdStage = lastPlayStage % 10;
        if (false == adStageList.Contains(checkAdStage))
        {
            return false;
        }

        CHMAdmob.Instance.ShowInterstitialAd();

        return true;
    }
}
