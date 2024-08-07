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
    [SerializeField] CHTMPro productName;
    [SerializeField] CHTMPro costText;
    [SerializeField] Button skinSelectBtn;
    [SerializeField] CHTMPro descText;
    [SerializeField] GameObject objGold;
    [SerializeField] GameObject objWon;

    Infomation.ShopInfo info;

    Data.Collection collectionData;
    Data.Shop shopData;

    void Start()
    {
        CHMIAP.Instance.purchaseState += (purchaseState) =>
        {
            if (purchaseState.productName != info.productName)
                return;

            switch (purchaseState.state)
            {
                case Defines.EPurchase.Success:
                    {
                        shopData.buy = true;
                    }
                    break;
                case Defines.EPurchase.Failure:
                    break;
            }
        };

        buyBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (shopData == null || collectionData == null)
                return;

            if (CanBuy() == false)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 60
                });

                return;
            }

            if (collectionData.value < info.gold)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 61
                });

                return;
            }

            if (info.gold >= 0)
            {
                shopData.buy = true;
                collectionData.value -= info.gold;

                if (info.skinIndex > 0)
                {
                    var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
                    loginData.selectCatShop = info.skinIndex;
                }

                BuyGoods(info.shopID);

                CHMData.Instance.SaveData(CHMMain.String.CatPang);
                CHMMain.UI.CloseUI(Defines.EUI.UIShop);
                
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 62
                });
            }
            else
            {
                CHMIAP.Instance.Purchase(info.productName);
            }
        });

        skinSelectBtn.OnClickAsObservable().Subscribe(_ =>
        {
            shopScript.SetCurrentSkin(info.skinIndex);
        });
    }

    public void Init(int index, Infomation.ShopInfo info)
    {
        this.info = info;

        collectionData = CHMData.Instance.GetCollectionData(CHMMain.String.Gold);
        shopData = CHMData.Instance.GetShopData(info.shopID.ToString());

        descText.SetStringID(info.descStringID);

        Debug.Log($"ShopID {shopData.key} {shopData.buy}");

        if (info.gold >= 0)
        {
            costText.SetText(info.gold, "Gold");

            objGold.SetActive(true);
            objWon.SetActive(false);
        }
        else
        {
            var price = CHMIAP.Instance.GetPrice(info.productName);
            var priceUnit = CHMIAP.Instance.GetPriceUnit(info.productName);

            costText.SetText(price, priceUnit);

            objGold.SetActive(false);
            objWon.SetActive(true);
        }

        SetImage(info.shopID);

        productName.SetStringID(info.titleStringID);
        skinSelectBtn.gameObject.SetActive(CanBuy() == false);
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

    bool CanBuy()
    {
        if (info.productName == "")
        {
            if (info.skinIndex >= 0 && shopData.buy)
            {
                return false;
            }
        }
        else
        {
            if (CHMIAP.Instance.CanBuyFromName(info.productName) == false)
            {
                return false;
            }
        }

        return true;
    }

    void BuyGoods(int shopID)
    {
        switch (shopID)
        {
            case 7:
                {
                    var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
                    loginData.hp += 10;
                }
                break;
            case 8:
                {
                    var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
                    loginData.attack += 1;
                }
                break;
        }
    }
}
