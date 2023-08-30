
using System;

public class Defines
{
    public enum EJsonType
    {
        None = -1,

        String,
        Select,
        Stage,

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
        CatPang2,
        CatPang3,
        CatPang4,
        CatPang5,
        CatPang6,

        Max
    }

    public enum EWallBlockType
    {
        None = -1,

        Wall0,
        Wall1,
        Wall2,
        Wall3,

        Max
    }

    public enum EPotalBlockType
    {
        None = -1,

        Potal0,
        Potal1,
        Potal2,
        Potal3,

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

    public enum EState
    {
        None = -1,

        Normal = 0,
        Match = 1,
        Potal = 2,
        Wall = 3,

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
        Button,

        Max
    }
}