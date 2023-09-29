using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
    List<Value> MakeList(Dictionary<Key, Value> dict);
}

public class CHMData : CHSingleton<CHMData>
{
    public Dictionary<string, Data.Login> loginDataDic = null;
    public Dictionary<string, Data.Stage> stageDataDic = null;
    public Dictionary<string, Data.Collection> collectionDataDic = null;
    public Dictionary<string, Data.Mission> missionDataDic = null;
    public Dictionary<string, Data.Shop> shopDataDic = null;

    public async Task LoadLocalData(string _path)
    {
        Debug.Log("Local Data Load");

        if (loginDataDic == null)
        {
            Debug.Log("Login Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Login>, string, Data.Login>(_path, Defines.EData.Login.ToString());
            loginDataDic = data.MakeDict();
        }

        if (stageDataDic == null)
        {
            Debug.Log("Stage Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Stage>, string, Data.Stage>(_path, Defines.EData.Stage.ToString());
            stageDataDic = data.MakeDict();
        }

        if (collectionDataDic == null)
        {
            Debug.Log("Collection Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Collection>, string, Data.Collection>(_path, Defines.EData.Collection.ToString());
            collectionDataDic = data.MakeDict();
        }

        if (missionDataDic == null)
        {
            Debug.Log("Mission Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Mission>, string, Data.Mission>(_path, Defines.EData.Mission.ToString());
            missionDataDic = data.MakeDict();
        }

        if (shopDataDic == null)
        {
            Debug.Log("Mission Local Data Load");
            var data = await LoadJsonToLocal<Data.ExtractData<Data.Shop>, string, Data.Shop>(_path, Defines.EData.Shop.ToString());
            shopDataDic = data.MakeDict();
        }
    }

    async Task<Loader> LoadJsonToLocal<Loader, Key, Value>(string _path, string _name) where Loader : ILoader<Key, Value>
    {
        string path = $"{Application.persistentDataPath}/{_path}.json";

        Debug.Log($"Local Path : {path}");

        var data = File.ReadAllText(path);

        // 데이터가 없을 경우 디폴트 데이터 저장
        if (data == "" || data.Contains($"\"{_name.ToLower()}List\":[]"))
        {
            TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

            CHMMain.Resource.LoadData(_name, (data) =>
            {
                Debug.Log($"Load {_name} Data is {data}");
                taskCompletionSource.SetResult(data);
            });

            var task = await taskCompletionSource.Task;

            return JsonUtility.FromJson<Loader>($"{{\"{_name.ToLower()}List\":{task.text}}}");
        }
        else
        {
            return JsonUtility.FromJson<Loader>(File.ReadAllText(path));
        }
    }

    public void SaveData(string _path)
    {
        string json = "";

        Data.ExtractData<Object> saveData = new Data.ExtractData<Object>();

        Data.ExtractData<Data.Login> loginData = new Data.ExtractData<Data.Login>();
        saveData.loginList = loginData.MakeList(loginDataDic);

        Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();
        saveData.stageList = stageData.MakeList(stageDataDic);

        Data.ExtractData<Data.Collection> collectionData = new Data.ExtractData<Data.Collection>();
        saveData.collectionList = collectionData.MakeList(collectionDataDic);

        Data.ExtractData<Data.Mission> missionData = new Data.ExtractData<Data.Mission>();
        saveData.missionList = missionData.MakeList(missionDataDic);

        Data.ExtractData<Data.Shop> shopData = new Data.ExtractData<Data.Shop>();
        saveData.shopList = shopData.MakeList(shopDataDic);

        json = JsonUtility.ToJson(saveData);

        Debug.Log($"Save Local Data is {json}");

        File.WriteAllText($"{Application.persistentDataPath}/{_path}.json", json);

#if UNITY_EDITOR == false && UNITY_ANDROID == true
        CHMGPGS.Instance.SaveCloud(_path, json, success =>
        {
            Debug.Log($"Save Cloud Data is {success} : {json}");
        });
#endif
    }

#if UNITY_EDITOR == false
public async Task LoadCloudData(string _path)
    {
        Debug.Log("Cloud Data Load");

        if (loginDataDic == null)
        {
            Debug.Log("Login Cloud Data Load");
            var data = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Login>, string, Data.Login>(_path, Defines.EData.Login.ToString());
            loginDataDic = data.MakeDict();
        }

        if (stageDataDic == null)
        {
            Debug.Log("Stage Cloud Data Load");
            var data = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Stage>, string, Data.Stage>(_path, Defines.EData.Stage.ToString());
            stageDataDic = data.MakeDict();
        }

        if (collectionDataDic == null)
        {
            Debug.Log("Collection Cloud Data Load");
            var data = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Collection>, string, Data.Collection>(_path, Defines.EData.Collection.ToString());
            collectionDataDic = data.MakeDict();
        }

        if (missionDataDic == null)
        {
            Debug.Log("Mission Cloud Data Load");
            var data = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Mission>, string, Data.Mission>(_path, Defines.EData.Mission.ToString());
            missionDataDic = data.MakeDict();
        }

        if (shopDataDic == null)
        {
            Debug.Log("Mission Local Data Load");
            var data = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Shop>, string, Data.Shop>(_path, Defines.EData.Shop.ToString());
            shopDataDic = data.MakeDict();
        }
    }
public async Task<Loader> LoadJsonToGPGSCloud<Loader, Key, Value>(string _path, string _name) where Loader : ILoader<Key, Value>
    {
        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();

        CHMGPGS.Instance.LoadCloud(_path, (success, data) =>
        {
            Debug.Log($"Load Cloud {_name} Data is {success} : {data}");
            taskCompletionSource.SetResult(data);
        });

        var stringTask = await taskCompletionSource.Task;

        // 데이터가 없을 경우 디폴트 데이터 저장
        if (stringTask == "" || stringTask.Contains($"\"{_name.ToLower()}List\":[]"))
        {
            Debug.Log($"{_name} Data is null");
            TaskCompletionSource<TextAsset> taskCompletionSource2 = new TaskCompletionSource<TextAsset>();

            CHMMain.Resource.LoadData(_name, (data) =>
            {
                Debug.Log($"Load {_name} Data : {data}");
                taskCompletionSource2.SetResult(data);
            });

            var task = await taskCompletionSource2.Task;

            return JsonUtility.FromJson<Loader>($"{{\"{_name.ToLower()}List\":{task.text}}}");
        }

        return JsonUtility.FromJson<Loader>(stringTask);
    }
#endif
    Data.Stage CreateStageData(string _key)
    {
        Debug.Log($"Create Stage {_key}");

        Data.Stage data = new Data.Stage
        {
            key = _key,
            stage = int.Parse(_key),
            clear = false,
            boomAllCount = 0
        };

        stageDataDic.Add(_key, data);

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

        collectionDataDic.Add(_key, data);

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

        missionDataDic.Add(_key, data);

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

        shopDataDic.Add(_key, data);

        return data;
    }

    public Data.Login GetLoginData(string _key)
    {
        if (CHMData.Instance.loginDataDic.TryGetValue(_key, out var data) == false)
        {
            return null;
        }

        return data;
    }

    public Data.Stage GetStageData(string _key)
    {
        if (CHMData.Instance.stageDataDic.TryGetValue(_key, out var data) == false)
        {
            data = CreateStageData(_key);
        }

        return data;
    }

    public Data.Collection GetCollectionData(string _key)
    {
        if (CHMData.Instance.collectionDataDic.TryGetValue(_key, out var data) == false)
        {
            data = CreateCollectionData(_key);
        }

        return data;
    }

    public Data.Mission GetMissionData(string _key)
    {
        if (CHMData.Instance.missionDataDic.TryGetValue(_key, out var data) == false)
        {
            data = CreateMissionData(_key);
        }

        return data;
    }

    public Data.Shop GetShopData(string _key)
    {
        if (CHMData.Instance.shopDataDic.TryGetValue(_key, out var data) == false)
        {
            data = CreateShopData(_key);
        }

        return data;
    }
}
