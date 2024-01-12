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
        else if (selectStage == (int)Defines.ESelectStage.Hard)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.HardStage);
        }
        else if (selectStage == (int)Defines.ESelectStage.Normal)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMMain.String.NormalStage);
        }

        var checkAdStage = lastPlayStage % 10;
        if (adStageList.Contains(checkAdStage) == false)
            return false;

        if (CHMIAP.Instance.CanBuyFromID(CHMMain.String.Product_ID_RemoveAD) == false)
            return false;

        CHMAdmob.Instance.ShowInterstitialAd();

        return true;
    }
}
