using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 시작 화면 UI. 시작·설정·종료 버튼
/// </summary>
public class UIGameStart : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("References")]
    [SerializeField] private UISetting settingUI;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject stageObject;

    private void Start()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (startButton != null) startButton.onClick.AddListener(OnStart);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSetting);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        Time.timeScale = 0f;
        hud?.SetActive(false);
        stageObject?.SetActive(false);
    }

    private void OnStart()
    {
        if (startPanel != null) startPanel.SetActive(false);

        Time.timeScale = 1f;
        hud?.SetActive(true);
        stageObject?.SetActive(true);
    }

    private void OnSetting()
    {
        if (settingUI != null) settingUI.OpenSettings();
        else Debug.LogWarning("UISetting is not assigned in UIGameStart!");
    }
    
    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
