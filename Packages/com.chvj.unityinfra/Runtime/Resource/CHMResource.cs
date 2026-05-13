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
        /// 라벨에 속한 모든 Addressables 에셋을 병렬로 preload하고 진행률을 보고.
        /// onProgress(ratio 0~1, currentKey)로 아직 로드 중인 키 하나를 표시용으로 전달.
        /// 로드한 핸들은 _loadedHandles에 캐싱되어 이후 Load&lt;T&gt; 호출이 즉시 반환됨.
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
            if (locations.Count == 0)
            {
                Addressables.Release(locHandle);
                onProgress?.Invoke(1f, string.Empty);
                return true;
            }

            // 1) 캐시 미적중 키만 추려 병렬 launch
            var pending = new List<(string key, AsyncOperationHandle<UnityEngine.Object> handle)>(locations.Count);
            foreach (var loc in locations)
            {
                string key = loc.ToString().Split('/').Last().Split('.').First();
                if (_loadedHandles.ContainsKey(key))
                    continue;
                var h = Addressables.LoadAssetAsync<UnityEngine.Object>(loc);
                pending.Add((key, h));
            }

            int total = pending.Count;
            if (total == 0)
            {
                Addressables.Release(locHandle);
                onProgress?.Invoke(1f, string.Empty);
                return true;
            }

            // 2) 한 프레임마다 완료 카운트 + 미완료 키 하나 보고
            while (true)
            {
                int done = 0;
                string currentKey = null;
                for (int i = 0; i < pending.Count; i++)
                {
                    var p = pending[i];
                    if (p.handle.IsDone) done++;
                    else if (currentKey == null) currentKey = p.key;
                }
                onProgress?.Invoke((float)done / total, currentKey ?? string.Empty);
                if (done == total) break;
                await Task.Yield();
            }

            // 3) 캐시 적재 + 실패 핸들 release
            bool allOk = true;
            foreach (var (key, handle) in pending)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (_loadedHandles.ContainsKey(key) == false)
                        _loadedHandles[key] = handle;
                    else
                        Addressables.Release(handle);
                }
                else
                {
                    Debug.LogWarning($"[CHMResource] Preload failed: {key}");
                    Addressables.Release(handle);
                    allOk = false;
                }
            }

            Addressables.Release(locHandle);
            onProgress?.Invoke(1f, string.Empty);
            return allOk;
        }

        public void Load<T>(Enum key, Action<T> callback) where T : UnityEngine.Object
        {
            Load(key.ToString(), callback);
        }

        public void Load<T>(string key, Action<T> callback) where T : UnityEngine.Object
        {
            if (_dicAssetInfo.TryGetValue(key, out var pathInfo) == false)
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

            var handle = Addressables.LoadAssetAsync<T>(pathInfo);
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
