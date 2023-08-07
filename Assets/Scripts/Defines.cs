
using System;

public class Defines
{
    public enum EJsonType
    {
        None = -1,

        String,
        Select,

        Max
    }

    public enum ENormalBlockType
    {
        None = -1,

        Cat1,
        Cat2,
        Cat3,
        Cat4,
        Cat5,

        Max
    }

    public enum ESpecailBlockType
    {
        None = -1,

        CatPang1,

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

    public enum EState
    {
        None = -1,

        Normal = 0,
        Match = 1,

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

        Max
    }
}