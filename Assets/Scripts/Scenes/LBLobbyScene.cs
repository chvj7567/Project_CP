using System;
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
    // GPGameScene 등에서 lobby로 돌아올 때 true로 세팅 → Start에서 startBtn 단계 건너뛰고 스테이지 메뉴 직행
    public static bool fromGame = false;

    // GameScene에서 다음 스테이지로 바로 이어가고 싶을 때 stage 번호 세팅 → 로비 진입 후 UIGameStart 자동 표시
    public static int pendingShowGameStartStage = 0;

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

    //# 로그인 진행 중 다른 UI 클릭을 차단하는 런타임 전체화면 raycast blocker (최초 1회 생성·재사용)
    GameObject _loginInputBlocker;

    //# 로그인 시작/종료 시 호출. on이면 blocker 생성·활성·최상단, off면 비활성.
    //# objWait 텍스트가 blocker에 가리지 않도록 blocker 위로 다시 올린다.
    void SetLoginBlocker(bool on)
    {
        if (on)
        {
            if (_loginInputBlocker == null)
            {
                _loginInputBlocker = new GameObject("LoginInputBlocker", typeof(RectTransform), typeof(Image));
                _loginInputBlocker.transform.SetParent(objWait.transform.parent, false);

                RectTransform rect = _loginInputBlocker.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                Image image = _loginInputBlocker.GetComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0f);
                image.raycastTarget = true;
            }

            _loginInputBlocker.SetActive(true);
            _loginInputBlocker.transform.SetAsLastSibling();
            objWait.transform.SetAsLastSibling();
        }
        else
        {
            if (_loginInputBlocker == null)
                return;

            _loginInputBlocker.SetActive(false);
        }
    }

    void InitButton()
    {
        if (initButton) return;
        initButton = true;

        startBtn.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.5))
            .Subscribe(async _ =>
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
#if UNITY_ANDROID && !UNITY_EDITOR
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
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!CHMData.Instance.GetLoginData(CHMString.Instance.CatPang).connectGPGS)
            {
                objWait.SetActive(true);
                SetLoginBlocker(true);
                ChvjUnityInfra.CHMGPGS.Instance.Login(async (success, localUser) =>
                {
                    await _loginHandler.SetGPGSLogin(success, localUser.userName);
                    objWait.SetActive(false);
                    SetLoginBlocker(false);
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

        //# 로비(FirstScene) 진입 즉시 BGM 재생. 시작 버튼/StageSelect 까지 기다리지 않는다.
        //# (이미 재생 중이면 CHMSound.Play 가 isPlaying 체크로 중복 무시)
        CHMSound.Instance.Play(Defines.ESound.Bgm);

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
#if UNITY_ANDROID && !UNITY_EDITOR
                    SetLoginBlocker(true);
                    ChvjUnityInfra.CHMGPGS.Instance.Login(async (success, localUser) =>
                    {
                        await _loginHandler.SetGPGSLogin(success, localUser.userName);
                        objWait.SetActive(false);
                        SetLoginBlocker(false);
                    });
#endif
                }
                else await _loginHandler.SetGPGSLogin(false, "");
            }
        });

        bundleDownload.Value = true;
        dataDownload.Value = true;

        var login = _loginHandler.GetGPGSLogin();
        connectGPGSBtn.gameObject.SetActive(!login);
        logoutBtn.gameObject.SetActive(login);

        InitButton();
        CHMData.Instance.GetShopData("1").buy = true;

        if (fromGame)
        {
            fromGame = false;
            int autoStartStage = pendingShowGameStartStage;
            pendingShowGameStartStage = 0;

            await StageSelect(PlayerPrefs.GetInt(CHMString.Instance.SelectStage));

            if (autoStartStage > 0)
            {
                CHMUI.Instance.ShowUI(Defines.EUI.UIGameStart, new UIGameStartArg { stage = autoStartStage });
            }
        }
        else
        {
            startBtn.gameObject.SetActive(true);
            //# 첫 시작 화면에도 설정 버튼 노출
            bombBtn.gameObject.SetActive(true);
            pageMove.ActiveMoveBtn(false);
        }
    }

    private void OnApplicationQuit() => CHMData.Instance.SaveData(CHMString.Instance.CatPang);

    async Task StageSelect(int select)
    {
        PlayerPrefs.SetInt(CHMString.Instance.SelectStage, select);

        // 페이지 초기화는 stageSelect1/2 컨테이너가 활성화된 뒤에 한다.
        // 자식 CHButton들이 Awake에서 GetComponent<Button>()을 셋팅하므로,
        // 비활성 상태로 두면 btnList[i].button이 null이라 OnClickAsObservable에서 NullRef.
        stageSelect1.SetActive(true);
        stageSelect2.SetActive(true);
        pageMove.Init((Defines.ESelectStage)select);

        startBtn.gameObject.SetActive(false);
        missionBtn.gameObject.SetActive(true);
        shopBtn.gameObject.SetActive(true);
        bombBtn.gameObject.SetActive(true);
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
