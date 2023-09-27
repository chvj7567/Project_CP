using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Defines;
using static Infomation;

public class CHMJson
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

    public void Init()
    {
        LoadJsonData();
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
    }

    void LoadJsonData()
    {
        loadCompleteFileCount = 0;
        actionList.Clear();

        actionList.Add(LoadStringInfo());
        actionList.Add(LoadSelectInfo());
        actionList.Add(LoadMonsterInfo());
        actionList.Add(LoadStageInfo());
        actionList.Add(LoadStageBlockInfo());
        actionList.Add(LoadMissionInfo());

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

    Action<TextAsset> LoadStringInfo()
    {
        Action<TextAsset> callback;

        stringInfoDic.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.String, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"stringInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.stringInfoArr)
            {
                stringInfoDic.Add(data.stringID, data.value);
            }

            ++loadCompleteFileCount;
        });

        return callback;
    }

    Action<TextAsset> LoadSelectInfo()
    {
        Action<TextAsset> callback;

        selectInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Select, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"selectInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.selectInfoArr)
            {
                selectInfoList.Add(data);
            }

            ++loadCompleteFileCount;
        });

        return callback;
    }

    Action<TextAsset> LoadMonsterInfo()
    {
        Action<TextAsset> callback;

        monsterInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Monster, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"monsterInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.monsterInfoArr)
            {
                monsterInfoList.Add(data);
            }

            ++loadCompleteFileCount;
        });

        return callback;
    }

    Action<TextAsset> LoadStageInfo()
    {
        Action<TextAsset> callback;

        stageInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Stage, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"stageInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.stageInfoArr)
            {
                stageInfoList.Add(data);
            }

            ++loadCompleteFileCount;
        });

        return callback;
    }

    Action<TextAsset> LoadStageBlockInfo()
    {
        Action<TextAsset> callback;

        stageBlockInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.StageBlock, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"stageBlockInfoArr\":" + textAsset.text + "}"));
            foreach (var data in jsonData.stageBlockInfoArr)
            {
                stageBlockInfoList.Add(data);
            }

            ++loadCompleteFileCount;
        });

        return callback;
    }

    Action<TextAsset> LoadMissionInfo()
    {
        Action<TextAsset> callback;

        missionInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Mission, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"missionInfoArr\":" + textAsset.text + "}"));
            foreach (var data in jsonData.missionInfoArr)
            {
                missionInfoList.Add(data);
            }

            ++loadCompleteFileCount;
        });

        return callback;
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
}
