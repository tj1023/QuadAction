using UnityEngine;
using System.Collections;

public enum ShopType { Item, WeaponUpgrade }

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
        {
            player.AddInteractable(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerInteraction player))
        {
            player.RemoveInteractable(this);
        }
    }

    public void Interact(GameObject interactor)
    {
        uiShop.Open(interactor, spawnPoint, shopType, defaultDialogue, purchaseDialogue, failDialogue);

        // PickupClosestItem이 Interact 후 Remove하므로 1프레임 뒤에 다시 추가
        if (interactor.TryGetComponent(out PlayerInteraction player))
            StartCoroutine(ReAddInteractable(player));
    }

    private IEnumerator ReAddInteractable(PlayerInteraction player)
    {
        yield return null;
        player.AddInteractable(this);
    }

    public Vector3 GetPosition() => transform.position;
}
