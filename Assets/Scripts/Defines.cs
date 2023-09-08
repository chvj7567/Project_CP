
using System;

public class Defines
{
    public enum EJsonType
    {
        None = -1,

        String,
        Select,
        Monster,
        Stage,
        StageBlock,

        Max
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

        CatPang1,
        CatPang2,
        CatPang3,
        CatPang4,
        CatPang5,

        Arrow1,
        Arrow2,
        Arrow3,
        Arrow4,
        Arrow5,
        Arrow6,

        Bomb,

        Wall,
        Potal,

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

        Max
    }

    public enum EData
    {
        None = -1,

        Stage,

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
        Check3Match,
        DownBlock,
        ChangeBlock,
        AfterDrag,

        Max
    }
}