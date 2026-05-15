using System.Threading.Tasks;
using UnityEngine;

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
    #endregion

    /// <summary>
    /// 모든 매니저 초기화 + 동일 Task 캐싱(idempotent). 부팅 시점에 awaitable로 호출.
    /// </summary>
    public static Task EnsureInitialized() => _initTask ??= InitAsync();

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
        ChvjUnityInfra.CHMUI.Instance.Init();

        // CHText/CHButton/CHToggle이 stringID/SFX 흐름에서 사용할 hook/provider 등록
        ChvjUnityInfra.CHText.StringProvider = new GameStringProvider();
        ChvjUnityInfra.CHText.FontProvider = new GameFontProvider();
        ChvjUnityInfra.CHButton.ClickSoundHook = () => CHMSound.Instance.Play(Defines.ESound.Cat);
        ChvjUnityInfra.CHToggle.ChangeSoundHook = () => CHMSound.Instance.Play(Defines.ESound.Cat);
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

        if (Input.GetKeyDown(KeyCode.Escape) && !isUIOpen && !_wasUIOpenLastFrame)
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
}
