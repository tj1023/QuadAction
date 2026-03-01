using UnityEngine;
using TMPro;

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
        if (moneyText)
            moneyText.text = $"${money}";
    }
}
