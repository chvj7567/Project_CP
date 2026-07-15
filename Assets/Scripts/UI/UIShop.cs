using System.Collections.Generic;
using UnityEngine;
using ChvjUnityInfra;
using UniRx;
using static ChvjUnityInfra.CHMIAP;
using UnityEngine.UI;

public class UIShopArg : CHUIArg
{
    
}

public class UIShop : UIBase
{
    UIShopArg arg;

    [SerializeField] CHText goldText;
    [SerializeField] CHText hpText;
    [SerializeField] CHText attackText;
    [SerializeField] Button tap1Btn;
    [SerializeField] Button tap2Btn;
    [SerializeField] ShopScrollView scrollView;
    [SerializeField] List<GameObject> skinImgList = new List<GameObject>();

    [SerializeField, ReadOnly] public ReactiveProperty<int> curTapIndex = new ReactiveProperty<int>();

    // 상점 탭 인덱스 — 스킨 탭 / 캐시(IAP) 탭
    const int ShopTabSkin = 1;
    const int ShopTabCash = 2;
    // IAP 상품 구매 시 지급하는 아이템 수량
    const int PurchaseItemGrantCount = 100;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIShopArg;

        //# 매 ShowUI(재진입) 마다 골드/HP/공격력을 현재 데이터로 갱신
        RefreshStatus();
    }

    //# 골드/HP/공격력 텍스트를 현재 데이터로 갱신 (InitUI에서 매 진입 호출)
    private void RefreshStatus()
    {
        Data.Login loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData == null)
            return;

        hpText.SetText(loginData.hp);
        attackText.SetText(loginData.attack);

        int gold = CHMData.Instance.GetCollectionData(CHMString.Instance.Gold).value;
        goldText.SetText(gold);
    }

    private void Start()
    {
        bool checkPurchase = ChvjUnityInfra.CHMIAP.Instance.HadPurchased(CHMString.Instance.Product_Name_RemoveAD);
        Data.Login loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData == null)
            return;

        if (checkPurchase)
        {
            Data.Shop shopData = CHMData.Instance.GetShopData("0");
            shopData.buy = true;

            CHMData.Instance.SaveData(CHMString.Instance.CatPang);
        }

        List<Infomation.ShopInfo> shopScriptList = CHMJson.Instance.GetShopInfoListAll();
        if (shopScriptList == null)
        {
            Debug.Log("Shop Script is Null");
            return;
        }

        curTapIndex.Subscribe(tapIndex =>
        {
            List<Infomation.ShopInfo> shopList = shopScriptList.FindAll(_ => _.tapIndex == tapIndex);

            if (tapIndex == ShopTabCash && ChvjUnityInfra.CHMIAP.Instance.IsInitialized == false)
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 109
                });

                ChvjUnityInfra.CHMIAP.Instance.Init();
                return;
            }

            scrollView.SetItemList(shopList);
        });

        SetCurrentSkin(loginData.selectCatShop);

        tap1Btn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex.Value = ShopTabSkin;
        });

        tap2Btn.OnClickAsObservable().Subscribe(_ =>
        {
            curTapIndex.Value = ShopTabCash;
        });

        ChvjUnityInfra.CHMIAP.Instance.purchaseState += PurchaseState;

        curTapIndex.Value = 1;
    }

    private void OnDestroy()
    {
        ChvjUnityInfra.CHMIAP.Instance.purchaseState -= PurchaseState;
    }

    void PurchaseState(PurchaseState purchaseState)
    {
        switch (purchaseState.state)
        {
            case ChvjUnityInfra.EPurchase.Success:
                {
                    PurchaseSuccess(purchaseState.productName);

                    CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                    {
                        stringID = 62
                    });
                }
                break;
            case ChvjUnityInfra.EPurchase.Failure:
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

        Data.Login loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData == null)
            return;
        
        if (productName == CHMString.Instance.Product_Name_AddTime)
        {
            loginData.addTimeItemCount += PurchaseItemGrantCount;
        }
        else if (productName == CHMString.Instance.Product_Name_AddMove)
        {
            loginData.addMoveItemCount += PurchaseItemGrantCount;
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

        Data.Login loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData != null)
        {
            loginData.selectCatShop = skinIndex;
        }
    }
}
