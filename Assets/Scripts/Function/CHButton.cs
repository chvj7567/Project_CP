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

    public TMP_Text text;
    public GameObject clearObj;
    public GameObject lockObj;
    public GameObject unlockObj;

    private void Awake()
    {
        button = GetComponent<Button>();
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
            
        });
    }
}
