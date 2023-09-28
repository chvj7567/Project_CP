using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIShopArg : CHUIArg
{
    
}

public class UIShop : UIBase
{
    UIShopArg arg;

    [SerializeField] CHTMPro goldText;
    [SerializeField] ShopScrollView scrollView;

    public override void InitUI(CHUIArg _uiArg)
    {
        arg = _uiArg as UIShopArg;
    }

    private void Start()
    {
        var gold = CHMData.Instance.GetCollectionData(CHMMain.String.gold).value;
        goldText.SetText(gold);

        var shopScript = CHMMain.Json.GetShopInfoList();
        if (shopScript == null)
        {
            Debug.Log("Shop Script is Null");
            return;
        }

        scrollView.SetItemList(shopScript);
    }
}
