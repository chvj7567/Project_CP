using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UniRx;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CHLoadingBarFromAssetBundle : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] TMP_Text downloadText;
    [SerializeField] bool useStreamingAssets;
    [SerializeField] bool googleDriveDownload;
    [SerializeField] string bundleKey;
    [SerializeField] List<string> googleDownloadKeyList = new List<string>();

    [SerializeField, ReadOnly] int totalDownloadCount = 0;
    [SerializeField, ReadOnly] int downloadingCount = 0;

    public Action bundleDownloadSuccess;

    string googleDriveDownloadURL = "https://docs.google.com/uc?export=download&id=";
    string url = "";

    private void Start()
    {
        if (CHMAssetBundle.Instance.firstDownload == false)
            return;

        if (loadingBar) loadingBar.fillAmount = 0f;

        totalDownloadCount = googleDownloadKeyList.Count;

        StartCoroutine(LoadAssetBundleAll());
        /*if (googleDriveDownload)
        {
            foreach (var key in googleDownloadKeyList)
            {
                StartCoroutine(DownloadAssetBundle(key));
            }
        }
        else
        {
            StartCoroutine(DownloadAllAssetBundle());
        }*/
    }

    IEnumerator LoadAssetBundleAll()
    {
        Debug.Log("LoadAssetBundle");

        string bundlePath = "";

        if (useStreamingAssets)
        {
            bundlePath = Path.Combine(Application.streamingAssetsPath, bundleKey);
        }
        else
        {
            bundlePath = Path.Combine(Application.persistentDataPath, bundleKey);
        }

        // 에셋 번들 로드
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        while (!bundleRequest.isDone)
        {
            yield return null;
        }

        AssetBundle assetBundle = bundleRequest.assetBundle;

        if (assetBundle == null)
        {
            Debug.Log($"Load Fail : {bundleKey}");
        }
        else
        {
            AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] arrBundleName = manifest.GetAllAssetBundles();

            assetBundle.Unload(false);

            totalDownloadCount = arrBundleName.Length;

            foreach (string name in arrBundleName)
            {
                yield return LoadAssetBundle(name);
            }
        }
    }

    IEnumerator LoadAssetBundle(string _bundleName)
    {
        string bundlePath = "";

        if (useStreamingAssets)
        {
            bundlePath = Path.Combine(Application.streamingAssetsPath, _bundleName);
        }
        else
        {
            bundlePath = Path.Combine(Application.persistentDataPath, _bundleName);
        }

        downloadText.text = $"{_bundleName} Downloading...";
        // 에셋 번들 로드
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // 다운로드 표시
        float downloadProgress = 0;

        while (!bundleRequest.isDone)
        {
            downloadProgress = bundleRequest.progress;

            if (loadingBar) loadingBar.fillAmount = downloadProgress / googleDownloadKeyList.Count * downloadingCount;
            if (loadingText) loadingText.text = downloadProgress / googleDownloadKeyList.Count * downloadingCount * 100f + "%";

            yield return null;
        }

        if (bundleRequest.assetBundle == null)
        {
            Debug.LogError($"{_bundleName} is Null");
        }
        else
        {
            downloadProgress = bundleRequest.progress;

            AssetBundle assetBundle = bundleRequest.assetBundle;

            CHMAssetBundle.Instance.LoadAssetBundle(_bundleName, assetBundle);

            ++downloadingCount;

            if (loadingBar) loadingBar.fillAmount = downloadProgress / googleDownloadKeyList.Count * downloadingCount;
            if (loadingText) loadingText.text = downloadProgress / googleDownloadKeyList.Count * downloadingCount * 100f + "%";

            downloadText.text = $"{_bundleName} Download Success";
            Debug.Log($"{_bundleName} Download Success");

            if (downloadingCount == totalDownloadCount)
            {
                if (loadingBar) loadingBar.fillAmount = 1;
                if (loadingText) loadingText.text = "100%";
                Debug.Log($"Bundle download Success");
                bundleDownloadSuccess.Invoke();
            }
        }
    }

    IEnumerator DownloadAllAssetBundle()
    {
        UnityWebRequest request;

        request = UnityWebRequestAssetBundle.GetAssetBundle(Path.Combine(url, bundleKey));

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
        if (googleDriveDownload)
        {
            request = UnityWebRequestAssetBundle.GetAssetBundle(Path.Combine(googleDriveDownloadURL, _bundleName));
        }
        else
        {
            request = UnityWebRequestAssetBundle.GetAssetBundle(Path.Combine(url, bundleKey));
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

            CHMAssetBundle.Instance.LoadAssetBundle(_bundleName, assetBundle);
        }
    }
}
