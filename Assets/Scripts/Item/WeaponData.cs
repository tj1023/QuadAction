using UnityEngine;

/// <summary>
/// 무기의 기획 데이터를 담는 ScriptableObject.
/// 공격력·공격 속도·슬롯·프리팹 등 **불변 설정값**만 보관합니다.
/// 
/// <para><b>설계 원칙</b>: ScriptableObject는 에셋으로 공유되므로,
/// 런타임에 변경되는 상태(업그레이드 레벨, 현재 탄약 등)를 ScriptableObject에 저장하면
/// 에디터 재시작 후에도 변경이 유지되는 버그가 발생합니다.
/// 따라서 런타임 상태는 <see cref="Weapon"/> 인스턴스에서 관리합니다.</para>
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public enum WeaponSlot { Primary = 0, Secondary = 1, Utility = 2 }
    public enum AttackType { Melee, Ranged, Throwable }
    
    [Header("Weapon Classification")]
    [SerializeField] private WeaponSlot weaponSlot;
    [SerializeField] private AttackType weaponAttackType;
    
    [Header("Weapon Info")]
    [SerializeField] private string weaponDisplayName;
    [SerializeField] private int attackPower;
    [SerializeField] private float attackRate;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Upgrade System")]
    [SerializeField] private int baseUpgradePrice = 100;
    [SerializeField] private int priceIncreasePerLevel = 50;
    [SerializeField] private int damageIncreasePerLevel = 5;

    [Header("Projectile (Ranged Only)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 30f;

    [Header("Throwable")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float explosionRadius = 10f;

    [Header("Visuals")]
    [SerializeField] private GameObject weaponModelPrefab;
    [SerializeField] private GameObject dropItemPrefab;
    [SerializeField] private Sprite weaponIcon;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;

    #region Read-Only Properties

    /// <summary>무기가 장착되는 슬롯 (Primary/Secondary/Utility).</summary>
    public WeaponSlot Slot => weaponSlot;

    /// <summary>공격 유형 (Melee/Ranged/Throwable).</summary>
    public AttackType WeaponAttackType => weaponAttackType;

    /// <summary>무기 표시 이름.</summary>
    public string WeaponName => weaponDisplayName;

    /// <summary>기본 공격력 (업그레이드 미적용).</summary>
    public int AttackPower => attackPower;

    /// <summary>공격 속도(초). 낮을수록 빠릅니다.</summary>
    public float AttackRate => attackRate;

    /// <summary>최대 탄약 수.</summary>
    public int MaxAmmo => maxAmmo;

    /// <summary>넉백 힘.</summary>
    public float KnockbackForce => knockbackForce;

    /// <summary>업그레이드 기본 가격.</summary>
    public int BaseUpgradePrice => baseUpgradePrice;

    /// <summary>레벨당 가격 증가분.</summary>
    public int PriceIncreasePerLevel => priceIncreasePerLevel;

    /// <summary>레벨당 데미지 증가분.</summary>
    public int DamageIncreasePerLevel => damageIncreasePerLevel;

    /// <summary>총알 프리팹. (Ranged 타입 전용)</summary>
    public GameObject BulletPrefab => bulletPrefab;

    /// <summary>총알 속도.</summary>
    public float BulletSpeed => bulletSpeed;

    /// <summary>수류탄 프리팹. (Throwable 타입 전용)</summary>
    public GameObject GrenadePrefab => grenadePrefab;

    /// <summary>폭발 반경.</summary>
    public float ExplosionRadius => explosionRadius;

    /// <summary>손에 들 무기 프리팹.</summary>
    public GameObject WeaponPrefab => weaponModelPrefab;

    /// <summary>바닥에 드롭될 아이템 프리팹.</summary>
    public GameObject DropPrefab => dropItemPrefab;

    /// <summary>UI에 표시할 무기 아이콘.</summary>
    public Sprite WeaponIcon => weaponIcon;

    /// <summary>공격 효과음.</summary>
    public AudioClip AttackSound => attackSound;

    #endregion
}
