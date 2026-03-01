using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private MeleeHitbox meleeHitbox;

    public WeaponData Data { get; private set; }
    public int CurrentAmmo { get; private set; }
    
    private float _lastAttackTime;
    
    public void Initialize(WeaponData data, int ammo = 0)
    {
        Data = data;
        CurrentAmmo = ammo;
    }
    
    public bool CanAttack()
    {
        // 1. 공격 속도(쿨타임) 체크
        if (Time.time - _lastAttackTime < Data.attackRate)
            return false;

        // 2. 원거리 무기인데 탄창이 비었는지 체크
        return !(Data.attackType == WeaponData.AttackType.Ranged && Data.slot == 0 && CurrentAmmo <= 0);
    }
    
    public void PerformAttack()
    {
        _lastAttackTime = Time.time;

        if (Data.attackType == WeaponData.AttackType.Ranged)
        {
            CurrentAmmo--;
        }
        else if (Data.attackType == WeaponData.AttackType.Throwable)
        {
            // 투척 무기는 1회성이므로 탄약을 쓰지 않고 매니저에서 즉시 파괴/슬롯 비움 처리
        }
    }

    public void EnableHitbox()
    {
        if (meleeHitbox != null)
            meleeHitbox.Activate(Data.attackPower, Data.knockbackForce);
    }

    public void DisableHitbox()
    {
        if (meleeHitbox != null)
            meleeHitbox.Deactivate();
    }

    public void FillAmmo(int amount)
    {
        CurrentAmmo = Mathf.Clamp(CurrentAmmo + amount, 0, Data.maxAmmo);
    }
}
