using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 글로벌 CHButton은 패키지 ChvjUnityInfra.CHButton을 상속해 prefab의 script GUID를 보존하면서
// 게임 특화 필드(text/clearObj/lockObj/unlockObj 등)를 함께 유지한다.
// P9 후속 정리에서 prefab을 [패키지 CHButton + LBStageButton]로 분리 후 삭제 예정.
public class CHButton : ChvjUnityInfra.CHButton
{
    [NonSerialized] public Button button;
    [NonSerialized] public Image image;
    [NonSerialized] public RectTransform rectTransform;

    public TMP_Text text;
    public GameObject clearObj;
    public GameObject lockObj;
    public GameObject unlockObj;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }
}
