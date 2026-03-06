using UnityEngine;

/// <summary>
/// 바닥에 드롭된 무기 아이템.
/// IInteractable을 구현하여 플레이어가 상호작용(E키)으로 무기를 장착합니다.
/// </summary>
public class WeaponItem : MonoBehaviour, IInteractable
{
    private const float RotationSpeed = 30f;

    [SerializeField] private WeaponData weaponData;

    /// <summary>이 아이템의 무기 데이터.</summary>
    public WeaponData Data => weaponData;

    private void Update()
    {
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
    }

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
        if (interactor.TryGetComponent(out PlayerWeaponManager weaponManager))
        {
            weaponManager.AddWeapon(weaponData);
            Destroy(gameObject);
        }
    }

    /// <inheritdoc/>
    public Vector3 GetPosition() => transform.position;
}