using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public static class CHMAdmob
{
    static string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    static string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    static string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    static BannerView bannerView;
    static InterstitialAd interstitialAd;
    static RewardedAd rewardedAd;

    static AdRequest adRequest;

    static public void Init()
    {
        MobileAds.Initialize(initStatus => { });

        adRequest = new AdRequest.Builder().Build();
    }

    static public void ShowBanner(AdPosition _position)
    {
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, _position);
        bannerView.LoadAd(adRequest);
        bannerView.Show();
    }

    static public void ShowInterstitialAd()
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

                    interstitialAd.Show();
                });
    }

    static public void ShowRewardedAd()
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

                    rewardedAd.Show(HandleReward);
                });
    }

    static void HandleReward(Reward _reward)
    {
        double currencyAmount = _reward.Amount;
        string currencyType = _reward.Type;

        Debug.Log("광고로 받은 보상: " + currencyAmount + " " + currencyType);
    }
}
