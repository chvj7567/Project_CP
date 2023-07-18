using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] List<string> liDownloadKey = new List<string>();

    private void Awake()
    {
        if (loadingBar) loadingBar.fillAmount = 0f;

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
            // 바로 Sprite로 가져오지 못하고 Texture2D로 가져온다.
            Texture2D guideCat = assetBundle.LoadAsset<Texture2D>("후추1");

            // 텍스쳐를 제대로 로드했으면 생성한다.
            if (guideCat != null)
            {
                // Texture2D를 Sprite로 변환하여 이미지 소스에 대입한다.
                Rect rect = new Rect(0, 0, guideCat.width, guideCat.height);
                loadingBar.sprite = Sprite.Create(guideCat, rect, new Vector2(0.5f, 0.5f));
            }
        }
    }
}
