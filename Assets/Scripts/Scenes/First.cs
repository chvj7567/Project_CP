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
    [SerializeField] Button logoutBtn;
    [SerializeField] TMP_Text connectText;
    [SerializeField] List<string> liDownloadKey = new List<string>();
    [SerializeField, ReadOnly] int backgroundIndex = 0;

    ReactiveProperty<bool> dataDownload = new ReactiveProperty<bool>();
    ReactiveProperty<bool> bundleDownload = new ReactiveProperty<bool>();
    int downloadCount = 0;

    CancellationTokenSource tokenSource;

    async void Start()
    {
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

        connectText.text = "Connect GPGS";
        downloadText.text = "";
        if (CHMAssetBundle.Instance.firstDownload == true)
        {
            pageMove.ActiveMoveBtn(false);
            loadingBar.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(true);
            downloadText.gameObject.SetActive(true);

            CHMAdmob.Instance.Init();

            backgroundIndex = 0;

            foreach (var key in liDownloadKey)
            {
                StartCoroutine(LoadAssetBundle(key));
            }
        }
        else
        {
            backgroundIndex = PlayerPrefs.GetInt("background");

            bundleDownload.Value = true;
            dataDownload.Value = true;

            pageMove.Init(PlayerPrefs.GetInt(CHMMain.String.stage));
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            missionBtn.gameObject.SetActive(true);

            var login = GetLoginState();
            connectGPGSBtn.gameObject.SetActive(login == false);
            logoutBtn.gameObject.SetActive(login);
        }

        ChangeBackgroundLoop();

        loadingBar.fillAmount = 0f;

        startBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (bundleDownload.Value == false || dataDownload.Value == false) return;

            pageMove.Init(PlayerPrefs.GetInt(CHMMain.String.stage));
            startBtn.gameObject.SetActive(false);
            missionBtn.gameObject.SetActive(true);
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            loadingBar.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);

            PlayerPrefs.SetInt(CHMMain.String.background, backgroundIndex);
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
            connectText.text = "Connect GPGS";
#if UNITY_EDITOR == false
            CHMGPGS.Instance.Logout();
#endif
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
                        await CHMData.Instance.LoadCloudData(CHMMain.String.catPang);
                        dataDownload.Value = true;

                        SetLoginState(true);
                    }
                    else
                    {
                        Debug.Log("GPGS Login Failed");
                        connectText.text = "Login Failed";
                    }
                });
            }
            else
            {
                await CHMData.Instance.LoadLocalData(CHMMain.String.catPang);
                dataDownload.Value = true;
                CHMData.Instance.SaveData(CHMMain.String.catPang);
            }
#else
                await CHMData.Instance.LoadLocalData(CHMMain.String.catPang);
                dataDownload.Value = true;
                CHMData.Instance.SaveData(CHMMain.String.catPang);
#endif
            }
            else if (CHMAssetBundle.Instance.firstDownload == true && _ == true && dataDownload.Value == true)
            {
                CHMAssetBundle.Instance.firstDownload = false;
                downloadText.gameObject.SetActive(false);
                startBtn.gameObject.SetActive(true);
            }
        });

        connectGPGSBtn.OnPointerClickAsObservable().Subscribe(_ =>
        {
            if (CHMData.Instance.loginDataDic.TryGetValue(CHMMain.String.catPang, out var data))
            {
                if (data.connectGPGS == true)
                    return;

                connectText.text = "Login...";
#if UNITY_EDITOR == false
                CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    if (success)
                    {
                        PlayerPrefs.SetInt(CHMMain.String.login, 1);
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
                PlayerPrefs.SetInt(CHMMain.String.login, 0);
                connectText.text = "Login Failed";
                data.connectGPGS = false;
#endif
            }
            else
            {
                connectText.text = "LoginData Load Failed";
                Debug.Log($"LoginData Load Failed");
            }
        });
    }

    bool SetLoginState(bool _active)
    {
        if (CHMData.Instance.loginDataDic.TryGetValue(CHMMain.String.catPang, out var data) == false)
            return false;

        data.connectGPGS = _active;

        connectGPGSBtn.gameObject.SetActive(_active == false);
        logoutBtn.gameObject.SetActive(_active);

        //CHMData.Instance.SaveData(CHMMain.String.catPang);

        return true;
    }

    bool GetLoginState()
    {
        if (CHMData.Instance.loginDataDic.TryGetValue(CHMMain.String.catPang, out var data) == false)
            return false;

        return data.connectGPGS;
    }

    bool GetPhoneLoginState()
    {
        return PlayerPrefs.GetInt(CHMMain.String.login) == 1;
    }

    IEnumerator LoadAssetBundle(string _bundleName)
    {
        string bundlePath = Path.Combine(Application.streamingAssetsPath, _bundleName);

        downloadText.text = $"{_bundleName} Downloading...";
        // 에셋 번들 로드
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // 다운로드 표시
        float downloadProgress = 0;

        while (!bundleRequest.isDone)
        {
            downloadProgress = bundleRequest.progress;

            if (loadingBar) loadingBar.fillAmount = downloadProgress / liDownloadKey.Count * downloadCount;
            if (loadingText) loadingText.text = downloadProgress / liDownloadKey.Count * downloadCount * 100f + "%";

            yield return null;
        }

        if (bundleRequest.assetBundle == null)
        {
            Debug.LogError($"{_bundleName} is Null");

            LoadAssetBundle(_bundleName);
        }
        else
        {
            downloadProgress = bundleRequest.progress;

            AssetBundle assetBundle = bundleRequest.assetBundle;

            CHMAssetBundle.Instance.LoadAssetBundle(_bundleName, assetBundle);

            ++downloadCount;

            if (loadingBar) loadingBar.fillAmount = downloadProgress / liDownloadKey.Count * downloadCount;
            if (loadingText) loadingText.text = downloadProgress / liDownloadKey.Count * downloadCount * 100f + "%";

            downloadText.text = $"{_bundleName} Download Success";

            if (downloadCount == liDownloadKey.Count)
            {
                Debug.Log($"Bundle download Success");
                bundleDownload.Value = true;
            }
        }
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
