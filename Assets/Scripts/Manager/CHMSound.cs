using UnityEngine;

public class CHMSound
{
    public float bgmVolume { get { return ChvjUnityInfra.CHMSound.Instance.BgmVolume; } }
    public float effectVolume { get { return ChvjUnityInfra.CHMSound.Instance.EffectVolume; } }
    public float Ratio { get { return ChvjUnityInfra.CHMSound.Instance.Ratio; } }

    public void Init()
    {
        ChvjUnityInfra.CHMSound.Instance.Init<Defines.ESound>(Defines.ESound.Bgm);
    }

    public void SetBGMVolume(float volume)
    {
        ChvjUnityInfra.CHMSound.Instance.SetBGMVolume(volume);
    }

    public void SetEffectVolume(float volume)
    {
        ChvjUnityInfra.CHMSound.Instance.SetEffectVolume(volume);
    }

    public void Play(Defines.ESound type, float pitch = 1.0f)
    {
        ChvjUnityInfra.CHMSound.Instance.Play(type, pitch);
    }
}
