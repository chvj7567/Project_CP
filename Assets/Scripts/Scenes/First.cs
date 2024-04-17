using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
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
    [SerializeField] Button bombBtn;
    [SerializeField] Button menuBtn;
    [SerializeField] Button rankingBtn;
    [SerializeField, ReadOnly] int backgroundIndex = 0;
    [SerializeField] CHAdvertise adScript;
    [SerializeField] ReactiveProperty<bool> dataDownload = new ReactiveProperty<bool>();
    [SerializeField] ReactiveProperty<bool> bundleDownload = new ReactiveProperty<bool>();
    [SerializeField] GameObject guideBackground;
    [SerializeField] Button guideBackgroundBtn;
    [SerializeField] List<RectTransform> guideHoleList = new List<RectTransform>();
    [SerializeField] CHTMPro guideDesc;
    [SerializeField] CHTMPro userID;
    [SerializeField] GameObject objWait;

    CancellationTokenSource tokenSource;

    bool initButton = false;
    bool firstStartBtnClick = false;

    void InitButton()
    {
        if (initButton)
            return;

        initButton = true;

        startBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (bundleDownload.Value == false || dataDownload.Value == false)
                return;

            firstStartBtnClick = true;

            var arg = new UIStageSelectArg();
            arg.stageSelect += async (select) =>
            {
                await StageSelect(select);
            };

            CHMMain.UI.ShowUI(Defines.EUI.UIStageSelect, arg);
        });

        missionBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.UI.ShowUI(Defines.EUI.UIMission, new CHUIArg());
        });

        logoutBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (GetGPGSLogin() == false)
                return;

            await SetGPGSLogin(false, "");

#if UNITY_ANDROID
            CHMGPGS.Instance.Logout();
#endif
        });

        shopBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.UI.ShowUI(Defines.EUI.UIShop, new CHUIArg());
        });

        bombBtn.OnClickAsObservable().Subscribe(_ =>
        {
            CHMMain.UI.ShowUI(Defines.EUI.UISetting, new CHUIArg());
        });

        rankingBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).connectGPGS == false)
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg
                {
                    stringID = 107
                });
            }
            else
            {
                CHMMain.UI.ShowUI(Defines.EUI.UIRank, new CHUIArg());
            }
        });

        connectGPGSBtn.OnPointerClickAsObservable().Subscribe(_ =>
        {
#if UNITY_ANDROID
            if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).connectGPGS == false)
            {
                objWait.SetActive(true);

                CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    await SetGPGSLogin(success, localUser.userName);

                    objWait.SetActive(false);
                });
            }
