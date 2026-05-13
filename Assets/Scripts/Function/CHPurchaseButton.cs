using System;
using UnityEngine;
using ChvjUnityInfra;
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
    [SerializeField] CHText priceText;
    [SerializeField] CHText priceUnitText;

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
        if (false == ChvjUnityInfra.CHMIAP.Instance.IsConsumableType(targetProductID))
        {
            if (ChvjUnityInfra.CHMIAP.Instance.HadPurchased(targetProductID))
            {
                Debug.Log("이미 구매한 상품");
                return;
            }
        }

        var price = ChvjUnityInfra.CHMIAP.Instance.GetPrice(targetProductID);
        var priceUnit = ChvjUnityInfra.CHMIAP.Instance.GetPriceUnit(targetProductID);

        SetPrice(price, priceUnit);

        ChvjUnityInfra.CHMIAP.Instance.Purchase(targetProductID);
    }

    public void SetPrice(decimal price, string priceUnit)
    {
        if (priceText) priceText.SetText(price);
        if (priceUnitText) priceUnitText.SetText(priceUnit);
    }
}
