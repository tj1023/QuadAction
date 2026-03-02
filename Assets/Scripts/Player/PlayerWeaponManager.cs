using System;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private Transform firePoint;

    private PlayerAnimator _animator;
    private Weapon[] _equippedWeapons;
    private Weapon _currentWeapon;
    private int _ammo;
    private int _maxSlots;
    private Camera _camera;

    private void Awake()
    {
        _animator = GetComponent<PlayerAnimator>();
        _maxSlots = Enum.GetValues(typeof(WeaponData.WeaponSlot)).Length;
        _equippedWeapons = new Weapon[_maxSlots];
        _camera =  Camera.main;
    }

    public void AddWeapon(WeaponData newWeaponData)
    {
        if (newWeaponData == null) return;
        
        int slotIndex = (int)newWeaponData.slot;

        // 해당 슬롯에 이미 무기가 있다면 기존 무기 드랍
        if (_equippedWeapons[slotIndex])
            DropWeapon(_equippedWeapons[slotIndex]);

        // 새 무기 프리팹을 손(weaponSlot) 위치에 생성
        GameObject weaponObj = Instantiate(newWeaponData.weaponPrefab, weaponSlot);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        // Weapon 컴포넌트를 가져와서 초기화
        if (!weaponObj.TryGetComponent(out Weapon newWeapon))
        {
            Debug.LogError($"{newWeaponData.weaponName}의 프리팹({weaponObj.name})에 Weapon 컴포넌트가 없습니다.");
            Destroy(weaponObj);
            return;
        }
        newWeapon.Initialize(newWeaponData); 

        // 배열에 등록하고 해당 슬롯 장착
        _equippedWeapons[slotIndex] = newWeapon;
        EventManager.OnWeaponAdded?.Invoke(slotIndex, newWeaponData);
        EquipWeaponByIndex(slotIndex, true);
        
        if (IsPrimaryRangedWeapon(newWeapon))
            Invoke(nameof(TryReload), 0.3f); 
    }
    
    public void EquipWeaponByIndex(int index, bool forceEquip = false)
    {
        if (index < 0 || index >= _maxSlots) return;

        Weapon targetWeapon = _equippedWeapons[index];
        if (targetWeapon == null) return; // 빈 슬롯
        if (targetWeapon == _currentWeapon && !forceEquip) return; // 이미 들고 있음
        
        _currentWeapon?.gameObject.SetActive(false);
        _currentWeapon = targetWeapon;
        _currentWeapon.gameObject.SetActive(true);
        
        _animator.PlaySwapAnimation();
        EventManager.OnWeaponEquipped?.Invoke(index);
        UpdateAmmoUI();
    }
    
    public void SwapWeapon(int direction)
    {
        if (_currentWeapon == null) return;
        int currentIndex = (int)_currentWeapon.Data.slot;
        int nextIndex = currentIndex;
        
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
                // 현재 들고 있는 무기가 아닐 때만 장착
                if (nextIndex != currentIndex)
                    EquipWeaponByIndex(nextIndex);
                break;
            }
        }
    }
    
    private void DropWeapon(Weapon weaponToDrop)
    {
        // 남은 탄창이 있다면 예비 탄약으로 환급
        if (IsPrimaryRangedWeapon(weaponToDrop))
        {
            _ammo += weaponToDrop.CurrentAmmo;
            Debug.Log($"버려진 무기의 총알 {weaponToDrop.CurrentAmmo}발 반환됨. 현재 총알 수: {_ammo}");
        }

        // 드랍용 프리팹(아이템) 바닥에 생성
        if (weaponToDrop.Data.dropPrefab != null)
        {
            Vector3 dropPosition = transform.position + transform.forward * 0.5f + Vector3.up * 1.2f;
            GameObject droppedItemObj = Instantiate(weaponToDrop.Data.dropPrefab, dropPosition, Quaternion.identity);
            
            if (droppedItemObj.TryGetComponent(out Rigidbody rb))
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.AddForce((transform.forward + Vector3.up * 0.5f).normalized * 4.0f, ForceMode.Impulse);
            }
        }

        // 손에 들고 있던 무기 오브젝트 파괴
        Destroy(weaponToDrop.gameObject);
    }

    public void AddAmmo(int amount)
    {
        _ammo += amount;
        UpdateAmmoUI();
    }
    
    public void TryAttack()
    {
        if (_currentWeapon == null) return;
        
        if (_currentWeapon.CanAttack())
        {
            _animator.CancelReloadAnimation();
            _currentWeapon.PerformAttack();
            
            WeaponData data = _currentWeapon.Data;
            
            switch (data.attackType)
            {
                case WeaponData.AttackType.Ranged:
                    SpawnBullet(data);
                    break;
                case WeaponData.AttackType.Throwable:
                    ThrowGrenade(data);
                    break;
            }
            
            UpdateAmmoUI();
            _animator.PlayAttackAnimation(data.attackType);
            
            if (IsPrimaryRangedWeapon(_currentWeapon) && _currentWeapon.CurrentAmmo <= 0)
                TryReload();
        }
    }

    public bool CanAttackCurrentWeapon()
    {
        return _currentWeapon &&  _currentWeapon.CanAttack();
    }

    public void EnableMeleeHitbox()
    {
        _currentWeapon?.EnableHitbox();
    }

    public void DisableMeleeHitbox()
    {
        _currentWeapon?.DisableHitbox();
    }
    
    public void TryReload()
    {
        if (!IsPrimaryRangedWeapon(_currentWeapon)) return;

        if (_currentWeapon.CurrentAmmo < _currentWeapon.Data.maxAmmo && _ammo > 0) 
            _animator.PlayReloadAnimation();
    }
    
    public void ExecuteReload()
    {
        int ammoToReload = Mathf.Min(_currentWeapon.Data.maxAmmo - _currentWeapon.CurrentAmmo, _ammo);
        _currentWeapon.FillAmmo(ammoToReload);
        _ammo -= ammoToReload;
        UpdateAmmoUI();
    }
    
    private void UpdateAmmoUI()
    {
        if (IsPrimaryRangedWeapon(_currentWeapon))
            EventManager.OnAmmoChanged?.Invoke(_currentWeapon.CurrentAmmo, _ammo);
        else
            EventManager.OnAmmoChanged?.Invoke(-1, -1);
    }

    private static bool IsPrimaryRangedWeapon(Weapon weapon)
    {
        return weapon && weapon.Data.slot == 0 && weapon.Data.attackType == WeaponData.AttackType.Ranged ;
    }

    private void SpawnBullet(WeaponData data)
    {
        if (data.bulletPrefab == null || firePoint == null) return;

        Vector3 direction = firePoint.forward;
        direction.y = 0;
        direction.Normalize();

        GameObject bulletObj = ObjectPool.Instance.Get(data.bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.Initialize(data.attackPower, data.bulletSpeed, direction, data.knockbackForce);
        }
    }

    private void ThrowGrenade(WeaponData data)
    {
        if (data.grenadePrefab == null || firePoint == null) return;

        Vector3 spawnPos = firePoint.position;

        // 마우스 위치로 레이캐스트
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return;
        
        Vector3 targetPos = hit.point;
        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 toTarget = targetPos - spawnPos;
        float horizontalDist = new Vector3(toTarget.x, 0, toTarget.z).magnitude;

        // 아크 높이를 거리에 비례 (가까울수록 낮은 아크 = 빠른 도착)
        float arcHeight = Mathf.Max(horizontalDist * 0.01f, 0.3f);
        float peakY = Mathf.Max(spawnPos.y, targetPos.y) + arcHeight;

        // 수직 속도: 정점까지 올라가는 데 필요한 초기 vy
        float dyUp = peakY - spawnPos.y;
        float vy = Mathf.Sqrt(2f * gravity * dyUp);

        // 올라가는 시간 + 내려오는 시간 = 총 비행 시간
        float tUp = vy / gravity;
        float dyDown = peakY - targetPos.y;
        float tDown = Mathf.Sqrt(2f * dyDown / gravity);
        float totalTime = tUp + tDown;

        // 수평 속도: 총 비행시간 동안 수평거리를 커버
        Vector3 horizontal = new Vector3(toTarget.x, 0, toTarget.z);
        Vector3 horizontalVelocity = horizontal / totalTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * vy;

        GameObject grenadeObj = Instantiate(data.grenadePrefab, spawnPos, Quaternion.identity);
        if (grenadeObj.TryGetComponent(out ThrownGrenade grenade))
        {
            grenade.Initialize(data.attackPower, data.knockbackForce, data.explosionRadius, velocity);
        }

        // 투척 무기는 1회성 — 슬롯에서 제거
        int slotIndex = (int)data.slot;
        if (_equippedWeapons[slotIndex])
        {
            Destroy(_equippedWeapons[slotIndex].gameObject);
            _equippedWeapons[slotIndex] = null;
            _currentWeapon = null;
            EventManager.OnWeaponRemoved?.Invoke(slotIndex);
        }
    }
}
