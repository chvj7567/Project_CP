using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEngine.Purchasing.MiniJSON;
using System;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
    List<Value> MakeList(Dictionary<Key, Value> dict);
}

public class CHMData : ChvjUnityInfra.CHSingletonStatic<CHMData>
{
    public readonly int BossStageStartValue = 100000;
    public bool newUser = false;

    // 신규 유저 기본 볼륨 (BGM/효과음)
    const float DefaultVolume = 0.2f;

    public Dictionary<string, Data.Login> loginLocalDataDic = null;
    public Dictionary<string, Data.Collection> collectionLocalDataDic = null;
    public Dictionary<string, Data.Mission> missionLocalDataDic = null;
    public Dictionary<string, Data.Shop> shopLocalDataDic = null;

    public Dictionary<string, Data.Login> loginCloudDataDic = null;
    public Dictionary<string, Data.Collection> collectionCloudDataDic = null;
    public Dictionary<string, Data.Mission> missionCloudDataDic = null;
    public Dictionary<string, Data.Shop> shopCloudDataDic = null;

    public async Task LoadLocalData(string _path)
    {
        Debug.Log("Local Data Load");

        if (loginLocalDataDic == null)
        {
            Debug.Log("Login Local Data Load");
            (bool, Data.ExtractData<Data.Login>) data = await LoadJsonToLocal<Data.ExtractData<Data.Login>, string, Data.Login>(_path, Defines.EData.Login.ToString());
            
            if (data.Item1)
            {
                data.Item2.loginList[0].languageType = Application.systemLanguage == SystemLanguage.Korean ? Defines.ELanguageType.Korea : Defines.ELanguageType.English;
            }

            loginLocalDataDic = data.Item2.MakeDict();
        }

        if (collectionLocalDataDic == null)
        {
            Debug.Log("Collection Local Data Load");
            (bool, Data.ExtractData<Data.Collection>) data = await LoadJsonToLocal<Data.ExtractData<Data.Collection>, string, Data.Collection>(_path, Defines.EData.Collection.ToString());
            collectionLocalDataDic = data.Item2.MakeDict();
        }

        if (missionLocalDataDic == null)
        {
            Debug.Log("Mission Local Data Load");
            (bool, Data.ExtractData<Data.Mission>) data = await LoadJsonToLocal<Data.ExtractData<Data.Mission>, string, Data.Mission>(_path, Defines.EData.Mission.ToString());
            missionLocalDataDic = data.Item2.MakeDict();
        }

        if (shopLocalDataDic == null)
        {
            Debug.Log("Shop Local Data Load");
            (bool, Data.ExtractData<Data.Shop>) data = await LoadJsonToLocal<Data.ExtractData<Data.Shop>, string, Data.Shop>(_path, Defines.EData.Shop.ToString());
            shopLocalDataDic = data.Item2.MakeDict();
        }
    }

    async Task<(bool, Loader)> LoadJsonToLocal<Loader, Key, Value>(string path, string name) where Loader : ILoader<Key, Value>
    {
        string localPath = $"{Application.persistentDataPath}/{path}.json";

        Debug.Log($"Local Path : {localPath}");
        if (File.Exists(localPath) == false)
        {
            // 신규 접속 유저인지 확인
            newUser = true;

            PlayerPrefs.SetInt(CHMString.Instance.HardStage, 0);
            PlayerPrefs.SetInt(CHMString.Instance.NormalStage, 0);
            PlayerPrefs.SetInt(CHMString.Instance.BossStage, 0 + CHMData.Instance.BossStageStartValue);

            PlayerPrefs.SetFloat(CHMString.Instance.BGMVolume, DefaultVolume);
            PlayerPrefs.SetFloat(CHMString.Instance.EffectVolume, DefaultVolume);
            PlayerPrefs.SetFloat(CHMString.Instance.Red, 1f);
            PlayerPrefs.SetFloat(CHMString.Instance.Green, 1f);
            PlayerPrefs.SetFloat(CHMString.Instance.Blue, 1f);
            PlayerPrefs.SetFloat(CHMString.Instance.Alpha, 1f);

            Debug.Log("New User");
            return (true, await LoadDefaultData<Loader>(name));
        }
        else
        {
            string data = File.ReadAllText(localPath);

            // 데이터가 없거나 로드한 리스트 정보가 비어 있는 경우
            if (data.Contains($"{name.ToLower()}List") == false || data.Contains($"\"{name.ToLower()}List\":[]"))
            {
                return (false, await LoadDefaultData<Loader>(name));
            }
            else
            {
                return (false, JsonUtility.FromJson<Loader>(File.ReadAllText(localPath)));
            }
        }
    }

    async Task<Loader> LoadDefaultData<Loader>(string _name)
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        // data/와 json/ 폴더의 basename 충돌(Mission, Shop) 해소: data 쪽에 Default suffix
        string resourceName = (_name == "Mission" || _name == "Shop") ? _name + "Default" : _name;

        CHMResource.Instance.LoadData(resourceName, (data) =>
        {
            Debug.Log($"Load Default {_name} Data is {data}");
            taskCompletionSource.SetResult(data);
        });

