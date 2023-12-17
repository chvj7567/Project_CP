using DG.Tweening;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIStageSelectArg : CHUIArg
{
    public Action<int> stageSelect;
}

public class UIStageSelect : UIBase
{
    UIStageSelectArg arg;

    [SerializeField] Button normalStageBtn;
    [SerializeField] Button bossStageBtn;
    [SerializeField] Button easyStageBtn;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIStageSelectArg;
    }

    private void Start()
    {
        normalStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Normal);
            CHMMain.UI.CloseUI(gameObject);
        });
        
        bossStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Boss);
            CHMMain.UI.CloseUI(gameObject);
        });

        easyStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Easy);
            CHMMain.UI.CloseUI(gameObject);
        });
    }
}
