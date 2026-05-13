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

        InitBtn();

        // 인스턴스 재사용 대비 상태 리셋
        useAddMoveItemCount = 0;
        useAddTimeItemCount = 0;

        var selectStage = (Defines.ESelectStage)PlayerPrefs.GetInt(CHMString.Instance.SelectStage);
        bool isBoss = selectStage == Defines.ESelectStage.Boss;

        if (isBoss) stageText.SetText(arg.stage - CHMData.Instance.BossStageStartValue);
        else stageText.SetText(arg.stage);

        var stageInfo = CHMJson.Instance.GetStageInfo(arg.stage);
        if (stageInfo != null)
        {
            if (stageInfo.targetScore < 0) targetScoreText.SetStringID(135);
            else { targetScoreText.SetStringID(1); targetScoreText.SetText(stageInfo.targetScore); }

            if (selectStage == Defines.ESelectStage.Normal || stageInfo.time < 0) timeText.SetStringID(135);
            else { timeText.SetStringID(1); timeText.SetText(stageInfo.time); }

            if (stageInfo.moveCount < 0) moveCountText.SetStringID(135);
            else { moveCountText.SetStringID(1); moveCountText.SetText(stageInfo.moveCount); }
        }

        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData != null)
        {
            maxMoveItemCount = myAddMoveItemCount = loginData.addMoveItemCount;
            maxTimeItemCount = myAddTimeItemCount = loginData.addTimeItemCount;

            myMoveItemCountText.SetText(myAddMoveItemCount);
            myTimeItemCountText.SetText(myAddTimeItemCount);

            useAddMoveItemCountText.SetText(useAddMoveItemCount);
            useAddTimeItemCountText.SetText(useAddTimeItemCount);

            // SetStringID로 베이스 텍스트 리셋 후 SetPlusString. 리셋 없이 재진입하면 " + N"이 누적됨.
            addMoveItemValueText.SetStringID(16);
            addMoveItemValueText.SetPlusString(CHMJson.Instance.GetConstValueInfo(Defines.EConstValue.AddMoveItemValue).ToString());
            addTimeItemValueText.SetStringID(17);
            addTimeItemValueText.SetPlusString(CHMJson.Instance.GetConstValueInfo(Defines.EConstValue.AddTimeItemValue).ToString());
        }

        SetItemsActive(!isBoss);
    }

    void SetItemsActive(bool active)
    {
        objItems.SetActive(active);

        myMoveItemCountText.gameObject.SetActive(active);
        myTimeItemCountText.gameObject.SetActive(active);
        useAddMoveItemCountText.gameObject.SetActive(active);
        useAddTimeItemCountText.gameObject.SetActive(active);
        addMoveItemValueText.gameObject.SetActive(active);
        addTimeItemValueText.gameObject.SetActive(active);

        myAddMoveItemBtn.gameObject.SetActive(active);
        myAddTimeItemBtn.gameObject.SetActive(active);
        addMoveItemBtn.gameObject.SetActive(active);
        addTimeItemBtn.gameObject.SetActive(active);
    }

    void InitBtn()
    {
        if (IsInitBtn)
            return;

        IsInitBtn = true;

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
    }
}
