using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CHLoadingBarFromAssetBundle : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] bool useS3 = false;
    [SerializeField] string bundleKey;
    [SerializeField] List<string> liDownloadKey = new List<string>();

    string downloadInitURL = "https://docs.google.com/uc?export=download&id=";
    string url = "";
    float tot = 0;

    private void Awake()
    {
        if (loadingBar) loadingBar.fillAmount = 0f;

        if (useS3 == false)
        {
            foreach (var key in liDownloadKey)
            {
                StartCoroutine(DownloadAssetBundle(key));
            }
        }
        else
        {
            StartCoroutine(DownloadAllAssetBundle());
        }
    }

    IEnumerator LoadAssetBundle(string _bundleName)
    {
        string bundlePath = Path.Combine(Application.persistentDataPath, _bundleName);

        // 에셋 번들 로드
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // 다운로드 표시
        while (!bundleRequest.isDone)
        {
            float downloadProgress = bundleRequest.progress;
            int downloadPercentage = Mathf.RoundToInt(downloadProgress * 100);

            if (loadingBar) loadingBar.fillAmount = downloadProgress;
            if (loadingText) loadingText.text = downloadPercentage.ToString() + "%";

            yield return null;
        }

        AssetBundle assetBundle = bundleRequest.assetBundle;

        if (assetBundle != null)
        {
            CHMAssetBundle.LoadAssetBundle(_bundleName, assetBundle);
        }
        else
        {
            if (useS3 == false)
            {
                foreach (var key in liDownloadKey)
                {
                    StartCoroutine(DownloadAssetBundle(key));
                }
            }
            else
            {
                StartCoroutine(DownloadAllAssetBundle());
            }
        }
    }

    IEnumerator DownloadAllAssetBundle()
    {
        UnityWebRequest request;
        if (useS3 == false)
        {
            request = UnityWebRequestAssetBundle.GetAssetBundle($"{downloadInitURL}{bundleKey}");
        }
        else
        {
            request = UnityWebRequestAssetBundle.GetAssetBundle($"{url}/Bundle");
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"Error : {request.error}");
        }
        else
        {
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);
            AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] arrBundleName = manifest.GetAllAssetBundles();

            assetBundle.Unload(false);
            foreach (string name in arrBundleName)
            {
                yield return DownloadAssetBundle(name);
            }
        }
    }

    IEnumerator DownloadAssetBundle(string _bundleName)
    {
        UnityWebRequest request;
        if (useS3 == false)
        {
            request = UnityWebRequestAssetBundle.GetAssetBundle($"{downloadInitURL}{_bundleName}");
        }
        else
        {
            request = UnityWebRequestAssetBundle.GetAssetBundle($"{url}/{_bundleName}");
        }

        request.SendWebRequest();

        // 다운로드 표시
        while (!request.isDone)
        {
            float downloadProgress = request.downloadProgress;
            int downloadPercentage = Mathf.RoundToInt(downloadProgress * 100);

            if (loadingBar) loadingBar.fillAmount = downloadProgress;
            if (loadingText) loadingText.text = downloadPercentage.ToString() + "%";

            yield return null;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"Error : {request.error}");
        }
        else
        {
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);

            // 에셋 번들 저장 경로 설정
            string savePath = Path.Combine(Application.persistentDataPath, _bundleName);
            Debug.Log($"Saving asset bundle to: {savePath}");

            // 파일 저장
            File.WriteAllBytes(savePath, request.downloadHandler.data);

            Debug.Log($"Success : {_bundleName}");

            CHMAssetBundle.LoadAssetBundle(_bundleName, assetBundle);
        }
    }
}
