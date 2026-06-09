using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CHMMain : MonoBehaviour
{
    static CHMMain m_instance;
    static Task _initTask;

    #region Core
    public static CHMPool Pool { get { EnsureKickoff(); return CHMPool.Instance; } }
    public static CHMResource Resource { get { EnsureKickoff(); return CHMResource.Instance; } }
    public static CHMUI UI { get { EnsureKickoff(); return CHMUI.Instance; } }
    public static CHMJson Json { get { EnsureKickoff(); return CHMJson.Instance; } }
    public static CHMString String { get { EnsureKickoff(); return CHMString.Instance; } }
    public static CHMSound Sound { get { EnsureKickoff(); return CHMSound.Instance; } }
    public static CHMTime Time { get { EnsureKickoff(); return CHMTime.Instance; } }
    #endregion

    /// <summary>
    /// 모든 매니저 초기화 + 동일 Task 캐싱(idempotent). 부팅 시점에 awaitable로 호출.
    /// </summary>
    public static Task EnsureInitialized() => _initTask ??= InitAsync();

#if UNITY_INFRA_APPUPDATE
    //# 인앱 업데이트 흐름을 위임받는 씬 비종속 호스트 인스턴스. EnsureInitialized() 이후 유효.
    public static CHMMain Instance => m_instance;
#endif

    static async Task InitAsync()
    {
        GameObject go = GameObject.Find("@CHMMain");
        if (go == null)
        {
            go = new GameObject { name = "@CHMMain" };
        }
        Object.DontDestroyOnLoad(go);

        m_instance = go.GetOrAddComponent<CHMMain>();

        await CHMResource.Instance.EnsureInit();
        await CHMJson.Instance.Init();
        await GameFontProvider.PreloadAsync();
        CHMPool.Instance.Init();
        CHMSound.Instance.Init();
        await CHMTime.Instance.Init(); // NTP 첫 시도 (실패해도 완료, 백그라운드 재시도)
        ChvjUnityInfra.CHMUI.Instance.Init();

        // CHText/CHButton/CHToggle이 stringID/SFX 흐름에서 사용할 hook/provider 등록
        ChvjUnityInfra.CHText.StringProvider = new GameStringProvider();
        ChvjUnityInfra.CHText.FontProvider = new GameFontProvider();
        ChvjUnityInfra.CHButton.ClickSoundHook = () => CHMSound.Instance.Play(Defines.ESound.Ppauk);
        ChvjUnityInfra.CHToggle.ChangeSoundHook = () => CHMSound.Instance.Play(Defines.ESound.Ppauk);

        // 일일 미션 — 보상형 광고 시청 시 카운터 +1
        ChvjUnityInfra.CHMAdmob.Instance.AcquireReward += () => DailyMissionService.OnAdWatched();
    }

    /// <summary>
    /// 기존 accessor 호환 — 접근만으로 초기화를 fire-and-forget 트리거.
    /// 정확한 await가 필요하면 EnsureInitialized()를 명시 호출.
    /// </summary>
    static void EnsureKickoff()
    {
        if (m_instance != null) return;
        GameObject go = GameObject.Find("@CHMMain");
        if (go == null) go = new GameObject { name = "@CHMMain" };
        Object.DontDestroyOnLoad(go);
        m_instance = go.GetOrAddComponent<CHMMain>();
        _ = EnsureInitialized();
    }

    // 직전 프레임의 UI 열림 여부. 같은 프레임에 패키지 CHMUI가 ESC로 UI를 닫은 경우
    // CheckUI=false가 되어 즉시 재오픈되는 race를 막기 위함.
    // Update 페이즈가 모두 끝난 LateUpdate에서 체크해야 EventSystem/패키지 CHMUI 처리 후의
    // 결정적 상태를 볼 수 있다 (Update 순서는 비결정적).
    bool _wasUIOpenLastFrame;

    private void LateUpdate()
    {
        if (_initTask == null || !_initTask.IsCompleted)
            return;

        bool isUIOpen = CHMUI.Instance.CheckUI;

        // GameScene 씬은 GPGameScene이 자체 처리(퍼즐 정지 + 메뉴 이동 팝업). 중복 트리거 방지를 위해 skip.
        bool inGameScene = SceneManager.GetActiveScene().name == nameof(Defines.EScene.GameScene);

        if (!inGameScene && Input.GetKeyDown(KeyCode.Escape) && !isUIOpen && !_wasUIOpenLastFrame)
        {
            CHMUI.Instance.ShowUI(Defines.EUI.UIConfirm, new UIConfirmArg
            {
                confirmType = EConfirmType.YesNo,
                txtTitle = CHMString.Instance.GetString(141),
                txtDesc = "",
                onYes = () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                },
            });
        }

        _wasUIOpenLastFrame = isUIOpen;
    }

    private void OnApplicationQuit()
    {
        if (m_instance != null)
        {
            CHMJson.Instance.Clear();
            CHMPool.Instance.Clear();
            Destroy(this);
        }
    }

