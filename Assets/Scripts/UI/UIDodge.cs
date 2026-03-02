using UnityEngine;
using UnityEngine.UI;

public class UIDodge : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;
    [SerializeField] private PlayerController playerController;

    private void Update()
    {
        if (!playerController || !cooldownFill) return;

        // 쿨타임 비율: 1 = 쿨타임 중, 0 = 사용 가능
        cooldownFill.fillAmount = playerController.DodgeCooldownRatio;
    }
}
