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

    public class PurchaseState
    {
        public string productName;
        public Defines.EPurchase state;
    }

    List<IAPInfo> productList = new List<IAPInfo>();

    IStoreController iStoreController; // 구매 과정을 제어하는 함수 제공
    IExtensionProvider iExtensionProvider; // 여러 플랫폼을 위한 확장 처리 제공

    public bool IsInitialized => iStoreController != null && iExtensionProvider != null;

    public Action<PurchaseState> purchaseState;

    public bool IsConsumableType(string productName)
    {
        var product = productList.Find(_ => _.productName == productName);
        if (product == null)
            return false;

        return product.productType == ProductType.Consumable;
    }

    public void Init()
    {
        if (IsInitialized)
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        productList.Add(new IAPInfo
        {
            productName = CHMMain.String.Product_Name_RemoveAD,
            productID = CHMMain.String.Product_ID_RemoveAD,
            productType = ProductType.NonConsumable
        });

        productList.Add(new IAPInfo
        {
            productName = CHMMain.String.Product_Name_AddTime,
            productID = CHMMain.String.Product_ID_AddTime,
            productType = ProductType.Consumable
        });

        productList.Add(new IAPInfo
        {
            productName = CHMMain.String.Product_Name_AddMove,
            productID = CHMMain.String.Product_ID_AddMove,
            productType = ProductType.Consumable
        });

        for (int i = 0; i < productList.Count; ++i)
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

        if (purchaseState != null)
            purchaseState.Invoke(new PurchaseState
            {
                productName = id,
                state = Defines.EPurchase.Success
            });

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason error)
    {
        var id = product.definition.id;
        Debug.Log($"구매 실패 - ID : {id}\n{error}");

        if (purchaseState != null)
            purchaseState.Invoke(new PurchaseState
            {
                productName = id,
                state = Defines.EPurchase.Failure
            });
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
        if (IsInitialized == false)
            return;

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log($"구매 복구 시도");

            var appleExt = iExtensionProvider.GetExtension<IAppleExtensions>();
            appleExt.RestoreTransactions(result => Debug.Log($"구매 복구 시도 결과 - {result}"));
        }
    }

    public bool HadPurchased(string productName)
    {
        if (IsInitialized == false)
            return false;

        Debug.Log($"Had Purchased 1 {productName}");

        var product = GetProduct(productName);
        if (product == null)
            return false;

        Debug.Log($"Had Purchased 2 {product.hasReceipt}");

        return product.hasReceipt;
    }

    public UnityEngine.Purchasing.Product GetProduct(string productName)
    {
        return iStoreController.products.WithID(productName);
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

    public bool CanBuyFromID(string productID)
    {
        var product = productList.Find(_ => _.productID == productID);
        if (product == null)
            return false;

        if (IsConsumableType(product.productID) == false)
        {
            if (HadPurchased(product.productName))
            {
                Debug.Log("이미 구매한 상품");
                return false;
            }
        }

        return true;
    }

    public bool CanBuyFromName(string productName)
    {
        var product = productList.Find(_ => _.productName == productName);
        if (product == null)
            return false;

        if (false == IsConsumableType(product.productID))
        {
            if (HadPurchased(product.productName))
            {
                Debug.Log("이미 구매한 상품");
                return false;
            }
        }

        return true;
    }
}