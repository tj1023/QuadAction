using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 무기 슬롯 시스템을 관리합니다.
/// 무기 추가·교체·드롭·공격·재장전 기능을 제공합니다.
/// 
/// <para><b>슬롯 구조</b>: WeaponData.WeaponSlot 열거형 크기만큼 슬롯을 생성하여
/// Primary(0)/Secondary(1)/Utility(2) 슬롯을 지원합니다.
/// 같은 슬롯에 새 무기를 장착하면 기존 무기를 드롭합니다.</para>
/// 
/// <para><b>예비 탄약 시스템</b>: _ammo는 전체 예비 탄약을 관리하며,
/// 주무기 원거리 무기만 탄창·예비 탄약 시스템을 사용합니다.</para>
/// </summary>
public class PlayerWeaponManager : MonoBehaviour
{
    private const float ReloadDelay = 0.3f;
    private const float DropForwardDistance = 2f;
    private const float DropUpwardDistance = 2f;
    private const float DropForce = 4.0f;
    private const float MouseRayMaxDistance = 100f;
    private const float MinArcHeight = 0.3f;
    private const float ArcHeightMultiplier = 0.01f;

    [SerializeField] private Transform weaponSlot;
    [SerializeField] private Transform firePoint;

    private PlayerAnimator _animator;
    private Weapon[] _equippedWeapons;
    private Weapon _currentWeapon;
    private int _ammo;
    private int _maxSlots;
    private Camera _camera;
    private Coroutine _reloadDelayCoroutine;

    private void Awake()
    {
        _animator = GetComponent<PlayerAnimator>();
        _maxSlots = Enum.GetValues(typeof(WeaponData.WeaponSlot)).Length;
        _equippedWeapons = new Weapon[_maxSlots];
        _camera = Camera.main;
    }

    /// <summary>
    /// 새 무기를 추가합니다. 해당 슬롯에 기존 무기가 있으면 드롭합니다.
    /// </summary>
    public void AddWeapon(WeaponData newWeaponData)
    {
        if (newWeaponData == null) return;
        
        int slotIndex = (int)newWeaponData.Slot;

        if (_equippedWeapons[slotIndex] != null)
            DropWeapon(_equippedWeapons[slotIndex]);

        GameObject weaponObj = Instantiate(newWeaponData.WeaponPrefab, weaponSlot);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        if (!weaponObj.TryGetComponent(out Weapon newWeapon))
        {
            Debug.LogError($"{newWeaponData.WeaponName}의 프리팹({weaponObj.name})에 Weapon 컴포넌트가 없습니다.");
            Destroy(weaponObj);
            return;
        }
        newWeapon.Initialize(newWeaponData); 

        _equippedWeapons[slotIndex] = newWeapon;
        EventManager.OnWeaponAdded?.Invoke(slotIndex, newWeaponData);
        EquipWeaponByIndex(slotIndex, true);
        
        // 주무기 원거리 무기면 딜레이 후 자동 재장전
        if (IsPrimaryRangedWeapon(newWeapon))
        {
            if (_reloadDelayCoroutine != null)
                StopCoroutine(_reloadDelayCoroutine);
            _reloadDelayCoroutine = StartCoroutine(DelayedReload());
        }
    }

    /// <summary>타입 안전한 딜레이 후 재장전. Invoke 대신 코루틴을 사용하여 취소 가능합니다.</summary>
    private IEnumerator DelayedReload()
    {
        yield return new WaitForSeconds(ReloadDelay);
        TryReload();
        _reloadDelayCoroutine = null;
    }
    
    /// <summary>슬롯 인덱스로 무기를 장착합니다.</summary>
    public void EquipWeaponByIndex(int index, bool forceEquip = false)
    {
        if (index < 0 || index >= _maxSlots) return;

        Weapon targetWeapon = _equippedWeapons[index];
        if (targetWeapon == null) return;
        if (targetWeapon == _currentWeapon && !forceEquip) return;
        
        _currentWeapon?.gameObject.SetActive(false);
        _currentWeapon = targetWeapon;
        _currentWeapon.gameObject.SetActive(true);
        
        _animator.PlaySwapAnimation();
        EventManager.OnWeaponEquipped?.Invoke(index);
        UpdateAmmoUI();
    }
    
