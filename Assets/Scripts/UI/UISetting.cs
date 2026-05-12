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

    [SerializeField] GameObject soundObj;

    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider effectSlider;

    [SerializeField] GameObject languageObj;
    [SerializeField] Button koreanBtn;
    [SerializeField] Button englishBtn;

    [SerializeField] GameObject guideInitObj;
    [SerializeField] Button guideInitBtn;

    [SerializeField] GameObject deleteObj;
    [SerializeField] Button deleteBtn;

    [SerializeField] GameObject colorObj;
    [SerializeField] Slider redSlider;
    [SerializeField] Slider greenSlider;
    [SerializeField] Slider blueSlider;
    [SerializeField] Slider alphaSlider;
    [SerializeField] Image blockBackground;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UISettingArg;
    }

    private void Start()
    {
        actBack += () =>
        {
            SaveSetting();
        };

        koreanBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
            loginData.languageType = Defines.ELanguageType.Korea;

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);

            Debug.Log(loginData.languageType);

            SceneManager.LoadScene(1);
        });

        englishBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
            loginData.languageType = Defines.ELanguageType.English;

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);

            Debug.Log(loginData.languageType);

            SceneManager.LoadScene(1);
        });

        bgmSlider.value = CHMSound.Instance.bgmVolume;
        effectSlider.value = CHMSound.Instance.effectVolume;

        bgmSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            CHMSound.Instance.SetBGMVolume(_);
        });

        effectSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            CHMSound.Instance.SetEffectVolume(_);
        });

        guideInitBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
            loginData.guideIndex = 0;

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);

            SceneManager.LoadScene(1);
        });

        deleteBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMUI.Instance.ShowUI(Defines.EUI.UIDataDelete, new UIDataDeleteArg());
        });

        redSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            blockBackground.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
        });

        greenSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            blockBackground.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
        });

        blueSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            blockBackground.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
        });

        alphaSlider.OnValueChangedAsObservable().Subscribe(_ =>
        {
            blockBackground.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
        });

        redSlider.value = PlayerPrefs.GetFloat(CHMString.Instance.Red);
        greenSlider.value = PlayerPrefs.GetFloat(CHMString.Instance.Green);
        blueSlider.value = PlayerPrefs.GetFloat(CHMString.Instance.Blue);
        alphaSlider.value = PlayerPrefs.GetFloat(CHMString.Instance.Alpha);
        blockBackground.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
    }

    void SaveSetting()
    {
        PlayerPrefs.SetFloat(CHMString.Instance.Red, redSlider.value);
        PlayerPrefs.SetFloat(CHMString.Instance.Green, greenSlider.value);
        PlayerPrefs.SetFloat(CHMString.Instance.Blue, blueSlider.value);
        PlayerPrefs.SetFloat(CHMString.Instance.Alpha, alphaSlider.value);
    }
}
