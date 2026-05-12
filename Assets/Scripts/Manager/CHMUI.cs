using UnityEngine;

public class CHMUI : ChvjUnityInfra.CHSingletonStatic<CHMUI>
{
    public void ShowUI(Defines.EUI _uiType, CHUIArg _uiArg)
    {
        ChvjUnityInfra.CHMUI.Instance.ShowUI(_uiType, _uiArg);
    }

    public void CloseUI(GameObject _uiObj)
    {
        if (_uiObj == null) return;
        var ui = _uiObj.GetComponent<UIBase>();
        if (ui != null) ui.Close(false);
    }

    public void CloseUI(Defines.EUI _uiType)
    {
        ChvjUnityInfra.CHMUI.Instance.CloseUI(_uiType, false);
    }
}
