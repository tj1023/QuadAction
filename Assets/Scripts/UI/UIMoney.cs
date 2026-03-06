using UnityEngine;
using TMPro;

/// <summary>
/// 소지금 UI. EventManager.OnMoneyChanged를 구독하여 자동 갱신됩니다.
/// </summary>
public class UIMoney : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void OnEnable()
    {
        EventManager.OnMoneyChanged += UpdateMoney;
    }

    private void OnDisable()
    {
        EventManager.OnMoneyChanged -= UpdateMoney;
    }

    private void UpdateMoney(int money)
    {
        if (moneyText != null)
            moneyText.text = $"${money}";
    }
}
