using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UISetting : MonoBehaviour, IPopupUI
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingPanel;
    
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button titleButton;

    [Header("Player Reference")]
    [SerializeField] private PlayerController player; // To disable/enable input

    private bool _isOpen = false;
    private float _previousTimeScale = 1f;

    private void Start()
    {
        // Initialize sliders with current SoundManager values
        if (SoundManager.Instance != null)
        {
            if (masterSlider) masterSlider.value = SoundManager.Instance.masterVolume;
            if (bgmSlider) bgmSlider.value = SoundManager.Instance.bgmVolume;
            if (sfxSlider) sfxSlider.value = SoundManager.Instance.sfxVolume;

            // Add listeners
            if (masterSlider) masterSlider.onValueChanged.AddListener(SetMasterVolume);
            if (bgmSlider) bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        if (closeButton) closeButton.onClick.AddListener(Close);
        if (titleButton) titleButton.onClick.AddListener(GoToTitle);

        if (settingPanel) settingPanel.SetActive(false);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnCancelNoUI += OpenSettings;
        }
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnCancelNoUI -= OpenSettings;
        }
    }

    public void OpenSettings()
    {
        if (_isOpen) return;
        
        _isOpen = true;
        if (settingPanel) settingPanel.SetActive(true);

        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (player != null)
            player.SetInputEnabled(false);
            
        if (UIManager.Instance != null)
            UIManager.Instance.PushUI(this);
    }

    public void Close()
    {
        if (!_isOpen) return;
        
        _isOpen = false;
        if (settingPanel) settingPanel.SetActive(false);

        Time.timeScale = _previousTimeScale;

        if (player != null)
            player.SetInputEnabled(true);
            
        if (UIManager.Instance != null)
            UIManager.Instance.PopUI(this);
    }

    private void SetMasterVolume(float volume)
    {
        if (SoundManager.Instance) SoundManager.Instance.SetMasterVolume(volume);
    }

    private void SetBgmVolume(float volume)
    {
        if (SoundManager.Instance) SoundManager.Instance.SetBgmVolume(volume);
    }

    private void SetSfxVolume(float volume)
    {
        if (SoundManager.Instance) SoundManager.Instance.SetSfxVolume(volume);
    }

    private void GoToTitle()
    {
        Time.timeScale = 1f; // Ensure time scale is reset before loading scene
        
        // Reload the current scene to effectively return to the Title/Start state
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
