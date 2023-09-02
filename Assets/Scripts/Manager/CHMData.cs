using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
    List<Value> MakeList(Dictionary<Key, Value> dict);
}

public class CHMData
{
    public Dictionary<string, Data.Stage> stageDataDic = new Dictionary<string, Data.Stage>();

    string stagePath;

    public async void Init()
    {
        stagePath = $"{Application.persistentDataPath}/{Defines.EData.Stage.ToString()}.json";

        Debug.Log($"Path:{stagePath}");

        var playerData = await LoadJson<Data.ExtractData<Data.Stage>, string, Data.Stage>(Defines.EData.Stage.ToString());

        stageDataDic = playerData.MakeDict();
    }

    async Task<Loader> LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        if (path == Defines.EData.Stage.ToString())
        {
            if (File.Exists(stagePath) == false)
            {
                TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

                CHMMain.Resource.LoadStageData((data) =>
                {
                    taskCompletionSource.SetResult(data);
                });

                var textAsset = await taskCompletionSource.Task;

                return JsonUtility.FromJson<Loader>("{\"stageList\":" + textAsset.text + "}");
            }
            else
            {
                return JsonUtility.FromJson<Loader>(File.ReadAllText(stagePath));
            }
        }

        return default(Loader);
    }

    public void SaveJson()
    {
        Data.ExtractData<Data.Stage> stageData = new Data.ExtractData<Data.Stage>();

        stageData.stageList = stageData.MakeList(stageDataDic);

        string json = JsonUtility.ToJson(stageData);
        File.WriteAllText(stagePath, json);
    }
}
