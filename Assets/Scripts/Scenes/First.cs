using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class First : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] List<Image> backgroundList = new List<Image>();
    [SerializeField] GameObject stageSelect1;
    [SerializeField] GameObject stageSelect2;
    [SerializeField] PageMove pageMove;
    [SerializeField] Button missionBtn;
    [SerializeField] Button startBtn;
    [SerializeField] Button connectGPGSBtn;
    [SerializeField] Button logoutBtn;
    [SerializeField] Button shopBtn;
    [SerializeField] Button boomBtn;
    [SerializeField, ReadOnly] int backgroundIndex = 0;
    [SerializeField] CHLoadingBarFromAssetBundle bundleLoadingScript;
    [SerializeField] CHAdvertise adScript;
    [SerializeField] ReactiveProperty<bool> dataDownload = new ReactiveProperty<bool>();
    [SerializeField] ReactiveProperty<bool> bundleDownload = new ReactiveProperty<bool>();
    [SerializeField] GameObject guideBackground;
    [SerializeField] Button tutorialBackgroundBtn;
    [SerializeField] List<RectTransform> tutorialHoleList = new List<RectTransform>();
    [SerializeField] CHTMPro guideDesc;

    CancellationTokenSource tokenSource;

    bool initButton = false;

    void InitButton()
    {
        if (initButton)
            return;

        initButton = true;

        startBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (bundleDownload.Value == false || dataDownload.Value == false) return;

            pageMove.Init(PlayerPrefs.GetInt(CHMMain.String.Stage));
            startBtn.gameObject.SetActive(false);
            missionBtn.gameObject.SetActive(true);
            shopBtn.gameObject.SetActive(true);
            boomBtn.gameObject.SetActive(true);
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);

            // 기본 스킨
            CHMData.Instance.GetShopData("1").buy = true;

            PlayerPrefs.SetInt(CHMMain.String.Background, backgroundIndex);

            CHMMain.Sound.Play(Defines.ESound.Bgm);

            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            if (loginData.tutorialIndex == 0)
            {
                Time.timeScale = 0;

                guideBackground.SetActive(true);
                guideBackground.transform.SetAsLastSibling();

                tutorialBackgroundBtn.gameObject.SetActive(true);
                tutorialBackgroundBtn.transform.SetAsLastSibling();

                var tutorialIndex = await TutorialStart();
                loginData.tutorialIndex = tutorialIndex;

                guideBackground.SetActive(false);
                tutorialBackgroundBtn.gameObject.SetActive(false);

                CHMData.Instance.SaveData(CHMMain.String.CatPang);
            }
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
            CHMMain.UI.ShowUI(Defines.EUI.UISetting, new CHUIArg());
        });

        connectGPGSBtn.OnPointerClickAsObservable().Subscribe(_ =>
        {
            if (CHMData.Instance.loginDataDic.TryGetValue(CHMMain.String.CatPang, out var data))
            {
                if (data.connectGPGS == true)
                    return;

#if UNITY_EDITOR == false
                CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    if (success)
                    {
                        PlayerPrefs.SetInt(CHMMain.String.Login, 1);
                        Debug.Log("GPGS Login Success");
                        dataDownload.Value = true;

                        SetLoginState(true);
                    }
                    else
                    {
                        Debug.Log("GPGS Login Failed");
                    }
                });
#else
                PlayerPrefs.SetInt(CHMMain.String.Login, 0);
                data.connectGPGS = false;
#endif
            }
        });
    }

    async void Start()
    {
        tokenSource = new CancellationTokenSource();

        stageSelect1.SetActive(false);
        stageSelect2.SetActive(false);
        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(false);
        connectGPGSBtn.gameObject.SetActive(false);
        logoutBtn.gameObject.SetActive(false);
        shopBtn.gameObject.SetActive(false);
        boomBtn.gameObject.SetActive(false);

        for (int i = 0; i < tutorialHoleList.Count; ++i)
        {
            tutorialHoleList[i].gameObject.SetActive(false);
        }

        guideBackground.SetActive(false);
        tutorialBackgroundBtn.gameObject.SetActive(false);

        CHMIAP.Instance.Init();
        CHMAdmob.Instance.Init();

        dataDownload.Subscribe(_ =>
        {
            if (CHMAssetBundle.Instance.firstDownload == true && _ == true && bundleDownload.Value == true)
            {
                CHMAssetBundle.Instance.firstDownload = false;
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
                CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    if (success)
                    {
                        Debug.Log("GPGS Login Success");
                        await CHMData.Instance.LoadCloudData(CHMMain.String.CatPang);
                        dataDownload.Value = true;

                        SetLoginState(true);
                    }
                    else
                    {
                        Debug.Log("GPGS Login Failed");

                        await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
                        SetLoginState(false);
                        dataDownload.Value = true;
                    }
                });
            }
            else
            {
                await CHMMain.Json.Init();
                Debug.Log($"@JsonPercent{CHMMain.Json.GetJsonLoadingPercent()}");
                await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
                SetLoginState(false);
                dataDownload.Value = true;
            }
#else
                await CHMMain.Json.Init();
                Debug.Log($"@JsonPercent{CHMMain.Json.GetJsonLoadingPercent()}");
                await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
                SetLoginState(false);
                dataDownload.Value = true;
#endif
            }
            else if (CHMAssetBundle.Instance.firstDownload == true && _ == true && dataDownload.Value == true)
            {
                CHMAssetBundle.Instance.firstDownload = false;
                startBtn.gameObject.SetActive(true);
            }
        });

        if (CHMAssetBundle.Instance.firstDownload == true)
        {
            pageMove.ActiveMoveBtn(false);
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

        bundleDownload.Value = true;

        InitButton();
    }

    async Task<int> TutorialStart()
    {
        TaskCompletionSource<int> tutorialCompleteTask = new TaskCompletionSource<int>();

        guideBackground.SetActive(true);

        for (int i = 0; i < tutorialHoleList.Count; ++i)
        {
            var tutorialInfo = CHMMain.Json.GetTutorialInfo(i + 5);
            if (tutorialInfo == null)
                break;

            tutorialHoleList[i].gameObject.SetActive(true);
            guideDesc.SetStringID(tutorialInfo.descStringID);

            TaskCompletionSource<bool> buttonClicktask = new TaskCompletionSource<bool>();

            var btnComplete = tutorialBackgroundBtn.OnClickAsObservable().Subscribe(_ =>
            {
                buttonClicktask.SetResult(true);
            });

            await buttonClicktask.Task;

            tutorialHoleList[i].gameObject.SetActive(false);

            btnComplete.Dispose();
        }

        tutorialCompleteTask.SetResult(tutorialHoleList.Count);

        return await tutorialCompleteTask.Task;
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
