using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(AudioSource))]
public class AudioManager : SingletonBehaviour<AudioManager>
{
    public static Action<bool> OnMusicStateChanged { get; set; }
    public static Action<bool> OnSoundStateChanged { get; set; }
    public static Action<bool> OnVibrateStateChanged { get; set; }
    [SerializeField] private AudioClip bgClip;
    [SerializeField] private List<AudioClip> audioClips = new();

    [Header("[AUDIO SOURCE]")]
    [SerializeField] private AudioSource audioSound;
    [SerializeField] private AudioSource audioMusic;

    public static bool Music
    {
        get
        {
            return PlayerPrefs.GetInt("music", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("music", value ? 1 : 0);
        }
    }
    public static bool Sound
    {
        get
        {
            return PlayerPrefs.GetInt("sound", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("sound", value ? 1 : 0);
        }
    }
    public static bool Vibration
    {
        get
        {
            return PlayerPrefs.GetInt("vibration", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("vibration", value ? 1 : 0);
        }
    }

    //private int _loop;

    #region MonoBehaviour

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
        PlayMusic();
    }

    public void ChangeSoundState()
    {
        Sound = !Sound;
        OnSoundStateChanged?.Invoke(Sound);
    }
    public void ChangeMusicState()
    {
        Music = !Music;
        if (Music) PlayMusic();
        else StopMusic();
        OnMusicStateChanged?.Invoke(Music);
    }
    public void ChangeVibrateState()
    {
        Vibration = !Vibration;
        if (Vibration) Vibrator.Vibrate(100);
        OnVibrateStateChanged?.Invoke(Vibration);
    }

    public static void SetButtonState(Button button, Sprite stateSprite)
    {
        button.GetComponent<Image>().sprite = stateSprite;
    }
    #endregion

    #region Public
    public void PlaySound(string clipName, float soundVolume = 1f)
    {
        if (!Sound) return;
        if (audioSound.clip != null &&
            clipName.Equals(audioSound.clip.name))
        {
            return;
        }
        AudioClip clip = GetClip(clipName);

        if (clip == null)
        {
            return;
        }
        audioSound.volume = soundVolume;
        audioSound.PlayOneShot(clip);
    }

    public void PlayMusic()
    {
        if (!Music && audioMusic.clip != null) return;
        audioMusic.Play();
    }

    public void StopMusic()
    {
        audioMusic.Stop();
    }

    public void StopSound()
    {
        audioSound.Stop();
    }

    public void PlayVibrate(long milis)
    {
        if (Vibration)
            Vibrator.Vibrate(milis);
    }
    #endregion
    private AudioClip GetClip(string soundName)
    {
        return audioClips.Find(x => string.Equals(x.name, soundName, StringComparison.CurrentCultureIgnoreCase));
    }
    public List<AudioClip> GetListAudioClip()
    {
        return audioClips;
    }
#if UNITY_EDITOR
    public void SetMusicClip()
    {
        if (bgClip != null)
        {
            audioMusic.clip = bgClip;
        }
    }
#endif
}