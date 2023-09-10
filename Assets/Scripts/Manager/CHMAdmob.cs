using UnityEngine;
using GoogleMobileAds.Api;

public static class CHMAdmob
{
    static string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    static string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    static string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    static BannerView bannerView;
    static InterstitialAd interstitialAd;
    static RewardedAd rewardedAd;

    static AdRequest adRequest;

    static bool advertise = false;

    static bool checkInit = false;

    static public bool GetAdvertise()
    {
        return advertise;
    }

    static public void Init()
    {
        if (checkInit == true)
            return;

        checkInit = true;

        MobileAds.Initialize(initStatus => { });

        adRequest = new AdRequest.Builder().Build();

        LoadInterstitialAd();
        LoadRewardedAd();
    }

    static public void ShowBanner(AdPosition _position)
    {
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, _position);
        bannerView.LoadAd(adRequest);
        bannerView.Show();
    }

    static void LoadInterstitialAd(bool _show = false)
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        InterstitialAd.Load(interstitialAdUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    interstitialAd = ad;
                    RegisterEventHandlers(interstitialAd);

                    if (_show == true)
                    {
                        interstitialAd.Show();
                    }
                });
    }

    static void ShowInterstitialAd()
    {
        advertise = true;

        if (interstitialAd != null && interstitialAd.CanShowAd() == true)
        {
            interstitialAd.Show();
        }
        else
        {
            LoadInterstitialAd(true);
        }
    }

    static void LoadRewardedAd(bool _show = false)
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        RewardedAd.Load(rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    rewardedAd = ad;
                    RegisterEventHandlers(rewardedAd);

                    if (_show == true)
                    {
                        rewardedAd.Show(RewardHandler);
                    }
                });
    }

    static public void ShowRewardedAd()
    {
        advertise = true;

        if (rewardedAd.CanShowAd() == true)
        {
            rewardedAd.Show(RewardHandler);
        }
        else
        {
            LoadRewardedAd(true);
        }
    }

    static void RewardHandler(Reward _reward)
    {
        double currencyAmount = _reward.Amount;
        string currencyType = _reward.Type;

        var boomAllChance = PlayerPrefs.GetInt("boomAllChance");
        PlayerPrefs.SetInt("boomAllChance", boomAllChance + 1);

        Debug.Log("광고로 받은 보상: " + boomAllChance + " => " + boomAllChance + 1);
    }

    static void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");

            advertise = false;

            LoadRewardedAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    static void RegisterEventHandlers(InterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");

            advertise = false;

            LoadInterstitialAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
}
