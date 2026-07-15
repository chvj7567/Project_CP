using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;
using UniRx;

public class ShopScrollViewItem : MonoBehaviour
{
    [SerializeField] UIShop shopScript;
    [SerializeField] List<GameObject> shopImgList = new List<GameObject>();
    [SerializeField] Button buyBtn;
    [SerializeField] CHText productName;
    [SerializeField] CHText costText;
    [SerializeField] Button skinSelectBtn;
    [SerializeField] CHText descText;
    [SerializeField] GameObject objGold;
    [SerializeField] GameObject objWon;

    Infomation.ShopInfo info;

    Data.Collection collectionData;
    Data.Shop shopData;

    // 능력치 강화 상품의 shopID
    const int ShopIdHpUpgrade = 7;
    const int ShopIdAttackUpgrade = 8;
    // HP 강화 상품 구매 시 증가하는 HP
    const int HpUpgradeAmount = 10;

    void Start()
    {
        ChvjUnityInfra.CHMIAP.Instance.purchaseState += (purchaseState) =>
        {
            if (purchaseState.productName != info.productName)
                return;

            switch (purchaseState.state)
            {
                case ChvjUnityInfra.EPurchase.Success:
                    {
                        shopData.buy = true;
                    }
                    break;
                case ChvjUnityInfra.EPurchase.Failure:
                    break;
            }
        };

        buyBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (shopData == null || collectionData == null)
                return;

            if (CanBuy() == false)
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 60
                });

                return;
            }

            if (collectionData.value < info.gold)
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
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
                    Data.Login loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
                    loginData.selectCatShop = info.skinIndex;
                }

                BuyGoods(info.shopID);

                CHMData.Instance.SaveData(CHMString.Instance.CatPang);
                CHMUI.Instance.CloseUI(Defines.EUI.UIShop);
                
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 62
                });
            }
            else
            {
                ChvjUnityInfra.CHMIAP.Instance.Purchase(info.productName);
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

        collectionData = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold);
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
            decimal price = ChvjUnityInfra.CHMIAP.Instance.GetPrice(info.productName);
            string priceUnit = ChvjUnityInfra.CHMIAP.Instance.GetPriceUnit(info.productName);

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
            if (ChvjUnityInfra.CHMIAP.Instance.CanBuyFromName(info.productName) == false)
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
            case ShopIdHpUpgrade:
                {
                    Data.Login loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
                    loginData.hp += HpUpgradeAmount;
                }
                break;
            case ShopIdAttackUpgrade:
                {
                    Data.Login loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
                    loginData.attack += 1;
                }
                break;
        }
    }
}
