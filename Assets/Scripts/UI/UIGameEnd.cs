using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
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
    [SerializeField] private CHTMPro goldText;
    [SerializeField] private CHTMPro goldx2Text;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button adBtn;
    [SerializeField] private Button backBtn;

    private bool received = false;

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

        CHMAdmob.Instance.AcquireReward += AcquireReward;

        BindUI();
    }

    private void BindUI()
    {
        nextBtn.OnClickAsObservable().Subscribe(_ =>
        {
            int currentStage = 0;
            Defines.ESelectStage selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage);
            switch (selectStage)
            {
                case ESelectStage.Hard: currentStage = PlayerPrefs.GetInt(CHMMain.String.HardStage); break;
                case ESelectStage.Boss: currentStage = PlayerPrefs.GetInt(CHMMain.String.BossStage); break;
                case ESelectStage.Normal: currentStage = PlayerPrefs.GetInt(CHMMain.String.NormalStage); break;
            }

            CHMMain.UI.ShowUI(Defines.EUI.UIGameStart, new UIGameStartArg
            {
                stage = currentStage + 1
            });

        }).AddTo(this);

        backBtn.OnClickAsObservable().Subscribe(_ =>
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
        }).AddTo(this);

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
        }).AddTo(this);
    }

    private void AcquireReward()
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

        received = true;

        Debug.Log($"Gold {before} => {after}");

        CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
        {
            stringID = 63
        });
    }
}
