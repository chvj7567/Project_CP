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

    // 첫 배경 전환까지의 대기 시간(ms)
    const int FirstBackgroundChangeDelayMs = 5000;
    // 이후 배경 전환 주기(ms)
    const int BackgroundChangeIntervalMs = 10000;
    // 배경 페이드 인/아웃 트윈 시간(초)
    const float BackgroundFadeDuration = 5f;

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

        SceneManager.LoadScene((int)Defines.EScene.FirstScene);
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
            await Task.Delay(FirstBackgroundChangeDelayMs, tokenSource.Token);
            while (true)
            {
                backgroundIndex = ChangeBackground();
                await Task.Delay(BackgroundChangeIntervalMs, tokenSource.Token);
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

        backgroundList[backgroundIndex].DOFade(0f, BackgroundFadeDuration);
        backgroundList[nextIndex].DOFade(1f, BackgroundFadeDuration);
        return nextIndex;
    }

    private void OnDestroy()
    {
        tokenSource?.Cancel();
        tokenSource?.Dispose();
    }
}
