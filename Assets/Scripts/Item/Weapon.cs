using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData Data { get; private set; }
    public int CurrentAmmo { get; private set; }
    
    private float _lastAttackTime;
    
    public void Initialize(WeaponData data, int ammo = 0)
    {
        Data = data;
        CurrentAmmo = ammo; // 주웠을 때는 0, 장전하면 증가
    }
    
    public bool CanAttack()
    {
        // 1. 공격 속도(쿨타임) 체크
        if (Time.time - _lastAttackTime < Data.attackRate)
            return false;

        // 2. 원거리 무기인데 탄창이 비었는지 체크
        if (Data.attackType == WeaponData.AttackType.Ranged && Data.slot == 0 && CurrentAmmo <= 0)
        {
            Debug.Log("탄창이 비었습니다! 장전이 필요합니다.");
            return false;
        }

        return true;
    }
    
    public void PerformAttack()
    {
        _lastAttackTime = Time.time;

        // 원거리 무기면 탄약 소모
        if (Data.attackType == WeaponData.AttackType.Ranged)
        {
            CurrentAmmo--;
        }
        else if (Data.attackType == WeaponData.AttackType.Throwable)
        {
            // 투척 무기는 1회성이므로 탄약을 쓰지 않고 매니저에서 즉시 파괴/슬롯 비움 처리
        }
    }

    public void FillAmmo(int amount)
    {
        CurrentAmmo = Mathf.Clamp(CurrentAmmo + amount, 0, Data.maxAmmo);
    }
}
