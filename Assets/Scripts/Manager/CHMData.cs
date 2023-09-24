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

    public async Task LoadLocalData(string _path)
    {
        Debug.Log("Local Data Load");

        if (loginDataDic == null)
        {
            Debug.Log("Login Local Data Load");
            var loginData = await LoadJsonToLocal<Data.ExtractData<Data.Login>, string, Data.Login>(_path, Defines.EData.Login.ToString());
            loginDataDic = loginData.MakeDict();
        }

        if (stageDataDic == null)
        {
            Debug.Log("Login Local Data Load");
            var stageData = await LoadJsonToLocal<Data.ExtractData<Data.Stage>, string, Data.Stage>(_path, Defines.EData.Stage.ToString());
            stageDataDic = stageData.MakeDict();
        }

        if (collectionDataDic == null)
        {
            Debug.Log("Collection Local Data Load");
            var collectionData = await LoadJsonToLocal<Data.ExtractData<Data.Collection>, string, Data.Collection>(_path, Defines.EData.Collection.ToString());
            collectionDataDic = collectionData.MakeDict();
        }
    }

    async Task<Loader> LoadJsonToLocal<Loader, Key, Value>(string _path, string _name) where Loader : ILoader<Key, Value>
    {
        string path = $"{Application.persistentDataPath}/{_path}.json";

        Debug.Log($"Local Path : {path}");

        if (File.Exists(path) == false)
        {
            TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

            CHMMain.Resource.LoadData(_name, (data) =>
            {
                Debug.Log($"Load Local Data is {data}");
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
            var loginData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Login>, string, Data.Login>(_path, Defines.EData.Login.ToString());
            loginDataDic = loginData.MakeDict();
        }

        if (stageDataDic == null)
        {
            Debug.Log("Stage Cloud Data Load");
            var stageData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Stage>, string, Data.Stage>(_path, Defines.EData.Stage.ToString());
            stageDataDic = stageData.MakeDict();
        }

        if (collectionDataDic == null)
        {
            Debug.Log("Collection Cloud Data Load");
            var collectionData = await LoadJsonToGPGSCloud<Data.ExtractData<Data.Collection>, string, Data.Collection>(_path, Defines.EData.Collection.ToString());
            collectionDataDic = collectionData.MakeDict();
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
                Debug.Log($"Load Game {_name} Data : {data}");
                taskCompletionSource2.SetResult(data);
            });

            var task = await taskCompletionSource2.Task;

            return JsonUtility.FromJson<Loader>($"{{\"{_name.ToLower()}List\":{task.text}}}");
        }

        return JsonUtility.FromJson<Loader>(stringTask);
    }
#endif
}
