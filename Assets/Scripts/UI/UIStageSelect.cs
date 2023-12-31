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
    [SerializeField] Button hardStageBtn;
    [SerializeField] Button bossStageBtn;

    [SerializeField] GameObject normalLockObj;
    [SerializeField] GameObject hardLockObj;
    [SerializeField] GameObject bossLockObj;

    bool hardStageLock = false;
    bool bossStageLock = false;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIStageSelectArg;
    }

    private void Start()
    {
        normalLockObj.SetActive(false);
        hardLockObj.SetActive(false);
        bossLockObj.SetActive(false);

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);

        var hardLockValue = CHMMain.Json.GetConstValueInfo(Defines.EConstValue.HardStage_NormalStageLock);
        if (hardLockValue > loginData.normalStage)
        {
            hardStageLock = true;
            hardLockObj.SetActive(true);
        }

        var bossLockValue = CHMMain.Json.GetConstValueInfo(Defines.EConstValue.BossStage_HardStageLock);
        if (bossLockValue > loginData.hardStage)
        {
            bossStageLock = true;
            bossLockObj.SetActive(true);
        }

        normalStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Normal);
            CHMMain.UI.CloseUI(gameObject);
        });

        hardStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (hardStageLock)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 110,
                    intValue = (int)hardLockValue
                });

                return;
            }

            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Hard);
            CHMMain.UI.CloseUI(gameObject);
        });
        
        bossStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (bossStageLock)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 111,
                    intValue = (int)bossLockValue
                });

                return;
            }

            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Boss);
            CHMMain.UI.CloseUI(gameObject);
        });
    }
}
