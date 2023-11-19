using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Purchasing;

public class CHMIAP : CHSingleton<CHMIAP>, IStoreListener
{
    public class IAPInfo
    {
        public string productName;
        public string productID;
        public ProductType productType;
    }

    List<IAPInfo> productList = new List<IAPInfo>();

    IStoreController iStoreController; // 구매 과정을 제어하는 함수 제공
    IExtensionProvider iExtensionProvider; // 여러 플랫폼을 위한 확장 처리 제공

    public bool IsInitialized => iStoreController != null && iExtensionProvider != null;

    public Action<Defines.EPurchase> purchaseState;

    public bool IsConsumableType(string productName)
    {
        var product = productList.Find(_ => _.productName == productName);
        if (product == null)
            return false;

        return product.productType == ProductType.Consumable;
    }

    public void PurchaseProduct(string productName)
    {
        var product = productList.Find(_ => _.productName == productName);
        if (product == null)
            return;

        switch (productName)
        {
            case "RemoveAD":
                {
                    Debug.Log($"구매 성공 처리 : {productName}");

                    var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
                    if (loginData == null)
                        return;

                    loginData.buyRemoveAD = true;

                    CHMData.Instance.SaveData(CHMMain.String.CatPang);
                }
                break;
        }
    }

    public void Init()
    {
        if (IsInitialized)
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        productList.Add(new IAPInfo
        {
            productName = CHMMain.String.Product_RemoveAD,
            productID = "com.catpang.product1",
            productType = ProductType.NonConsumable
        });
        
        for(int i = 0; i < productList.Count; ++i)
        {
            var product = productList[i];

            builder.AddProduct(product.productName, product.productType, new IDs
            {
                {product.productID, GooglePlay.Name }
            });
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extension)
    {
        Debug.Log($"유니티 IAP 초기화 성공");
        iStoreController = controller;
        iExtensionProvider = extension;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"유니티 IAP 초기화 실패 {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"유니티 IAP 초기화 실패 {error}\n{message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var id = args.purchasedProduct.definition.id;
        Debug.Log($"구매 성공 - ID : {id}");

        PurchaseProduct(id);

        if (purchaseState != null)
            purchaseState.Invoke(Defines.EPurchase.Success);

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason error)
    {
        Debug.LogWarning($"구매 실패 - ID : {product.definition.id}\n{error}");

        if (purchaseState != null)
            purchaseState.Invoke(Defines.EPurchase.Failure);
    }

    public void Purchase(string productName)
    {
        if (false == IsInitialized)
            return;

        var product = GetProduct(productName);

        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"구매 시도 - ID : {product.definition.id}");
            iStoreController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log($"구매 시도 불가 - ID : {productName}");
        }
    }

    public void RestorePurchase()
    {
        if (false == IsInitialized)
            return;

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log($"구매 복구 시도");

            var appleExt = iExtensionProvider.GetExtension<IAppleExtensions>();
            appleExt.RestoreTransactions(result => Debug.Log($"구매 복구 시도 결과 - {result}"));
        }
    }

    public bool HadPurchased(string productID)
    {
        if (false == IsInitialized)
            return false;

        var product = GetProduct(productID);
        if (product == null)
            return false;

        return product.hasReceipt;
    }

    public UnityEngine.Purchasing.Product GetProduct(string productID)
    {
        return iStoreController.products.WithID(productID);
    }

    public decimal GetPrice(string productID)
    {
        var product = GetProduct(productID);
        if (product == null)
            return 0;

        return product.metadata.localizedPrice;
    }

    public string GetPriceUnit(string productID)
    {
        var product = GetProduct(productID);
        if (product == null)
            return "";

        return product.metadata.isoCurrencyCode;
    }
}