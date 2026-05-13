using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;

public class LBLobbyScene : MonoBehaviour
{
    [SerializeField] Canvas canvas;
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
    [SerializeField] CHAdvertise adScript;
    [SerializeField] ReactiveProperty<bool> dataDownload = new ReactiveProperty<bool>();
    [SerializeField] ReactiveProperty<bool> bundleDownload = new ReactiveProperty<bool>();
    [SerializeField] GameObject guideBackground;
    [SerializeField] Button guideBackgroundBtn;
    [SerializeField] List<RectTransform> guideHoleList = new List<RectTransform>();
    [SerializeField] CHText guideDesc;
    [SerializeField] CHText userID;
    [SerializeField] GameObject objWait;

    CancellationTokenSource tokenSource;
    bool initButton = false;
    bool firstStartBtnClick = false;

    LBLoginHandler _loginHandler;
    LBTutorial _tutorial;

    void InitButton()
    {
        if (initButton) return;
        initButton = true;

        startBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (!bundleDownload.Value || !dataDownload.Value) return;
            firstStartBtnClick = true;
            var arg = new UIStageSelectArg();
            arg.stageSelect += async (select) => await StageSelect(select);
            CHMUI.Instance.ShowUI(Defines.EUI.UIStageSelect, arg);
        });

        missionBtn.OnClickAsObservable().Subscribe(_ => CHMUI.Instance.ShowUI(Defines.EUI.UIMission, new CHUIArg()));

        logoutBtn.OnClickAsObservable().Subscribe(async _ =>
        {
            if (!_loginHandler.GetGPGSLogin()) return;
            await _loginHandler.SetGPGSLogin(false, "");
#if UNITY_ANDROID
            ChvjUnityInfra.CHMGPGS.Instance.Logout();
#endif
        });

        shopBtn.OnClickAsObservable().Subscribe(_ => CHMUI.Instance.ShowUI(Defines.EUI.UIShop, new CHUIArg()));
        bombBtn.OnClickAsObservable().Subscribe(_ => CHMUI.Instance.ShowUI(Defines.EUI.UISetting, new CHUIArg()));

        rankingBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (!CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).connectGPGS)
                CHMUI.Instance.ShowUI(Defines.EUI.UIAlarm, new UIAlarmArg { stringID = 107 });
            else
                CHMUI.Instance.ShowUI(Defines.EUI.UIRank, new CHUIArg());
        });

        connectGPGSBtn.OnPointerClickAsObservable().Subscribe(_ =>
        {
#if UNITY_ANDROID
            if (!CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).connectGPGS)
            {
                objWait.SetActive(true);
                ChvjUnityInfra.CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    await _loginHandler.SetGPGSLogin(success, localUser.userName);
                    objWait.SetActive(false);
                });
            }
#endif
        });

        menuBtn.OnClickAsObservable().Subscribe(_ =>
        {
            var arg = new UIStageSelectArg();
            arg.stageSelect += async (select) => await StageSelect(select);
            CHMUI.Instance.ShowUI(Defines.EUI.UIStageSelect, arg);
        });
    }

    async void Start()
    {
        tokenSource = new CancellationTokenSource();

        _loginHandler = new LBLoginHandler();
        _loginHandler.Init(userID, connectGPGSBtn, logoutBtn, objWait, async (success) =>
        {
            dataDownload.Value = true;
            if (success && firstStartBtnClick && CHMData.Instance.newUser == false)
                await StageSelect(PlayerPrefs.GetInt(CHMString.Instance.SelectStage));
        });

        _tutorial = new LBTutorial();
        _tutorial.Init(guideBackground, guideBackgroundBtn, guideHoleList, guideDesc);

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
        foreach (var h in guideHoleList) h.gameObject.SetActive(false);
        guideBackground.SetActive(false);
        guideBackgroundBtn.gameObject.SetActive(false);

        ChvjUnityInfra.CHMIAP.Instance.Init();
        ChvjUnityInfra.CHMAdmob.Instance.Init();

        bundleDownload.Subscribe(async bl =>
        {
            if (bl && !dataDownload.Value)
            {
                if (_loginHandler.GetPhoneLoginState())
                {
                    objWait.SetActive(true);
#if UNITY_ANDROID
                    ChvjUnityInfra.CHMGPGS.Instance.Login(async (success, localUser) =>
                    {
                        await _loginHandler.SetGPGSLogin(success, localUser.userName);
                        objWait.SetActive(false);
                    });
#endif
                }
                else await _loginHandler.SetGPGSLogin(false, "");
            }
        });

        {
            bundleDownload.Value = true;
            dataDownload.Value = true;

            pageMove.Init((Defines.ESelectStage)PlayerPrefs.GetInt(CHMString.Instance.SelectStage));
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            missionBtn.gameObject.SetActive(true);
            shopBtn.gameObject.SetActive(true);
            bombBtn.gameObject.SetActive(true);
            menuBtn.gameObject.SetActive(true);
            rankingBtn.gameObject.SetActive(true);

            var login = _loginHandler.GetGPGSLogin();
            connectGPGSBtn.gameObject.SetActive(!login);
            logoutBtn.gameObject.SetActive(login);

            adScript.GetAdvertise();

            var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
            if (loginData.guideIndex == 0)
            {
                Time.timeScale = 0;
                guideBackground.SetActive(true);
                guideBackground.transform.SetAsLastSibling();
                guideBackgroundBtn.gameObject.SetActive(true);
                guideBackgroundBtn.transform.SetAsLastSibling();

                loginData.guideIndex = await _tutorial.TutorialStart();

                guideBackground.SetActive(false);
                guideBackgroundBtn.gameObject.SetActive(false);
                Time.timeScale = 1;
                CHMData.Instance.SaveData(CHMString.Instance.CatPang);
            }
        }

        bundleDownload.Value = true;
        InitButton();
        CHMData.Instance.GetShopData("1").buy = true;
    }

    private void OnApplicationQuit() => CHMData.Instance.SaveData(CHMString.Instance.CatPang);

    async Task StageSelect(int select)
    {
        PlayerPrefs.SetInt(CHMString.Instance.SelectStage, select);
        pageMove.Init((Defines.ESelectStage)select);
        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(true);
        shopBtn.gameObject.SetActive(true);
        bombBtn.gameObject.SetActive(true);
        stageSelect1.SetActive(true);
        stageSelect2.SetActive(true);
        menuBtn.gameObject.SetActive(true);
        rankingBtn.gameObject.SetActive(true);

        CHMSound.Instance.Play(Defines.ESound.Bgm);

        var loginData = CHMData.Instance.GetLoginData(CHMString.Instance.CatPang);
        if (loginData.guideIndex == 0)
        {
            Time.timeScale = 0;
            guideBackground.SetActive(true);
            guideBackground.transform.SetAsLastSibling();
            guideBackgroundBtn.gameObject.SetActive(true);
            guideBackgroundBtn.transform.SetAsLastSibling();

            loginData.guideIndex = await _tutorial.TutorialStart();

            guideBackground.SetActive(false);
            guideBackgroundBtn.gameObject.SetActive(false);
            Time.timeScale = 1;
            CHMData.Instance.SaveData(CHMString.Instance.CatPang);
        }
    }
}
