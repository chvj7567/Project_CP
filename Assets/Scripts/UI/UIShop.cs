using System.Collections.Generic;
using UnityEngine;
using static CHMIAP;

public class UIShopArg : CHUIArg
{
    
}

public class UIShop : UIBase
{
    UIShopArg arg;

    [SerializeField] CHTMPro goldText;
    [SerializeField] ShopScrollView scrollView;
    [SerializeField] List<GameObject> skinImgList = new List<GameObject>();

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIShopArg;
    }

    private void Start()
    {
        var gold = CHMData.Instance.GetCollectionData(CHMMain.String.Gold).value;
        goldText.SetText(gold);

        var shopScript = CHMJson.Instance.GetShopInfoList();
        if (shopScript == null)
        {
            Debug.Log("Shop Script is Null");
            return;
        }

        scrollView.SetItemList(shopScript);

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData != null)
        {
            SetCurrentSkin(loginData.selectCatShop);
        }

        CHMIAP.Instance.purchaseState += PurchaseState;
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
            loginData.addTimeItemCount += 1;
        }
        else if (productName == CHMMain.String.Product_Name_AddMove)
        {
            loginData.addMoveItemCount += 1;
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
