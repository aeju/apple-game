using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 사운드 이펙트 및 BGM을 관리합니다.
/// AudioClip이 할당되지 않은 경우에도 에러 없이 동작합니다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips - Cat Theme (Inspector에서 할당)")]
    public AudioClip BGMClip;            // BGM - 재즈 고양이 카페 분위기
    public AudioClip SuccessClip;        // 성공 - 냥!
    public AudioClip FailClip;           // 실패 - 흐응
    public AudioClip ComboClip;          // 콤보 - 골골송 (purr)
    public AudioClip CountdownClip;      // 카운트다운
    public AudioClip GameOverClip;       // 게임 오버 - 하품
    public AudioClip ButtonClickClip;    // 버튼 클릭

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

        // 이름으로 클립을 찾을 수 있도록 매핑 (고양이 테마 별칭 포함)
        _clipMap = new Dictionary<string, AudioClip>();
        RegisterClip("success", SuccessClip);
        RegisterClip("meow", SuccessClip);       // 냥! (성공 별칭)
        RegisterClip("fail", FailClip);
        RegisterClip("hiss", FailClip);           // 흐응 (실패 별칭)
        RegisterClip("combo", ComboClip);
        RegisterClip("purr", ComboClip);          // 골골송 (콤보 별칭)
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
            // 특정 효과음의 볼륨 조정 (필요시 여기서 수정)
            float volumeMultiplier = 1.0f;
            
            // countdown 효과음을 1.5배 크게
            if (name == "countdown")
                volumeMultiplier = 1.5f;
            
            // fail 효과음을 0.25배 작게 (조용하게)
            else if (name == "fail" || name == "hiss")
                volumeMultiplier = 0.25f;
            
            // 예시: meow 효과음을 2배 크게
            // if (name == "meow" || name == "success")
            //     volumeMultiplier = 2.0f;
            
            _sfxSource.PlayOneShot(clip, SFXVolume * volumeMultiplier);
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

    /// <summary>
    /// 모든 효과음을 정지합니다.
    /// </summary>
    public void StopAllSFX()
    {
        if (_sfxSource != null)
        {
            _sfxSource.Stop();
        }
    }
}
