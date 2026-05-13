using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ChvjUnityInfra
{
    public class CHMResource : CHSingletonStatic<CHMResource>
    {
        private const string LabelName = "Resource";
        private bool _initialize = false;

        private Dictionary<string, IResourceLocation> _dicAssetInfo = new Dictionary<string, IResourceLocation>();
        // 로드된 핸들 보관 — 같은 키 재로드 시 캐시 사용 + 명시적 Unload 가능
        private Dictionary<string, AsyncOperationHandle> _loadedHandles = new Dictionary<string, AsyncOperationHandle>();

        public async Task<bool> Init()
        {
            if (_initialize)
                return false;

            _initialize = true;

            TaskCompletionSource<bool> initComplete = new TaskCompletionSource<bool>();

            Addressables.InitializeAsync().Completed += (handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    initComplete.TrySetResult(false);
                }
                else
                {
                    initComplete.TrySetResult(true);
                }

                Addressables.Release(handle);
            };

            await initComplete.Task;

            return await SaveLocationInfo();
        }

        private async Task<bool> SaveLocationInfo()
        {
            TaskCompletionSource<bool> saveComplete = new TaskCompletionSource<bool>();

            Addressables.LoadResourceLocationsAsync(LabelName).Completed += (handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    saveComplete.TrySetResult(false);
                }
                else
                {
                    foreach (var pathInfo in handle.Result)
                    {
                        string key = pathInfo.ToString().Split('/').Last().Split('.').First();
                        if (_dicAssetInfo.ContainsKey(key) == false)
                        {
                            _dicAssetInfo.Add(key, pathInfo);
                        }
                    }

                    saveComplete.TrySetResult(true);
                }

                Addressables.Release(handle);
            };

            return await saveComplete.Task;
        }

        /// <summary>
        /// 라벨에 속한 Addressables의 번들 의존성을 download/캐시 워밍 + 키 리스트 순회로 진행률 보고.
        /// onProgress(ratio 0~1, currentKey)로 현재 표시할 키 하나를 전달.
        /// 에셋을 메모리에 적재하지는 않음 — 게임 측 Load&lt;T&gt;에서 lazy 로드됨.
        /// 타입 mismatch (PNG가 Texture2D로 캐싱되어 Sprite 로드 시 실패) 회피.
        /// </summary>
        public async Task<bool> PreloadByLabelAsync(string label = LabelName, Action<float, string> onProgress = null)
        {
            var locHandle = Addressables.LoadResourceLocationsAsync(label);
            await locHandle.Task;

            if (locHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(locHandle);
                onProgress?.Invoke(1f, string.Empty);
                return false;
            }

            var locations = locHandle.Result;
            int total = locations.Count;
            if (total == 0)
            {
                Addressables.Release(locHandle);
                onProgress?.Invoke(1f, string.Empty);
                return true;
            }

            // 키 표시용 순회 진행률과 번들 다운로드를 병렬 처리.
            // 번들 다운로드는 로컬 빌드에서 거의 즉시 완료되고, 원격에서는 실제 바이트 진행을 반영.
            var downloadHandle = Addressables.DownloadDependenciesAsync(label, false);

            int displayIdx = 0;
            while (!downloadHandle.IsDone)
            {
                float ratio = downloadHandle.PercentComplete;
                string key = locations[displayIdx % total].ToString().Split('/').Last().Split('.').First();
                onProgress?.Invoke(ratio, key);
                displayIdx++;
                await Task.Yield();
            }

            bool success = downloadHandle.Status == AsyncOperationStatus.Succeeded;
            Addressables.Release(downloadHandle);
            Addressables.Release(locHandle);

            onProgress?.Invoke(1f, string.Empty);
            return success;
        }

        public void Load<T>(Enum key, Action<T> callback) where T : UnityEngine.Object
        {
            Load(key.ToString(), callback);
        }

        public void Load<T>(string key, Action<T> callback) where T : UnityEngine.Object
        {
            if (_dicAssetInfo.ContainsKey(key) == false)
            {
                Debug.LogWarning($"[CHMResource] Asset key not found: {key}");
                callback?.Invoke(null);
                return;
            }

            // 캐시 사용 — 같은 키로 이미 로드됐고 타입 일치하면 즉시 반환
            if (_loadedHandles.TryGetValue(key, out var cached) && cached.IsValid())
            {
                if (cached.Result is T cachedResult)
                {
                    callback?.Invoke(cachedResult);
                    return;
                }
            }

            // address(string)으로 로드 — IResourceLocation 사용 시 ResourceType과 T가 정확히 일치해야 하지만
            // 문자열 키로 로드하면 Addressables의 sub-asset resolution이 작동
            // (예: Sprite-imported PNG location.ResourceType이 Texture2D여도 LoadAssetAsync<Sprite>가 sprite 반환).
            var handle = Addressables.LoadAssetAsync<T>(key);
            _loadedHandles[key] = handle;
            handle.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    callback?.Invoke(h.Result);
                }
                else
                {
                    Debug.LogWarning($"[CHMResource] Load failed: {key}");
                    callback?.Invoke(null);
                }
            };
        }

        public void Instantiate<T>(Enum key, Action<T> callback) where T : UnityEngine.Object
        {
            Instantiate(key.ToString(), callback);
        }

        public void Instantiate<T>(string key, Action<T> callback) where T : UnityEngine.Object
        {
            Load<T>(key, (resource) =>
            {
                if (resource == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                callback?.Invoke(UnityEngine.Object.Instantiate(resource));
            });
        }

        /// <summary>
        /// 특정 키에 대해 로드된 Addressables 핸들을 release. 같은 키 다음 Load 시 다시 로드됨.
        /// </summary>
        public void Unload(Enum key) => Unload(key.ToString());

        public void Unload(string key)
        {
            if (_loadedHandles.TryGetValue(key, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                _loadedHandles.Remove(key);
            }
        }

        /// <summary>
        /// 로드된 모든 Addressables 핸들 release. Scene 전환 시 메모리 회수 등에 사용.
        /// </summary>
        public void UnloadAll()
        {
            foreach (var handle in _loadedHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadedHandles.Clear();
        }
    }
}
