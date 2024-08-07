using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using UniRx.Triggers;
using TMPro;

[RequireComponent(typeof(Button))]
public class CHButton : MonoBehaviour
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

    private void Start()
    {
        button.OnPointerEnterAsObservable().Subscribe(_ =>
        {
            //button.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .5f);
        });

        button.OnPointerExitAsObservable().Subscribe(_ =>
        {
            //button.transform.DOScale(new Vector3(1f, 1f, 1f), .5f);
        });

        button.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.Sound.Play(Defines.ESound.Cat);
        });
    }
}
