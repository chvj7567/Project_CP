using System.Threading.Tasks;
using UnityEngine;

public class CHMMain : MonoBehaviour
{
    static CHMMain m_instance;
    static Task _initTask;

    #region Core
    CHMPool m_pool = new CHMPool();
    CHMResource m_resource = new CHMResource();
    CHMUI m_ui = new CHMUI();
    CHMJson m_json = new CHMJson();
    CHMString m_string = new CHMString();
    CHMSound m_sound = new CHMSound();

    public static CHMPool Pool { get { EnsureKickoff(); return m_instance.m_pool; } }
    public static CHMResource Resource { get { EnsureKickoff(); return m_instance.m_resource; } }
    public static CHMUI UI { get { EnsureKickoff(); return m_instance.m_ui; } }
    public static CHMJson Json { get { EnsureKickoff(); return m_instance.m_json; } }
    public static CHMString String { get { EnsureKickoff(); return m_instance.m_string; } }
    public static CHMSound Sound { get { EnsureKickoff(); return m_instance.m_sound; } }
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

        await m_instance.m_resource.EnsureInit();
        await m_instance.m_json.Init();
        m_instance.m_pool.Init();
        m_instance.m_sound.Init();
        ChvjUnityInfra.CHMUI.Instance.Init();
    }

    /// <summary>
    /// 기존 accessor 호환 — 접근만으로 초기화를 fire-and-forget 트리거.
    /// 정확한 await가 필요하면 EnsureInitialized()를 명시 호출.
    /// </summary>
    static void EnsureKickoff()
    {
        if (m_instance != null) return;
        // 인스턴스만 먼저 만들어 accessor가 null 반환 안 하도록.
        GameObject go = GameObject.Find("@CHMMain");
        if (go == null) go = new GameObject { name = "@CHMMain" };
        Object.DontDestroyOnLoad(go);
        m_instance = go.GetOrAddComponent<CHMMain>();

        // 본격 init은 Task pattern으로 비동기 진행 (이미 진행 중이면 캐싱)
        _ = EnsureInitialized();
    }

    private void OnApplicationQuit()
    {
        if (m_instance != null)
        {
            m_instance.m_json.Clear();
            m_instance.m_pool.Clear();
            Destroy(this);
        }
    }
}
