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

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIMissionArg;
    }

    private void Start()
    {
        scrollView.SetItemList(CHMMain.Json.GetMissionInfoList());
    }
}
