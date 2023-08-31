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
        public int stage = -1;
        public Defines.EState blockState;
        public int index = -1;
        public int hp = -1;
        public int row = -1;
        public int col = -1;
    }
}
