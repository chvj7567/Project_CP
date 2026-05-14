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
    [SerializeField] private GameObject successObj;
    [SerializeField] private GameObject failedObj;
    [SerializeField] private CHText goldText;
    [SerializeField] private CHText goldx2Text;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button retryBtn;
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
            if (successObj != null) successObj.SetActive(false);
            if (failedObj != null) failedObj.SetActive(true);

            resultText.DOText("Failed...", 1f);
            goldText.SetText(0);
            goldx2Text.SetText(0);
        }
        else if (arg.result == Defines.EGameState.GameClear)
        {
            if (successObj != null) successObj.SetActive(true);
            if (failedObj != null) failedObj.SetActive(false);

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

            int nextStage = currentStage + 1;

            // 다음 스테이지 PlayerPrefs도 갱신 — LBLobbyScene이 StageSelect 흐름에서 이 값을 기준으로 페이지를 잡는다.
            switch (selectStage)
            {
                case ESelectStage.Hard: PlayerPrefs.SetInt(CHMString.Instance.HardStage, nextStage); break;
                case ESelectStage.Boss: PlayerPrefs.SetInt(CHMString.Instance.BossStage, nextStage); break;
                case ESelectStage.Normal: PlayerPrefs.SetInt(CHMString.Instance.NormalStage, nextStage); break;
            }

            // FirstScene 진입 후 자동으로 다음 스테이지의 UIGameStart 띄우도록 요청.
            LBLobbyScene.fromGame = true;
            LBLobbyScene.pendingShowGameStartStage = nextStage;

            // 게임 상태 정리 (claimBtn과 동일 패턴).
            Time.timeScale = 1;
            CHInstantiateButton.ResetBlockDict();
            CHMPool.Instance.Clear();

            SceneManager.LoadScene(1);

        }).AddTo(this);

        if (retryBtn != null)
        {
            retryBtn.OnClickAsObservable().Subscribe(_ =>
            {
                int currentStage = 0;
                Defines.ESelectStage selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMString.Instance.SelectStage);
                switch (selectStage)
                {
                    case ESelectStage.Hard: currentStage = PlayerPrefs.GetInt(CHMString.Instance.HardStage); break;
                    case ESelectStage.Boss: currentStage = PlayerPrefs.GetInt(CHMString.Instance.BossStage); break;
                    case ESelectStage.Normal: currentStage = PlayerPrefs.GetInt(CHMString.Instance.NormalStage); break;
                }

                // 같은 스테이지로 재시작 — PlayerPrefs는 그대로 두고, 로비 진입 후 동일 stage의 UIGameStart 자동 표시.
                LBLobbyScene.fromGame = true;
                LBLobbyScene.pendingShowGameStartStage = currentStage;

                Time.timeScale = 1;
                CHInstantiateButton.ResetBlockDict();
                CHMPool.Instance.Clear();

                SceneManager.LoadScene(1);

            }).AddTo(this);
        }

        claimBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (received == true)
            {
                // 로비 진입 시 startBtn 단계 건너뛰고 스테이지 메뉴 직행.
                LBLobbyScene.fromGame = true;

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

            // 로비 진입 시 startBtn 단계 건너뛰고 스테이지 메뉴 직행.
            LBLobbyScene.fromGame = true;

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
