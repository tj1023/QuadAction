using UnityEngine;

/// <summary>
/// 보스 적의 Animator 파라미터를 캡슐화한 래퍼 클래스.
/// AttackIndex 파라미터로 미사일(0)/바위(1)/점프(2) 공격 애니메이션을 선택합니다.
/// </summary>
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

    /// <summary>이동 상태 애니메이션을 설정합니다.</summary>
    public void SetMoving(bool moving)
    {
        _animator.SetBool(IsMoving, moving);
    }

    /// <summary>다음 공격 트리거에 사용할 애니메이션 인덱스를 지정합니다.</summary>
    public void SetAttackIndex(int index)
    {
        _animator.SetInteger(AttackIndex, index);
    }

    /// <summary>공격 트리거를 발동합니다.</summary>
    public void TriggerAttack()
    {
        _animator.SetTrigger(DoAttack);
    }

    /// <summary>사망 트리거를 발동합니다.</summary>
    public void TriggerDeath()
    {
        _animator.SetTrigger(DoDeath);
    }
}
