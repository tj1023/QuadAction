using UnityEngine;

/// <summary>
/// 무기 인스턴스의 런타임 상태(탄약·업그레이드 레벨)를 관리하는 컴포넌트.
/// WeaponData(ScriptableObject)의 불변 기획 데이터를 참조하되,
/// 게임 진행에 따라 변하는 상태는 이 클래스에서 관리합니다.
/// 
/// <para><b>업그레이드 시스템</b>: 데미지 = basePower + (upgradeLevel × damagePerLevel).
/// 업그레이드 가격 = basePrice + (upgradeLevel × pricePerLevel).
/// ScriptableObject에 저장하지 않으므로 게임 재시작 시 자동 초기화됩니다.</para>
/// </summary>
public class Weapon : MonoBehaviour
{
    [SerializeField] private MeleeHitbox meleeHitbox;

    /// <summary>이 무기의 기획 데이터.</summary>
    public WeaponData Data { get; private set; }

    /// <summary>현재 탄창 내 탄약 수.</summary>
    public int CurrentAmmo { get; private set; }

    /// <summary>현재 업그레이드 레벨.</summary>
    public int UpgradeLevel { get; private set; }
    
    private float _lastAttackTime;
    
    /// <summary>무기를 초기화합니다. PlayerWeaponManager.AddWeapon에서 호출됩니다.</summary>
    public void Initialize(WeaponData data, int ammo = 0)
    {
        Data = data;
        CurrentAmmo = ammo;
        UpgradeLevel = 0;
    }

    /// <summary>
    /// 현재 업그레이드가 반영된 데미지를 계산합니다.
    /// </summary>
    public int GetCurrentDamage()
    {
        return Data.AttackPower + (UpgradeLevel * Data.DamageIncreasePerLevel);
    }

    /// <summary>
    /// 다음 업그레이드에 필요한 비용을 계산합니다.
    /// </summary>
    public int GetCurrentUpgradePrice()
    {
        return Data.BaseUpgradePrice + (UpgradeLevel * Data.PriceIncreasePerLevel);
    }

    /// <summary>업그레이드 레벨을 1 증가시킵니다.</summary>
    public void Upgrade()
    {
        UpgradeLevel++;
    }
    
    /// <summary>공격 가능 여부를 판정합니다. 쿨타임 + 탄약 검사.</summary>
    public bool CanAttack()
    {
        // 공격 속도(쿨타임) 체크
        if (Time.time - _lastAttackTime < Data.AttackRate)
            return false;

        // 주무기 원거리 무기인데 탄창이 비었는지 체크
        return !(Data.WeaponAttackType == WeaponData.AttackType.Ranged
                 && Data.Slot == 0
                 && CurrentAmmo <= 0);
    }
    
    /// <summary>공격을 수행합니다. 쿨타임을 리셋하고 원거리 시 탄약을 소비합니다.</summary>
    public void PerformAttack()
    {
        _lastAttackTime = Time.time;

        if (Data.WeaponAttackType == WeaponData.AttackType.Ranged)
            CurrentAmmo--;
    }

    /// <summary>근접 히트박스를 활성화합니다.</summary>
    public void EnableHitbox()
    {
        if (meleeHitbox != null)
            meleeHitbox.Activate(GetCurrentDamage(), Data.KnockbackForce);
    }

    /// <summary>근접 히트박스를 비활성화합니다.</summary>
    public void DisableHitbox()
    {
        if (meleeHitbox != null)
            meleeHitbox.Deactivate();
    }

    /// <summary>탄약을 충전합니다. 최대 탄약 수를 초과하지 않습니다.</summary>
    public void FillAmmo(int amount)
    {
        CurrentAmmo = Mathf.Clamp(CurrentAmmo + amount, 0, Data.MaxAmmo);
    }
}
