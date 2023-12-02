
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
        Tutorial,
        TutorialStage,

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
        UIBoom,

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

        // 일반 블럭은 앞에서 처리
        Cat1,
        Cat2,
        Cat3,
        Cat4,
        Cat5,
        Cat6,
        Cat7,
        Cat8,
        Cat9,
        Cat10,

        Arrow1,
        Arrow2,
        Arrow3,
        Arrow4,
        Arrow5,
        Arrow6,

        Wall,
        Potal,

        CatPang,
        PinkBomb,
        YellowBomb,
        OrangeBomb,
        GreenBomb,
        BlueBomb,

        Fish,

        CatHat1,
        CatHat2,
        CatHat3,
        CatHat4,
        CatHat5,

        CatSkin1,
        CatSkin2,
        CatSkin3,
        CatSkin4,
        CatSkin5,

        Locker1,
        Locker2,
        Locker3,
        Locker4,
        Locker5,

        Max
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

    public enum EGameResult
    {
        None = -1,

        GameOver,
        GameClear,
        GameOverWait,
        GameClearWait,

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

    public enum EPurchase
    {
        Success,
        Failure,
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
}