using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 사운드 이펙트 및 BGM을 관리합니다.
/// AudioClip이 할당되지 않은 경우에도 에러 없이 동작합니다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips (Optional - Inspector에서 할당)")]
    public AudioClip BGMClip;
    public AudioClip SuccessClip;
    public AudioClip FailClip;
    public AudioClip ComboClip;
    public AudioClip CountdownClip;
    public AudioClip GameOverClip;
    public AudioClip ButtonClickClip;

    [Header("Settings")]
    [Range(0f, 1f)] public float BGMVolume = 0.5f;
    [Range(0f, 1f)] public float SFXVolume = 0.8f;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private Dictionary<string, AudioClip> _clipMap;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        // BGM용 AudioSource
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume = BGMVolume;

        // SFX용 AudioSource
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = SFXVolume;

        // 이름으로 클립을 찾을 수 있도록 매핑
        _clipMap = new Dictionary<string, AudioClip>();
        RegisterClip("success", SuccessClip);
        RegisterClip("fail", FailClip);
        RegisterClip("combo", ComboClip);
        RegisterClip("countdown", CountdownClip);
        RegisterClip("gameover", GameOverClip);
        RegisterClip("button", ButtonClickClip);
    }

    private void RegisterClip(string name, AudioClip clip)
    {
        if (clip != null)
            _clipMap[name] = clip;
    }

    /// <summary>
    /// BGM을 재생합니다.
    /// </summary>
    public void PlayBGM()
    {
        if (BGMClip == null) return;
        _bgmSource.clip = BGMClip;
        _bgmSource.volume = BGMVolume;
        _bgmSource.Play();
    }

    /// <summary>
    /// BGM을 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    /// <summary>
    /// 이름으로 SFX를 재생합니다.
    /// </summary>
    public void PlaySFX(string name)
    {
        if (_clipMap.TryGetValue(name, out var clip))
        {
            _sfxSource.PlayOneShot(clip, SFXVolume);
        }
    }

    /// <summary>
    /// 직접 AudioClip으로 SFX를 재생합니다.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            _sfxSource.PlayOneShot(clip, SFXVolume);
    }

    /// <summary>
    /// BGM 볼륨을 설정합니다.
    /// </summary>
    public void SetBGMVolume(float vol)
    {
        BGMVolume = Mathf.Clamp01(vol);
        _bgmSource.volume = BGMVolume;
    }

    /// <summary>
    /// SFX 볼륨을 설정합니다.
    /// </summary>
    public void SetSFXVolume(float vol)
    {
        SFXVolume = Mathf.Clamp01(vol);
        _sfxSource.volume = SFXVolume;
    }
}
