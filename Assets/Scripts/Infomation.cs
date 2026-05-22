using System;
using UnityEngine;

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
        public int tutorialID = -1;

        // 노말 모드 보정: 시간 제한 제거, 그리고 목표 점수가 있으면 절반 / 없으면 이동 횟수 2배.
        // GPGameScene(실제 게임 로직)와 UIGameStart(시작 화면 표시)가 동일하게 호출해 값이 어긋나지 않도록 한다.
        public void ApplyNormalModifiers()
        {
            time = -1;
            if (targetScore > 0) targetScore /= 2;
            else if (moveCount > 0) moveCount *= 2;
        }
    }

    [Serializable]
    public class StageBlockInfo
    {
        public int stage = -1;
        public Defines.EBlockState blockState;
        public int hp = -1;
        public int row = -1;
        public int col = -1;
        public bool tutorialBlock = false;
    }

    [Serializable]
    public class MissionInfo
    {
        public int missionID = -1;
        public int tapIndex = -1;
        public int descStringID = -1;
        public Defines.EBlockState collectionType = Defines.EBlockState.None;
        public Defines.EDailyCounter dailyCounter = Defines.EDailyCounter.None; // 일일 미션 전용 카운터
        public int clearValue = -1;
        public int addValue = -1;
        public Defines.EReward reward = Defines.EReward.None;
        public int rewardCount = -1;
    }

    [Serializable]
    public class ShopInfo
    {
        public int shopID = -1;
        public int skinIndex = -1;
        public string productName = "";
        public int gold = -1;
        public int titleStringID = -1;
        public int descStringID = -1;
        public int tapIndex = -1;
    }

    [Serializable]
    public class GuideInfo
    {
        public int guideIndex = -1;
        public int descStringID = -1;
    }

    [Serializable]
    public class TutorialInfo
    {
        public int tutorialStageID = -1;
        public int descStringID = -1;
        public Defines.EBlockState connectNextBlock = Defines.EBlockState.None;
        public int descNextBlockStringID = -1;
    }

    [Serializable]
    public class RankInfo
    {
        public string userID = "";
        public Texture2D profileTexture;
        public int stageRank = -1;
        public Int64 stage = -1;
    }

    [Serializable]
    public class ConstValueInfo
    {
        public Defines.EConstValue variable;
        public Int64 value;
    }
}
