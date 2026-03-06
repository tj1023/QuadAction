using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 설정 UI. ESC 키로 열리며, 볼륨 조절 + 타이틀 복귀 기능을 제공합니다.
/// IPopupUI를 구현하여 UIManager 스택에 등록되고 ESC 순서 닫기를 지원합니다.
/// 
/// <para><b>TimeScale 관리</b>: 열릴 때 TimeScale을 0으로 설정하여 게임을 일시정지하고,
/// 닫힐 때 이전 TimeScale로 복원합니다.</para>
/// </summary>
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
    [SerializeField] private PlayerController player;

    private bool _isOpen;
    private float _previousTimeScale = 1f;

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            if (masterSlider != null) masterSlider.value = SoundManager.Instance.MasterVolume;
            if (bgmSlider != null) bgmSlider.value = SoundManager.Instance.BgmVolume;
            if (sfxSlider != null) sfxSlider.value = SoundManager.Instance.SfxVolume;

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (titleButton != null) titleButton.onClick.AddListener(GoToTitle);

        if (settingPanel != null) settingPanel.SetActive(false);
        
        if (UIManager.Instance != null)
            UIManager.Instance.OnCancelNoUI += OpenSettings;
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnCancelNoUI -= OpenSettings;
    }

    /// <summary>설정 UI를 엽니다. 게임 일시정지 + 플레이어 입력 비활성화.</summary>
    public void OpenSettings()
    {
        if (_isOpen) return;
        
        _isOpen = true;
        if (settingPanel != null) settingPanel.SetActive(true);

        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (player != null)
            player.SetInputEnabled(false);
            
        if (UIManager.Instance != null)
            UIManager.Instance.PushUI(this);
    }

    /// <inheritdoc/>
    public void Close()
    {
        if (!_isOpen) return;
        
        _isOpen = false;
        if (settingPanel != null) settingPanel.SetActive(false);

        Time.timeScale = _previousTimeScale;

        if (player != null)
            player.SetInputEnabled(true);
            
        if (UIManager.Instance != null)
            UIManager.Instance.PopUI(this);
    }

    private void SetMasterVolume(float volume)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetMasterVolume(volume);
    }

    private void SetBgmVolume(float volume)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetBgmVolume(volume);
    }

    private void SetSfxVolume(float volume)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetSfxVolume(volume);
    }

    private void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
