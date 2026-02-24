using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        Ammo,
        Coin,
        Grenade,
        Heart,
        Weapon
    };

    [Header("Item Settings")]
    [SerializeField] private ItemType type;
    [SerializeField] private int value;
    [SerializeField] private WeaponData weaponData;

    public ItemType Type => type;
    public int Value => value;
    public WeaponData WeaponData => weaponData;
    private bool IsAutoPickup => type != ItemType.Weapon;
    
    private void Update()
    {
        transform.Rotate(Vector3.up * (30 * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerInteraction player = other.GetComponent<PlayerInteraction>();
        if (player == null) return;

        if (IsAutoPickup)
        {
            player.AcquireItem(this);
        }
        else
        {
            player.AddNearbyItem(this);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerInteraction player = other.GetComponent<PlayerInteraction>();
        if (player == null) return;
        
        if (!IsAutoPickup)
        {
            player.RemoveNearbyItem(this);
        }
    }

    public void Collect()
    {
        Destroy(gameObject);
    }
}
