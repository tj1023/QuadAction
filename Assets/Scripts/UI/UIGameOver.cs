using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 게임 오버 UI. 플레이어 사망 시 표시, 재시작 버튼
/// </summary>
public class UIGameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
    }

    private void OnEnable()
    {
        EventManager.OnPlayerDeath += ShowGameOverPanel;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDeath -= ShowGameOverPanel;
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
