using UnityEngine;
using System.Collections;

/// <summary>
/// 상점 비콘. 플레이어가 상호작용하면 UIShop을 엽니다.
/// IInteractable을 구현하여 PlayerInteraction 시스템과 연동됩니다.
/// 
/// <para><b>상호작용 유지</b>: Interact 후 PlayerInteraction이 목록에서 제거하므로,
/// 1프레임 뒤에 다시 추가하여 상점을 닫고 다시 열 수 있도록 합니다.</para>
/// </summary>
public class Shop : MonoBehaviour, IInteractable
{
    [SerializeField] private UIShop uiShop;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ShopType shopType;

    [Header("Dialogue")]
    [SerializeField] private string defaultDialogue;
    [SerializeField] private string purchaseDialogue;
    [SerializeField] private string failDialogue;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerInteraction player))
            player.AddInteractable(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerInteraction player))
            player.RemoveInteractable(this);
    }

    /// <inheritdoc/>
    public void Interact(GameObject interactor)
    {
        uiShop.Open(interactor, spawnPoint, shopType, defaultDialogue, purchaseDialogue, failDialogue);

        if (interactor.TryGetComponent(out PlayerInteraction player))
            StartCoroutine(ReAddInteractable(player));
    }

    /// <summary>1프레임 후 상호작용 목록에 재추가하여 반복 상호작용을 가능하게 합니다.</summary>
    private IEnumerator ReAddInteractable(PlayerInteraction player)
    {
        yield return null;
        player.AddInteractable(this);
    }

    /// <inheritdoc/>
    public Vector3 GetPosition() => transform.position;
}
