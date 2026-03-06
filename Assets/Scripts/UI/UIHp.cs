using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어 HP 바 UI. EventManager.OnHpChanged를 구독하여 자동 갱신됩니다.
/// </summary>
public class UIHp : MonoBehaviour
{
    [SerializeField] private Image hpBar;
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
        if (maxHp <= 0) return;
        
        float ratio = (float)currentHp / maxHp;
        
        if (hpBar != null) hpBar.fillAmount = ratio;
        if (hpText != null) hpText.text = $"{currentHp} / {maxHp}";
    }
}
