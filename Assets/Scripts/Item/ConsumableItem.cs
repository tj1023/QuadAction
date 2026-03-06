using UnityEngine;

/// <summary>
/// 자동 획득되는 소모품 아이템 (탄약, 코인, 회복).
/// ICollectible을 구현하여 플레이어 트리거 충돌 시 자동으로 수집됩니다.
/// 
/// <para><b>확장성</b>: ConsumableType 열거형에 새 타입을 추가하고
/// Collect의 switch문에 로직을 추가하면 새로운 소모품을 쉽게 확장할 수 있습니다.</para>
/// </summary>
public class ConsumableItem : MonoBehaviour, ICollectible
{
    private enum ConsumableType
    {
        Ammo,
        Coin,
        Heart
    }

    private const float RotationSpeed = 30f;

    [Header("Item Settings")]
    [SerializeField] private ConsumableType type;
    [SerializeField] private int value;

    private void Update()
    {
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Collect(other.gameObject);
    }

    /// <inheritdoc/>
    public void Collect(GameObject collector)
    {
        switch (type)
        {
            case ConsumableType.Ammo:
                if (collector.TryGetComponent(out PlayerWeaponManager weaponManager))
                    weaponManager.AddAmmo(value);
                break;
            case ConsumableType.Coin:
                if (collector.TryGetComponent(out PlayerStats statsForMoney))
                    statsForMoney.AddMoney(value);
                break;
            case ConsumableType.Heart:
                if (collector.TryGetComponent(out PlayerStats statsForHp))
                    statsForHp.Heal(value);
                break;
        }
        
        Destroy(gameObject);
    }
}