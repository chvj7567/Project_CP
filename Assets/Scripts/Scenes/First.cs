using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class First : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] List<Image> backgroundList = new List<Image>();
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] GameObject stageSelect1;
    [SerializeField] GameObject stageSelect2;
    [SerializeField] PageMove pageMove;
    [SerializeField] TMP_Text downloadText;
    [SerializeField] Button missionBtn;
    [SerializeField] Button startBtn;
    [SerializeField] Button connectGPGSBtn;
    [SerializeField] TMP_Text connectText;
    [SerializeField] Button logoutBtn;
    [SerializeField] Button shopBtn;
    [SerializeField] Button boomBtn;
    [SerializeField, ReadOnly] int backgroundIndex = 0;
    [SerializeField] CHLoadingBarFromAssetBundle bundleLoadingScript;
    [SerializeField] CHAdvertise adScript;
    [SerializeField] ReactiveProperty<bool> dataDownload = new ReactiveProperty<bool>();
    [SerializeField] ReactiveProperty<bool> bundleDownload = new ReactiveProperty<bool>();

    CancellationTokenSource tokenSource;

    async void Start()
    {
        CHMIAP.Instance.Init();

        tokenSource = new CancellationTokenSource();

        loadingBar.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
        downloadText.gameObject.SetActive(false);
        stageSelect1.SetActive(false);
        stageSelect2.SetActive(false);
        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(false);
        connectGPGSBtn.gameObject.SetActive(false);
        logoutBtn.gameObject.SetActive(false);
        shopBtn.gameObject.SetActive(false);
        boomBtn.gameObject.SetActive(false);

        connectText.text = "Google";
        downloadText.text = "";

        startBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (bundleDownload.Value == false || dataDownload.Value == false) return;

            pageMove.Init(PlayerPrefs.GetInt(CHMMain.String.Stage));
            startBtn.gameObject.SetActive(false);
            missionBtn.gameObject.SetActive(true);
            shopBtn.gameObject.SetActive(true);
            boomBtn.gameObject.SetActive(true);
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            loadingBar.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);

            // 기본 스킨
            CHMData.Instance.GetShopData("1").buy = true;

            PlayerPrefs.SetInt(CHMMain.String.Background, backgroundIndex);

            CHMMain.Sound.Play(Defines.ESound.Bgm);
        });

        missionBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.UI.ShowUI(Defines.EUI.UIMission, new CHUIArg());
        });

        logoutBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (GetLoginState() == false)
                return;

            SetLoginState(false);
            connectText.text = "Google";
#if UNITY_EDITOR == false
            CHMGPGS.Instance.Logout();
