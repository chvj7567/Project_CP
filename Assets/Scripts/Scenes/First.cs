using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class First : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] Button startBtn;
    [SerializeField] List<string> liDownloadKey = new List<string>();

    private void Awake()
    {
        if (loadingBar) loadingBar.fillAmount = 0f;
        if (startBtn)
        {
            startBtn.OnClickAsObservable().Subscribe(_ =>
            {
                SceneManager.LoadScene(1);
            });
        }

        foreach (var key in liDownloadKey)
        {
            StartCoroutine(LoadAssetBundle(key));
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
        int downloadPercentage = 0;

        while (!bundleRequest.isDone)
        {
            downloadProgress = bundleRequest.progress;
            downloadPercentage = Mathf.RoundToInt(downloadProgress * 100);

            if (loadingBar) loadingBar.fillAmount = downloadProgress;
            if (loadingText) loadingText.text = downloadPercentage.ToString() + "%";

            yield return null;
        }

        downloadProgress = bundleRequest.progress;
        downloadPercentage = Mathf.RoundToInt(downloadProgress * 100);

        if (loadingBar) loadingBar.fillAmount = downloadProgress;
        if (loadingText) loadingText.text = downloadPercentage.ToString() + "%";

        AssetBundle assetBundle = bundleRequest.assetBundle;

        CHMAssetBundle.LoadAssetBundle(_bundleName, assetBundle);

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
