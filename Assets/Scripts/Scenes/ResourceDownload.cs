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

#if UNITY_INFRA_APPUPDATE
        await RunAppUpdateFlow();
#endif

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

#if UNITY_INFRA_APPUPDATE
    //# 부팅 시 인앱 업데이트 흐름. Immediate는 차단(강제), Flexible은 백그라운드.
    //# UI 표시/완료 콜백은 씬 비종속 호스트(CHMMain)에 위임한다 — 이 씬은 곧 파괴되므로
    //# Flexible 다운로드 완료(수십 초~분) 시점엔 콜백 대상이 살아 있어야 한다.
    async Task RunAppUpdateFlow()
    {
        ChvjUnityInfra.EAppUpdateAction action = await ChvjUnityInfra.CHMAppUpdate.Instance.CheckAsync();

        if (action == ChvjUnityInfra.EAppUpdateAction.Immediate)
        {
            //# 강제 업데이트는 탈출 불가 — 취소(닫기)해도 안내 후 무한 재요청(spec §8).
            await CHMMain.Instance.RunForcedImmediateLoopAsync();
        }
        else if (action == ChvjUnityInfra.EAppUpdateAction.Flexible)
        {
            //# 완료 콜백을 씬 비종속 호스트로 라우팅(부팅 씬 파괴 후에도 안전).
            ChvjUnityInfra.CHMAppUpdate.Instance.StartFlexible(CHMMain.Instance.ShowFlexibleCompleteConfirm);
        }
    }
#endif
}
