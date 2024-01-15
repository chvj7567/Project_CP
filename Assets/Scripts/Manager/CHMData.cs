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

public class CHMData : CHSingleton<CHMData>
{
    public readonly int BossStageStartValue = 100000;
    public bool newUser = false;

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
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Login>, string, Data.Login>(_path, Defines.EData.Login.ToString());
            
            if (data.Item1)
            {
                data.Item2.loginList[0].languageType = Application.systemLanguage == SystemLanguage.Korean ? Defines.ELanguageType.Korea : Defines.ELanguageType.English;
            }

            loginLocalDataDic = data.Item2.MakeDict();
        }

        if (collectionLocalDataDic == null)
        {
            Debug.Log("Collection Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Collection>, string, Data.Collection>(_path, Defines.EData.Collection.ToString());
            collectionLocalDataDic = data.Item2.MakeDict();
        }

        if (missionLocalDataDic == null)
        {
            Debug.Log("Mission Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Mission>, string, Data.Mission>(_path, Defines.EData.Mission.ToString());
            missionLocalDataDic = data.Item2.MakeDict();
        }

        if (shopLocalDataDic == null)
        {
            Debug.Log("Shop Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Shop>, string, Data.Shop>(_path, Defines.EData.Shop.ToString());
            shopLocalDataDic = data.Item2.MakeDict();
        }
    }

    async Task<(bool, Loader)> LoadJsonToLocal<Loader, Key, Value>(string path, string name) where Loader : ILoader<Key, Value>
    {
        string localPath = $"{Application.persistentDataPath}/{path}.json";

        Debug.Log($"Local Path : {localPath}");
        if (File.Exists(localPath) == false)
        {
            // 신규 유저
            newUser = true;

            // 기본 스킨
            GetShopData("1").buy = true;

            PlayerPrefs.SetInt(CHMMain.String.HardStage, 0);
            PlayerPrefs.SetInt(CHMMain.String.NormalStage, 0);
            PlayerPrefs.SetInt(CHMMain.String.BossStage, 0 + CHMData.Instance.BossStageStartValue);

            PlayerPrefs.SetFloat(CHMMain.String.BGMVolume, .2f);
            PlayerPrefs.SetFloat(CHMMain.String.EffectVolume, .2f);
            PlayerPrefs.SetFloat(CHMMain.String.Red, 1f);
            PlayerPrefs.SetFloat(CHMMain.String.Green, 1f);
            PlayerPrefs.SetFloat(CHMMain.String.Blue, 1f);
            PlayerPrefs.SetFloat(CHMMain.String.Alpha, 1f);

            Debug.Log("New User");
            return (true, await LoadDefaultData<Loader>(name));
        }
        else
        {
            var data = File.ReadAllText(localPath);

            // 데이터가 없을 경우 디폴트 데이터 저장
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

        CHMMain.Resource.LoadData(_name, (data) =>
        {
            Debug.Log($"Load Default {_name} Data is {data}");
            taskCompletionSource.SetResult(data);
        });

        var task = await taskCompletionSource.Task;

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
            CHMGPGS.Instance.SaveCloud(_path, json, success =>
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
            var data = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Login>, string, Data.Login>(path, Defines.EData.Login.ToString());
            loginLocalDataDic = loginCloudDataDic = data.MakeDict();
        }

        if (collectionCloudDataDic == null)
        {
            Debug.Log("Collection Cloud Data Load");
            var data2 = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Collection>, string, Data.Collection>(path, Defines.EData.Collection.ToString());
            collectionLocalDataDic = collectionCloudDataDic = data2.MakeDict();
        }

        if (missionCloudDataDic == null)
        {
            Debug.Log("Mission Cloud Data Load");
            var data3 = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Mission>, string, Data.Mission>(path, Defines.EData.Mission.ToString());
            missionLocalDataDic = missionCloudDataDic = data3.MakeDict();
        }

        if (shopCloudDataDic == null)
        {
            Debug.Log("Shop Cloud Data Load");
            var data4 = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Shop>, string, Data.Shop>(path, Defines.EData.Shop.ToString());
            shopLocalDataDic = shopCloudDataDic = data4.MakeDict();
        }
    }

    public async Task<Loader> LoadJsonToGPGSCloud<Loader, Key, Value>(string path, string name) where Loader : ILoader<Key, Value>
    {
        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();

        CHMGPGS.Instance.LoadCloud(path, (success, data) =>
        {
            Debug.Log($"Load Cloud {name} Data is {success} : {data}");
            taskCompletionSource.SetResult(data);
        });

        var stringTask = await taskCompletionSource.Task;

        // 데이터가 없을 경우 디폴트 데이터 저장
        if (stringTask.Contains($"{name.ToLower()}List") == false || stringTask.Contains($"\"{name.ToLower()}List\":[]"))
        {
            return await LoadDefaultData<Loader>(name);
        }

        return JsonUtility.FromJson<Loader>(stringTask);
    }

    public void DeleteData(string path, Action<bool> callback)
    {
        Debug.Log($"Delete : {Application.persistentDataPath}/{path}.json");
        File.Delete($"{Application.persistentDataPath}/{path}.json");

        loginLocalDataDic.Clear();
        collectionLocalDataDic.Clear();
        missionLocalDataDic.Clear();
        shopLocalDataDic.Clear();

        if (GetLoginData(path).connectGPGS)
        {
            CHMGPGS.Instance.DeleteCloud(path, success =>
            {
                Debug.Log($"Delete Cloud Data is {success} : ");

                loginCloudDataDic.Clear();
                collectionCloudDataDic.Clear();
                missionCloudDataDic.Clear();
                shopCloudDataDic.Clear();
                callback(success);
            });
        }
        else
        {
            callback(true);
        }
    }
#endif
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
        if (loginLocalDataDic.TryGetValue(_key, out var data) == false)
        {
            return CreateLoginData(_key);
        }

        return data;
    }

    public Data.Collection GetCollectionData(string _key)
    {
        if (collectionLocalDataDic.TryGetValue(_key, out var data) == false)
        {
            data = CreateCollectionData(_key);
        }

        return data;
    }

    public Data.Mission GetMissionData(string _key)
    {
        if (missionLocalDataDic.TryGetValue(_key, out var data) == false)
        {
            data = CreateMissionData(_key);
        }

        return data;
    }

    public Data.Shop GetShopData(string _key)
    {
        if (shopLocalDataDic.TryGetValue(_key, out var data) == false)
        {
            data = CreateShopData(_key);
        }

        return data;
    }
}
