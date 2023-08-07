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
}
