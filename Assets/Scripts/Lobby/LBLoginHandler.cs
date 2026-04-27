using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LBLoginHandler
{
    CHTMPro _userID;
    Button _connectGPGSBtn;
    Button _logoutBtn;
    GameObject _objWait;
    Func<bool, Task> _onLoginComplete;

    public void Init(CHTMPro userID, Button connectGPGSBtn, Button logoutBtn, GameObject objWait, Func<bool, Task> onLoginComplete)
    {
        _userID = userID;
        _connectGPGSBtn = connectGPGSBtn;
        _logoutBtn = logoutBtn;
        _objWait = objWait;
        _onLoginComplete = onLoginComplete;
    }

    public bool GetGPGSLogin()
    {
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        Debug.Log($"GetLoginState : {loginData.connectGPGS}");
        return loginData.connectGPGS;
    }

    public bool GetPhoneLoginState() => PlayerPrefs.GetInt(CHMMain.String.Login) == 1;

    public async Task<bool> SetGPGSLogin(bool success, string gpgsUserName)
    {
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        loginData.connectGPGS = success;
        Debug.Log($"SetLoginState : {loginData.connectGPGS}");

        if (success)
        {
            Debug.Log($"GPGS Login Success : {gpgsUserName}");
#if UNITY_ANDROID
            await CHMData.Instance.LoadCloudData(CHMMain.String.CatPang);
#endif
            _userID.gameObject.SetActive(true);
            _userID.SetText(gpgsUserName);
        }
        else
        {
            _userID.gameObject.SetActive(false);
            Debug.Log($"GPGS Login Failed");
            await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
        }

        CHMData.Instance.GetShopData("1").buy = true;
        PlayerPrefs.SetInt(CHMMain.String.Login, success ? 1 : 0);
        _connectGPGSBtn.gameObject.SetActive(!success);
        _logoutBtn.gameObject.SetActive(success);

        CHMData.Instance.SaveData(CHMMain.String.CatPang);
        if (_onLoginComplete != null) await _onLoginComplete.Invoke(success);
        return true;
    }
}
