using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ShopScrollViewItem : MonoBehaviour
{
    [SerializeField] UIShop shopScript;
    [SerializeField] List<GameObject> shopImgList = new List<GameObject>();
    [SerializeField] Button buyBtn;
    [SerializeField] CHTMPro buyGoldText;
    [SerializeField] Button skinSelectBtn;

    Infomation.ShopInfo info;

    Data.Collection collectionData;
    Data.Shop shopData;

    void Start()
    {
        buyBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (shopData == null || collectionData == null)
                return;

            if (shopData.buy == true)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Already Buy"
                });

                return;
            }

            if (collectionData.value < info.gold)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Not Enough Gold"
                });

                return;
            }

            if (info.gold >= 0)
            {
                shopData.buy = true;
                collectionData.value -= info.gold;
                skinSelectBtn.gameObject.SetActive(true);
                shopScript.SetCurrentSkin(info.shopID - 1);

                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Buy Success"
                });
            }
            else
            {
                CHMIAP.Instance.Purchase(info.productName);
            }

            CHMMain.UI.CloseUI(Defines.EUI.UIShop);
        });

        skinSelectBtn.OnClickAsObservable().Subscribe(_ =>
        {
            shopScript.SetCurrentSkin(info.shopID - 1);
        });
    }

    public void Init(int index, Infomation.ShopInfo info)
    {
        this.info = info;

        collectionData = CHMData.Instance.GetCollectionData(CHMMain.String.gold);
        shopData = CHMData.Instance.GetShopData(info.shopID.ToString());

        if (info.gold >= 0)
        {
            buyGoldText.SetText(info.gold, "Gold");

            skinSelectBtn.gameObject.SetActive(shopData.buy);
        }
        else
        {
            var price = CHMIAP.Instance.GetPrice(info.productName);
            var priceUnit = CHMIAP.Instance.GetPriceUnit(info.productName);

            buyGoldText.SetText(price, priceUnit);

            var checkBuy = CHMIAP.Instance.HadPurchased(info.productName);

            if (false == CHMIAP.Instance.IsConsumableType(info.productName))
            {
                skinSelectBtn.gameObject.SetActive(checkBuy);
            }
            else
            {
                skinSelectBtn.gameObject.SetActive(false);
            }
        }

        SetImage(info.shopID);
    }

    void SetImage(int selectCatShop)
    {
        for (int i = 0; i < shopImgList.Count; ++i)
        {
            if (i == selectCatShop)
            {
                shopImgList[i].SetActive(true);
            }
            else
            {
                shopImgList[i].SetActive(false);
            }
        }
    }
}
