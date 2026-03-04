using UnityEngine;

public class BossAnimator : MonoBehaviour
{
    private Animator _animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int DoAttack = Animator.StringToHash("DoAttack");
    private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");
    private static readonly int DoDeath = Animator.StringToHash("DoDeath");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void SetMoving(bool moving)
    {
        _animator.SetBool(IsMoving, moving);
    }

    public void SetAttackIndex(int index)
    {
        _animator.SetInteger(AttackIndex, index);
    }

    public void TriggerAttack()
    {
        _animator.SetTrigger(DoAttack);
    }

    public void TriggerDeath()
    {
        _animator.SetTrigger(DoDeath);
    }
}
