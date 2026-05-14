using DG.Tweening;
#if UNITY_ANDROID
using GooglePlayGames.BasicApi;
#endif
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;
using static Defines;

public class UINicknameArg : CHUIArg
{
    
}

public class UINickname : UIBase
{
    UINicknameArg arg;

    [SerializeField] CHText nameText;
    [SerializeField] TMP_InputField name;
    [SerializeField] Button enterBtn;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UINicknameArg;
    }

    private void Start()
    {
        nameText.SetText("");

        enterBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
            if (loginData.userID == "")
            {
                if (loginData.connectGPGS)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    ChvjUnityInfra.CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_normal_stage_rank, 0, (success) =>
                    {
                        if (success)
                        {
                            ChvjUnityInfra.CHMGPGS.Instance.LoadCustomLeaderboardArray(GPGSIds.leaderboard_normal_stage_rank, 1, LeaderboardStart.PlayerCentered, LeaderboardTimeSpan.AllTime, (success, data) =>
                            {
                                if (success)
                                {
                                    Debug.Log($"Save ID / Name : {data.Id} / {name.text}");
                                    nameText.SetText(name.text);
                                    loginData.userID = data.Id;
                                    loginData.nickname = name.text;
                                    CHMData.Instance.SaveData(CHMString.Instance.CatPang);

                                    CHMUI.Instance.CloseUI(gameObject);
                                }
                            });
                        }
                    });
#endif
                }
                else
                {
                    Debug.Log($"Save ID / Name : {name.text}");
                    nameText.SetText(name.text);
                    loginData.nickname = name.text;
                    CHMData.Instance.SaveData(CHMString.Instance.CatPang);

                    CHMUI.Instance.CloseUI(gameObject);
                }
            }
            else
            {
                CHMUI.Instance.CloseUI(gameObject);
            }
        });
    }
}
