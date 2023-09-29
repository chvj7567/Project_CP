using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ShopScrollViewItem : MonoBehaviour
{
    [SerializeField] UIShop shopScript;
    [SerializeField] List<GameObject> shopImgList = new List<GameObject>();
    [SerializeField] Button buyBtn;
    [SerializeField] CHTMPro buyGoldText;
    [SerializeField] Button skinSelectBtn;

    Infomation.ShopInfo info;

    Data.Collection collectionData;
    Data.Shop shopData;

    void Start()
    {
        buyBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (shopData == null || collectionData == null)
                return;

            if (shopData.buy == true)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Already Buy"
                });

                return;
            }

            if (collectionData.value < info.gold)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    alarmText = "Not Enough Gold"
                });

                return;
            }

            CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
            {
                alarmText = "Buy Success"
            });

            shopData.buy = true;
            collectionData.value -= info.gold;
            skinSelectBtn.gameObject.SetActive(true);

            shopScript.SetCurrentSkin(info.shopID - 1);

            CHMMain.UI.CloseUI(Defines.EUI.UIShop);
        });

        skinSelectBtn.OnClickAsObservable().Subscribe(_ =>
        {
            shopScript.SetCurrentSkin(info.shopID - 1);
        });
    }

    public void Init(int _index, Infomation.ShopInfo _info)
    {
        info = _info;

        collectionData = CHMData.Instance.GetCollectionData(CHMMain.String.gold);
        shopData = CHMData.Instance.GetShopData(_info.shopID.ToString());

        buyGoldText.SetText(info.gold);

        SetImage(info.shopID);

        skinSelectBtn.gameObject.SetActive(shopData.buy);
    }

    void SetImage(int _selectCatShop)
    {
        for (int i = 0; i < shopImgList.Count; ++i)
        {
            if (i == _selectCatShop - 1)
            {
                shopImgList[i].SetActive(true);
            }
            else
            {
                shopImgList[i].SetActive(false);
            }
        }
    }
}
