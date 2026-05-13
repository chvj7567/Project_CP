using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameStartArg : CHUIArg
{
    public int stage;
}

public class UIGameStart : UIBase
{
    UIGameStartArg arg;

    [SerializeField] GameObject objItems;

    [SerializeField] CHText stageText;
    [SerializeField] CHText targetScoreText;
    [SerializeField] CHText timeText;
    [SerializeField] CHText moveCountText;
    [SerializeField] CHText myMoveItemCountText;
    [SerializeField] CHText myTimeItemCountText;
    [SerializeField] CHText useAddMoveItemCountText;
    [SerializeField] CHText useAddTimeItemCountText;
    [SerializeField] CHText addMoveItemValueText;
    [SerializeField] CHText addTimeItemValueText;

    [SerializeField] Button myAddMoveItemBtn;
    [SerializeField] Button myAddTimeItemBtn;
    [SerializeField] Button addMoveItemBtn;
    [SerializeField] Button addTimeItemBtn;
    [SerializeField] Button startBtn;

    bool IsInitBtn = false;

    int maxMoveItemCount = 0;
    int maxTimeItemCount = 0;
    int myAddMoveItemCount = 0;
    int myAddTimeItemCount = 0;
    int useAddMoveItemCount = 0;
    int useAddTimeItemCount = 0;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIGameStartArg;
    }

    private void Start()
    {
        InitBtn();

        if (PlayerPrefs.GetInt(CHMString.Instance.SelectStage) == (int)Defines.ESelectStage.Boss)
        {
            stageText.SetText(arg.stage - CHMData.Instance.BossStageStartValue);
        }
        else
        {
            stageText.SetText(arg.stage);
        }

        var stageInfo = CHMJson.Instance.GetStageInfo(arg.stage);
        if (stageInfo != null)
        {
            if (stageInfo.targetScore < 0) targetScoreText.SetStringID(135);
            else targetScoreText.SetText(stageInfo.targetScore);

            if (stageInfo.time < 0) timeText.SetStringID(135);
            else timeText.SetText(stageInfo.time);

            if (stageInfo.moveCount < 0) moveCountText.SetStringID(135);
            else moveCountText.SetText(stageInfo.moveCount);
        }

        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData == null)
            return;

        maxMoveItemCount = myAddMoveItemCount = loginData.addMoveItemCount;
        maxTimeItemCount = myAddTimeItemCount = loginData.addTimeItemCount;

        myMoveItemCountText.SetText(myAddMoveItemCount);
        myTimeItemCountText.SetText(myAddTimeItemCount);

        useAddMoveItemCountText.SetText(useAddMoveItemCount);
        useAddTimeItemCountText.SetText(useAddTimeItemCount);

        addMoveItemValueText.SetPlusString(CHMJson.Instance.GetConstValueInfo(Defines.EConstValue.AddMoveItemValue).ToString());
        addTimeItemValueText.SetPlusString(CHMJson.Instance.GetConstValueInfo(Defines.EConstValue.AddTimeItemValue).ToString());
    }

    void InitBtn()
    {
        if (IsInitBtn)
            return;

        IsInitBtn = true;

        objItems.SetActive(true);

        startBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
            if (loginData == null)
                return;

            if (loginData.addMoveItemCount >= useAddMoveItemCount)
            {
                loginData.addMoveItemCount -= useAddMoveItemCount;
                loginData.useMoveItemCount = useAddMoveItemCount;
            }

            if (loginData.addTimeItemCount >= useAddTimeItemCount)
            {
                loginData.addTimeItemCount -= useAddTimeItemCount;
                loginData.useTimeItemCount = useAddTimeItemCount;
            }

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);

            SceneManager.LoadScene(2);
        });

        myAddMoveItemBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (myAddMoveItemCount >= maxMoveItemCount)
                return;

            myMoveItemCountText.SetText(++myAddMoveItemCount);
            useAddMoveItemCountText.SetText(--useAddMoveItemCount);
        });

        myAddTimeItemBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (myAddTimeItemCount >= maxTimeItemCount)
                return;

            myTimeItemCountText.SetText(++myAddTimeItemCount);
            useAddTimeItemCountText.SetText(--useAddTimeItemCount);
        });

        addMoveItemBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (myAddMoveItemCount <= 0)
                return;

            myMoveItemCountText.SetText(--myAddMoveItemCount);
            useAddMoveItemCountText.SetText(++useAddMoveItemCount);
        });

        addTimeItemBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (myAddTimeItemCount <= 0)
                return;

            myTimeItemCountText.SetText(--myAddTimeItemCount);
            useAddTimeItemCountText.SetText(++useAddTimeItemCount);
        });

        if (PlayerPrefs.GetInt(CHMString.Instance.SelectStage) == (int)Defines.ESelectStage.Boss)
        {
            myMoveItemCountText.gameObject.SetActive(false);
            myTimeItemCountText.gameObject.SetActive(false);
            useAddMoveItemCountText.gameObject.SetActive(false);
            useAddTimeItemCountText.gameObject.SetActive(false);
            addMoveItemValueText.gameObject.SetActive(false);
            addTimeItemValueText.gameObject.SetActive(false);

            myAddMoveItemBtn.gameObject.SetActive(false);
            myAddTimeItemBtn.gameObject.SetActive(false);
            addMoveItemBtn.gameObject.SetActive(false);
            addTimeItemBtn.gameObject.SetActive(false);

            objItems.SetActive(false);
        }
    }
}