#endif
        });

        shopBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.UI.ShowUI(Defines.EUI.UIShop, new CHUIArg());
        });

        boomBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.UI.ShowUI(Defines.EUI.UIBoom, new CHUIArg());
        });

        connectGPGSBtn.OnPointerClickAsObservable().Subscribe(_ =>
        {
            if (CHMData.Instance.loginDataDic.TryGetValue(CHMMain.String.CatPang, out var data))
            {
                if (data.connectGPGS == true)
                    return;

                connectText.text = "Login...";
#if UNITY_EDITOR == false
                CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    if (success)
                    {
                        PlayerPrefs.SetInt(CHMMain.String.Login, 1);
                        Debug.Log("GPGS Login Success");
                        connectText.text = "Login Success";
                        dataDownload.Value = true;

                        SetLoginState(true);
                    }
                    else
                    {
                        Debug.Log("GPGS Login Failed");
                        connectText.text = "Login Failed";
                    }
                });
#else
                PlayerPrefs.SetInt(CHMMain.String.Login, 0);
                connectText.text = "Login Failed";
                data.connectGPGS = false;
#endif
            }
            else
            {
                connectText.text = "LoginData Load Failed";
            }
        });

        dataDownload.Subscribe(_ =>
        {
            if (CHMAssetBundle.Instance.firstDownload == true && _ == true && bundleDownload.Value == true)
            {
                CHMAssetBundle.Instance.firstDownload = false;
                downloadText.gameObject.SetActive(false);
                startBtn.gameObject.SetActive(true);
            }
        });

        bundleLoadingScript.bundleLoadSuccess += () =>
        {
            bundleDownload.Value = true;
        };

        bundleDownload.Subscribe(async _ =>
        {
            if (_ == true && dataDownload.Value == false)
            {
#if UNITY_EDITOR == false
            if (GetPhoneLoginState() == true)
            {
                downloadText.text = "Login...";
                CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    if (success)
                    {
                        Debug.Log("GPGS Login Success");
                        connectText.text = "Login Success";
                        await CHMData.Instance.LoadCloudData(CHMMain.String.CatPang);
                        dataDownload.Value = true;

                        SetLoginState(true);
                    }
                    else
                    {
                        Debug.Log("GPGS Login Failed");
                        connectText.text = "Login Failed";

                        await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
                        SetLoginState(false);
                        dataDownload.Value = true;
                    }
                });
            }
            else
            {
                await CHMJson.Instance.Init();
                Debug.Log($"@JsonPercent{CHMJson.Instance.GetJsonLoadingPercent()}");
                await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
                SetLoginState(false);
                dataDownload.Value = true;
            }
#else
                await CHMJson.Instance.Init();
                Debug.Log($"@JsonPercent{CHMJson.Instance.GetJsonLoadingPercent()}");
                await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
                SetLoginState(false);
                dataDownload.Value = true;
#endif
                //CHMData.Instance.SaveData(CHMMain.String.catPang);
            }
            else if (CHMAssetBundle.Instance.firstDownload == true && _ == true && dataDownload.Value == true)
            {
                CHMAssetBundle.Instance.firstDownload = false;
                downloadText.gameObject.SetActive(false);
                startBtn.gameObject.SetActive(true);
            }
        });

        if (CHMAssetBundle.Instance.firstDownload == true)
        {
            pageMove.ActiveMoveBtn(false);
            loadingBar.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(true);
            downloadText.gameObject.SetActive(true);
            bundleLoadingScript.Init();
            
            CHMAdmob.Instance.Init();

            backgroundIndex = 0;
        }
        else
        {
            backgroundIndex = PlayerPrefs.GetInt(CHMMain.String.Background);

            bundleDownload.Value = true;
            dataDownload.Value = true;

            var stage = PlayerPrefs.GetInt(CHMMain.String.Stage);
            pageMove.Init(stage);
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            missionBtn.gameObject.SetActive(true);
            shopBtn.gameObject.SetActive(true);
            boomBtn.gameObject.SetActive(true);

            var login = GetLoginState();
            connectGPGSBtn.gameObject.SetActive(login == false);
            logoutBtn.gameObject.SetActive(login);

            adScript.GetAdvertise(stage);
        }

        ChangeBackgroundLoop();

        loadingBar.fillAmount = 0f;
    }

    private void OnApplicationQuit()
    {
        CHMData.Instance.SaveData(CHMMain.String.CatPang);
    }

    bool SetLoginState(bool _active)
    {
        if (CHMData.Instance.loginDataDic.TryGetValue(CHMMain.String.CatPang, out var data) == false)
            return false;

        data.connectGPGS = _active;

        PlayerPrefs.SetInt(CHMMain.String.Login, 0);

        connectGPGSBtn.gameObject.SetActive(_active == false);
        logoutBtn.gameObject.SetActive(_active);

        //CHMData.Instance.SaveData(CHMMain.String.catPang);

        return true;
    }

    bool GetLoginState()
    {
        if (CHMData.Instance.loginDataDic.TryGetValue(CHMMain.String.CatPang, out var data) == false)
            return false;

        return data.connectGPGS;
    }

    bool GetPhoneLoginState()
    {
        return PlayerPrefs.GetInt(CHMMain.String.Login) == 1;
    }

    void CopyFile(string _fileDirectoryPath, string _destDirectoryPath, string _fileName)
    {
        if (Directory.Exists(_fileDirectoryPath) == false)
            return;

        if (Directory.Exists(_destDirectoryPath) == false)
        {
            Directory.CreateDirectory(_destDirectoryPath);
        }

        File.Copy(Path.Combine(_fileDirectoryPath, _fileName), Path.Combine(_destDirectoryPath, _fileName));

        Debug.Log($"Copy Success : {_fileName}");
    }

    async Task ChangeBackgroundLoop()
    {
        for (int i = 0; i < backgroundList.Count; ++i)
        {
            if (i != backgroundIndex)
            {
                Color color = backgroundList[i].color;
                color.a = 0f;
                backgroundList[i].color = color;
            }
            else
            {
                Color color = backgroundList[i].color;
                color.a = 1f;
                backgroundList[i].color = color;
            }
        }

        await Task.Delay(5000, tokenSource.Token);

        try
        {
            while (true)
            {
                backgroundIndex = ChangeBackground();

                await Task.Delay(10000, tokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancle Change Background");
        }
    }

    int ChangeBackground()
    {
        if (backgroundIndex >= backgroundList.Count)
            return 0;

        int nextIndex = backgroundIndex + 1;
        if (nextIndex >= backgroundList.Count)
        {
            nextIndex = 0;
        }

        backgroundList[backgroundIndex].DOFade(0f, 5f);
        backgroundList[nextIndex].DOFade(1f, 5f);

        return nextIndex;
    }
}
