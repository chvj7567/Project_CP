using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CHMSound
{
    AudioSource[] audioSourceList = new AudioSource[(int)Defines.ESound.Max];
    Dictionary<string, AudioClip> audioClipDict = new Dictionary<string, AudioClip>();
    float bgmVolume = 0.2f;
    float effectVolume = 0.2f;

    public float Ratio { get; private set; } = 0.5f;

    public void Init()
    {
        GameObject root = GameObject.Find("@CHMSound");
        if (root == null)
        {
            root = new GameObject { name = "@CHMSound" };

            string[] soundNames = Enum.GetNames(typeof(Defines.ESound));
            for (int i = 0; i < (int)Defines.ESound.Max; i++)
            {
                if (soundNames[i] == "None")
                    continue;

                GameObject go = new GameObject { name = soundNames[i] };
                audioSourceList[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            audioSourceList[(int)Defines.ESound.Bgm].loop = true;

            UnityEngine.Object.DontDestroyOnLoad(root);
        }
    }

    async Task<AudioClip> LoadSound(Defines.ESound _eSound)
    {
        TaskCompletionSource<AudioClip> taskCompletionSource = new TaskCompletionSource<AudioClip>();

        CHMMain.Resource.LoadSound(_eSound, (sound) =>
        {
            taskCompletionSource.SetResult(sound);
        });

        var ret = await taskCompletionSource.Task;

        return ret;
    }

    public void SetBGMVolume(float ratio)
    {
        Ratio = ratio;
        audioSourceList[(int)Defines.ESound.Bgm].volume = bgmVolume * Ratio;
    }

    public async void Play(Defines.ESound type, float pitch = 1.0f, float volume = 1.0f)
    {
        AudioClip audioClip = await GetOrAddAudioClip(type);

        AudioSource audioSource = audioSourceList[(int)type];

        audioSource.pitch = pitch;

        if (type == Defines.ESound.Bgm)
        {
            audioSource.volume = bgmVolume * Ratio;

            if (audioSource.isPlaying)
                return;

            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            audioSource.volume = effectVolume * Ratio * volume;
            audioSource.PlayOneShot(audioClip);
        }
    }

    async Task<AudioClip> GetOrAddAudioClip(Defines.ESound type)
    {
        AudioClip audioClip = null;

        if (audioClipDict.TryGetValue(type.ToString(), out audioClip) == false)
        {
            audioClip = await LoadSound(type);
            audioClipDict.Add(type.ToString(), audioClip);
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing! {type.ToString()}");

        return audioClip;
    }
}
