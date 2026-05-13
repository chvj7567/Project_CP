using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Defines;

public class UIGameEndArg : CHUIArg
{
    public Defines.EClearState clearState = Defines.EClearState.None;
    public Defines.EGameState result = Defines.EGameState.None;
    public int gold;
}

public class UIGameEnd : UIBase
{
    private UIGameEndArg arg;

    [SerializeField] private TMP_Text resultText;
    [SerializeField] private CHText goldText;
    [SerializeField] private CHText goldx2Text;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button adBtn;
    [SerializeField] private Button claimBtn;

    private bool received = false;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIGameEndArg;
    }

    private void OnDestroy()
    {
        ChvjUnityInfra.CHMAdmob.Instance.AcquireReward -= AcquireReward;
    }

    private void Start()
    {
        if (arg.result == Defines.EGameState.GameOver)
        {
            resultText.DOText("Failed...", 1f);
            goldText.SetText(0);
            goldx2Text.SetText(0);
        }
        else if (arg.result == Defines.EGameState.GameClear)
        {
            resultText.DOText("CLEAR!", 1f);

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

        ChvjUnityInfra.CHMAdmob.Instance.AcquireReward += AcquireReward;

        BindUI();
    }

    private void BindUI()
    {
        nextBtn.OnClickAsObservable().Subscribe(_ =>
        {
            int currentStage = 0;
            Defines.ESelectStage selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMString.Instance.SelectStage);
            switch (selectStage)
            {
                case ESelectStage.Hard: currentStage = PlayerPrefs.GetInt(CHMString.Instance.HardStage); break;
                case ESelectStage.Boss: currentStage = PlayerPrefs.GetInt(CHMString.Instance.BossStage); break;
                case ESelectStage.Normal: currentStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage); break;
            }

            CHMUI.Instance.ShowUI(Defines.EUI.UIGameStart, new UIGameStartArg
            {
                stage = currentStage + 1
            });

        }).AddTo(this);

        claimBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (received == true)
            {
                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMUI.Instance.CloseUI(Defines.EUI.UIAlarm);
                CHMPool.Instance.Clear();

                SceneManager.LoadScene(1);
                return;
            }

            var before = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value;
            var after = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value += arg.gold;

            received = true;

            Debug.Log($"Gold {before} => {after}");

            CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 63
            });

            Time.timeScale = 1;
            CHInstantiateButton.ResetBlockDict();
            CHMUI.Instance.CloseUI(Defines.EUI.UIAlarm);
            CHMPool.Instance.Clear();

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);
            SceneManager.LoadScene(1);
        }).AddTo(this);

        adBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (ChvjUnityInfra.CHMIAP.Instance.CanBuyFromID(CHMString.Instance.Product_ID_RemoveAD) == false)
            {
                AcquireReward();
            }
            else
            {
                ChvjUnityInfra.CHMAdmob.Instance.ShowRewardedAd();
            }
        }).AddTo(this);
    }

    private void AcquireReward()
    {
        if (received == true)
        {
            CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                stringID = 64
            });

            return;
        }

        var before = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value;
        var after = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value += arg.gold * 3;

        received = true;

        Debug.Log($"Gold {before} => {after}");

        CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
        {
            stringID = 63
        });
    }
}
