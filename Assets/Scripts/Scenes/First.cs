using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class First : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] GameObject stageGroupObj;
    [SerializeField] Button startBtn;
    [SerializeField] List<string> liDownloadKey = new List<string>();

    bool canStart = false;
    int downloadCount = 0;
    private void Start()
    {
        startBtn.gameObject.SetActive(true);
        stageGroupObj.SetActive(false);

        if (loadingBar) loadingBar.fillAmount = 0f;
        if (startBtn)
        {
            startBtn.OnClickAsObservable().Subscribe(_ =>
            {
                if (canStart == false) return;

                startBtn.gameObject.SetActive(false);
                stageGroupObj.SetActive(true);
                loadingBar.gameObject.SetActive(false);
                loadingText.gameObject.SetActive(false);
            });
        }

        if (CHMAssetBundle.firstDownload == true)
        {
            foreach (var key in liDownloadKey)
            {
                StartCoroutine(LoadAssetBundle(key));
            }
        }
        else
        {
            canStart = true;
            loadingBar.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);
        }
    }

    IEnumerator LoadAssetBundle(string _bundleName)
    {
        string bundlePath = Path.Combine(Application.streamingAssetsPath, _bundleName);

        Debug.Log(bundlePath);

        // 에셋 번들 로드
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // 다운로드 표시
        float downloadProgress = 0;

        ++downloadCount;

        while (!bundleRequest.isDone)
        {
            downloadProgress = bundleRequest.progress;

            if (loadingBar) loadingBar.fillAmount = downloadProgress;
            if (loadingText) loadingText.text = downloadProgress / liDownloadKey.Count * downloadCount * 100f+ "%";

            yield return null;
        }

        downloadProgress = bundleRequest.progress;

        if (loadingBar) loadingBar.fillAmount = downloadProgress;
        if (loadingText) loadingText.text = downloadProgress / liDownloadKey.Count * downloadCount * 100f + "%";

        AssetBundle assetBundle = bundleRequest.assetBundle;

        CHMAssetBundle.LoadAssetBundle(_bundleName, assetBundle);

        if (downloadCount == liDownloadKey.Count)
        {
            canStart = true;
            CHMAssetBundle.firstDownload = false;
        }

        /*if (assetBundle != null)
        {
            // 바로 Sprite로 가져오지 못하고 Texture2D로 가져온다.
            Texture2D guideCat = assetBundle.LoadAsset<Texture2D>("huchu1");

            // 텍스쳐를 제대로 로드했으면 생성한다.
            if (guideCat != null)
            {
                // Texture2D를 Sprite로 변환하여 이미지 소스에 대입한다.
                Rect rect = new Rect(0, 0, guideCat.width, guideCat.height);
                loadingBar.sprite = Sprite.Create(guideCat, rect, new Vector2(0.5f, 0.5f));

            }
        }*/
    }
}