    /// <summary>현재 무기에서 direction 방향으로 다음 무기로 교체합니다.</summary>
    public void SwapWeapon(int direction)
    {
        if (_currentWeapon == null) return;
        int currentIndex = (int)_currentWeapon.Data.Slot;
        int nextIndex = currentIndex;
        
        for (int i = 0; i < _maxSlots; i++)
        {
            nextIndex += direction;
            
            if (nextIndex >= _maxSlots) nextIndex = 0;
            else if (nextIndex < 0) nextIndex = _maxSlots - 1;

            if (_equippedWeapons[nextIndex] != null)
            {
                if (nextIndex != currentIndex)
                    EquipWeaponByIndex(nextIndex);
                break;
            }
        }
    }
    
    /// <summary>무기를 드롭합니다. 잔여 탄약은 예비 탄약으로 환급됩니다.</summary>
    private void DropWeapon(Weapon weaponToDrop)
    {
        if (IsPrimaryRangedWeapon(weaponToDrop))
            _ammo += weaponToDrop.CurrentAmmo;

        if (weaponToDrop.Data.DropPrefab != null)
        {
            Vector3 dropPosition = transform.position
                                   + transform.forward * DropForwardDistance
                                   + Vector3.up * DropUpwardDistance;
            GameObject droppedItemObj = Instantiate(weaponToDrop.Data.DropPrefab, dropPosition, Quaternion.identity);
            
            if (droppedItemObj.TryGetComponent(out Rigidbody rb))
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.AddForce((transform.forward + Vector3.up).normalized * DropForce, ForceMode.Impulse);
            }
        }

