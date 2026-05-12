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

        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);

        var hardLockValue = CHMJson.Instance.GetConstValueInfo(Defines.EConstValue.HardStage_NormalStageLock);
        if (hardLockValue > loginData.normalStage)
        {
            hardStageLock = true;
            hardLockObj.SetActive(true);
        }

        var bossLockValue = CHMJson.Instance.GetConstValueInfo(Defines.EConstValue.BossStage_HardStageLock);
        if (bossLockValue > loginData.hardStage)
        {
            bossStageLock = true;
            bossLockObj.SetActive(true);
        }

        normalStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Normal);
            CHMUI.Instance.CloseUI(gameObject);
        });

        hardStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (hardStageLock)
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 110,
                    intValue = (int)hardLockValue
                });

                return;
            }

            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Hard);
            CHMUI.Instance.CloseUI(gameObject);
        });
        
        bossStageBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (bossStageLock)
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 111,
                    intValue = (int)bossLockValue
                });

                return;
            }

            arg.stageSelect?.Invoke((int)Defines.ESelectStage.Boss);
            CHMUI.Instance.CloseUI(gameObject);
        });
    }
}
