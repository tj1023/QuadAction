using UnityEngine;

/// <summary>
/// 일반 적의 Animator 파라미터를 캡슐화한 래퍼 클래스.
/// 문자열 직접 참조 대신 <see cref="Animator.StringToHash"/>로 캐싱된 해시를 사용하여
/// 매 프레임 문자열 비교에 따른 GC 할당을 방지합니다.
/// </summary>
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

    /// <summary>이동 상태 애니메이션을 설정합니다.</summary>
    public void SetMoving(bool moving)
    {
        _animator.SetBool(IsMoving, moving);
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
