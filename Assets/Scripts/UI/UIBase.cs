using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    [ReadOnly] public Defines.EUI eUIType;
    [ReadOnly] public int uid = 0;

    [SerializeField] Button backBtn;

    private void Awake()
    {
        GameObject _canvas = GameObject.FindGameObjectWithTag("UICanvas");
        if (_canvas)
        {
            transform.SetParent(_canvas.transform, false);
        }

        if (backBtn)
        {
            backBtn.OnClickAsObservable().Subscribe(_ =>
            {
                CHMMain.UI.CloseUI(gameObject);
            });
        }
    }

    public virtual void InitUI(CHUIArg _uiArg) { }

    public virtual void CloseUI() { }
}
