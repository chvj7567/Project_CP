
using System;

public class Defines
{
    public enum EJsonType
    {
        None = -1,

        StringKorea,
        StringEnglish,
        Select,
        Monster,
        Stage,
        StageBlock,
        Mission,
        Shop,
        Guide,
        Tutorial,
        ConstValue,

        Max
    }

    public enum ELanguageType
    {
        Korea,
        English,
    }

    public enum EResourceType
    {
        None = -1,

        Major,
        Unit,
        UI,
        Json,
        Effect,
        Decal,
        Scriptable,
        Sprite,
        Sound,

        Max
    }

    public enum EAssetPiece
    {
        None = -1,

        Materials,
        Meshes,
        Shaders,
        Texture,

        Max
    }

    public enum EUI
    {
        None = -1,

        EventSystem,
        UICamera,
        UICanvas,
        UIChoice,
        UIAlarm,
        UIMission,
        UIShop,
        UIGameStart,
        UIGameEnd,
        UISetting,
        UIStageSelect,
        UINickname,
        UIRank,
        UIDataDelete,

        Max
    }

    public enum EEffect
    {
        None = -1,

        FireCracker,
        Damage,
        BlueBall,

        Max
    }

    public enum EBlockState
    {
        None = -1,

        Cat1      = 0,
        Cat2      = 1,
        Cat3      = 2,
        Cat4      = 3,
        Cat5      = 4,
        Cat6      = 5,
        Cat7      = 6,

        Arrow1    = 10,
        Arrow2    = 11,
        Arrow3    = 12,
        Arrow4    = 13,
        Arrow5    = 14,
        Arrow6    = 15,

        Wall      = 16,
        Potal     = 17,

        CatPang   = 18,
        PinkBomb  = 19,
        YellowBomb = 20,
        OrangeBomb = 21,
        GreenBomb = 22,
        BlueBomb  = 23,

        Fish      = 24,

        CatHat1   = 25,
        CatHat2   = 26,
        CatHat3   = 27,
        CatHat4   = 28,
        CatHat5   = 29,

        CatSkin1  = 30,
        CatSkin2  = 31,
        CatSkin3  = 32,
        CatSkin4  = 33,
        CatSkin5  = 34,

        Locker1   = 35,
        Locker2   = 36,
        Locker3   = 37,
        Locker4   = 38,
        Locker5   = 39,

        CatBox1   = 40,
        CatBox2   = 41,
        CatBox3   = 42,
        CatBox4   = 43,
        CatBox5   = 44,

        WallCreator  = 45,
        PotalCreator = 46,

        LockerBox1 = 47,
        LockerBox2 = 48,
        LockerBox3 = 49,
        LockerBox4 = 50,
        LockerBox5 = 51,

        RainbowPang = 52,

        Ball      = 53,

        Max       = 54
    }

    public enum EDirection
    {
        None = -1,

        Horizontal,
        Vertical,

        Max
    }

    public enum ESelect
    {
        None = -1,

        Power,
        Delay,
        Lotto,
        AddCat,
        CatPangUpgrade,
        Speed,

        Max
    }

    public enum ESound
    {
        None = -1,

        Bgm,
        Gold,
        Cat,
        Charang,
        Ching,
        Pising,
        Ppauk,

        Max
    }

    public enum EData
    {
        None = -1,

        Login,
        Stage,
        Collection,
        Mission,
        Shop,

        Max
    }

    public enum EBackground
    {
        None = -1,

        Background1,
        Background2,
        Background3,
        Background4,

        Max
    }

    public enum ELog
    {
        None = -1,

        CreateMap,
        UpdateMap,
        CanMatch,
        CheckMap,
        CheckSquareMatch,
        RemoveMatchBlock,
        CreateBoomBlock,
        Check3Match,
        DownBlock,
        ChangeBlock,
        AfterDrag,

        Max
    }

    public enum EGameState
    {
        None = -1,

        CatPang,
        GameOver,
        GameClear,
        GameOverWait,
        GameClearWait,
        NormalOrHardStagePlay,
        BossStagePlay,

        Max
    }

    public enum EClearState
    {
        None = -1,

        NotDoing,
        Doing,
        Clear,

        Max
    }

    public enum EReward
    {
        None = -1,

        Gold,
        AddTime,
        AddMove,

        Max
    }

    public enum EPangEffect
    {
        None = -1,

        Blue,
        Brown,
        Pink,
        Yellow,
        Green,
        Explosion,
        Center_Hit,
        Move_Line,
        Move_Line2,
        

        Max
    }

    public enum ESelectStage
    {
        Hard = 1,
        Boss = 2,
        Normal = 3,
    }

    public enum EDrag
    {
        None = -1,

        Click,
        Up,
        Down,
        Left,
        Right
    }

    public enum EConstValue
    {
        NormalStageGuideMaxIndex = 1,
        BossStageGuideMaxIndex = 2,
        HardStage_NormalStageLock = 3,
        BossStage_HardStageLock = 4
    }
}