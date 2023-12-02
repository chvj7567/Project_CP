using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIBoomArg : CHUIArg
{
    
}

public class UIBoom : UIBase
{
    UIBoomArg arg;

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
        arg = _uiArg as UIBoomArg;
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
        });

        englishBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            loginData.languageType = Defines.ELanguageType.English;

            CHMData.Instance.SaveData(CHMMain.String.CatPang);

            Debug.Log(loginData.languageType);
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
