using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private float showDelay = 2f;

    private void Awake()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.OnPlayerDeath += OnPlayerDeath;
        if (retryButton) retryButton.onClick.AddListener(OnRetry);
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDeath -= OnPlayerDeath;
        if (retryButton) retryButton.onClick.RemoveListener(OnRetry);
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(ShowGameOverDelayed());
    }

    private IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSeconds(showDelay);

        if (gameOverPanel) gameOverPanel.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
