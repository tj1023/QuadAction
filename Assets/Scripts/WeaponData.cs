using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public int attackPower;
    public float attackRate;

    [Header("Visuals")]
    public GameObject weaponPrefab;
}
