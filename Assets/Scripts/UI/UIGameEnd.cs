using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameEndArg : CHUIArg
{
    public Defines.EGameResult result = Defines.EGameResult.None;
    public int gold;
}

public class UIGameEnd : UIBase
{
    UIGameEndArg arg;

    [SerializeField] TMP_Text resultText;
    [SerializeField] CHTMPro goldText;
    [SerializeField] CHTMPro goldx3Text;
    [SerializeField] Button menuBtn;
    [SerializeField] Button adBtn;

    bool received = false;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIGameEndArg;
    }

    private void Start()
    {
        if (arg.result == Defines.EGameResult.GameOver)
        {
            resultText.DOText("Game Over", 1f);
        }
        else if (arg.result == Defines.EGameResult.GameClear)
        {
            resultText.DOText("Game Clear", 1f);
        }

        goldText.SetText(arg.gold);
        goldx3Text.SetText(arg.gold * 3);

        menuBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (received == true)
            {
                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMMain.UI.CloseUI(Defines.EUI.UIAlarm);
                CHMMain.Pool.Clear();

                SceneManager.LoadScene(0);
                return;
            }

            var before = CHMData.Instance.GetCollectionData(CHMMain.String.gold).value;
            var after = CHMData.Instance.GetCollectionData(CHMMain.String.gold).value += arg.gold;

            received = true;

            Debug.Log($"Gold {before} => {after}");

            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                alarmText = "Received Reward"
            });

            Time.timeScale = 1;
            CHInstantiateButton.ResetBlockDict();
            CHMMain.UI.CloseUI(Defines.EUI.UIAlarm);
            CHMMain.Pool.Clear();

            SceneManager.LoadScene(0);
        });

        adBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMAdmob.Instance.ShowRewardedAd();
        });

        CHMAdmob.Instance.AcquireReward += () =>
        {
            if (received == true)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Already Received Reward"
                });

                return;
            }

            var before = CHMData.Instance.GetCollectionData(CHMMain.String.gold).value;
            var after = CHMData.Instance.GetCollectionData(CHMMain.String.gold).value += arg.gold * 3;

            //adBtn.gameObject.SetActive(false);
            received = true;

            Debug.Log($"Gold {before} => {after}");

            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                alarmText = "Received Reward"
            });
        };
    }
}
