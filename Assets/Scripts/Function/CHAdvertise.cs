using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHAdvertise : MonoBehaviour
{
    //# 스테이지 그룹 크기 (stage % 그룹 크기로 그룹 내 위치를 계산)
    const int StageGroupSize = 10;

    //# 그룹 내 위치(stage % StageGroupSize)가 이 값일 때 전면 광고 노출
    static List<int> adStageList = new List<int> { 2, 5, 8, 0 };

    //# 클리어한 스테이지(난이도 내 1-based)를 받아 전면 광고 노출 여부를 결정한다.
    //# PlayerPrefs를 내부에서 읽지 않는다 — 호출부가 증가 전 값을 명시 인자로 전달.
    public static bool GetAdvertise(int clearedStage)
    {
        int checkAdStage = clearedStage % StageGroupSize;
        if (adStageList.Contains(checkAdStage) == false)
            return false;

        //# 광고 제거를 이미 구매했으면(구매 불가 == false) 노출하지 않는다.
        if (ChvjUnityInfra.CHMIAP.Instance.CanBuyFromID(CHMString.Instance.Product_ID_RemoveAD) == false)
            return false;

        ChvjUnityInfra.CHMAdmob.Instance.ShowInterstitialAd();

        return true;
    }
}
