using UnityEngine;

/// <summary>
/// 플레이어 Animator 파라미터를 캡슐화한 래퍼 클래스.
/// 상체/하체 레이어 분리를 활용하여 이동 중 사격, 회피 등 동시 애니메이션을 지원합니다.
/// 
/// <para><b>레이어 구조</b>:
/// Layer 0 (Base) — 이동(Idle/Run), 근접 공격(Swing), 사격(Shot), 투척(Throw), 회피(Dodge)
/// Layer 1 (UpperBody) — 재장전(Reload), 무기 교체(Swap). 
/// SetUpperBodyWeight(0)으로 회피 중 상체 애니메이션을 차단합니다.</para>
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int DoDodge = Animator.StringToHash("DoDodge");
    private static readonly int DoSwap = Animator.StringToHash("DoSwap");
    private static readonly int DoReload = Animator.StringToHash("DoReload");
    private static readonly int DoDeath = Animator.StringToHash("DoDeath");

    private const int UpperBodyLayerIndex = 1;
    
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    
    /// <summary>이동 상태 애니메이션을 설정합니다.</summary>
    public void SetMoving(bool isMoving)
    {
        _animator.SetBool(IsMovingHash, isMoving);
    }
    
    /// <summary>회피 트리거를 발동합니다.</summary>
    public void TriggerDodge()
    {
        _animator.SetTrigger(DoDodge);
    }

    /// <summary>무기 교체 트리거를 리셋합니다. 회피 시작 시 잔여 트리거를 제거하는 데 사용됩니다.</summary>
    public void ResetSwapTrigger()
    {
        _animator.ResetTrigger(DoSwap);
    }
    
    /// <summary>
    /// 상체 레이어 가중치를 설정합니다.
    /// 회피 중에는 0으로 설정하여 하체 모션만 재생되도록 합니다.
    /// </summary>
    public void SetUpperBodyWeight(float weight)
    {
        if (_animator.layerCount > UpperBodyLayerIndex)
        {
            _animator.SetLayerWeight(UpperBodyLayerIndex, weight);
        }
    }
    
    /// <summary>무기 교체 애니메이션을 재생합니다.</summary>
    public void PlaySwapAnimation()
    {
        if (_animator.layerCount > UpperBodyLayerIndex)
        {
            _animator.Play("Swap", UpperBodyLayerIndex, 0f);
        }
    }

    /// <summary>공격 타입에 맞는 공격 애니메이션을 재생합니다.</summary>
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

    /// <summary>현재 공격 애니메이션이 재생 중인지 확인합니다.</summary>
    public bool IsPlayingAttackAnimation()
    {
        if (_animator == null) return false;
        
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Swing") || stateInfo.IsName("Shot") || stateInfo.IsName("Throw");
    }
    
    /// <summary>재장전 애니메이션을 트리거합니다.</summary>
    public void PlayReloadAnimation()
    {
        _animator.SetTrigger(DoReload);
    }
    
    /// <summary>재장전 애니메이션을 취소하고 상체 레이어를 비웁니다.</summary>
    public void CancelReloadAnimation()
    {
        _animator.ResetTrigger(DoReload);
        _animator.CrossFade("Empty", 0.1f, 1);
    }

    /// <summary>사망 트리거를 발동합니다.</summary>
    public void TriggerDeath()
    {
        _animator.SetTrigger(DoDeath);
    }
}
