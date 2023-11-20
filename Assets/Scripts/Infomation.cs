using System;

public class Infomation
{
    [Serializable]
    public class StringInfo
    {
        public int stringID = -1;
        public string value = "";
    }

    [Serializable]
    public class SelectInfo
    {
        public Defines.ESelect eSelect = Defines.ESelect.None;
        public int titleStr = -1;
        public float value = -1f;
        public Int64 frequency = -1L;
        public int scoreCost = -1;
    }

    [Serializable]
    public class MonsterInfo
    {
        public int stage = -1;
        public int index = -1;
        public int hp = -1;
        public float moveTime = -1f;
    }

    [Serializable]
    public class StageInfo
    {
        public int group = -1;
        public int stage = -1;
        public int blockTypeCount = -1;
        public int boardSize = -1;
        public float time = -1;
        public int targetScore = -1;
        public int moveCount = -1;
    }

    [Serializable]
    public class StageBlockInfo
    {
        public int stage = -1;
        public Defines.EBlockState blockState;
        public int hp = -1;
        public int row = -1;
        public int col = -1;
    }

    [Serializable]
    public class MissionInfo
    {
        public int missionID = -1;
        public Defines.EBlockState collectionType = Defines.EBlockState.None;
        public int clearValue = -1;
        public int addValue = -1;
    }

    [Serializable]
    public class ShopInfo
    {
        public int shopID = -1;
        public string productName = "";
        public int gold = -1;
        public int titleStringID = -1;
    }
}
