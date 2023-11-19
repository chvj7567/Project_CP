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
