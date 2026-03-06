using UnityEngine;
using TMPro;

/// <summary>
/// 탄약 수 UI. 주무기 원거리 무기의 현재 탄창 / 예비 탄약을 표시합니다.
/// 비 원거리 무기에서는 (-1, -1)이 전달되어 비활성화됩니다.
/// </summary>
public class UIAmmo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;

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
        if (ammoText != null) ammoText.text = current < 0 ? "" : $"{current}/{reserve}";
    }
}
