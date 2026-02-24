using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponSlot;
    
    private readonly List<WeaponData> _ownedWeapons = new List<WeaponData>();
    private readonly Dictionary<WeaponData, GameObject> _weaponInstances = new Dictionary<WeaponData, GameObject>();
    private GameObject _currentWeaponObject;
    private WeaponData _currentWeaponData;
    private int _currentWeaponIndex = -1;

    public void AddWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null) return;
        
        if (!_ownedWeapons.Contains(newWeapon))
        {
            _ownedWeapons.Add(newWeapon);
            Debug.Log($"무기 획득: {newWeapon.weaponName}");
            
            EquipWeapon(newWeapon); 
        }
        else
        {
            Debug.Log("이미 보유한 무기입니다!");
        }
    }
    
    private void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null || weaponData.weaponPrefab == null) return;

        // 1. 기존에 들고 있던 무기가 있다면 숨김
        if (_currentWeaponObject && _weaponInstances.TryGetValue(_currentWeaponData, out var currentWeapon))
        {
            currentWeapon.SetActive(false);
        }

        // 2. 장착하려는 무기가 생성된 적 있는지 확인
        if (_weaponInstances.ContainsKey(weaponData))
        {
            // 생성된 적 있다면 active
            _weaponInstances[weaponData].SetActive(true);
            _currentWeaponObject = _weaponInstances[weaponData];
        }
        else
        {
            // 처음 줍는 무기라면 새로 생성하고 등록
            _currentWeaponObject = Instantiate(weaponData.weaponPrefab, weaponSlot);
            _currentWeaponObject.transform.localPosition = Vector3.zero;
            _currentWeaponObject.transform.localRotation = Quaternion.identity;
            
            _weaponInstances.Add(weaponData, _currentWeaponObject);
        }

        // 3. 현재 무기 데이터 갱신
        _currentWeaponData = weaponData;
        _currentWeaponIndex = _ownedWeapons.IndexOf(weaponData);
        Debug.Log($"무기 장착 완료: {_currentWeaponData.weaponName}");
    }

    public void SwapWeapon(int direction)
    {
        if(_ownedWeapons.Count <= 1) return;
        
        int newIndex = _currentWeaponIndex + direction;

        if (newIndex >= _ownedWeapons.Count)
            newIndex = 0;
        else if (newIndex < 0)
            newIndex = _ownedWeapons.Count - 1;
        
        EquipWeapon(_ownedWeapons[newIndex]);
    }
}
