using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using UniRx.Triggers;
using TMPro;

[RequireComponent(typeof(Button))]
public class CHPurchaseButton : MonoBehaviour
{
    [NonSerialized] public Button button;

    [SerializeField] string targetProductID;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.OnClickAsObservable().Subscribe(_ =>
        {
            HandleClick();
        });
    }

    public void HandleClick()
    {
        if (targetProductID == CHMIAP.ProductNonConsumable ||
            targetProductID == CHMIAP.ProductSubscription)
        {
            if (CHMIAP.Instance.HadPurchased(targetProductID))
            {
                Debug.Log("�̹� ������ ��ǰ");
                return;
            }
        }

        CHMIAP.Instance.Purchase(targetProductID);
    }
}
