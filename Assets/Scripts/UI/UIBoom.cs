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

    [SerializeField] Button soundBtn;
    [SerializeField] GameObject soundObj;
    [SerializeField] Button boomBtn;
    [SerializeField] GameObject boomObj;

    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider effectSlider;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIBoomArg;
    }

    private void Start()
    {
        soundBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (false == soundObj.activeSelf)
            {
                soundObj.SetActive(true);
                boomObj.SetActive(false);
            }
        });

        boomBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (false == boomObj.activeSelf)
            {
                boomObj.SetActive(true);
                soundObj.SetActive(false);
            }
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
