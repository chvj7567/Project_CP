
using System;

public class Defines
{
    public enum EJsonType
    {
        None = -1,

        String,

        Max
    }

    public enum ENormalBlockType
    {
        None = -1,

        huchu1,
        huchu2,
        huchu3,
        huchu4,

        Max
    }

    public enum ESpecailBlockType
    {
        None = -1,

        Boom,

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
        UIAlarm,
        UIMenu,

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
}