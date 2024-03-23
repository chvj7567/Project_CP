using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIDataDeleteArg : CHUIArg
{
    
}

public class UIDataDelete : UIBase
{
    UIDataDeleteArg arg;

    [SerializeField] Button yesBtn;
    [SerializeField] Button noBtn;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIDataDeleteArg;
    }

    private void Start()
    {
        yesBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMData.Instance.DeleteData(CHMMain.String.CatPang, (ret) =>
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 126
                });

                CHMGPGS.Instance.Logout();

                SceneManager.LoadScene(1);
            });
        });

        noBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.UI.CloseUI(gameObject);
        });
    }
}
