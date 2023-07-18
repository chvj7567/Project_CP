using System;
using System.Collections.Generic;
using UnityEngine;
using static Infomation;

public class CHMJson
{
    [Serializable]
    public class JsonData
    {
        public StringInfo[] stringDataArray;
    }

    int loadCompleteFileCount = 0;
    int loadingFileCount = 0;

    List<Action<TextAsset>> actionList = new List<Action<TextAsset>>();
    Dictionary<int, string> stringInfoDic = new Dictionary<int, string>();

    public void Init()
    {
        LoadJsonData();
    }

    public void Clear()
    {
        actionList.Clear();
        stringInfoDic.Clear();
    }

    void LoadJsonData()
    {
        loadCompleteFileCount = 0;
        actionList.Clear();

        //actionList.Add(LoadStringData());

        loadingFileCount = actionList.Count;
    }

    public float GetJsonLoadingPercent()
    {
        if (loadingFileCount == 0 || loadCompleteFileCount == 0)
        {
            return 0;
        }

        return ((float)loadCompleteFileCount) / loadingFileCount * 100f;
    }

    Action<TextAsset> LoadStringData()
    {
        Action<TextAsset> callback;

        stringInfoDic.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.String, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"stringInfoArray\":" + textAsset.text + "}"));
            foreach (var data in jsonData.stringDataArray)
            {
                stringInfoDic.Add(data.stringID, data.value);
            }

            ++loadCompleteFileCount;
        });

        return callback;
    }

    public string TryGetString(int _stringID)
    {
        if (stringInfoDic.TryGetValue(_stringID, out string result))
        {
            return result;
        }

        return "";
    }
}
