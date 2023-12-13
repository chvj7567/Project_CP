using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Defines;
using static Infomation;

public class CHMJson
{
    [Serializable]
    public class JsonData
    {
        public StringInfo[] stringKoreaInfoArr;
        public StringInfo[] stringEnglishInfoArr;
        public SelectInfo[] selectInfoArr;
        public MonsterInfo[] monsterInfoArr;
        public StageInfo[] stageInfoArr;
        public StageBlockInfo[] stageBlockInfoArr;
        public MissionInfo[] missionInfoArr;
        public ShopInfo[] shopInfoArr;
        public TutorialInfo[] tutorialInfoArr;
        public TutorialStageInfo[] tutorialStageInfoArr;
    }

    int loadCompleteFileCount = 0;
    int loadingFileCount = 0;

    List<Action<TextAsset>> actionList = new List<Action<TextAsset>>();

    Dictionary<int, string> stringKoreaInfoDic = new Dictionary<int, string>();
    Dictionary<int, string> stringEnglishInfoDic = new Dictionary<int, string>();
    List<SelectInfo> selectInfoList = new List<SelectInfo>();
    List<MonsterInfo> monsterInfoList = new List<MonsterInfo>();
    List<StageInfo> stageInfoList = new List<StageInfo>();
    List<StageBlockInfo> stageBlockInfoList = new List<StageBlockInfo>();
    List<MissionInfo> missionInfoList = new List<MissionInfo>();
    List<ShopInfo> shopInfoList = new List<ShopInfo>();
    List<TutorialInfo> tutorialInfoList = new List<TutorialInfo>();
    List<TutorialStageInfo> tutorialStageInfoList = new List<TutorialStageInfo>();

    public async Task Init()
    {
        await LoadJsonData();
    }

    public void Clear()
    {
        actionList.Clear();
        stringKoreaInfoDic.Clear();
        selectInfoList.Clear();
        monsterInfoList.Clear();
        stageInfoList.Clear();
        stageBlockInfoList.Clear();
        missionInfoList.Clear();
        shopInfoList.Clear();
        tutorialStageInfoList.Clear();
    }

    async Task LoadJsonData()
    {
        Debug.Log("LoadJsonData");
        loadCompleteFileCount = 0;
        actionList.Clear();

        await LoadStringKoreaInfo();
        await LoadStringEnglishInfo();
        await LoadStageInfo();
        await LoadStageBlockInfo();
        await LoadMissionInfo();
        await LoadShopInfo();
        await LoadTutorialInfo();
        await LoadTutorialStageInfo();

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

    async Task<TextAsset> LoadStringKoreaInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;
        stringKoreaInfoDic.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.StringKorea, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"stringKoreaInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.stringKoreaInfoArr)
            {
                stringKoreaInfoDic.Add(data.stringID, data.value);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });
        
        return await taskCompletionSource.Task;
    }

    async Task<TextAsset> LoadStringEnglishInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;
        stringEnglishInfoDic.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.StringEnglish, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"stringEnglishInfoArr\":" + textAsset.text + "}");
            foreach (var data in jsonData.stringEnglishInfoArr)
            {
                stringEnglishInfoDic.Add(data.stringID, data.value);
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

    async Task<TextAsset> LoadTutorialInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;

        tutorialInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.Tutorial, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"tutorialInfoArr\":" + textAsset.text + "}"));
            foreach (var data in jsonData.tutorialInfoArr)
            {
                tutorialInfoList.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    async Task<TextAsset> LoadTutorialStageInfo()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;

        tutorialStageInfoList.Clear();

        CHMMain.Resource.LoadJson(Defines.EJsonType.TutorialStage, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>(("{\"tutorialStageInfoArr\":" + textAsset.text + "}"));
            foreach (var data in jsonData.tutorialStageInfoArr)
            {
                tutorialStageInfoList.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    public string GetStringInfo(int stringID, Defines.ELanguageType languageType)
    {
        switch (languageType)
        {
            case ELanguageType.Korea:
                {
                    if (stringKoreaInfoDic.TryGetValue(stringID, out string result))
                    {
                        return result;
                    }
                }
                break;
            case ELanguageType.English:
                {
                    if (stringEnglishInfoDic.TryGetValue(stringID, out string result))
                    {
                        return result;
                    }
                }
                break;
        }

        return "";
    }

    public SelectInfo GetSelectInfo(ESelect eSelect)
    {
        var selectList = selectInfoList.FindAll(_ => _.eSelect == eSelect);

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

    public MonsterInfo GetMonsterInfo(int stage, int index)
    {
        return monsterInfoList.Find(_ => _.stage == stage && _.index == index);
    }

    public StageInfo GetStageInfo(int stage)
    {
        return stageInfoList.Find(_ => _.stage == stage);
    }

    public List<StageInfo> GetStageInfoList(int group)
    {
        return stageInfoList.FindAll(_ => _.group == group);
    }

    public int GetMaxStageGroup(int maxGroup)
    {
        return stageInfoList.FindAll(_ => _.group <= maxGroup).Max(_ => _.group);
    }

    public int GetMaxStageGroup()
    {
        return stageInfoList.Max(_ => _.group);
    }

    public List<StageBlockInfo> GetStageBlockInfoList(int stage)
    {
        return stageBlockInfoList.FindAll(_ => _.stage == stage);
    }

    public List<MissionInfo> GetMissionInfoListAll()
    {
        return missionInfoList;
    }

    public List<ShopInfo> GetShopInfoListAll()
    {
        return shopInfoList;
    }

    public List<TutorialInfo> GetTutorialInfoListAll()
    {
        return tutorialInfoList;
    }

    public TutorialInfo GetTutorialInfo(int tutorialIndex)
    {
        return tutorialInfoList.Find(_ => _.tutorialIndex == tutorialIndex);
    }

    public List<TutorialStageInfo> GetTutorialStageInfoListAll()
    {
        return tutorialStageInfoList;
    }

    public TutorialStageInfo GetTutorialStageInfo(int tutorialID)
    {
        return tutorialStageInfoList.Find(_ => _.tutorialStageID == tutorialID);
    }
}
