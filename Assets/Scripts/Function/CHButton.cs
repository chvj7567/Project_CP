using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using UniRx.Triggers;

[RequireComponent(typeof(Button))]
public class CHButton : MonoBehaviour
{
    [NonSerialized]
    public Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        button.OnPointerEnterAsObservable().Subscribe(_ =>
        {
            button.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .5f);
        });

        button.OnPointerExitAsObservable().Subscribe(_ =>
        {
            button.transform.DOScale(new Vector3(1f, 1f, 1f), .5f);
        });

        button.OnClickAsObservable().Subscribe(_ =>
        {
            
        });
    }
}
