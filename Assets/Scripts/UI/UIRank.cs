using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIRankArg : CHUIArg
{
    
}

public class UIRank : UIBase
{
    UIRankArg arg;

    [SerializeField] RankScrollView scrollView;

    [SerializeField] Button normalRankTapBtn;
    [SerializeField] Button hardRankTapBtn;
    [SerializeField] Button bossRankTapBtn;

    [SerializeField] CHTMPro curRankDesc;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIRankArg;
    }

    private async void Start()
    {
        normalRankTapBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            curRankDesc.SetStringID(68);
            var rankList = await GetRankList(Defines.ESelectStage.Normal);
            scrollView.SetItemList(rankList);
        });

        hardRankTapBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            curRankDesc.SetStringID(77);
            var rankList = await GetRankList(Defines.ESelectStage.Hard);
            scrollView.SetItemList(rankList);
        });

        bossRankTapBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            curRankDesc.SetStringID(69);
            var rankList = await GetRankList(Defines.ESelectStage.Boss);
            scrollView.SetItemList(rankList);
        });

        curRankDesc.SetStringID(68);
        scrollView.SetItemList(await GetRankList(Defines.ESelectStage.Normal));
    }

    async Task<List<Infomation.RankInfo>> GetRankList(Defines.ESelectStage selectStage)
    {
        string gpgsID = "";

        switch (selectStage)
        {
            case Defines.ESelectStage.Hard:
                gpgsID = GPGSIds.leaderboard_hard_stage_rank;
                break;
            case Defines.ESelectStage.Boss:
                gpgsID = GPGSIds.leaderboard_boss_stage_rank;
                break;
            case Defines.ESelectStage.Normal:
                gpgsID = GPGSIds.leaderboard_normal_stage_rank;
                break;
        }

        List<Infomation.RankInfo> rankList = new List<Infomation.RankInfo>();

        if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).connectGPGS)
        {
            TaskCompletionSource<bool> myFirstRankTask = new TaskCompletionSource<bool>();

            // 자기 정보 가져왔을 때 없으면(사이즈 0이면) 넣어주기
            CHMGPGS.Instance.LoadCustomLeaderboardArray(gpgsID, 1, LeaderboardStart.PlayerCentered, LeaderboardTimeSpan.AllTime, (success, data) =>
            {
                if (success)
                {
                    if (data != null && data.Scores != null)
                    {
                        if (data.Scores.Length == 0)
                        {
                            CHMGPGS.Instance.ReportLeaderboard(gpgsID, 0, (success) =>
                            {
                                if (success)
                                {
                                    Debug.Log("InsertMyFirstRank Success");
                                }

                                myFirstRankTask.SetResult(true);
                            });
                        }
                        else
                        {
                            myFirstRankTask.SetResult(true);
                        }
                    }
                }
            });

            await myFirstRankTask.Task;

            TaskCompletionSource<bool> rankTaskComplete = new TaskCompletionSource<bool>();

            CHMGPGS.Instance.LoadAllLeaderboardArray(gpgsID, scores =>
            {
                CHMGPGS.Instance.LoadUsers(scores, (userProfiles) =>
                {
                    for (int i = 0; i < userProfiles.Length; i++)
                    {
                        rankList.Add(new Infomation.RankInfo
                        {
                            userID = userProfiles[i].userName,
                            stageRank = scores[i].rank,
                            stage = scores[i].value
                        });
                    }

                    rankTaskComplete.SetResult(true);
                });
            });

            await rankTaskComplete.Task;
        }

        return rankList;
    }
}
