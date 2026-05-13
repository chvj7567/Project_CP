using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameFontProvider : ChvjUnityInfra.IFontProvider
{
    private static TMP_FontAsset _font;
    private static Material _material;

    public static Task PreloadAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        ChvjUnityInfra.CHMResource.Instance.Load<TMP_FontAsset>("Gaegu-Bold SDF", font =>
        {
            _font = font;
            _material = font != null ? font.material : null;
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    public TMP_FontAsset GetFont() => _font;
    public Material GetFontMaterial() => _material;
}
