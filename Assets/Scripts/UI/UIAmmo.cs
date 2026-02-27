using UnityEngine;
using TMPro;

public class UIAmmo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    
    private void OnEnable()
    {
        EventManager.OnAmmoChanged += UpdateAmmoText;
    }

    private void OnDisable()
    {
        EventManager.OnAmmoChanged -= UpdateAmmoText;
    }

    private void UpdateAmmoText(int currentAmmo, int reserveAmmo)
    {
        ammoText.text = currentAmmo < 0 ? "" : $"{currentAmmo}/{reserveAmmo}";
    }
}
