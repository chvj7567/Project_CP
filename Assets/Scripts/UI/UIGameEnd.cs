using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameEndArg : CHUIArg
{
    public Defines.EClearState clearState = Defines.EClearState.None;
    public Defines.EGameState result = Defines.EGameState.None;
    public int gold;
}

public class UIGameEnd : UIBase
{
    UIGameEndArg arg;

    [SerializeField] TMP_Text resultText;
    [SerializeField] CHTMPro goldText;
    [SerializeField] CHTMPro goldx2Text;
    [SerializeField] Button menuBtn;
    [SerializeField] Button adBtn;

    bool received = false;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIGameEndArg;
    }

    private void Start()
    {
        if (arg.result == Defines.EGameState.GameOver)
        {
            resultText.DOText("Game Over", 1f);
            goldText.SetText(0);
            goldx2Text.SetText(0);
        }
        else if (arg.result == Defines.EGameState.GameClear)
        {
            resultText.DOText("Game Clear", 1f);

            if (arg.clearState == Defines.EClearState.Clear)
            {
                goldText.SetText(0);
                goldx2Text.SetText(0);
            }
            else
            {
                goldText.SetText(arg.gold);
                goldx2Text.SetText(arg.gold * 2);
            }
        }

        menuBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (received == true)
            {
                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMMain.UI.CloseUI(Defines.EUI.UIAlarm);
                CHMMain.Pool.Clear();

                SceneManager.LoadScene(1);
                return;
            }

            var before = CHMData.Instance.GetCollectionData(CHMMain.String.Gold).value;
            var after = CHMData.Instance.GetCollectionData(CHMMain.String.Gold).value += arg.gold;

            received = true;

            Debug.Log($"Gold {before} => {after}");

            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 63
            });

            Time.timeScale = 1;
            CHInstantiateButton.ResetBlockDict();
            CHMMain.UI.CloseUI(Defines.EUI.UIAlarm);
            CHMMain.Pool.Clear();

            CHMData.Instance.SaveData(CHMMain.String.CatPang);
            SceneManager.LoadScene(1);
        });

        adBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (CHMIAP.Instance.CanBuyFromID(CHMMain.String.Product_ID_RemoveAD) == false)
            {
                AcquireReward();
            }
            else
            {
                CHMAdmob.Instance.ShowRewardedAd();
            }
        });

        CHMAdmob.Instance.AcquireReward += AcquireReward;
    }

    void AcquireReward()
    {
        if (received == true)
        {
            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 64
            });

            return;
        }

        var before = CHMData.Instance.GetCollectionData(CHMMain.String.Gold).value;
        var after = CHMData.Instance.GetCollectionData(CHMMain.String.Gold).value += arg.gold * 3;

        //adBtn.gameObject.SetActive(false);
        received = true;

        Debug.Log($"Gold {before} => {after}");

        CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
        {
            stringID = 63
        });
    }
}
