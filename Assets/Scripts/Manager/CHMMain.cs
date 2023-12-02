using UnityEngine;

public class CHMMain : MonoBehaviour
{
    static CHMMain m_instance;
    static CHMMain Instance { get { Init(); return m_instance; } }

    #region Core
    CHMPool m_pool = new CHMPool();
    CHMResource m_resource = new CHMResource();
    CHMUI m_ui = new CHMUI();
    CHMJson m_json = new CHMJson();
    CHMString m_string = new CHMString();
    CHMSound m_sound = new CHMSound();

    public static CHMPool Pool { get { return Instance.m_pool; } }
    public static CHMResource Resource { get { return Instance.m_resource; } }
    public static CHMUI UI { get { return Instance.m_ui; } }
    public static CHMJson Json { get { return Instance.m_json; } }
    public static CHMString String { get { return Instance.m_string; } }
    public static CHMSound Sound { get { return Instance.m_sound; } }
    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {
        UI.UpdateUI();
    }

    static async void Init()
    {
        if (m_instance == null)
        {
            GameObject go = GameObject.Find("@CHMMain");
            if (go == null)
            {
                go = new GameObject { name = "@CHMMain" };
            }

            Object.DontDestroyOnLoad(go);

            m_instance = go.GetOrAddComponent<CHMMain>();

            await m_instance.m_json.Init();
            m_instance.m_pool.Init();
            m_instance.m_sound.Init();
        }
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
