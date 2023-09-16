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
    public Dictionary<string, Data.Stage> stageDataDic = new Dictionary<string, Data.Stage>();

    string stagePath;

    public async Task LoadLocalData()
    {
        stagePath = $"{Application.persistentDataPath}/{Defines.EData.Stage.ToString()}.json";

        Debug.Log($"Path:{stagePath}");

        var stageData = await LoadJsonToLocal<Data.ExtractData<Data.Stage>, string, Data.Stage>(Defines.EData.Stage.ToString());
        stageDataDic = stageData.MakeDict();
    }

    public async Task LoadCloudData()
    {
        var stageData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Stage>, string, Data.Stage>(Defines.EData.Stage.ToString());
        stageDataDic = stageData.MakeDict();
    }

    async Task<Loader> LoadJsonToLocal<Loader, Key, Value>(string name) where Loader : ILoader<Key, Value>
    {
        if (name == Defines.EData.Stage.ToString())
        {
            if (File.Exists(stagePath) == false)
            {
                TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

                CHMMain.Resource.LoadStageData((data) =>
                {
                    taskCompletionSource.SetResult(data);
                });

                var task = await taskCompletionSource.Task;

                Debug.Log($"Load Local Data is {task.text}");

                return JsonUtility.FromJson<Loader>("{\"stageList\":" + task.text + "}");
            }
            else
            {
                return JsonUtility.FromJson<Loader>(File.ReadAllText(stagePath));
            }
        }

        return default(Loader);
    }

    public void SaveJsonToLocal()
    {
        Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();

        stageData.stageList = stageData.MakeList(stageDataDic);

        string json = JsonUtility.ToJson(stageData);

        Debug.Log($"Save Data is {json}");

        File.WriteAllText(stagePath, json);;
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
        if (stringTask == "")
        {
            TaskCompletionSource<TextAsset> taskCompletionSource2 = new TaskCompletionSource<TextAsset>();

            CHMMain.Resource.LoadStageData((data) =>
            {
                Debug.Log($"Load Game Data : {data}");
                taskCompletionSource2.SetResult(data);
            });

            var task = await taskCompletionSource2.Task;

            return JsonUtility.FromJson<Loader>("{\"stageList\":" + task.text + "}");
        }

        return JsonUtility.FromJson<Loader>(stringTask);
    }

    public void SaveJsonToGPGSCloud()
    {
        Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();

        stageData.stageList = stageData.MakeList(stageDataDic);

        string data = JsonUtility.ToJson(stageData);

        CHMGPGS.Instance.SaveCloud(Defines.EData.Stage.ToString(), data, success =>
        {
            Debug.Log($"Save Data is {success} : {data}");
        });
    }
}
