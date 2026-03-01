using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator _animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int DoAttack = Animator.StringToHash("DoAttack");
    private static readonly int DoDeath = Animator.StringToHash("DoDeath");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void SetMoving(bool moving)
    {
        _animator.SetBool(IsMoving, moving);
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
