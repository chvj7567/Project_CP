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
        var checkPurchase = CHMIAP.Instance.HadPurchased(CHMMain.String.Product_Name_RemoveAD);
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData == null)
            return;

        if (checkPurchase)
        {
            loginData.buyRemoveAD = true;

            var shopData = CHMData.Instance.GetShopData("0");
            shopData.buy = true;

            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }

        var gold = CHMData.Instance.GetCollectionData(CHMMain.String.Gold).value;
        goldText.SetText(gold);

        var shopScriptList = CHMMain.Json.GetShopInfoListAll();
        if (shopScriptList == null)
        {
            Debug.Log("Shop Script is Null");
            return;
        }

        curTapIndex.Subscribe(tapIndex =>
        {
            var shopList = shopScriptList.FindAll(_ => _.tapIndex == tapIndex);

            scrollView.SetItemList(shopList);
        });

        loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData != null)
        {
            SetCurrentSkin(loginData.selectCatShop);
        }

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

                    CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        alarmText = "Purchase Success"
                    });
                }
                break;
            case Defines.EPurchase.Failure:
                {
                    CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        alarmText = "Purchase Failure"
                    });
                }
                break;
        }
    }

    void PurchaseSuccess(string productName)
    {
        Debug.Log($"PurchaseSuccess {productName}");

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData == null)
            return;

        if (productName == CHMMain.String.Product_Name_RemoveAD)
        {
            loginData.buyRemoveAD = true;
        }
        else if (productName == CHMMain.String.Product_Name_AddTime)
        {
            loginData.addTimeItemCount += 10;
        }
        else if (productName == CHMMain.String.Product_Name_AddMove)
        {
            loginData.addMoveItemCount += 10;
        }

        CHMData.Instance.SaveData(CHMMain.String.CatPang);
        CHMMain.UI.CloseUI(Defines.EUI.UIShop);
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

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData != null)
        {
            loginData.selectCatShop = skinIndex;
        }
    }
}
