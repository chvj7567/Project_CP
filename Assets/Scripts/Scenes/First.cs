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

        // ���� ���� �ε�
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // �ٿ�ε� ǥ��
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
            // �ٷ� Sprite�� �������� ���ϰ� Texture2D�� �����´�.
            Texture2D guideCat = assetBundle.LoadAsset<Texture2D>("huchu1");

            // �ؽ��ĸ� ����� �ε������� �����Ѵ�.
            if (guideCat != null)
            {
                // Texture2D�� Sprite�� ��ȯ�Ͽ� �̹��� �ҽ��� �����Ѵ�.
                Rect rect = new Rect(0, 0, guideCat.width, guideCat.height);
                loadingBar.sprite = Sprite.Create(guideCat, rect, new Vector2(0.5f, 0.5f));

            }
        }*/
    }
}
