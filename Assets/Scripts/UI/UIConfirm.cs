using System;
using ChvjUnityInfra;
using UnityEngine;

public enum EConfirmType
{
    Confirm,
    YesNo,
}

public class UIConfirmArg : CHUIArg
{
    public EConfirmType confirmType = EConfirmType.Confirm;
    public string txtTitle;
    public Color colorTitle = Color.black;
    public string txtDesc;
    public Color colorDesc = Color.black;
    public Action onYes;
    public Action onNo;
}

public class UIConfirm : UIBase
{
    UIConfirmArg _arg;

    [SerializeField] CHText txtTitle;
    [SerializeField] CHText txtDesc;
    [SerializeField] CHButton btnYes;
    [SerializeField] CHButton btnNo;

    public override void InitUI(CHUIArg _uiArg)
    {
        _arg = _uiArg as UIConfirmArg;

        txtTitle.SetText(_arg.txtTitle);
        txtTitle.SetColor(_arg.colorTitle);

        txtDesc.SetText(_arg.txtDesc);
        txtDesc.SetColor(_arg.colorDesc);

        switch (_arg.confirmType)
        {
            case EConfirmType.Confirm:
                {
                    btnNo.gameObject.SetActive(false);
                }
                break;
            case EConfirmType.YesNo:
                {
                    btnNo.gameObject.SetActive(true);
                }
                break;
        }

        btnYes.OnClick(() =>
        {
            _arg.onYes?.Invoke();
            CHMUI.Instance.CloseUI(gameObject);
        });

        btnNo.OnClick(() =>
        {
            _arg.onNo?.Invoke();
            CHMUI.Instance.CloseUI(gameObject);
        });
    }
}
