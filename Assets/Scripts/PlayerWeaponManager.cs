using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponSlot;

    private int _maxSlots;
    private WeaponData[] _equippedWeapons;
    private readonly Dictionary<WeaponData, GameObject> _weaponInstances = new Dictionary<WeaponData, GameObject>();
    private GameObject _currentWeaponObject;
    private WeaponData _currentWeaponData;
    private int _currentWeaponIndex = -1;
    private PlayerAnimator _animator;

    private void Awake()
    {
        _animator = GetComponent<PlayerAnimator>();
        _maxSlots = Enum.GetValues(typeof(WeaponData.WeaponSlot)).Length;
        _equippedWeapons = new WeaponData[_maxSlots];
    }

    public void AddWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null) return;

        // 무기 데이터에 설정된 슬롯 번호(0, 1, 2)를 가져옴
        int slotIndex = (int)newWeapon.slot;

        // 해당 슬롯에 이미 다른 무기가 있다면 교체 처리
        if (_equippedWeapons[slotIndex] != null && _equippedWeapons[slotIndex] != newWeapon)
        {
            Debug.Log($"기존 {_equippedWeapons[slotIndex].weaponName}을(를) 버림");
            // TODO: 무기를 땅에 떨어뜨리는 로직 추가
        }

        // 새 무기를 해당 슬롯에 할당
        _equippedWeapons[slotIndex] = newWeapon;
        EventManager.OnWeaponAdded?.Invoke(slotIndex, newWeapon);
        Debug.Log($"무기 획득: {newWeapon.weaponName} ({newWeapon.slot} 슬롯)");
        
        EquipWeaponByIndex(slotIndex);
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
        _currentWeaponIndex = (int)weaponData.slot;
        Debug.Log($"무기 장착 완료: {_currentWeaponData.weaponName} (공격타입: {_currentWeaponData.attackType})");
    }

    // 숫자 키로 직접 슬롯 선택
    public void EquipWeaponByIndex(int index)
    {
        if (index < 0 || index >= _equippedWeapons.Length) return;

        WeaponData targetWeapon = _equippedWeapons[index];
        
        // 해당 슬롯이 비어있거나, 이미 들고 있는 무기라면 무시
        if (targetWeapon == null || targetWeapon == _currentWeaponData) return;

        _animator?.ForceRestartSwap();
        EquipWeapon(targetWeapon);
        
        EventManager.OnWeaponEquipped?.Invoke(index);
    }
    
    public void SwapWeapon(int direction)
    {
        if (_currentWeaponIndex == -1) return;

        int nextIndex = _currentWeaponIndex;
        
        // 슬롯 개수까지만 빈 슬롯을 건너뜀
        for (int i = 0; i < _maxSlots; i++)
        {
            nextIndex += direction;
            
            // 인덱스 순환
            if (nextIndex >= _maxSlots) nextIndex = 0;
            else if (nextIndex < 0) nextIndex = _maxSlots - 1;

            // 빈 슬롯이 아니라면 무기 교체 시도
            if (_equippedWeapons[nextIndex] != null)
            {
                // 찾은 무기가 지금 들고 있는 것과 다를 때만 장착 (무기가 1개일 때 무한 스왑 방지)
                if (nextIndex != _currentWeaponIndex)
                {
                    EquipWeaponByIndex(nextIndex);
                }
                break;
            }
        }
    }
}
