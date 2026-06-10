using UnityEngine;

public class CHAdvertise : MonoBehaviour
{
    //# 전면 광고를 노출할 스테이지 간격 — 10의 배수 스테이지(10,20,30...) 클리어 시 노출
    public const int InterstitialStageInterval = 10;

    //# 클리어한 스테이지(난이도 내 1-based)가 노출 조건을 만족하면 전면 광고를 띄운다.
    //# PlayerPrefs를 내부에서 읽지 않는다 — 호출부가 클리어한 스테이지 값을 명시 인자로 전달.
    //# 노멀/하드 전용 — 보스 스테이지(오프셋 100000+)는 1-based 진행도가 아니므로 호출부에서 제외한다.
    public static void ShowInterstitialIfEligible(int clearedStage)
    {
        //# clearedStage가 0이면 0 % 10 == 0으로 오판되므로 명시 가드 — 10의 배수 스테이지만 노출.
        if (clearedStage > 0 && clearedStage % InterstitialStageInterval == 0)
        {
#if UNITY_INFRA_IAP
            //# 광고 제거를 이미 구매했으면(구매 불가 == false) 노출하지 않는다.
            if (ChvjUnityInfra.CHMIAP.Instance.CanBuyFromID(CHMString.Instance.Product_ID_RemoveAD) == false)
                return;
#endif

#if UNITY_INFRA_ADS
            ChvjUnityInfra.CHMAdmob.Instance.ShowInterstitialAd();
#endif
        }
    }
}
