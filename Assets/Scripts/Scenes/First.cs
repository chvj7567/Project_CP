using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UniRx;
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
    [SerializeField] Button startBtn;
    [SerializeField] List<string> liDownloadKey = new List<string>();
    [SerializeField, ReadOnly] int backgroundIndex = 0;

    bool dataDownload = false;
    bool bundleDownload = false;
    int downloadCount = 0;

    CancellationTokenSource tokenSource;

    int i = 1;
    private void Start()
    {
        tokenSource = new CancellationTokenSource();

        loadingBar.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
        stageSelect1.SetActive(false);
        stageSelect2.SetActive(false);
        startBtn.gameObject.SetActive(false);

        if (CHMAssetBundle.Instance.firstDownload == true)
        {
            pageMove.ActiveMoveBtn(false);
            loadingBar.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(true);
            startBtn.gameObject.SetActive(true);

            CHMGPGS.Instance.Login(async (success, localUser) =>
            {
                if (success)
                {
                    Debug.Log("GPGS Login Success");
                    await CHMData.Instance.LoadCloudData();
                    dataDownload = true;
                }
                else
                {
                    Debug.Log("GPGS Login Failed");
                    await CHMData.Instance.LoadLocalData();
                    dataDownload = true;
                }
            });

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

            bundleDownload = true;
            dataDownload = true;

            pageMove.Init(PlayerPrefs.GetInt("stage"));
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
        }

        ChangeBackgroundLoop();

        loadingBar.fillAmount = 0f;

        startBtn.OnClickAsObservable().Subscribe(_ =>
        {
            if (bundleDownload == false || dataDownload == false) return;

            pageMove.Init(PlayerPrefs.GetInt("Stage"));
            startBtn.gameObject.SetActive(false);
            stageSelect1.SetActive(true);
            stageSelect2.SetActive(true);
            loadingBar.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);

            PlayerPrefs.SetInt("background", backgroundIndex);
        });
    }

    IEnumerator LoadAssetBundle(string _bundleName)
    {
        string bundlePath = Path.Combine(Application.streamingAssetsPath, _bundleName);

        // 에셋 번들 로드
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // 다운로드 표시
        float downloadProgress = 0;

        while (!bundleRequest.isDone)
        {
            downloadProgress = bundleRequest.progress;

            if (loadingBar) loadingBar.fillAmount = downloadProgress / liDownloadKey.Count * downloadCount;
            if (loadingText) loadingText.text = downloadProgress / liDownloadKey.Count * downloadCount * 100f+ "%";

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

            Debug.Log($"{_bundleName} download");

            if (downloadCount == liDownloadKey.Count)
            {
                Debug.Log($"Bundle download Success");
                bundleDownload = true;
                CHMAssetBundle.Instance.firstDownload = false;
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
