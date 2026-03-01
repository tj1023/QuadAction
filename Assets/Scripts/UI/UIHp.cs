using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHp : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI hpText;

    private void OnEnable()
    {
        EventManager.OnHpChanged += UpdateHp;
    }

    private void OnDisable()
    {
        EventManager.OnHpChanged -= UpdateHp;
    }

    private void UpdateHp(int currentHp, int maxHp)
    {
        if (fillImage)
            fillImage.fillAmount = (float)currentHp / maxHp;

        if (hpText)
            hpText.text = $"{currentHp}/{maxHp}";
    }
}
