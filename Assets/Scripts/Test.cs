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

        // ���� ���� �ε�
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // �ٿ�ε� ǥ��
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
            // �ٷ� Sprite�� �������� ���ϰ� Texture2D�� �����´�.
            Texture2D guideCat = assetBundle.LoadAsset<Texture2D>("����1");

            // �ؽ��ĸ� ����� �ε������� �����Ѵ�.
            if (guideCat != null)
            {
                // Texture2D�� Sprite�� ��ȯ�Ͽ� �̹��� �ҽ��� �����Ѵ�.
                Rect rect = new Rect(0, 0, guideCat.width, guideCat.height);
                loadingBar.sprite = Sprite.Create(guideCat, rect, new Vector2(0.5f, 0.5f));
            }
        }
    }
}
