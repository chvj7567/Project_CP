using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIBoomArg : CHUIArg
{
    
}

public class UIBoom : UIBase
{
    UIBoomArg arg;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIBoomArg;
    }
}
