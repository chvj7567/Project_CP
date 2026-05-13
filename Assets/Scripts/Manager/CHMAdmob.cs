using System;
using GoogleMobileAds.Api;

// 글로벌 CHMAdmob 어댑터: 게임 코드의 CHMAdmob.Instance.X 호출을 패키지 ChvjUnityInfra.CHMAdmob에 위임.
// 광고 ID는 Assets/Resources/ChvjUnityInfra/AdConfig.asset에서 로드됨.
public class CHMAdmob
{
    private static CHMAdmob _instance;
    public static CHMAdmob Instance => _instance ??= new CHMAdmob();

    public Action AcquireReward
    {
        get => ChvjUnityInfra.CHMAdmob.Instance.AcquireReward;
        set => ChvjUnityInfra.CHMAdmob.Instance.AcquireReward = value;
    }

    public Action CloseAD
    {
        get => ChvjUnityInfra.CHMAdmob.Instance.CloseAD;
        set => ChvjUnityInfra.CHMAdmob.Instance.CloseAD = value;
    }

    public void Init() => ChvjUnityInfra.CHMAdmob.Instance.Init();
    public void ShowBanner(AdPosition position) => ChvjUnityInfra.CHMAdmob.Instance.ShowBanner(position);
    public void ShowInterstitialAd() => ChvjUnityInfra.CHMAdmob.Instance.ShowInterstitialAd();
    public void ShowRewardedAd() => ChvjUnityInfra.CHMAdmob.Instance.ShowRewardedAd();
}
