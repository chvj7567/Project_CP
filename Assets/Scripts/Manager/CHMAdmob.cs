using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class CHMAdmob : CHSingleton<CHMAdmob>
{
    string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    string interstitialAdUnitId = "ca-app-pub-7085378387310828/3775873067";
    string rewardedAdUnitId = "ca-app-pub-7085378387310828/7433429939";

    // test
    //string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    //string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    BannerView bannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;

    AdRequest adRequest;

    bool checkInit = false;

    public Action AcquireReward;
    public Action CloseAD;

    public void Init()
    {
        if (checkInit == true)
            return;

        checkInit = true;

        MobileAds.Initialize(initStatus => { });

        adRequest = new AdRequest.Builder().Build();

        LoadInterstitialAd();
        LoadRewardedAd();
        //ShowBanner(AdPosition.Top);
    }

    public void ShowBanner(AdPosition _position)
    {
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, _position);
        bannerView.LoadAd(adRequest);
    }

    void LoadInterstitialAd(bool _show = false)
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

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd() == true)
        {
            interstitialAd.Show();
        }
        else
        {
            LoadInterstitialAd(true);
        }
    }

    void LoadRewardedAd(bool _show = false)
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

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd() == true)
        {
            rewardedAd.Show(RewardHandler);
        }
        else
        {
            LoadRewardedAd(true);
        }
    }

    void RewardHandler(Reward _reward)
    {
        double currencyAmount = _reward.Amount;
        string currencyType = _reward.Type;

        if (AcquireReward != null)
        {
            AcquireReward.Invoke();
        }
    }

    void RegisterEventHandlers(RewardedAd ad)
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

            if (CloseAD != null)
            {
                CloseAD.Invoke();
            }

            LoadRewardedAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    void RegisterEventHandlers(InterstitialAd ad)
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

            if (CloseAD != null)
            {
                CloseAD.Invoke();
            }

            LoadInterstitialAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
}
