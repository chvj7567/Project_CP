
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

    // 빌드 인덱스 순서 그대로. enum 이름은 Assets/Scenes 의 .unity 파일명과 정확히 일치시킬 것
    // (SceneManager.GetActiveScene().name 과 nameof(EScene.X) 직접 비교를 위함).
    public enum EScene
    {
        None = -1,

        ResourceDownloadScene,
        FirstScene,
        GameScene,
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
        UIAlarm,
        UIMission,
        UIShop,
        UIGameStart,
        UIGameEnd,
        UISetting,
        UIStageSelect,
        UINickname,
        UIRank,
        UIConfirm,

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

        // 25~39: 옛 CatHat/CatSkin/Locker 스킨 슬롯 (삭제됨). StageBlock.json의 int 값 호환을 위해 빈 슬롯로 유지

        CatBox1   = 40,
        CatBox2   = 41,
        CatBox3   = 42,
        CatBox4   = 43,
        CatBox5   = 44,

        WallCreator  = 45,
        PotalCreator = 46,

        // 47~51: 옛 LockerBox 슬롯 (삭제됨). StageBlock.json의 int 값 호환을 위해 빈 슬롯로 유지

        RainbowPang = 52,

        Ball      = 53,

        // 신규 고양이 스킨 (테마 6종 × Cat1~5). 끝에 append 하여 0~53 슬롯 보존
        CatCrown1      = 54,
        CatCrown2      = 55,
        CatCrown3      = 56,
        CatCrown4      = 57,
        CatCrown5      = 58,

        CatFlowers1    = 59,
        CatFlowers2    = 60,
        CatFlowers3    = 61,
        CatFlowers4    = 62,
        CatFlowers5    = 63,

        CatMushroom1   = 64,
        CatMushroom2   = 65,
        CatMushroom3   = 66,
        CatMushroom4   = 67,
        CatMushroom5   = 68,

        CatParty1      = 69,
        CatParty2      = 70,
        CatParty3      = 71,
        CatParty4      = 72,
        CatParty5      = 73,

        CatSanta1      = 74,
        CatSanta2      = 75,
        CatSanta3      = 76,
        CatSanta4      = 77,
        CatSanta5      = 78,

        CatStrawberry1 = 79,
        CatStrawberry2 = 80,
        CatStrawberry3 = 81,
        CatStrawberry4 = 82,
        CatStrawberry5 = 83,

        Max            = 84
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

    public enum EDailyCounter
    {
        // 미션에서 카운터 미사용 — collectionType 기반 또는 일일 미션이 아닌 경우
        None = -1,

        // 출석 (UIMission 일일 탭 진입 시 즉시 1로 설정)
        Attendance = 0,

        // 오늘 노멀 스테이지 클리어 횟수 (재클리어 포함)
        NormalStageClear = 1,

        // 오늘 매치로 파괴된 블록 총 개수 (Wall/Locker 등 직접 매치 불가 항목 제외)
        BlockDestroy = 2,

        // 오늘 보상형 광고 시청 횟수
        AdWatch = 3,
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
        BossStage_HardStageLock = 4,
        AddMoveItemValue = 5,
        AddTimeItemValue = 6
    }
}