using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHAdvertise : MonoBehaviour
{
    List<int> adStageList = new List<int> { 2, 5, 8, 0 };
    public bool GetAdvertise(int stage)
    {
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.catPang);
        if (loginData == null || loginData.buyRemoveAD)
            return false;

        var checkAdStage = stage % 10;

        if (false == adStageList.Contains(checkAdStage))
        {
            return false;
        }

        CHMAdmob.Instance.ShowInterstitialAd();

        return true;
    }
}
