using UnityEngine;

public class WeaponItem : MonoBehaviour, IInteractable
{
    [SerializeField] private WeaponData weaponData;
    public WeaponData Data => weaponData;

    private void Update()
    {
        transform.Rotate(Vector3.up * (30 * Time.deltaTime));
    }

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
        if (interactor.TryGetComponent(out PlayerWeaponManager weaponManager))
        {
            weaponManager.AddWeapon(weaponData);

            Destroy(gameObject);
        }
    }

    public Vector3 GetPosition() => transform.position;
}