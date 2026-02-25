using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    // 슬롯 종류: 0(주무기), 1(보조무기), 2(유틸리티)
    public enum WeaponSlot { Primary = 0, Secondary = 1, Utility = 2 }
    
    // 공격 종류: 근접, 원거리, 투척
    public enum AttackType { Melee, Ranged, Throwable }
    
    [Header("Weapon Classification")]
    public WeaponSlot slot;
    public AttackType attackType;
    
    [Header("Weapon Info")]
    public string weaponName;
    public int attackPower;
    public float attackRate;

    [Header("Visuals")]
    public GameObject weaponPrefab;
    public Sprite weaponIcon;
}
