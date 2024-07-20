using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
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
    [SerializeField] List<string> googleDownloadValueList = new List<string>();

    [SerializeField, ReadOnly] int totalLoadCount = 0;
    [SerializeField, ReadOnly] int loadingCount = 0;
    [SerializeField, ReadOnly] int totalDownoadCount = 0;
    [SerializeField, ReadOnly] int downloadingCount = 0;

    public Action bundleLoadSuccess;
    public Action bundleDownloadSuccess;

    string googleDriveDownloadURL = "https://docs.google.com/uc?export=download&id=";
    string url = "";

    public bool Init()
    {
        if (CHMAssetBundle.Instance.firstDownload == false)
            return false;

        if (loadingBar) loadingBar.fillAmount = 0f;

        totalDownoadCount = totalLoadCount = googleDownloadKeyList.Count;

        // 에셋 번들 저장 경로 설정
        string savePath = Path.Combine(Application.persistentDataPath, bundleKey);
        if (Directory.Exists(savePath) == false)
        {
            Directory.CreateDirectory(savePath);
        }

        if (googleDriveDownload)
        {
            for (int i = 0; i < googleDownloadKeyList.Count; ++i)
            {
                CHMAssetBundle.Instance.bundleDic.Add(googleDownloadValueList[i], googleDownloadKeyList[i]);
            }

            for (int i = 0; i < googleDownloadKeyList.Count; ++i)
            {
                StartCoroutine(DownloadAssetBundle(googleDownloadKeyList[i]));
            }
        }
        else
        {
            for (int i = 0; i < googleDownloadValueList.Count; ++i)
            {
                CHMAssetBundle.Instance.bundleDic.Add(googleDownloadValueList[i], googleDownloadValueList[i]);
            }

            StartCoroutine(LoadAssetBundleAll());
        }

        bundleDownloadSuccess += () =>
        {
            foreach (var key in googleDownloadKeyList)
            {
                StartCoroutine(LoadAssetBundle(key));
            }
        };

        return true;
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

        Debug.Log($"BundlePath : {bundlePath}");

        // 에셋 번들 로드
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        while (!bundleRequest.isDone)
        {
            yield return null;
        }

        AssetBundle assetBundle = bundleRequest.assetBundle;

        if (assetBundle == null)
        {
            Debug.LogError($"Load Fail : {bundleKey}");
        }
        else
        {
            AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] arrBundleName = manifest.GetAllAssetBundles();

            assetBundle.Unload(false);

            totalLoadCount = arrBundleName.Length;

            foreach (string name in arrBundleName)
            {
                yield return LoadAssetBundle(name);
            }
        }
    }

    IEnumerator LoadAssetBundle(string bundleName)
    {
        string bundlePath = "";
        AssetBundleCreateRequest bundleRequest;
        if (useStreamingAssets)
        {
            bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
            bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        }
        else
        {
            bundlePath = Path.Combine(Application.persistentDataPath, bundleKey);
            bundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(bundlePath + "/", $"{bundleName}.unity3d"));
        }

        downloadText.text = $"{bundleName} Loading...";

        // 다운로드 표시
        float downloadProgress = 0;

        while (bundleRequest.isDone == false)
        {
            downloadProgress = bundleRequest.progress;

            loadingBar.fillAmount = downloadProgress / totalLoadCount * loadingCount;
            loadingText.text = downloadProgress / totalLoadCount * loadingCount * 100f + "%";

            yield return null;
        }

        if (bundleRequest.assetBundle == null)
        {
            Debug.LogError($"{bundleName} is Null");
        }
        else
        {
            downloadProgress = bundleRequest.progress;

            AssetBundle assetBundle = bundleRequest.assetBundle;
            CHMAssetBundle.Instance.LoadAssetBundle(bundleName, assetBundle);

            ++loadingCount;

            loadingBar.fillAmount = downloadProgress / totalLoadCount * loadingCount;
            loadingText.text = downloadProgress / totalLoadCount * loadingCount * 100f + "%";
            downloadText.text = $"{bundleName} Load Success ({loadingCount}/{totalLoadCount})";

            if (loadingCount == totalLoadCount)
            {
                loadingBar.fillAmount = 1;
                loadingText.text = "100%";
                downloadText.text = $"Bundle Load Complete";
                bundleLoadSuccess?.Invoke();
            }
        }
    }

    IEnumerator DownloadAllAssetBundle()
    {
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error : {request.error}");
        }
        else
        {
            // 에셋 번들 저장 경로 설정
            string savePath = Path.Combine(Application.persistentDataPath, bundleKey);

            // 파일 저장
            File.WriteAllBytes(savePath, request.downloadHandler.data);

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

    IEnumerator DownloadAssetBundle(string bundleName)
    {
        string path = "";
        if (googleDriveDownload)
        {
            path = $"{googleDriveDownloadURL}{bundleName}";
        }
        else
        {
            path = Path.Combine(url, bundleName);
        }

        Debug.Log(path);

        UnityWebRequest request = UnityWebRequest.Get(path);

        request.SendWebRequest();

        downloadText.text = $"{bundleName} Loading...";

        // 다운로드 표시
        float downloadProgress = 0;
        while (request.isDone == false)
        {
            downloadProgress = request.downloadProgress;
            int downloadPercentage = Mathf.RoundToInt(downloadProgress * 100);

            loadingBar.fillAmount = downloadProgress / totalDownoadCount * loadingCount;
            loadingText.text = downloadProgress / totalDownoadCount * loadingCount * 100f + "%";

            yield return null;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error : {request.error}");
        }
        else
        {
            // 에셋 번들 저장 경로 설정
            string savePath = Path.Combine(Application.persistentDataPath, bundleKey);

            // 파일 저장
            File.WriteAllBytes(savePath + "/" + $"{bundleName}.unity3d", request.downloadHandler.data);

            Debug.Log($"Saving asset bundle to: {savePath}");

            ++downloadingCount;

            loadingBar.fillAmount = downloadProgress / totalDownoadCount * loadingCount;
            loadingText.text = downloadProgress / totalDownoadCount * loadingCount * 100f + "%";
            downloadText.text = $"{bundleName} Download Success ({loadingCount}/{totalLoadCount})";

            if (downloadingCount == totalDownoadCount)
            {
                loadingBar.fillAmount = 1;
                loadingText.text = "100%";
                downloadText.text = $"Bundle Load Complete";
                bundleDownloadSuccess.Invoke();
            }
        }
    }
}
