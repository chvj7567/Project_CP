using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISettingArg : CHUIArg
{
    
}

public class UISetting : UIBase
{
    UISettingArg arg;

    [SerializeField] Button etcBtn;
    [SerializeField] GameObject soundObj;
    [SerializeField] Button boomBtn;
    [SerializeField] GameObject boomObj;

    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider effectSlider;

    [SerializeField] GameObject languageObj;
    [SerializeField] Button koreanBtn;
    [SerializeField] Button englishBtn;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UISettingArg;
    }

    private void Start()
    {
        etcBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (false == soundObj.activeSelf)
            {
                soundObj.SetActive(true);
                boomObj.SetActive(false);
                languageObj.SetActive(true);
            }
        });

        boomBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (false == boomObj.activeSelf)
            {
                boomObj.SetActive(true);
                soundObj.SetActive(false);
                languageObj.SetActive(false);
            }
        });

        koreanBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            loginData.languageType = Defines.ELanguageType.Korea;

            CHMData.Instance.SaveData(CHMMain.String.CatPang);

            Debug.Log(loginData.languageType);

            SceneManager.LoadScene(1);
        });

        englishBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            loginData.languageType = Defines.ELanguageType.English;

            CHMData.Instance.SaveData(CHMMain.String.CatPang);

            Debug.Log(loginData.languageType);

            SceneManager.LoadScene(1);
        });

        bgmSlider.value = CHMMain.Sound.bgmVolume;
        effectSlider.value = CHMMain.Sound.effectVolume;

        bgmSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            CHMMain.Sound.SetBGMVolume(_);
        });

        effectSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            CHMMain.Sound.SetEffectVolume(_);
        });
    }
}
