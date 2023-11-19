using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHAdvertise : MonoBehaviour
{
    List<int> adStageList = new List<int> { 2, 5, 8, 0 };
    public bool GetAdvertise(int stage)
    {
        SetRemoveAD();

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData == null || loginData.buyRemoveAD)
            return false;

        var checkAdStage = stage % 10;

        if (false == adStageList.Contains(checkAdStage))
        {
            return false;
        }

        CHMAdmob.Instance.ShowInterstitialAd();

        return true;
    }

    void SetRemoveAD()
    {
        var checkPurchase = CHMIAP.Instance.HadPurchased(CHMMain.String.Product_Name_RemoveAD);
        if (checkPurchase)
        {
            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            if (loginData != null && false == loginData.buyRemoveAD)
            {
                loginData.buyRemoveAD = true;
                CHMData.Instance.SaveData(CHMMain.String.CatPang);
            }
        }
    }
}