        TextAsset task = await taskCompletionSource.Task;

        return JsonUtility.FromJson<Loader>($"{{\"{_name.ToLower()}List\":{task.text}}}");
    }

    public void SaveData(string _path)
    {
        string json = "";

        Data.ExtractData<UnityEngine.Object> saveData = new Data.ExtractData<UnityEngine.Object>();

        Data.ExtractData<Data.Login> loginData = new Data.ExtractData<Data.Login>();
        saveData.loginList = loginData.MakeList(loginLocalDataDic);

        Data.ExtractData<Data.Collection> collectionData = new Data.ExtractData<Data.Collection>();
        saveData.collectionList = collectionData.MakeList(collectionLocalDataDic);

        Data.ExtractData<Data.Mission> missionData = new Data.ExtractData<Data.Mission>();
        saveData.missionList = missionData.MakeList(missionLocalDataDic);

        Data.ExtractData<Data.Shop> shopData = new Data.ExtractData<Data.Shop>();
        saveData.shopList = shopData.MakeList(shopLocalDataDic);

#if UNITY_ANDROID
        json = JsonUtility.ToJson(saveData);
#else
        json = JsonUtility.ToJson(saveData, true);
#endif

        Debug.Log($"Save Local Data is {json}");

        File.WriteAllText($"{Application.persistentDataPath}/{_path}.json", json);

#if UNITY_ANDROID
        if (saveData.loginList.First().connectGPGS == true)
        {
            ChvjUnityInfra.CHMGPGS.Instance.SaveCloud(_path, json, success =>
            {
                Debug.Log($"Save Cloud Data is {success} : {json}");
            });
        }
#endif
    }

#if UNITY_ANDROID
    public async Task LoadCloudData(string path)
    {
        Debug.Log("Cloud Data Load");

        if (loginCloudDataDic == null)
        {
            Debug.Log("Login Cloud Data Load");
            Data.ExtractData<Data.Login> data = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Login>, string, Data.Login>(path, Defines.EData.Login.ToString());

            // 일일 미션 충돌 해소: 클라우드 데이터를 일괄 교체하기 전에
            // 로컬과 클라우드의 lastDailyResetDateKey("yyyyMMdd" UTC)를 비교한다.
            // 로컬이 더 미래(오늘 오프라인 진행)이면 로컬 일일 필드를 클라우드 엔트리에 덮어쓴다.
            // 클라우드가 같거나 미래이면 클라우드 값을 그대로 신뢰한다.
            Dictionary<string, Data.Login> cloudDict = data.MakeDict();
            if (loginLocalDataDic != null)
            {
                foreach (KeyValuePair<string, Data.Login> kvp in cloudDict)
                {
                    if (loginLocalDataDic.TryGetValue(kvp.Key, out Data.Login localLogin) == false)
                        continue;

                    Data.Login cloudLogin = kvp.Value;

                    //# 언어는 기기 로컬 전용 — 클라우드 값으로 덮어쓰지 않는다 (옵션에서만 변경)
                    cloudLogin.languageType = localLogin.languageType;

                    //# 스테이지 진행도는 로컬·클라우드 중 더 높은 값을 채택 — 미로그인 진행도 유실 방지
                    cloudLogin.normalStage = Math.Max(cloudLogin.normalStage, localLogin.normalStage);
                    cloudLogin.hardStage   = Math.Max(cloudLogin.hardStage, localLogin.hardStage);
                    cloudLogin.bossStage   = Math.Max(cloudLogin.bossStage, localLogin.bossStage);

                    // 로컬 날짜 키가 클라우드보다 미래이면 로컬 일일 필드 우선
                    if (string.Compare(localLogin.lastDailyResetDateKey, cloudLogin.lastDailyResetDateKey, StringComparison.Ordinal) > 0)
                    {
                        Debug.Log($"[CHMData] 일일 미션 충돌: 로컬({localLogin.lastDailyResetDateKey}) > 클라우드({cloudLogin.lastDailyResetDateKey}) → 로컬 우선");
                        cloudLogin.lastDailyResetDateKey      = localLogin.lastDailyResetDateKey;
                        cloudLogin.stageClearCountToday       = localLogin.stageClearCountToday;
                        cloudLogin.blockDestroyCountToday     = localLogin.blockDestroyCountToday;
                        cloudLogin.adWatchCountToday          = localLogin.adWatchCountToday;
                        cloudLogin.attendanceTodayDone        = localLogin.attendanceTodayDone;
                        cloudLogin.dailyCollectionSnapshotJson = localLogin.dailyCollectionSnapshotJson;
                    }
                    else
                    {
                        Debug.Log($"[CHMData] 일일 미션 충돌: 클라우드({cloudLogin.lastDailyResetDateKey}) >= 로컬({localLogin.lastDailyResetDateKey}) → 클라우드 우선");
                    }
                }
            }

            loginLocalDataDic = loginCloudDataDic = cloudDict;
        }

        if (collectionCloudDataDic == null)
        {
            Debug.Log("Collection Cloud Data Load");
            Data.ExtractData<Data.Collection> data2 = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Collection>, string, Data.Collection>(path, Defines.EData.Collection.ToString());
            collectionLocalDataDic = collectionCloudDataDic = data2.MakeDict();
        }

        if (missionCloudDataDic == null)
        {
            Debug.Log("Mission Cloud Data Load");
            Data.ExtractData<Data.Mission> data3 = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Mission>, string, Data.Mission>(path, Defines.EData.Mission.ToString());
            missionLocalDataDic = missionCloudDataDic = data3.MakeDict();
        }

        if (shopCloudDataDic == null)
        {
            Debug.Log("Shop Cloud Data Load");
            Data.ExtractData<Data.Shop> data4 = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Shop>, string, Data.Shop>(path, Defines.EData.Shop.ToString());
            shopLocalDataDic = shopCloudDataDic = data4.MakeDict();
        }
    }

    public async Task<Loader> LoadJsonToGPGSCloud<Loader, Key, Value>(string path, string name) where Loader : ILoader<Key, Value>
    {
        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();

        ChvjUnityInfra.CHMGPGS.Instance.LoadCloud(path, (success, data) =>
        {
            Debug.Log($"Load Cloud {name} Data is {success} : {data}");
            taskCompletionSource.SetResult(data);
        });

        string stringTask = await taskCompletionSource.Task;

        // 데이터가 없거나 로드한 리스트 정보가 비어 있는 경우
        if (stringTask.Contains($"{name.ToLower()}List") == false || stringTask.Contains($"\"{name.ToLower()}List\":[]"))
        {
            return await LoadDefaultData<Loader>(name);
        }

        return JsonUtility.FromJson<Loader>(stringTask);
    }

