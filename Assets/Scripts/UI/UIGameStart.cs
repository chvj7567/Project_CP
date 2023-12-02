using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameStartArg : CHUIArg
{
    public int stage;
}

public class UIGameStart : UIBase
{
    UIGameStartArg arg;

    [SerializeField] CHTMPro stageText;
    [SerializeField] CHTMPro myMoveItemCountText;
    [SerializeField] CHTMPro myTimeItemCountText;
    [SerializeField] CHTMPro useAddMoveItemCountText;
    [SerializeField] CHTMPro useAddTimeItemCountText;

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

        stageText.SetText(arg.stage);

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData == null)
            return;

        maxMoveItemCount = myAddMoveItemCount = loginData.addMoveItemCount;
        maxTimeItemCount = myAddTimeItemCount = loginData.addTimeItemCount;

        myMoveItemCountText.SetText(myAddMoveItemCount);
        myTimeItemCountText.SetText(myAddTimeItemCount);

        useAddMoveItemCountText.SetText(useAddMoveItemCount);
        useAddTimeItemCountText.SetText(useAddTimeItemCount);
    }

    void InitBtn()
    {
        if (IsInitBtn)
            return;

        IsInitBtn = true;

        startBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
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

            CHMData.Instance.SaveData(CHMMain.String.CatPang);

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
