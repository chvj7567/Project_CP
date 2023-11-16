using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHAdvertise : MonoBehaviour
{
    List<int> adStageList = new List<int> { 3, 7, 10 };
    public bool GetAdvertise(int stage)
    {
        var checkAdStage = stage % 10;

        if (false == adStageList.Contains(checkAdStage))
        {
            return false;
        }

        CHMAdmob.Instance.ShowInterstitialAd();

        return true;
    }
}
