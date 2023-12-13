using DG.Tweening;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIStageSelectArg;
    }

    private void Start()
    {
        normalStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            arg.stageSelect?.Invoke(1);
            CHMMain.UI.CloseUI(gameObject);
        });
        
        bossStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            arg.stageSelect?.Invoke(2);
            CHMMain.UI.CloseUI(gameObject);
        });
    }
}