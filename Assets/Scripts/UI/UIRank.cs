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

    [SerializeField] Button normalRankTapBtn;
    [SerializeField] Button hardRankTapBtn;
    [SerializeField] Button bossRankTapBtn;
    [SerializeField] RankScrollView scrollView;

    [SerializeField] List<Infomation.RankInfo> rankList = new List<Infomation.RankInfo>();
    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIRankArg;
    }

    private void Start()
    {
        normalRankTapBtn.OnClickAsObservable().Subscribe(_ =>
        {
            rankList.Clear();

            if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).connectGPGS)
            {
                CHMGPGS.Instance.LoadAllLeaderboardArray(GPGSIds.leaderboard_normal_stage_rank, scores =>
                {
                    for (int i = 0; i < scores.Length; i++)
                        Debug.Log($"{i}, {scores[i].rank}, {scores[i].value}, {scores[i].userID}, {scores[i].date}\n");

                    CHMGPGS.Instance.LoadUsers(scores, (userProfiles) =>
                    {

                    });
                });
            }
        });
    }
}