#endif
        });

        menuBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var arg = new UIStageSelectArg();
            arg.stageSelect += async (select) =>
            {
                await StageSelect(select);
            };

            CHMMain.UI.ShowUI(Defines.EUI.UIStageSelect, arg);
        });
    }

    async void Start()
    {
        tokenSource = new CancellationTokenSource();

        // DB 미사용으로 인해 닉네임은 미사용(랭킹에 다른 유저 닉네임 못 가져옴)
        /*if (CHMData.Instance.GetLoginData(CHMMain.String.CatPang).nickname == "")
        {
            CHMMain.UI.ShowUI(Defines.EUI.UINickname, new CHUIArg());
        }*/

        menuBtn.gameObject.SetActive(false);
        stageSelect1.SetActive(false);
        stageSelect2.SetActive(false);
        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(false);
        connectGPGSBtn.gameObject.SetActive(false);
        logoutBtn.gameObject.SetActive(false);
        shopBtn.gameObject.SetActive(false);
        bombBtn.gameObject.SetActive(false);
        rankingBtn.gameObject.SetActive(false);
        objWait.SetActive(false);


        userID.gameObject.SetActive(false);

        for (int i = 0; i < guideHoleList.Count; ++i)
        {
            guideHoleList[i].gameObject.SetActive(false);
        }

        guideBackground.SetActive(false);
        guideBackgroundBtn.gameObject.SetActive(false);

        CHMIAP.Instance.Init();
        CHMAdmob.Instance.Init();

        dataDownload.Subscribe(async dataDownload =>
        {
            if (CHMAssetBundle.Instance.firstDownload && dataDownload && bundleDownload.Value)
            {
                CHMAssetBundle.Instance.firstDownload = false;
                startBtn.gameObject.SetActive(true);
            }
        });

        bundleDownload.Subscribe(async bundleDownload =>
        {
            if (bundleDownload && dataDownload.Value == false)
            {
                if (GetPhoneLoginState())
                {
                    objWait.SetActive(true);

                    CHMGPGS.Instance.Login(async (success, localUser) =>
                    {
                        await SetGPGSLogin(success, localUser.userName);

                        objWait.SetActive(false);
                    });
                }
                else
                {
                    await SetGPGSLogin(false, "");
                }
            }
            else if (CHMAssetBundle.Instance.firstDownload == true && bundleDownload == true && dataDownload.Value == true)
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

            pageMove.Init((Defines.ESelectStage)PlayerPrefs.GetInt(CHMMain.String.SelectStage));
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            missionBtn.gameObject.SetActive(true);
            shopBtn.gameObject.SetActive(true);
            bombBtn.gameObject.SetActive(true);
            menuBtn.gameObject.SetActive(true);
            rankingBtn.gameObject.SetActive(true);

            var login = GetGPGSLogin();
            connectGPGSBtn.gameObject.SetActive(login == false);
            logoutBtn.gameObject.SetActive(login);

            adScript.GetAdvertise();

            var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
            if (loginData.guideIndex == 0)
            {
                Time.timeScale = 0;

                guideBackground.SetActive(true);
                guideBackground.transform.SetAsLastSibling();

                guideBackgroundBtn.gameObject.SetActive(true);
                guideBackgroundBtn.transform.SetAsLastSibling();

                var tutorialIndex = await TutorialStart();
                loginData.guideIndex = tutorialIndex;

                guideBackground.SetActive(false);
                guideBackgroundBtn.gameObject.SetActive(false);

                Time.timeScale = 1;

                CHMData.Instance.SaveData(CHMMain.String.CatPang);
            }
        }

        ChangeBackgroundLoop();

        bundleDownload.Value = true;

        InitButton();

        // 기본 스킨
        CHMData.Instance.GetShopData("1").buy = true;
    }

    async Task<int> TutorialStart()
    {
        TaskCompletionSource<int> tutorialCompleteTask = new TaskCompletionSource<int>();

        guideBackground.SetActive(true);

        for (int i = 0; i < guideHoleList.Count; ++i)
        {
            var tutorialInfo = CHMMain.Json.GetGuideInfo(i + 1);
            if (tutorialInfo == null)
                break;

            guideHoleList[i].gameObject.SetActive(true);
            guideDesc.SetStringID(tutorialInfo.descStringID);

            TaskCompletionSource<bool> buttonClicktask = new TaskCompletionSource<bool>();

            var btnComplete = guideBackgroundBtn.OnClickAsObservable().Subscribe(_ =>
            {
                buttonClicktask.SetResult(true);
            });

            await buttonClicktask.Task;

            guideHoleList[i].gameObject.SetActive(false);

            btnComplete.Dispose();
        }

        tutorialCompleteTask.SetResult(guideHoleList.Count);

        return await tutorialCompleteTask.Task;
    }

    private void OnApplicationQuit()
    {
        CHMData.Instance.SaveData(CHMMain.String.CatPang);
    }

    async Task<bool> SetGPGSLogin(bool success, string gpgsUserName)
    {
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        loginData.connectGPGS = success;

        Debug.Log($"SetLoginState : {loginData.connectGPGS}");

        if (success)
        {
            Debug.Log($"GPGS Login Success : {gpgsUserName}");
            await CHMData.Instance.LoadCloudData(CHMMain.String.CatPang);

            userID.gameObject.SetActive(true);
            userID.SetText(gpgsUserName);

            if (firstStartBtnClick && CHMData.Instance.newUser == false)
                await StageSelect(PlayerPrefs.GetInt(CHMMain.String.SelectStage));
        }
        else
        {
            userID.gameObject.SetActive(false);
            Debug.Log($"GPGS Login Failed {success.ToString()}");
            await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
        }

        PlayerPrefs.SetInt(CHMMain.String.Login, success ? 1 : 0);

        connectGPGSBtn.gameObject.SetActive(success == false);
        logoutBtn.gameObject.SetActive(success);
        dataDownload.Value = true;

        CHMData.Instance.SaveData(CHMMain.String.CatPang);

        return true;
    }

    bool GetGPGSLogin()
    {
        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);

        Debug.Log($"GetLoginState : {loginData.connectGPGS}");

        return loginData.connectGPGS;
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

    async Task StageSelect(int select)
    {
        PlayerPrefs.SetInt(CHMMain.String.SelectStage, select);

        pageMove.Init((Defines.ESelectStage)select);
        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(true);
        shopBtn.gameObject.SetActive(true);
        bombBtn.gameObject.SetActive(true);
        stageSelect1.SetActive(true);
        stageSelect2.SetActive(true);
        menuBtn.gameObject.SetActive(true);
        rankingBtn.gameObject.SetActive(true);

        PlayerPrefs.SetInt(CHMMain.String.Background, backgroundIndex);

        CHMMain.Sound.Play(Defines.ESound.Bgm);

        var loginData = CHMData.Instance.GetLoginData(CHMMain.String.CatPang);
        if (loginData.guideIndex == 0)
        {
            Time.timeScale = 0;

            guideBackground.SetActive(true);
            guideBackground.transform.SetAsLastSibling();

            guideBackgroundBtn.gameObject.SetActive(true);
            guideBackgroundBtn.transform.SetAsLastSibling();

            var tutorialIndex = await TutorialStart();
            loginData.guideIndex = tutorialIndex;

            guideBackground.SetActive(false);
            guideBackgroundBtn.gameObject.SetActive(false);

            Time.timeScale = 1;

            CHMData.Instance.SaveData(CHMMain.String.CatPang);
        }
    }
}
