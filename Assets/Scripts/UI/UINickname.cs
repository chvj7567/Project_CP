using DG.Tweening;
using GooglePlayGames.BasicApi;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UINicknameArg : CHUIArg
{
    
}

public class UINickname : UIBase
{
    UINicknameArg arg;

    [SerializeField] CHTMPro nameText;
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
            var userID = CHMData.Instance.GetLoginData(CHMMain.String.CatPang).userID;
            if (userID == "")
            {
                CHMGPGS.Instance.ReportLeaderboard(GPGSIds.leaderboard_normal_stage_rank, 0, (success) =>
                {
                    if (success)
                    {
                        CHMGPGS.Instance.LoadCustomLeaderboardArray(GPGSIds.leaderboard_normal_stage_rank, 1, LeaderboardStart.PlayerCentered, LeaderboardTimeSpan.AllTime, (success, data) =>
                        {
                            if (success)
                            {
                                Debug.Log($"Save ID / Name : {data.Id} / {name.text}");
                                nameText.SetText(name.text);
                                CHMData.Instance.GetLoginData(CHMMain.String.CatPang).userID = data.Id;
                                CHMData.Instance.GetLoginData(CHMMain.String.CatPang).nickname = name.text;
                                CHMData.Instance.SaveData(CHMMain.String.CatPang);

                                CHMMain.UI.CloseUI(gameObject);
                            }
                        });
                    }
                });
            }
            else
            {
                CHMMain.UI.CloseUI(gameObject);
            }
        });
    }
}
