using UnityEngine;

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
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        transform.SetParent(null); // Must be root for DontDestroyOnLoad
        DontDestroyOnLoad(gameObject);
        
        InitializeAudioSources();

        if (globalBgm != null)
        {
            PlayBGM(globalBgm);
        }
    }
    
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
    
    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeScale);
    }
    
    public void PlayUiSfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || _uiSource == null) return;
        _uiSource.PlayOneShot(clip, uiVolume * masterVolume * volumeScale);
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null || _bgmSource == null) return;
        
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        
        _bgmSource.clip = clip;
        _bgmSource.volume = bgmVolume * masterVolume;
        _bgmSource.Play();
    }

    // --- Settings UI Methods ---
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (_bgmSource != null) _bgmSource.volume = bgmVolume * masterVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        // Note: sfxSource volume only affects the base volume for the *next* PlayOneShot
        // UI volume uses the same logic.
    }

    private void UpdateAllVolumes()
    {
        if (_bgmSource != null) _bgmSource.volume = bgmVolume * masterVolume;
        // SFX and UI volumes are multiplicative inside PlayOneShot, 
        // they will naturally use the updated master/sfx/ui fields on the next play call.
    }
}
