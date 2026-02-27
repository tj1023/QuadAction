using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int DoDodge = Animator.StringToHash("DoDodge");
    private static readonly int DoSwap = Animator.StringToHash("DoSwap");
    private static readonly int DoSwing = Animator.StringToHash("DoSwing");
    private static readonly int DoShot = Animator.StringToHash("DoShot");
    private static readonly int DoThrow = Animator.StringToHash("DoThrow");
    private static readonly int DoReload = Animator.StringToHash("DoReload");

    private const int UpperBodyLayerIndex = 1;
    
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    
    public void SetMoving(bool isMoving)
    {
        _animator.SetBool(IsMovingHash, isMoving);
    }
    
    public void TriggerDodge()
    {
        _animator.SetTrigger(DoDodge);
    }

    public void ResetSwapTrigger()
    {
        _animator.ResetTrigger(DoSwap);
    }
    
    public void SetUpperBodyWeight(float weight)
    {
        if (_animator.layerCount > UpperBodyLayerIndex)
        {
            _animator.SetLayerWeight(UpperBodyLayerIndex, weight);
        }
    }
    
    public void PlaySwapAnimation()
    {
        if (_animator.layerCount > UpperBodyLayerIndex)
        {
            _animator.Play("Swap", UpperBodyLayerIndex, 0f);
        }
    }

    public void PlayAttackAnimation(WeaponData.AttackType attackType)
    {
        switch (attackType)
        {
            case WeaponData.AttackType.Melee:
                _animator.SetTrigger(DoSwing);
                break;
            case WeaponData.AttackType.Ranged:
                _animator.SetTrigger(DoShot);
                break;
            case WeaponData.AttackType.Throwable:
                _animator.SetTrigger(DoThrow);
                break;
        }
    }
    
    public void PlayReloadAnimation()
    {
        _animator.SetTrigger(DoReload);
    }
    
    public void CancelReloadAnimation()
    {
        _animator.ResetTrigger(DoReload);
        _animator.CrossFade("Empty", 0.1f, 1);
    }
}
