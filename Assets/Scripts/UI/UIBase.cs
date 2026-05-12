using System;
using UnityEngine;

public class CHUIArg : ChvjUnityInfra.UIArg
{
    public static readonly CHUIArg empty = new CHUIArg();
}

public abstract class UIBase : ChvjUnityInfra.UIBase
{
    public Defines.EUI eUIType
    {
        get => UIType is Defines.EUI e ? e : Defines.EUI.None;
    }

    // background/back 버튼뿐 아니라 ESC, 프로그램에서 호출한 Close()까지 모든 경로에서 fire한다.
    // 기존 코드는 background/back 클릭 시에만 발생했으므로 widening. 부작용 우려되는 callback은 등록 X.
    protected Action actBack;

    public sealed override void InitUI(ChvjUnityInfra.UIArg arg)
    {
        InitUI(arg as CHUIArg ?? CHUIArg.empty);
    }

    public virtual void InitUI(CHUIArg _uiArg) { }

    public virtual void CloseUI() { }

    public override void Close(bool reuse = true)
    {
        actBack?.Invoke();
        CloseUI();
        base.Close(reuse);
    }
}
