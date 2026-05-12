using System.Collections.Generic;
using UnityEngine;
using UniRx;
using static CHMIAP;
using UnityEngine.UI;

public class UIShopArg : CHUIArg
{
    
}

public class UIShop : UIBase
{
    UIShopArg arg;

    [SerializeField] CHTMPro goldText;
    [SerializeField] CHTMPro hpText;
    [SerializeField] CHTMPro attackText;
    [SerializeField] Button tap1Btn;
    [SerializeField] Button tap2Btn;
    [SerializeField] ShopScrollView scrollView;
    [SerializeField] List<GameObject> skinImgList = new List<GameObject>();

    [SerializeField, ReadOnly] public ReactiveProperty<int> curTapIndex = new ReactiveProperty<int>();

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIShopArg;
    }

    private void Start()
    {
        var checkPurchase = CHMIAP.Instance.HadPurchased(CHMString.Instance.Product_Name_RemoveAD);
        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData == null)
            return;

        hpText.SetText(loginData.hp);
        attackText.SetText(loginData.attack);

        if (checkPurchase)
        {
            var shopData = CHMData.Instance.GetShopData("0");
            shopData.buy = true;

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);
        }

        var gold = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value;
        goldText.SetText(gold);

        var shopScriptList = CHMJson.Instance.GetShopInfoListAll();
        if (shopScriptList == null)
        {
            Debug.Log("Shop Script is Null");
            return;
        }

        curTapIndex.Subscribe(tapIndex =>
        {
            var shopList = shopScriptList.FindAll(_ => _.tapIndex == tapIndex);

            if (tapIndex == 2 && CHMIAP.Instance.IsInitialized == false)
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 109
                });

                CHMIAP.Instance.Init();
                return;
            }

            scrollView.SetItemList(shopList);
        });

        SetCurrentSkin(loginData.selectCatShop);

        tap1Btn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex.Value = 1;
        });

        tap2Btn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex.Value = 2;
        });

        CHMIAP.Instance.purchaseState += PurchaseState;

        curTapIndex.Value = 1;
    }

    private void OnDestroy()
    {
        CHMIAP.Instance.purchaseState -= PurchaseState;
    }

    void PurchaseState(PurchaseState purchaseState)
    {
        switch (purchaseState.state)
        {
            case Defines.EPurchase.Success:
                {
                    PurchaseSuccess(purchaseState.productName);

                    CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        stringID = 62
                    });
                }
                break;
            case Defines.EPurchase.Failure:
                {
                    CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        stringID = 65
                    });
                }
                break;
        }
    }

    void PurchaseSuccess(string productName)
    {
        Debug.Log($"PurchaseSuccess {productName}");

        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData == null)
            return;
        
        if (productName == CHMString.Instance.Product_Name_AddTime)
        {
            loginData.addTimeItemCount += 10;
        }
        else if (productName == CHMString.Instance.Product_Name_AddMove)
        {
            loginData.addMoveItemCount += 10;
        }

        CHMData.Instance.SaveData(CHMString.Instance.CatPang);
        CHMUI.Instance.CloseUI(Defines.EUI.UIShop);
    }

    public void SetCurrentSkin(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= skinImgList.Count)
            return;

        for (int i = 0; i < skinImgList.Count; ++i)
        {
            if (i == skinIndex)
            {
                skinImgList[i].SetActive(true);
            }
            else
            {
                skinImgList[i].SetActive(false);
            }
        }

        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData != null)
        {
            loginData.selectCatShop = skinIndex;
        }
    }
}
