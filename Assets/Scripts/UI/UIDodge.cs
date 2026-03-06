using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 회피 쿨다운 UI. PlayerController의 DodgeCooldownRatio를 읽어
/// fillAmount로 시각화합니다.
/// </summary>
public class UIDodge : MonoBehaviour
{
    [SerializeField] private Image cooldownImage;
    [SerializeField] private PlayerController playerController;

    private void Update()
    {
        if (cooldownImage == null || playerController == null) return;
        
        cooldownImage.fillAmount = playerController.DodgeCooldownRatio;
    }
}
