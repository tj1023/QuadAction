using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int DoDodge = Animator.StringToHash("DoDodge");
    private static readonly int DoSwap = Animator.StringToHash("DoSwap");
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
                _animator.Play("Swing", 0, 0f);
                break;
            case WeaponData.AttackType.Ranged:
                _animator.Play("Shot", 0, 0f);
                break;
            case WeaponData.AttackType.Throwable:
                _animator.Play("Throw", 0, 0f);
                break;
        }
    }

    public bool IsPlayingAttackAnimation()
    {
        if (_animator == null) return false;
        
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Swing") || stateInfo.IsName("Shot") || stateInfo.IsName("Throw");
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
