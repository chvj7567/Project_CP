using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Defines;
using static Infomation;

public class CHMJson : CHSingleton<CHMJson>
{
    [Serializable]
    public class JsonData
    {
        public StringInfo[] stringInfoArr;
        public SelectInfo[] selectInfoArr;
        public MonsterInfo[] monsterInfoArr;
        public StageInfo[] stageInfoArr;
        public StageBlockInfo[] stageBlockInfoArr;
        public MissionInfo[] missionInfoArr;
        public ShopInfo[] shopInfoArr;
    }

    int loadCompleteFileCount = 0;
    int loadingFileCount = 0;

    List<Action<TextAsset>> actionList = new List<Action<TextAsset>>();

    Dictionary<int, string> stringInfoDic = new Dictionary<int, string>();
    List<SelectInfo> selectInfoList = new List<SelectInfo>();
    List<MonsterInfo> monsterInfoList = new List<MonsterInfo>();
    List<StageInfo> stageInfoList = new List<StageInfo>();
    List<StageBlockInfo> stageBlockInfoList = new List<StageBlockInfo>();
    List<MissionInfo> missionInfoList = new List<MissionInfo>();
    List<ShopInfo> shopInfoList = new List<ShopInfo>();

    public async Task Init()
    {
        await LoadJsonData();
    }

    public void Clear()
    {
        actionList.Clear();
        stringInfoDic.Clear();
        selectInfoList.Clear();
        monsterInfoList.Clear();
        stageInfoList.Clear();
        stageBlockInfoList.Clear();
        missionInfoList.Clear();
        shopInfoList.Clear();
    }

    async Task LoadJsonData()
    {
        Debug.Log("LoadJsonData");
        loadCompleteFileCount = 0;
        actionList.Clear();

        await LoadStringInfo();
        await LoadStageInfo();
        await LoadStageBlockInfo();
        await LoadMissionInfo();
        await LoadShopInfo();

        /*actionList.Add(LoadStringInfo());
        actionList.Add(LoadSelectInfo());
        actionList.Add(LoadMonsterInfo());
        actionList.Add(LoadStageInfo());
        actionList.Add(LoadStageBlockInfo());
        actionList.Add(LoadMissionInfo());
        actionList.Add(LoadShopInfo());

        loadingFileCount = actionList.Count;*/

        loadingFileCount = loadCompleteFileCount;
    }

    public float GetJsonLoadingPercent()
    {
        if (loadingFileCount == 0 || loadCompleteFileCount == 0)
        {
            return -1;
        }

        return ((float)loadCompleteFileCount) / loadingFileCount * 100f;
    }

    async Task<TextAsset> LoadStringInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;
        stringInfoDic.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.String, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"stringInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.stringInfoArr)
            {
                stringInfoDic.Add(data.stringID, data.value);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });
        
        return await taskCompletionSource.Task;
    }

    async Task<TextAsset> LoadStageInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;

        stageInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Stage, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"stageInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.stageInfoArr)
            {
                stageInfoList.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    async Task<TextAsset> LoadStageBlockInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;

        stageBlockInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.StageBlock, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"stageBlockInfoArr\":" + textAsset.text + "}"));
            foreach (var data in jsonData.stageBlockInfoArr)
            {
                stageBlockInfoList.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    async Task<TextAsset> LoadMissionInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;

        missionInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Mission, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"missionInfoArr\":" + textAsset.text + "}"));
            foreach (var data in jsonData.missionInfoArr)
            {
                missionInfoList.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    async Task<TextAsset> LoadShopInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;

        shopInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Shop, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"shopInfoArr\":" + textAsset.text + "}"));
            foreach (var data in jsonData.shopInfoArr)
            {
                shopInfoList.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    public string GetStringInfo(int _stringID)
    {
        if (stringInfoDic.TryGetValue(_stringID, out string result))
        {
            return result;
        }

        return "";
    }

    public SelectInfo GetSelectInfo(ESelect _eSelect)
    {
        var selectList = selectInfoList.FindAll(_ => _.eSelect == _eSelect);

        Int64 totFrequency = 0L;
        for (int i = 0; i < selectList.Count; ++i)
        {
            totFrequency += selectList[i].frequency;
        }

        var selectFrequency = UnityEngine.Random.Range(0, totFrequency);

        Int64 tempFrequency = 0L;
        for (int i = 0; i < selectList.Count; ++i)
        {
            tempFrequency += selectList[i].frequency;
            if (selectFrequency <= tempFrequency)
            {
                return selectList[i];
            }
        }

        return new SelectInfo();
    }

    public MonsterInfo GetMonsterInfo(int _stage, int _index)
    {
        return monsterInfoList.Find(_ => _.stage == _stage && _.index == _index);
    }

    public StageInfo GetStageInfo(int _stage)
    {
        return stageInfoList.Find(_ => _.stage == _stage);
    }

    public List<StageInfo> GetStageInfoList(int _group)
    {
        return stageInfoList.FindAll(_ => _.group == _group);
    }

    public int GetMaxStageGroup()
    {
        return stageInfoList.Max(_ => _.group);
    }

    public List<StageBlockInfo> GetStageBlockInfoList(int _stage)
    {
        return stageBlockInfoList.FindAll(_ => _.stage == _stage);
    }

    public List<MissionInfo> GetMissionInfoList()
    {
        return missionInfoList;
    }

    public List<ShopInfo> GetShopInfoList()
    {
        return shopInfoList;
    }
}
