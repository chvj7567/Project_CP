using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    [ReadOnly] public Defines.EUI eUIType;
    [ReadOnly] public int uid = 0;

    [SerializeField] Button backgroundBtn;
    [SerializeField] Button backBtn;

    protected Action actBack;

    private void Awake()
    {
        if (backgroundBtn)
        {
            backgroundBtn.OnClickAsObservable().Subscribe(_ =>
            {
                actBack?.Invoke();
                CHMMain.UI.CloseUI(gameObject);
            }).AddTo(this);
        }

        if (backBtn)
        {
            backBtn.OnClickAsObservable().Subscribe(_ =>
            {
                actBack?.Invoke();
                CHMMain.UI.CloseUI(gameObject);
            }).AddTo(this);
        }
    }

    public virtual void InitUI(CHUIArg _uiArg) { }

    public virtual void CloseUI() { }
}
