using UnityEngine;
using TMPro;

/// <summary>
/// 탄약 수 UI. 주무기 원거리 무기의 현재 탄창 / 예비 탄약을 표시합니다.
/// 비 원거리 무기에서는 (-1, -1)이 전달되어 비활성화됩니다.
/// </summary>
public class UIAmmo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject ammoPanel;

    private void OnEnable()
    {
        EventManager.OnAmmoChanged += UpdateAmmo;
    }

    private void OnDisable()
    {
        EventManager.OnAmmoChanged -= UpdateAmmo;
    }

    private void UpdateAmmo(int current, int reserve)
    {
        if (current < 0 || reserve < 0)
        {
            if (ammoPanel != null) ammoPanel.SetActive(false);
            return;
        }

        if (ammoPanel != null) ammoPanel.SetActive(true);
        if (ammoText != null) ammoText.text = $"{current} / {reserve}";
    }
}