#if UNITY_INFRA_APPUPDATE
    //# 인앱 업데이트 흐름의 씬 비종속 호스트.
    //# ResourceDownload는 부팅 씬과 함께 파괴되므로, Flexible 다운로드 완료(수십 초~분)나
    //# 앱 재개(resume) 시점엔 어느 씬이든 살아 있는 DontDestroyOnLoad인 CHMMain이 UI를 띄운다.

    //# Flexible 설치가 실제로 트리거(onYes → CompleteFlexibleUpdate)됐는지의 영구 latch.
    //# true가 되면 이후 어떤 재개에서도 완료 안내를 다시 띄우지 않는다. 거절(No/닫기)은 latch하지 않아
    //# 다음 resume에서(GameScene 가드 통과 시) 재권유된다.
    bool _flexibleInstallTriggered;
    //# 완료 안내 팝업이 현재 떠 있는 동안 중복 표시를 막는 가드. 팝업이 닫히면(onClose) 해제.
    bool _flexibleConfirmOpen;
    //# resume 재확인이 동시에 여러 번 돌지 않도록 하는 가드.
    bool _resumeChecking;
    //# Immediate 강제 루프의 단일 in-flight 가드. 부팅(ResourceDownload await)·resume 어느 쪽에서
    //# 호출하든 이미 루프가 돌고 있으면 즉시 no-op 반환해 StartImmediateAsync/안내 팝업 2중 실행을 막는다.
    bool _immediateLoopRunning;

    //# Immediate 강제 업데이트 흐름. 취소(닫기)해도 안내 후 무한 재요청(spec §8, 탈출 불가).
    //# 부팅 호출부(ResourceDownload)와 resume 재개 양쪽에서 사용.
    public async Task RunForcedImmediateLoopAsync()
    {
        //# 이미 루프가 진행 중이면 새 호출은 즉시 완료 Task로 반환(부팅 await 의미 유지, 중복 실행 방지).
        //# Unity 단일 스레드이므로 첫 await 전 check-and-set은 원자적.
        if (_immediateLoopRunning)
            return;

        _immediateLoopRunning = true;
        try
        {
            bool ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            while (ok == false)
            {
                await ShowForcedUpdateConfirm();
                ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            }
        }
        finally
        {
            _immediateLoopRunning = false;
        }
    }

    //# 강제 업데이트 안내 팝업. 확인/닫기 어느 경로든 완료되면 호출부가 즉시 재요청한다(탈출 불가).
    Task ShowForcedUpdateConfirm()
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        UIConfirmArg arg = new UIConfirmArg
        {
            confirmType = EConfirmType.Confirm,
            txtTitle = CHMString.Instance.GetString(CHMString.Instance.AppUpdateForcedTitle),
            txtDesc = CHMString.Instance.GetString(CHMString.Instance.AppUpdateForcedDesc),
            onYes = () => tcs.TrySetResult(true),
            onClose = () => tcs.TrySetResult(true),
        };
        CHMUI.Instance.ShowUI(Defines.EUI.UIConfirm, arg);
        return tcs.Task;
    }

    //# Flexible 다운로드 완료 시 재시작 안내. Yes면 설치.
    //# 부팅 StartFlexible 콜백과 resume 양쪽이 호출할 수 있다.
    //# 가드 순서: ① 설치 이미 트리거됨 → 영구 skip ② 안내 팝업이 이미 떠 있음 → skip
    //#            ③ GameScene 플레이 중 → 보류(아무것도 set하지 않고 return — 다음 resume에서 재시도)
    public void ShowFlexibleCompleteConfirm()
    {
        if (_flexibleInstallTriggered)
            return;

        if (_flexibleConfirmOpen)
            return;

        //# GameScene 플레이 도중엔 퍼즐을 끊지 않도록 완료 안내만 보류. (강제 Immediate는 어느 씬이든 막아야 하므로 가드하지 않음)
        //# LateUpdate의 ESC 처리와 동일한 씬 판정 재사용.
        bool inGameScene = SceneManager.GetActiveScene().name == nameof(Defines.EScene.GameScene);
        if (inGameScene)
            return;

        _flexibleConfirmOpen = true;
        UIConfirmArg arg = new UIConfirmArg
        {
            confirmType = EConfirmType.YesNo,
            txtTitle = CHMString.Instance.GetString(CHMString.Instance.AppUpdateReadyTitle),
            txtDesc = CHMString.Instance.GetString(CHMString.Instance.AppUpdateReadyDesc),
            onYes = () =>
            {
                //# 실제 설치 실행 시점에 영구 latch(거절은 latch하지 않아 다음 resume 재권유 가능).
                _flexibleInstallTriggered = true;
                ChvjUnityInfra.CHMAppUpdate.Instance.CompleteFlexibleUpdate();
            },
            //# Yes/No/ESC/Background 어떤 경로든 OnDisable에서 발화 → 모든 닫힘에서 open 가드 해제.
            onClose = () => _flexibleConfirmOpen = false,
        };
        CHMUI.Instance.ShowUI(Defines.EUI.UIConfirm, arg);
    }

    //# 앱 재개 시 미완료 업데이트 재확인(spec §5.1).
    //# pauseStatus==false 가 재개 엣지. 부팅 초기화가 끝난 뒤에만 동작.
    private async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            return;

        if (_initTask == null || _initTask.IsCompleted == false)
            return;

        if (_resumeChecking)
            return;

        _resumeChecking = true;
        try
        {
            ChvjUnityInfra.EAppUpdateResume resume = await ChvjUnityInfra.CHMAppUpdate.Instance.CheckResumeAsync();

            if (resume == ChvjUnityInfra.EAppUpdateResume.ImmediateInProgress)
            {
                await RunForcedImmediateLoopAsync();
            }
            else if (resume == ChvjUnityInfra.EAppUpdateResume.FlexibleDownloaded)
            {
                ShowFlexibleCompleteConfirm();
            }
        }
        finally
        {
            _resumeChecking = false;
        }
    }
#endif
}
