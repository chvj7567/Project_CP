using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CHTool
{
    [Serializable]
    public class StageInfoJson
    {
        public List<Infomation.StageInfo> stageList = new List<Infomation.StageInfo>();
    }

    [Serializable]
    public class StageBlockInfoJson
    {
        public List<Infomation.StageBlockInfo> stageBlockList = new List<Infomation.StageBlockInfo>();
    }

    [Serializable]
    public class StringKoreaInfoJson
    {
        public List<Infomation.StringInfo> stringList = new List<Infomation.StringInfo>();
    }

    [Serializable]
    public class StringEnglishInfoJson
    {
        public List<Infomation.StringInfo> stringList = new List<Infomation.StringInfo>();
    }
}
