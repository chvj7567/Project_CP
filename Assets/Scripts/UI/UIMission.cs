using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIMissionArg : CHUIArg
{
    
}

public class UIMission : UIBase
{
    UIMissionArg arg;

    [SerializeField] MissionScrollView scrollView;
    [SerializeField] Button normalTapBtn;
    [SerializeField] Button specialTapBtn;
    [SerializeField] CHTMPro curTapText;

    [SerializeField, ReadOnly] int curTapIndex;
    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIMissionArg;
    }

    private void Start()
    {
        normalTapBtn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex = 1;
            curTapText.SetStringID(121);
            scrollView.SetItemList(CHMMain.Json.GetMissionInfoList(curTapIndex));
        });

        specialTapBtn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex = 2;
            curTapText.SetStringID(122);
            scrollView.SetItemList(CHMMain.Json.GetMissionInfoList(curTapIndex));
        });

        curTapIndex = 1;
        curTapText.SetStringID(121);
        scrollView.SetItemList(CHMMain.Json.GetMissionInfoList(curTapIndex));
    }
}
