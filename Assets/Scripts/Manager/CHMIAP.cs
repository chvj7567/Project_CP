using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Purchasing;

public class CHMIAP : CHSingleton<CHMIAP>, IStoreListener
{
    public const string ProductConsumable = "test";
    public const string ProductNonConsumable = "NonConsumable";
    public const string ProductSubscription = "Subscription";

    public const string _AOS_ConsumableID = "com.studio.app.consumable";
    public const string _IOS_ConsumableID = "com.studio.app.consumable";

    public const string _AOS_NonConsumableID = "com.studio.app.nonConsumable";
    public const string _IOS_NonConsumableID = "com.studio.app.nonConsumable";

    public const string _AOS_SubscriptionID = "com.studio.app.subscription";
    public const string _IOS_SubscriptionID = "com.studio.app.subscription";

    IStoreController iStoreController; // 구매 과정을 제어하는 함수 제공
    IExtensionProvider iExtensionProvider; // 여러 플랫폼을 위한 확장 처리 제공

    public bool IsInitialized => iStoreController != null && iExtensionProvider != null;

    public void Init()
    {
        if (IsInitialized)
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(
            ProductConsumable, ProductType.Consumable, new IDs()
            {
                {ProductConsumable, GooglePlay.Name},
                //{_IOS_ConsumableID, AppleAppStore.Name}
            });

        /*builder.AddProduct(
            ProductNonConsumable, ProductType.NonConsumable, new IDs()
            {
                {_AOS_NonConsumableID, GooglePlay.Name},
                {_IOS_NonConsumableID, AppleAppStore.Name}
            });

        builder.AddProduct(
            ProductSubscription, ProductType.Subscription, new IDs()
            {
                {_AOS_SubscriptionID, GooglePlay.Name},
                {_IOS_SubscriptionID, AppleAppStore.Name}
            });*/

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

        switch (id)
        {
            case ProductConsumable:
                {
                    Debug.Log($"구매 성공 처리 : {id}");
                }
                break;
            case ProductNonConsumable:
                {
                    Debug.Log($"구매 성공 처리 : {id}");
                }
                break;
            case ProductSubscription:
                {
                    Debug.Log($"구매 성공 처리 : {id}");
                }
                break;
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason error)
    {
        Debug.LogWarning($"구매 실패 - ID : {product.definition.id}\n{error}");
    }

    public void Purchase(string productID)
    {
        if (false == IsInitialized)
            return;

        var product = GetProduct(productID);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"구매 시도 - ID : {product.definition.id}");
            iStoreController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log($"구매 시도 불가 - ID : {productID}");
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