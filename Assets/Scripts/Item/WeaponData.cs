using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public enum WeaponSlot { Primary = 0, Secondary = 1, Utility = 2 }
    public enum AttackType { Melee, Ranged, Throwable }
    
    [Header("Weapon Classification")]
    public WeaponSlot slot;
    public AttackType attackType;
    
    [Header("Weapon Info")]
    public string weaponName;
    public int attackPower;
    public float attackRate;
    public int maxAmmo;
    public float knockbackForce = 5f;

    [Header("Upgrade System")]
    public int currentUpgradeLevel = 0;
    public int baseUpgradePrice = 100;
    public int priceIncreasePerLevel = 50;
    public int damageIncreasePerLevel = 5;

    public int GetCurrentDamage()
    {
        return attackPower + (currentUpgradeLevel * damageIncreasePerLevel);
    }

    public int GetCurrentUpgradePrice()
    {
        return baseUpgradePrice + (currentUpgradeLevel * priceIncreasePerLevel);
    }

    public void ResetUpgrade()
    {
        currentUpgradeLevel = 0;
    }

    [Header("Projectile (Ranged Only)")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 30f;

    [Header("Throwable")]
    public GameObject grenadePrefab;
    public float explosionRadius = 10f;

    [Header("Visuals")]
    public GameObject weaponPrefab;
    public GameObject dropPrefab;
    public Sprite weaponIcon;

    [Header("Audio")]
    public AudioClip attackSound;
}
