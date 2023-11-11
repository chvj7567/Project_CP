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
    [SerializeField] CHTMPro priceText;
    [SerializeField] CHTMPro priceUnitText;

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
                Debug.Log("이미 구매한 상품");
                return;
            }
        }

        var price = CHMIAP.Instance.GetPrice(targetProductID);
        var priceUnit = CHMIAP.Instance.GetPriceUnit(targetProductID);

        SetPrice(price, priceUnit);

        CHMIAP.Instance.Purchase(targetProductID);
    }

    public void SetPrice(decimal price, string priceUnit)
    {
        if (priceText) priceText.SetText(price);
        if (priceUnitText) priceUnitText.SetText(priceUnit);
    }
}
