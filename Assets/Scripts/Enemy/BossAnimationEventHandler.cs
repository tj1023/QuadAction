using UnityEngine;

public class BossAnimationEventHandler : MonoBehaviour
{
    private BossController _boss;

    private void Awake()
    {
        _boss = GetComponentInParent<BossController>();
    }
    
    public void OnFireMissileLeft()
    {
        _boss?.FireMissile(0);
    }

    public void OnFireMissileRight()
    {
        _boss?.FireMissile(1);
    }
    
    public void OnFireRock()
    {
        _boss?.FireRock();
    }
    
    public void OnAttackHitStart()
    {
        _boss?.EnableMeleeHitbox();
    }

    public void OnAttackHitEnd()
    {
        _boss?.DisableMeleeHitbox();
    }
    
    public void OnAttackEnd()
    {
        _boss?.OnAttackAnimationEnd();
    }
}