        Destroy(weaponToDrop.gameObject);
    }

    /// <summary>예비 탄약을 증가시킵니다.</summary>
    public void AddAmmo(int amount)
    {
        _ammo += amount;
        UpdateAmmoUI();
    }
    
    /// <summary>현재 무기로 공격을 시도합니다.</summary>
    public void TryAttack()
    {
        if (_currentWeapon == null) return;
        
        if (_currentWeapon.CanAttack())
        {
            _animator.CancelReloadAnimation();
            _currentWeapon.PerformAttack();
            
            WeaponData data = _currentWeapon.Data;
            
            switch (data.WeaponAttackType)
            {
                case WeaponData.AttackType.Ranged:
                    SpawnBullet(data);
                    break;
                case WeaponData.AttackType.Throwable:
                    ThrowGrenade(data);
                    break;
            }
            
            UpdateAmmoUI();
            _animator.PlayAttackAnimation(data.WeaponAttackType);
            
            if (data.AttackSound != null)
                SoundManager.Instance.PlaySfx(data.AttackSound);
            
            if (IsPrimaryRangedWeapon(_currentWeapon) && _currentWeapon.CurrentAmmo <= 0)
                TryReload();
        }
    }

    /// <summary>현재 무기가 공격 가능한지 확인합니다.</summary>
    public bool CanAttackCurrentWeapon()
    {
        return _currentWeapon != null && _currentWeapon.CanAttack();
    }

    /// <summary>현재 무기의 근접 히트박스를 활성화합니다.</summary>
    public void EnableMeleeHitbox()
    {
        _currentWeapon?.EnableHitbox();
    }

    /// <summary>현재 무기의 근접 히트박스를 비활성화합니다.</summary>
    public void DisableMeleeHitbox()
    {
        _currentWeapon?.DisableHitbox();
    }
    
    /// <summary>재장전을 시도합니다. 탄창이 가득 차있거나 예비 탄약이 없으면 무시됩니다.</summary>
    public void TryReload()
    {
        if (!IsPrimaryRangedWeapon(_currentWeapon)) return;

        if (_currentWeapon.CurrentAmmo < _currentWeapon.Data.MaxAmmo && _ammo > 0) 
            _animator.PlayReloadAnimation();
    }
    
    /// <summary>재장전을 실행합니다. AnimationEventHandler에서 호출됩니다.</summary>
    public void ExecuteReload()
    {
        int ammoToReload = Mathf.Min(_currentWeapon.Data.MaxAmmo - _currentWeapon.CurrentAmmo, _ammo);
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

    /// <summary>주무기 슬롯(0)의 원거리 무기인지 확인합니다.</summary>
    private static bool IsPrimaryRangedWeapon(Weapon weapon)
    {
        return weapon != null
               && weapon.Data.Slot == 0
               && weapon.Data.WeaponAttackType == WeaponData.AttackType.Ranged;
    }

    /// <summary>총알을 Object Pool에서 꺼내 발사합니다.</summary>
    private void SpawnBullet(WeaponData data)
    {
        if (data.BulletPrefab == null || firePoint == null) return;

        Vector3 direction = firePoint.forward;
        direction.y = 0;
        direction.Normalize();

        GameObject bulletObj = ObjectPool.Instance.Get(
            data.BulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        if (bulletObj.TryGetComponent(out Bullet bullet))
        {
            bullet.Initialize(
                _currentWeapon.GetCurrentDamage(),
                data.BulletSpeed,
                direction,
                data.KnockbackForce);
        }
    }

    /// <summary>
    /// 수류탄을 생성하여 마우스 위치로 포물선 투척합니다.
    /// 수직·수평 속도를 물리적으로 계산하여 자연스러운 아크 궤적을 만듭니다.
    /// </summary>
    private void ThrowGrenade(WeaponData data)
    {
        if (data.GrenadePrefab == null || firePoint == null) return;

        Vector3 spawnPos = firePoint.position;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, MouseRayMaxDistance))
            return;
        
        Vector3 targetPos = hit.point;
        float gravityMagnitude = Mathf.Abs(Physics.gravity.y);

        Vector3 toTarget = targetPos - spawnPos;
        float horizontalDist = new Vector3(toTarget.x, 0, toTarget.z).magnitude;

        // 아크 높이를 거리에 비례시켜 가까운 대상에는 낮은 아크 적용
        float arcHeight = Mathf.Max(horizontalDist * ArcHeightMultiplier, MinArcHeight);
        float peakY = Mathf.Max(spawnPos.y, targetPos.y) + arcHeight;

        float dyUp = peakY - spawnPos.y;
        float vy = Mathf.Sqrt(2f * gravityMagnitude * dyUp);

        float tUp = vy / gravityMagnitude;
        float dyDown = peakY - targetPos.y;
        float tDown = Mathf.Sqrt(2f * dyDown / gravityMagnitude);
        float totalTime = tUp + tDown;

        Vector3 horizontal = new Vector3(toTarget.x, 0, toTarget.z);
        Vector3 horizontalVelocity = horizontal / totalTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * vy;

        GameObject grenadeObj = Instantiate(data.GrenadePrefab, spawnPos, Quaternion.identity);
        if (grenadeObj.TryGetComponent(out ThrownGrenade grenade))
        {
            grenade.Initialize(
                _currentWeapon.GetCurrentDamage(),
                data.KnockbackForce,
                data.ExplosionRadius,
                velocity);
        }

        // 투척 무기는 1회성 — 슬롯에서 제거
        int slotIndex = (int)data.Slot;
        if (_equippedWeapons[slotIndex] != null)
        {
            Destroy(_equippedWeapons[slotIndex].gameObject);
            _equippedWeapons[slotIndex] = null;
            _currentWeapon = null;
            EventManager.OnWeaponRemoved?.Invoke(slotIndex);
        }
    }

    /// <summary>특정 WeaponData에 해당하는 장착된 무기를 반환합니다. 장착하지 않았다면 null을 반환합니다.</summary>
    public Weapon GetWeapon(WeaponData weaponData)
    {
        if (weaponData == null) return null;
        
        int slotIndex = (int)weaponData.Slot;
        if (slotIndex >= 0 && slotIndex < _maxSlots)
        {
            Weapon weapon = _equippedWeapons[slotIndex];
            if (weapon != null && weapon.Data == weaponData)
                return weapon;
        }
        return null;
    }
}