#endif

    public void DeleteData(string path, Action<bool> callback)
    {
        Debug.Log($"Delete : {Application.persistentDataPath}/{path}.json");
        File.Delete($"{Application.persistentDataPath}/{path}.json");

        //# Clear() 이후에 GetLoginData 를 호출하면 connectGPGS=false 인 새 데이터가 생성되므로, Clear() 전에 값을 보관
        bool connectGPGS = GetLoginData(path).connectGPGS;

        loginLocalDataDic.Clear();
        collectionLocalDataDic.Clear();
        missionLocalDataDic.Clear();
        shopLocalDataDic.Clear();

        if (connectGPGS)
        {
#if UNITY_ANDROID
            ChvjUnityInfra.CHMGPGS.Instance.DeleteCloud(path, success =>
            {
                Debug.Log($"Delete Cloud Data is {success} : ");

                loginCloudDataDic.Clear();
                collectionCloudDataDic.Clear();
                missionCloudDataDic.Clear();
                shopCloudDataDic.Clear();
                callback(success);
            });
#else
            callback(true);
#endif
        }
        else
        {
            callback(true);
        }
    }
    Data.Login CreateLoginData(string _key)
    {
        Debug.Log($"Create Login {_key}");

        Data.Login data = new Data.Login
        {
            key = _key,
            connectGPGS = false,
            selectCatShop = 0,
            guideIndex = 0,
            languageType = Application.systemLanguage == SystemLanguage.Korean ? Defines.ELanguageType.Korea : Defines.ELanguageType.English,
        };

        loginLocalDataDic.Add(_key, data);

        return data;
    }

    Data.Collection CreateCollectionData(string _key)
    {
        Debug.Log($"Create Collection {_key}");

        Data.Collection data = new Data.Collection
        {
            key = _key,
            value = 0
        };

        collectionLocalDataDic.Add(_key, data);

        return data;
    }

    Data.Mission CreateMissionData(string _key)
    {
        Debug.Log($"Create Mission {_key}");

        Data.Mission data = new Data.Mission
        {
            key = _key,
            startValue = 0,
            clearState = 0,
            repeatCount = 0
        };

        missionLocalDataDic.Add(_key, data);

        return data;
    }

    Data.Shop CreateShopData(string _key)
    {
        Debug.Log($"Create Shop {_key}");

        Data.Shop data = new Data.Shop
        {
            key = _key,
            buy = false
        };

        shopLocalDataDic.Add(_key, data);

        return data;
    }

    public Data.Login GetLoginData(string _key)
    {
        if (loginLocalDataDic.TryGetValue(_key, out Data.Login data) == false)
        {
            return CreateLoginData(_key);
        }

        return data;
    }

    public Data.Collection GetCollectionData(string _key)
    {
        if (collectionLocalDataDic.TryGetValue(_key, out Data.Collection data) == false)
        {
            data = CreateCollectionData(_key);
        }

        return data;
    }

    public Data.Mission GetMissionData(string _key)
    {
        if (missionLocalDataDic.TryGetValue(_key, out Data.Mission data) == false)
        {
            data = CreateMissionData(_key);
        }

        return data;
    }

    public Data.Shop GetShopData(string _key)
    {
        if (shopLocalDataDic.TryGetValue(_key, out Data.Shop data) == false)
        {
            data = CreateShopData(_key);
        }

        return data;
    }
}
