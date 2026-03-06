using UnityEngine;

/// <summary>
/// 전역 사운드 매니저(Singleton). BGM·SFX·UI 사운드를 분리된 AudioSource로 관리합니다.
/// 
/// <para><b>설계 의도</b>: 채널(BGM/SFX/UI)별 AudioSource를 분리하여
/// 볼륨을 독립적으로 제어하고, PlayOneShot을 사용해 동시 재생을 지원합니다.
/// DontDestroyOnLoad로 씬 전환 시에도 BGM이 끊기지 않습니다.</para>
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();
                
                if (_instance == null)
                {
                    GameObject obj = new GameObject("SoundManager");
                    _instance = obj.AddComponent<SoundManager>();
                }
            }
            return _instance;
        }
    }

    [Header("BGM Setting")]
    [SerializeField] private AudioClip globalBgm;

    [Header("Audio Sources")]
    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private AudioSource _uiSource;
    
    [Header("Settings")]
    [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float uiVolume = 1f;

    /// <summary>현재 마스터 볼륨 (0~1).</summary>
    public float MasterVolume => masterVolume;

    /// <summary>현재 BGM 볼륨 (0~1).</summary>
    public float BgmVolume => bgmVolume;

    /// <summary>현재 SFX 볼륨 (0~1).</summary>
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        transform.SetParent(null); // DontDestroyOnLoad은 루트 오브젝트만 가능
        DontDestroyOnLoad(gameObject);
        
        InitializeAudioSources();

        if (globalBgm != null)
            PlayBGM(globalBgm);
    }
    
    /// <summary>
    /// 채널별 AudioSource를 동적으로 생성합니다.
    /// Inspector에서 할당하지 않아 프리팹 의존성을 줄입니다.
    /// </summary>
    private void InitializeAudioSources()
    {
        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
        }
        
        if (_uiSource == null)
        {
            _uiSource = gameObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.spatialBlend = 0f;
        }
        
        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = true;
            _bgmSource.loop = true;
            _bgmSource.spatialBlend = 0f;
        }
    }
    
    /// <summary>월드 공간 효과음을 재생합니다. PlayOneShot으로 동시 재생을 지원합니다.</summary>
    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeScale);
    }
    
    /// <summary>UI 효과음을 재생합니다. 2D 사운드(spatialBlend=0)로 출력됩니다.</summary>
    public void PlayUiSfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || _uiSource == null) return;
        _uiSource.PlayOneShot(clip, uiVolume * masterVolume * volumeScale);
    }

    /// <summary>BGM을 교체하여 재생합니다. 이미 같은 클립이 재생 중이면 무시합니다.</summary>
    private void PlayBGM(AudioClip clip)
    {
        if (clip == null || _bgmSource == null) return;
        
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        
        _bgmSource.clip = clip;
        _bgmSource.volume = bgmVolume * masterVolume;
        _bgmSource.Play();
    }

    #region Settings UI Methods

    /// <summary>마스터 볼륨을 설정하고 모든 채널에 즉시 반영합니다.</summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    /// <summary>BGM 볼륨을 설정합니다.</summary>
    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (_bgmSource != null) _bgmSource.volume = bgmVolume * masterVolume;
    }

    /// <summary>SFX 볼륨을 설정합니다. 다음 PlayOneShot 호출부터 적용됩니다.</summary>
    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateAllVolumes()
    {
        if (_bgmSource != null) _bgmSource.volume = bgmVolume * masterVolume;
        // SFX·UI 볼륨은 PlayOneShot에서 곱연산되므로 별도 갱신 불필요
    }

    #endregion
}
