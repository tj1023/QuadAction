using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    
    private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int _dodgeHash = Animator.StringToHash("DoDodge");
    private readonly int _swapHash = Animator.StringToHash("DoSwap");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    
    public void SetMoving(bool isMoving)
    {
        _animator.SetBool(_isMovingHash, isMoving);
    }
    
    public void TriggerDodge()
    {
        _animator.SetTrigger(_dodgeHash);
    }
    
    public void TriggerSwap()
    {
        _animator.SetTrigger(_swapHash);
    }
}
