using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static Defines;

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

    [SerializeField] GameObject noRankingDataObj;

    [SerializeField] int aiCount;

    [SerializeField, ReadOnly] public Defines.ESelectStage curTap;
    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIRankArg;
    }

    private async void Start()
    {
        normalRankTapBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (curTap == Defines.ESelectStage.Normal)
                return;

            curRankDesc.SetStringID(68);
            curTap = Defines.ESelectStage.Normal;
            var rankList = await GetRankList(Defines.ESelectStage.Normal);

            noRankingDataObj.SetActive(false);

            if (rankList.Count == 0)
            {
                noRankingDataObj.SetActive(true);
            }

            scrollView.SetItemList(rankList);
        });

        hardRankTapBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (curTap == Defines.ESelectStage.Hard)
                return;

            curRankDesc.SetStringID(77);
            curTap = Defines.ESelectStage.Hard;
            var rankList = await GetRankList(Defines.ESelectStage.Hard);

            noRankingDataObj.SetActive(false);

            if (rankList.Count == 0)
            {
                noRankingDataObj.SetActive(true);
            }

            scrollView.SetItemList(rankList);
        });

        bossRankTapBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (curTap == Defines.ESelectStage.Boss)
                return;

            curRankDesc.SetStringID(69);
            curTap = Defines.ESelectStage.Boss;
            var rankList = await GetRankList(Defines.ESelectStage.Boss);

            noRankingDataObj.SetActive(false);

            if (rankList.Count == 0)
            {
                noRankingDataObj.SetActive(true);
            }

            scrollView.SetItemList(rankList);
        });

        curRankDesc.SetStringID(68);
        curTap = Defines.ESelectStage.Normal;

        var rankList = await GetRankList(Defines.ESelectStage.Normal);

        noRankingDataObj.SetActive(false);

        if (rankList.Count == 0)
        {
            noRankingDataObj.SetActive(true);
        }

        scrollView.SetItemList(rankList);
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
            TaskCompletionSource<bool> rankTaskComplete = new TaskCompletionSource<bool>();

            CHMGPGS.Instance.LoadCustomLeaderboardArray(gpgsID, 10, LeaderboardStart.TopScores, LeaderboardTimeSpan.AllTime, (success, data) =>
            {
                if (success)
                {
                    if (data != null && data.Scores != null)
                    {
                        var scores = data.Scores;

                        if (scores.Length == 0)
                        {
                            rankTaskComplete.SetResult(true);
                        }
                        else
                        {
                            CHMGPGS.Instance.LoadUsers(scores, (userProfiles) =>
                            {
                                int lastRank = 0;
                                for (int i = 0; i < userProfiles.Length; ++i)
                                {
                                    rankList.Add(new Infomation.RankInfo
                                    {
                                        userID = userProfiles[i].userName,
                                        profileTexture = userProfiles[i].image,
                                        stageRank = scores[i].rank,
                                        stage = scores[i].value
                                    });

                                    lastRank = scores[i].rank;
                                }

                                for (int i = 0; i < aiCount; ++i)
                                {
                                    rankList.Add(new Infomation.RankInfo
                                    {
                                        userID = $"AI {i + 1}",
                                        stageRank = ++lastRank,
                                        stage = 0
                                    });
                                }

                                rankTaskComplete.SetResult(true);
                            });
                        }
                    }
                }
            });

            /*CHMGPGS.Instance.LoadAllLeaderboardArray(gpgsID, scores =>
            {
                Debug.Log($"4  {gpgsID}/{scores.Length}");

                CHMGPGS.Instance.LoadUsers(scores, (userProfiles) =>
                {
                    int lastRank = 0;
                    for (int i = 0; i < userProfiles.Length; ++i)
                    {
                        rankList.Add(new Infomation.RankInfo
                        {
                            userID = userProfiles[i].userName,
                            profileTexture = userProfiles[i].image,
                            stageRank = scores[i].rank,
                            stage = scores[i].value
                        });

                        lastRank = scores[i].rank;
                    }

                    for (int i = 0; i < aiCount; ++i)
                    {
                        rankList.Add(new Infomation.RankInfo
                        {
                            userID = $"AI {i + 1}",
                            stageRank = ++lastRank,
                            stage = 0
                        });
                    }

                    rankTaskComplete.SetResult(true);
                });
            });*/

            await rankTaskComplete.Task;
        }

        return rankList;
    }
}
