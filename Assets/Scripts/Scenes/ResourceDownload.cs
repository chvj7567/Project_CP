using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResourceDownload : MonoBehaviour
{
    [SerializeField] List<Image> backgroundList = new List<Image>();

    CancellationTokenSource tokenSource;
    int backgroundIndex = 0;

    private async void Start()
    {
        tokenSource = new CancellationTokenSource();
        _ = ChangeBackgroundLoop();

        await CHMMain.EnsureInitialized();
        await CHMData.Instance.LoadLocalData(CHMString.Instance.CatPang);
        SceneManager.LoadScene(1);
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
