using System;
using System.Collections;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class Login
    {
        public string key = "";
        public string userID = "";
        public string nickname = "";
        public bool connectGPGS = false;
        public int hardStage = 0;
        public int normalStage = 0;
        public int bossStage = 0;
        public int selectCatShop = 0;
        public int guideIndex = 0;
        public Defines.ELanguageType languageType = Defines.ELanguageType.English;
        public bool buyRemoveAD = false;
        public int addTimeItemCount = 0;
        public int addMoveItemCount = 0;
        public int useTimeItemCount = 0;
        public int useMoveItemCount = 0;
        public int hp = 100;
        public int attack = 0;
    }

    [Serializable]
    public class Stage
    {
        public string key = "";
        public int stage = -1;
        public Defines.EClearState clearState = Defines.EClearState.None;
        public int boomAllCount = -1;
    }

    [Serializable]
    public class Collection
    {
        public string key = "";
        public int value = -1;
    }

    [Serializable]
    public class Mission
    {
        public string key = "";
        public Defines.EBlockState blockState = Defines.EBlockState.None;
        public int startValue = -1;
        public Defines.EClearState clearState = Defines.EClearState.None;
        public int repeatCount = -1;
    }

    [Serializable]
    public class Shop
    {
        public string key = "";
        public bool buy = false;
    }

    [Serializable]
    public class ExtractData<T> : ILoader<string, T> where T : class
    {
        public List<Login> loginList = new List<Login>();
        public List<Collection> collectionList = new List<Collection>();
        public List<Mission> missionList = new List<Mission>();
        public List<Shop> shopList = new List<Shop>();

        public Dictionary<string, T> MakeDict()
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();

            if (typeof(T) == typeof(Login))
            {
                foreach (Login data in loginList)
                    dict.Add(data.key, data as T);
            }
            else if (typeof(T) == typeof(Collection))
            {
                foreach (Collection data in collectionList)
                    dict.Add(data.key, data as T);
            }
            else if (typeof(T) == typeof(Mission))
            {
                foreach (Mission data in missionList)
                    dict.Add(data.key, data as T);
            }
            else if (typeof(T) == typeof(Shop))
            {
                foreach (Shop data in shopList)
                    dict.Add(data.key, data as T);
            }

            return dict;
        }

        public List<T> MakeList(Dictionary<string, T> dict)
        {
            List<T> list = new List<T>();

            foreach (var data in dict)
                list.Add(data.Value);

            return list;
        }

        public bool CheckData(Dictionary<string, T> dict)
        {
            if (typeof(T) == typeof(Login))
            {
                foreach (var data in dict)
                {
                    var temp = data.Value as Login;
                    if (temp == null)
                        return false;

                    if (temp.key == "")
                        return false;
                }
            }
            else if (typeof(T) == typeof(Collection))
            {
                foreach (var data in dict)
                {
                    var temp = data.Value as Collection;
                    if (temp == null)
                        return false;

                    if (temp.key == "")
                        return false;

                    if (temp.value == -1)
                        return false;
                }
            }
            else if (typeof(T) == typeof(Mission))
            {
                foreach (var data in dict)
                {
                    var temp = data.Value as Mission;
                    if (temp == null)
                        return false;

                    if (temp.key == "")
                        return false;

                    if (temp.startValue == -1 ||
                        temp.clearState == Defines.EClearState.None ||
                        temp.repeatCount == -1)
                        return false;
                }
            }
            else if (typeof(T) == typeof(Shop))
            {
                foreach (var data in dict)
                {
                    var temp = data.Value as Shop;
                    if (temp == null)
                        return false;

                    if (temp.key == "")
                        return false;
                }
            }

            return true;
        }
    }
}