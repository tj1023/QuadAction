using UnityEngine;
using UnityEngine.UI;

public class UIGameStart : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject stageObject;
    [SerializeField] private UISetting settingUI;

    [Header("Weapon Data Reset")]
    [SerializeField] private WeaponData[] resetTargetWeapons;

    private void Start()
    {
        Time.timeScale = 0f;
        startButton?.onClick.AddListener(OnStartClicked);
        settingButton?.onClick.AddListener(OnSettingClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
        hud?.SetActive(false);
        stageObject?.SetActive(false);
    }

    private void OnStartClicked()
    {
        Time.timeScale = 1f;
        hud?.SetActive(true);
        stageObject?.SetActive(true);
        gameObject.SetActive(false);

        if (resetTargetWeapons != null)
        {
            foreach (var weapon in resetTargetWeapons)
            {
                if (weapon != null)
                    weapon.ResetUpgrade();
            }
        }
    }

    private void OnSettingClicked()
    {
        if (settingUI != null)
        {
            settingUI.OpenSettings();
        }
        else
        {
            Debug.LogWarning("UISetting is not assigned in UIGameStart!");
        }
    }
    
    private static void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
