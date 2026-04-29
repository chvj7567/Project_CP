using UnityEngine;

[CreateAssetMenu(fileName = "WarmTheme", menuName = "CatPang/WarmTheme")]
public class WarmTheme : ScriptableObject
{
    [Header("Backgrounds")]
    public Color bg        = new Color(0xFE / 255f, 0xF4 / 255f, 0xEC / 255f); // #fef4ec
    public Color surface   = new Color(1f, 1f, 1f);                             // #ffffff
    public Color surface2  = new Color(0xFD / 255f, 0xE7 / 255f, 0xD3 / 255f); // #fde7d3

    [Header("Ink")]
    public Color ink       = new Color(0x3D / 255f, 0x2A / 255f, 0x1E / 255f); // #3d2a1e
    public Color inkSoft   = new Color(0x8B / 255f, 0x6F / 255f, 0x5C / 255f); // #8b6f5c
    public Color inkDim    = new Color(0xCD / 255f, 0xB8 / 255f, 0xA7 / 255f); // #cdb8a7

    [Header("Accent")]
    public Color accent    = new Color(0xFF / 255f, 0x8F / 255f, 0x65 / 255f); // #ff8f65
    public Color accent2   = new Color(0xFF / 255f, 0xC8 / 255f, 0x9A / 255f); // #ffc89a

    [Header("Card Variants")]
    public Color mint      = new Color(0xB8 / 255f, 0xD9 / 255f, 0xCB / 255f); // #b8d9cb
    public Color pink      = new Color(0xF7 / 255f, 0xC4 / 255f, 0xC4 / 255f); // #f7c4c4
    public Color yellow    = new Color(0xFF / 255f, 0xE1 / 255f, 0x9B / 255f); // #ffe19b

    [Header("Shadow")]
    public Color shadow    = new Color(0x7B / 255f, 0x4D / 255f, 0x2E / 255f, 0.12f); // rgba(123,77,46,0.12)

    [Header("Danger")]
    public Color danger    = new Color(0xE7 / 255f, 0x4C / 255f, 0x3C / 255f); // #e74c3c

    public static WarmTheme Instance { get; private set; }

    void OnEnable()
    {
        Instance = this;
    }
}
