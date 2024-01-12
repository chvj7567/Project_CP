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
        backAction += () =>
        {
            PlayerPrefs.SetFloat(CHMMain.String.BGMVolume, bgmSlider.value);
            PlayerPrefs.SetFloat(CHMMain.String.EffectVolume, effectSlider.value);
            PlayerPrefs.SetFloat(CHMMain.String.Red, redSlider.value);
            PlayerPrefs.SetFloat(CHMMain.String.Green, greenSlider.value);
            PlayerPrefs.SetFloat(CHMMain.String.Blue, blueSlider.value);
            PlayerPrefs.SetFloat(CHMMain.String.Alpha, alphaSlider.value);
        };

        etcBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (false == soundObj.activeSelf)
            {
                soundObj.SetActive(true);
                boomObj.SetActive(false);
                languageObj.SetActive(true);
                guideInitObj.SetActive(true);
                deleteObj.SetActive(true);
                colorObj.SetActive(true);
            }
        });

        boomBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (false == boomObj.activeSelf)
            {
                boomObj.SetActive(true);
                soundObj.SetActive(false);
                languageObj.SetActive(false);
                guideInitObj.SetActive(false);
                deleteObj.SetActive(false);
                colorObj.SetActive(false);
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

        guideInitBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            loginData.guideIndex = 0;

            CHMData.Instance.SaveData(CHMMain.String.CatPang);

            SceneManager.LoadScene(1);
        });

        deleteBtn.OnClickAsObservable().Subscribe(_ =>
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

        redSlider.value = PlayerPrefs.GetFloat(CHMMain.String.Red);
        greenSlider.value = PlayerPrefs.GetFloat(CHMMain.String.Green);
        blueSlider.value = PlayerPrefs.GetFloat(CHMMain.String.Blue);
        alphaSlider.value = PlayerPrefs.GetFloat(CHMMain.String.Alpha);
        blockBackground.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
    }
}
