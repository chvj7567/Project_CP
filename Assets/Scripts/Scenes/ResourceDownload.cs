using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Threading;
using DG.Tweening;

public class ResourceDownload : MonoBehaviour
{
    [SerializeField] CHLoadingBarFromAssetBundle script;
    [SerializeField] List<Image> backgroundList = new List<Image>();

    CancellationTokenSource tokenSource;

    int backgroundIndex = 0;

    private async void Start()
    {
        tokenSource = new CancellationTokenSource();

        ChangeBackgroundLoop();

        var initialize = script.Init();
        if (initialize == false)
        {
            SceneManager.LoadScene(1);
            return;
        }
        
        script.bundleLoadSuccess += async () =>
        {
            await CHMData.Instance.LoadLocalData(CHMMain.String.CatPang);
            SceneManager.LoadScene(1);
        };
    }

    async Task ChangeBackgroundLoop()
    {
        for (int i = 0; i < backgroundList.Count; ++i)
        {
            if (i != backgroundIndex)
            {
                Color color = backgroundList[i].color;
                color.a = 0f;
                backgroundList[i].color = color;
            }
            else
            {
                Color color = backgroundList[i].color;
                color.a = 1f;
                backgroundList[i].color = color;
            }
        }

        await Task.Delay(5000, tokenSource.Token);

        try
        {
            while (true)
            {
                backgroundIndex = ChangeBackground();

                await Task.Delay(10000, tokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancle Change Background");
        }
    }

    int ChangeBackground()
    {
        if (backgroundIndex >= backgroundList.Count)
            return 0;

        int nextIndex = backgroundIndex + 1;
        if (nextIndex >= backgroundList.Count)
        {
            nextIndex = 0;
        }

        backgroundList[backgroundIndex].DOFade(0f, 5f);
        backgroundList[nextIndex].DOFade(1f, 5f);

        return nextIndex;
    }
}
