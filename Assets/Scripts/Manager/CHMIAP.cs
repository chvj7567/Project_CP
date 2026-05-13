using System;
using UnityEngine.Purchasing;

// 글로벌 CHMIAP 어댑터: 게임 코드의 CHMIAP.Instance.X 호출을 패키지 ChvjUnityInfra.CHMIAP에 위임.
// 상품은 Assets/Resources/ChvjUnityInfra/IAPProductConfig.asset에서 로드됨.
public class CHMIAP
{
    public class PurchaseState
    {
        public string productName;
        public Defines.EPurchase state;
    }

    public Action<PurchaseState> purchaseState;

    private static CHMIAP _instance;
    public static CHMIAP Instance => _instance ??= new CHMIAP();

    private CHMIAP()
    {
        // 패키지 purchaseState 이벤트 구독: 패키지 ChvjUnityInfra.EPurchase → 게임 Defines.EPurchase 변환
        ChvjUnityInfra.CHMIAP.Instance.purchaseState += pkgState =>
        {
            purchaseState?.Invoke(new PurchaseState
            {
                productName = pkgState.productName,
                state = pkgState.state == ChvjUnityInfra.EPurchase.Success
                    ? Defines.EPurchase.Success
                    : Defines.EPurchase.Failure,
            });
        };
    }

    public bool IsInitialized => ChvjUnityInfra.CHMIAP.Instance.IsInitialized;
    public bool IsConsumableType(string productName) => ChvjUnityInfra.CHMIAP.Instance.IsConsumableType(productName);
    public void Init() => ChvjUnityInfra.CHMIAP.Instance.Init();
    public void Purchase(string productName) => ChvjUnityInfra.CHMIAP.Instance.Purchase(productName);
    public void RestorePurchase() => ChvjUnityInfra.CHMIAP.Instance.RestorePurchase();
    public bool HadPurchased(string productName) => ChvjUnityInfra.CHMIAP.Instance.HadPurchased(productName);
    public Product GetProduct(string productName) => ChvjUnityInfra.CHMIAP.Instance.GetProduct(productName);
    public decimal GetPrice(string productID) => ChvjUnityInfra.CHMIAP.Instance.GetPrice(productID);
    public string GetPriceUnit(string productID) => ChvjUnityInfra.CHMIAP.Instance.GetPriceUnit(productID);
    public bool CanBuyFromID(string productID) => ChvjUnityInfra.CHMIAP.Instance.CanBuyFromID(productID);
    public bool CanBuyFromName(string productName) => ChvjUnityInfra.CHMIAP.Instance.CanBuyFromName(productName);
}
