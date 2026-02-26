using UnityEngine;

public class ConsumableItem : MonoBehaviour, ICollectible
{
    private enum ConsumableType
    {
        Ammo,
        Coin,
        Heart
    }

    [Header("Item Settings")]
    [SerializeField] private ConsumableType type;
    [SerializeField] private int value;

    private void Update()
    {
        transform.Rotate(Vector3.up * (30 * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }

    public void Collect(GameObject collector)
    {
        switch (type)
        {
            case ConsumableType.Ammo:
                if (collector.TryGetComponent(out PlayerWeaponManager weaponManager))
                {
                    weaponManager.AddAmmo(value);
                    Debug.Log($"탄약 획득: {value}");
                }
                break;
            case ConsumableType.Coin:
                if (collector.TryGetComponent(out PlayerStats statsForMoney))
                {
                    statsForMoney.AddMoney(value);
                    Debug.Log($"골드 획득: {value}");
                }
                break;
            case ConsumableType.Heart:
                if (collector.TryGetComponent(out PlayerStats statsForHp))
                {
                    statsForHp.Heal(value);
                    Debug.Log($"HP 회복: {value}");
                }
                break;
        }
        
        Destroy(gameObject);
    }
}