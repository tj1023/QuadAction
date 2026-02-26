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

    [Header("Visuals")]
    public GameObject weaponPrefab;
    public GameObject dropPrefab;
    public Sprite weaponIcon;
}
