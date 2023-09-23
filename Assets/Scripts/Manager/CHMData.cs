using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
    List<Value> MakeList(Dictionary<Key, Value> dict);
}

public class CHMData : CHSingleton<CHMData>
{
    public Dictionary<string, Data.Login> loginDataDic = new Dictionary<string, Data.Login>();
    public Dictionary<string, Data.Stage> stageDataDic = new Dictionary<string, Data.Stage>();

    public async Task LoadLocalData()
    {
        var loginData = await LoadJsonToLocal<Data.ExtractData<Data.Login>, string, Data.Login>(Defines.EData.Login.ToString());
        var stageData = await LoadJsonToLocal<Data.ExtractData<Data.Stage>, string, Data.Stage>(Defines.EData.Stage.ToString());

        loginDataDic = loginData.MakeDict();
        stageDataDic = stageData.MakeDict();
    }

    async Task<Loader> LoadJsonToLocal<Loader, Key, Value>(string name) where Loader : ILoader<Key, Value>
    {
        string path = $"{Application.persistentDataPath}/{name}.json";

        Debug.Log($"Local Path : {path}");

        if (File.Exists(path) == false)
        {
            TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

            CHMMain.Resource.LoadData(name, (data) =>
            {
                Debug.Log($"Load Local Data is {data}");
                taskCompletionSource.SetResult(data);
            });

            var task = await taskCompletionSource.Task;

            return JsonUtility.FromJson<Loader>($"{{\"{name.ToLower()}List\":{task.text}}}");
        }
        else
        {
            return JsonUtility.FromJson<Loader>(File.ReadAllText($"{Application.persistentDataPath}/{name}.json"));
        }
    }

    public void SaveJsonToLocal(string name)
    {
        string json = "";

        if (name == Defines.EData.Login.ToString())
        {
            Data.ExtractData<Data.Login> loginData = new Data.ExtractData<Data.Login>();

            loginData.loginList = loginData.MakeList(loginDataDic);

            json = JsonUtility.ToJson(loginData);
        }
        else if (name == Defines.EData.Stage.ToString())
        {
            Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();

            stageData.stageList = stageData.MakeList(stageDataDic);

            json = JsonUtility.ToJson(stageData);
        }

        Debug.Log($"Save Data is {json}");

        File.WriteAllText($"{Application.persistentDataPath}/{name.ToLower()}.json", json);
    }

#if UNITY_EDITOR == false
public async Task LoadCloudData()
    {
        var loginData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Login>, string, Data.Login>(Defines.EData.Login.ToString());
        var stageData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Stage>, string, Data.Stage>(Defines.EData.Stage.ToString());

        loginDataDic = loginData.MakeDict();
        stageDataDic = stageData.MakeDict();
    }
public async Task<Loader> LoadJsonToGPGSCloud<Loader, Key, Value>(string name) where Loader : ILoader<Key, Value>
    {
        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();

        CHMGPGS.Instance.LoadCloud(name, (success, data) =>
        {
            Debug.Log($"Load Cloud Data is {success} : {data}");
            taskCompletionSource.SetResult(data);
        });

        var stringTask = await taskCompletionSource.Task;

        // 데이터가 없을 경우 디폴트 데이터 저장
        if (stringTask.Contains($"\"{name.ToLower()}List\":[]"))
        {
            Debug.Log($"{name}Data is null");
            TaskCompletionSource<TextAsset> taskCompletionSource2 = new TaskCompletionSource<TextAsset>();

            CHMMain.Resource.LoadData(name, (data) =>
            {
                Debug.Log($"Load Game Data : {data}");
                taskCompletionSource2.SetResult(data);
            });

            var task = await taskCompletionSource2.Task;

            return JsonUtility.FromJson<Loader>($"{{\"{name.ToLower()}List\":{task.text}}}");
        }

        return JsonUtility.FromJson<Loader>(stringTask);
    }

    public void SaveJsonToGPGSCloud(string name)
    {
        string json = "";

        if (name == Defines.EData.Login.ToString())
        {
            Data.ExtractData<Data.Login> loginData = new Data.ExtractData<Data.Login>();

            loginData.loginList = loginData.MakeList(loginDataDic);

            json = JsonUtility.ToJson(loginData);
        }
        else if (name == Defines.EData.Stage.ToString())
        {
            Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();

            stageData.stageList = stageData.MakeList(stageDataDic);

            json = JsonUtility.ToJson(stageData);
        }

        CHMGPGS.Instance.SaveCloud(name, json, success =>
        {
            Debug.Log($"Save Data is {success} : {json}");
        });
    }
#endif
}
