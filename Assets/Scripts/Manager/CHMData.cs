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

    public async Task LoadLocalData()
    {
        Debug.Log("Local Data Load");

        if (loginDataDic == null)
        {
            Debug.Log("Login Local Data Load");
            var loginData = await LoadJsonToLocal<Data.ExtractData<Data.Login>, string, Data.Login>(Defines.EData.Login.ToString());
            loginDataDic = loginData.MakeDict();
        }

        if (stageDataDic == null)
        {
            Debug.Log("Login Local Data Load");
            var stageData = await LoadJsonToLocal<Data.ExtractData<Data.Stage>, string, Data.Stage>(Defines.EData.Stage.ToString());
            stageDataDic = stageData.MakeDict();
        }
    }

    async Task<Loader> LoadJsonToLocal<Loader, Key, Value>(string name) where Loader : ILoader<Key, Value>
    {
        string path = $"{Application.persistentDataPath}/CatPang.json";

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
            return JsonUtility.FromJson<Loader>(File.ReadAllText(path));
        }
    }

    public void SaveJsonToLocal()
    {
        string json = "";

        Data.ExtractData<Object> saveData = new Data.ExtractData<Object>();

        Data.ExtractData<Data.Login> loginData = new Data.ExtractData<Data.Login>();
        saveData.loginList = loginData.MakeList(loginDataDic);

        Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();
        saveData.stageList = stageData.MakeList(stageDataDic);

        json = JsonUtility.ToJson(saveData);

        Debug.Log($"Save Local Data is {json}");

        File.WriteAllText($"{Application.persistentDataPath}/CatPang.json", json);
    }

#if UNITY_EDITOR == false
public async Task LoadCloudData()
    {
        Debug.Log("Cloud Data Load");

        if (loginDataDic == null)
        {
            Debug.Log("Login Cloud Data Load");
            var loginData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Login>, string, Data.Login>(Defines.EData.Login.ToString());
            loginDataDic = loginData.MakeDict();
        }

        if (stageDataDic == null)
        {
            Debug.Log("Stage Cloud Data Load");
            var stageData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Stage>, string, Data.Stage>(Defines.EData.Stage.ToString());
            stageDataDic = stageData.MakeDict();
        }
    }
public async Task<Loader> LoadJsonToGPGSCloud<Loader, Key, Value>(string name) where Loader : ILoader<Key, Value>
    {
        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();

        CHMGPGS.Instance.LoadCloud("CatPang", (success, data) =>
        {
            Debug.Log($"Load Cloud {name} Data is {success} : {data}");
            taskCompletionSource.SetResult(data);
        });

        var stringTask = await taskCompletionSource.Task;

        // 데이터가 없을 경우 디폴트 데이터 저장
        if (stringTask == "" || stringTask.Contains($"\"{name.ToLower()}List\":[]"))
        {
            Debug.Log($"{name} Data is null");
            TaskCompletionSource<TextAsset> taskCompletionSource2 = new TaskCompletionSource<TextAsset>();

            CHMMain.Resource.LoadData(name, (data) =>
            {
                Debug.Log($"Load Game {name} Data : {data}");
                taskCompletionSource2.SetResult(data);
            });

            var task = await taskCompletionSource2.Task;

            return JsonUtility.FromJson<Loader>($"{{\"{name.ToLower()}List\":{task.text}}}");
        }

        return JsonUtility.FromJson<Loader>(stringTask);
    }

    public void SaveJsonToGPGSCloud()
    {
        string json = "";

        Data.ExtractData<Object> saveData = new Data.ExtractData<Object>();

        Data.ExtractData<Data.Login> loginData = new Data.ExtractData<Data.Login>();
        saveData.loginList = loginData.MakeList(loginDataDic);

        Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();
        saveData.stageList = stageData.MakeList(stageDataDic);

        json = JsonUtility.ToJson(saveData);

        Debug.Log($"Save {json}");

        CHMGPGS.Instance.SaveCloud("CatPang", json, success =>
        {
            Debug.Log($"Save Cloud Data is {success} : {json}");
        });
    }
#endif
}
