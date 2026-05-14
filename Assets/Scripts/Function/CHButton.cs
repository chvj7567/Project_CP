using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 글로벌 CHButton은 패키지 ChvjUnityInfra.CHButton을 상속해 prefab의 script GUID를 보존하면서
// 게임 특화 필드(text/clearObj/lockObj/unlockObj 등)를 함께 유지한다.
// P9 후속 정리에서 prefab을 [패키지 CHButton + LBStageButton]로 분리 후 삭제 예정.
public class CHButton : ChvjUnityInfra.CHButton
{
    // 비활성 GameObject에서 접근될 수 있으므로 Awake에 의존하지 않고 lazy 캐싱.
    private Button _buttonCache;
    private Image _imageCache;
    private RectTransform _rectTransformCache;

    public Button button
    {
        get
        {
            if (_buttonCache == null) _buttonCache = GetComponent<Button>();
            return _buttonCache;
        }
    }

    public Image image
    {
        get
        {
            if (_imageCache == null) _imageCache = GetComponent<Image>();
            return _imageCache;
        }
    }

    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransformCache == null) _rectTransformCache = GetComponent<RectTransform>();
            return _rectTransformCache;
        }
    }

    public TMP_Text text;
    public GameObject clearObj;
    public GameObject lockObj;
    public GameObject unlockObj;
}
