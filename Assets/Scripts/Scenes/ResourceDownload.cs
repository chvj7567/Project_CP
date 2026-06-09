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
    async Task RunAppUpdateFlow()
    {
        ChvjUnityInfra.EAppUpdateAction action = await ChvjUnityInfra.CHMAppUpdate.Instance.CheckAsync();

        if (action == ChvjUnityInfra.EAppUpdateAction.Immediate)
        {
            bool ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            //# 강제 업데이트는 탈출 불가 — 취소(닫기)해도 안내 후 무한 재요청(spec §8).
            while (ok == false)
            {
                await ShowForcedUpdateConfirm();
                ok = await ChvjUnityInfra.CHMAppUpdate.Instance.StartImmediateAsync();
            }
        }
        else if (action == ChvjUnityInfra.EAppUpdateAction.Flexible)
        {
            ChvjUnityInfra.CHMAppUpdate.Instance.StartFlexible(ShowFlexibleCompleteConfirm);
        }
    }

    //# 강제 업데이트 안내 팝업. 확인/닫기 어느 경로든 완료되면 호출부가 즉시 재요청한다(탈출 불가).
    Task ShowForcedUpdateConfirm()
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        UIConfirmArg arg = new UIConfirmArg
        {
            confirmType = EConfirmType.Confirm,
            txtTitle = CHMString.Instance.GetString(CHMString.Instance.AppUpdateForcedTitle),
            txtDesc = CHMString.Instance.GetString(CHMString.Instance.AppUpdateForcedDesc),
            onYes = () => tcs.TrySetResult(true),
            onClose = () => tcs.TrySetResult(true),
        };
        CHMUI.Instance.ShowUI(Defines.EUI.UIConfirm, arg);
        return tcs.Task;
    }

    //# Flexible 다운로드 완료 시 재시작 안내. Yes면 설치.
    void ShowFlexibleCompleteConfirm()
    {
        UIConfirmArg arg = new UIConfirmArg
        {
            confirmType = EConfirmType.YesNo,
            txtTitle = CHMString.Instance.GetString(CHMString.Instance.AppUpdateReadyTitle),
            txtDesc = CHMString.Instance.GetString(CHMString.Instance.AppUpdateReadyDesc),
            onYes = () => ChvjUnityInfra.CHMAppUpdate.Instance.CompleteFlexibleUpdate(),
        };
        CHMUI.Instance.ShowUI(Defines.EUI.UIConfirm, arg);
    }
#endif
}
