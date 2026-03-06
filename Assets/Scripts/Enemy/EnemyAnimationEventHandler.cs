using UnityEngine;

/// <summary>
/// 일반 적 애니메이션 이벤트를 EnemyController에 전달하는 브릿지 컴포넌트.
/// 
/// <para><b>설계 이유</b>: Unity 애니메이션 이벤트는 Animator가 부착된 GameObject에서만
/// 호출할 수 있으므로, 자식 오브젝트의 Animator에서 부모의 EnemyController를 호출하려면
/// 별도의 중계용 컴포넌트가 필요합니다.</para>
/// </summary>
public class EnemyAnimationEventHandler : MonoBehaviour
{
    private EnemyController _controller;

    private void Awake()
    {
        _controller = GetComponentInParent<EnemyController>();
    }
    
    /// <summary>근접 공격 히트 판정 시작. 히트박스를 활성화합니다.</summary>
    public void OnAttackHitStart()
    {
        _controller?.EnableHitbox();
    }
    
    /// <summary>근접 공격 히트 판정 종료. 히트박스를 비활성화합니다.</summary>
    public void OnAttackHitEnd()
    {
        _controller?.DisableHitbox();
    }

    /// <summary>원거리 공격 발사 타이밍. 투사체를 생성합니다.</summary>
    public void OnFire()
    {
        _controller?.Fire();
    }
}
