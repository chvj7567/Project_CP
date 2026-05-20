using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHAdvertise : MonoBehaviour
{
    // 스테이지 그룹 크기 (stage % 그룹 크기로 그룹 내 위치를 계산)
    const int StageGroupSize = 10;

    // 그룹 내 위치(stage % StageGroupSize)가 이 값일 때 전면 광고 노출
    List<int> adStageList = new List<int> { 2, 5, 8, 0 };

    public bool GetAdvertise()
    {
        var lastPlayStage = 0;
        var selectStage = PlayerPrefs.GetInt(CHMString.Instance.SelectStage);
        if (selectStage == (int)Defines.ESelectStage.Boss)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMString.Instance.BossStage) - CHMData.Instance.BossStageStartValue;
        }
        else if (selectStage == (int)Defines.ESelectStage.Hard)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMString.Instance.HardStage);
        }
        else if (selectStage == (int)Defines.ESelectStage.Normal)
        {
            lastPlayStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage);
        }

        var checkAdStage = lastPlayStage % StageGroupSize;
        if (adStageList.Contains(checkAdStage) == false)
            return false;

        if (ChvjUnityInfra.CHMIAP.Instance.CanBuyFromID(CHMString.Instance.Product_ID_RemoveAD) == false)
            return false;

        ChvjUnityInfra.CHMAdmob.Instance.ShowInterstitialAd();

        return true;
    }
}
