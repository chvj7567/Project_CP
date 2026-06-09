#if UNITY_INFRA_APPUPDATE
using System;
using System.Threading.Tasks;
using Google.Play.AppUpdate;
using Google.Play.Common;
using UnityEngine;

namespace ChvjUnityInfra
{
    //# Google Play In-App Updates 매니저. Android 전용, 옵트인(UNITY_INFRA_APPUPDATE).
    public class CHMAppUpdate : CHSingletonStatic<CHMAppUpdate>
    {
        private AppUpdateManager _manager;
        private AppUpdateInfo _info;
        private Action _onFlexibleDownloaded;

        private AppUpdateManager Manager => _manager ??= new AppUpdateManager();

        //# 업데이트 정보 조회 + 정책 판정. 실패/없음/에디터는 None.
        public async Task<EAppUpdateAction> CheckAsync()
        {
#if UNITY_EDITOR
            return EAppUpdateAction.None;
#else
            try
            {
                PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> op = Manager.GetAppUpdateInfo();
                await ToTask(op);

                if (op.Error != AppUpdateErrorCode.NoError)
                    return EAppUpdateAction.None;

                _info = op.GetResult();

                bool available = _info.UpdateAvailability == UpdateAvailability.UpdateAvailable;
                bool immediateAllowed = _info.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions());
                bool flexibleAllowed = _info.IsUpdateTypeAllowed(AppUpdateOptions.FlexibleAppUpdateOptions());

                return AppUpdatePolicy.Decide(available, _info.UpdatePriority, immediateAllowed, flexibleAllowed);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CHMAppUpdate] CheckAsync 실패: {e.Message}");
                return EAppUpdateAction.None;
            }
#endif
        }

        //# Immediate 강제 업데이트. 성공 시 시스템이 앱 재시작. 취소/실패면 false.
        public async Task<bool> StartImmediateAsync()
        {
            if (_info == null)
                return false;

            try
            {
                AppUpdateRequest req = Manager.StartUpdate(_info, AppUpdateOptions.ImmediateAppUpdateOptions());
                await ToTask(req);
                return req.Error == AppUpdateErrorCode.NoError;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CHMAppUpdate] StartImmediate 실패: {e.Message}");
                return false;
            }
        }

        //# Flexible 백그라운드 다운로드. 완료 시 onDownloaded 1회.
        public void StartFlexible(Action onDownloaded)
        {
            if (_info == null)
                return;

            _onFlexibleDownloaded = onDownloaded;
            AppUpdateRequest req = Manager.StartUpdate(_info, AppUpdateOptions.FlexibleAppUpdateOptions());
            req.Completed += _ =>
            {
                if (req.Status == AppUpdateStatus.Downloaded)
                {
                    Action cb = _onFlexibleDownloaded;
                    _onFlexibleDownloaded = null;
                    cb?.Invoke();
                }
            };
        }

        //# 다운로드 완료분 설치(앱 재시작).
        public void CompleteFlexibleUpdate()
        {
            Manager.CompleteUpdate();
        }

        //# PlayAsyncOperation(Completed 이벤트) → Task 변환
        private static Task ToTask(PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> op)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            op.Completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }

        //# AppUpdateRequest(Completed 이벤트) → Task 변환
        private static Task ToTask(AppUpdateRequest req)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            req.Completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
#endif
