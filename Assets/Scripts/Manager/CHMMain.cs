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
        CHMPool.Instance.Init();
        CHMSound.Instance.Init();
        ChvjUnityInfra.CHMUI.Instance.Init();
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
