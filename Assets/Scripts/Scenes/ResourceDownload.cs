using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResourceDownload : MonoBehaviour
{
    [SerializeField] List<Image> backgroundList = new List<Image>();
    [SerializeField] Image loadingBar;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] TMP_Text downloadText;

    CancellationTokenSource tokenSource;
    int backgroundIndex = 0;

    private async void Start()
    {
        tokenSource = new CancellationTokenSource();
        _ = ChangeBackgroundLoop();

        SetProgress(0f, string.Empty);

        await CHMResource.Instance.EnsureInit();
        await CHMResource.Instance.PreloadAsync(SetProgress);

        await CHMMain.EnsureInitialized();
        await CHMData.Instance.LoadLocalData(CHMString.Instance.CatPang);

        SetProgress(1f, string.Empty);

        SceneManager.LoadScene(1);
    }

    void SetProgress(float ratio, string key)
    {
        if (loadingBar != null) loadingBar.fillAmount = ratio;
        if (loadingText != null) loadingText.text = $"{Mathf.RoundToInt(ratio * 100f)}%";
        if (downloadText != null && string.IsNullOrEmpty(key) == false) downloadText.text = key;
    }

    async Task ChangeBackgroundLoop()
    {
        for (int i = 0; i < backgroundList.Count; ++i)
        {
            Color color = backgroundList[i].color;
            color.a = (i == backgroundIndex) ? 1f : 0f;
            backgroundList[i].color = color;
        }

        try
        {
            await Task.Delay(5000, tokenSource.Token);
            while (true)
            {
                backgroundIndex = ChangeBackground();
                await Task.Delay(10000, tokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancel Change Background");
        }
    }

    int ChangeBackground()
    {
        if (backgroundIndex >= backgroundList.Count) return 0;
        int nextIndex = backgroundIndex + 1;
        if (nextIndex >= backgroundList.Count) nextIndex = 0;

        backgroundList[backgroundIndex].DOFade(0f, 5f);
        backgroundList[nextIndex].DOFade(1f, 5f);
        return nextIndex;
    }

    private void OnDestroy()
    {
        tokenSource?.Cancel();
        tokenSource?.Dispose();
    }
}
