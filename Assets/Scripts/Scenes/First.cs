using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class First : MonoBehaviour
{
    [SerializeField] Canvas canvas;
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

                CHMMain.Resource.InstantiateEffect(Defines.EEffect.FireCracker, (effect) =>
                {
                    effect.transform.SetParent(canvas.transform);
                    var rectTransform = effect.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition3D = new Vector3(0, 0, -100);
                    rectTransform.localScale = Vector3.one;
                });
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

            startBtn.gameObject.SetActive(false);
            stageGroupObj.SetActive(true);
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
    }
}
