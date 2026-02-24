using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    
    private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int _dodgeHash = Animator.StringToHash("DoDodge");
    private readonly int _swapHash = Animator.StringToHash("DoSwap");

    private const int UpperBodyLayerIndex = 1;
    
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

    public void ForceRestartSwap()
    {
        if (_animator.layerCount > UpperBodyLayerIndex)
        {
            _animator.Play("Swap", UpperBodyLayerIndex, 0f);
        }
    }

    public void ResetTriggers()
    {
        _animator.ResetTrigger(_swapHash);
    }
    
    public void SetUpperBodyWeight(float weight)
    {
        if (_animator.layerCount > UpperBodyLayerIndex)
        {
            _animator.SetLayerWeight(UpperBodyLayerIndex, weight);
        }
    }
}
