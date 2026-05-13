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
            CHMData.Instance.DeleteData(CHMString.Instance.CatPang, (ret) =>
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 126
                });

#if UNITY_ANDROID
                ChvjUnityInfra.CHMGPGS.Instance.Logout();
#endif

                SceneManager.LoadScene(1);
            });
        });

        noBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMUI.Instance.CloseUI(gameObject);
        });
    }
}
