using UnityEngine;

/// <summary>
/// 플레이어 애니메이션 이벤트를 관련 컴포넌트에 전달하는 브릿지.
/// Animator가 자식 오브젝트에 있으므로 이벤트를 부모의 컨트롤러/매니저로 중계합니다.
/// </summary>
public class AnimationEventHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerWeaponManager _weaponManager;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
        _weaponManager = GetComponentInParent<PlayerWeaponManager>();
    }
    
    /// <summary>회피 애니메이션 종료 이벤트.</summary>
    public void OnDodgeEnd()
    {
        _playerController?.EndDodge();
    }

    /// <summary>재장전 완료 이벤트. 탄약 충전을 실행합니다.</summary>
    public void OnReloadComplete()
    {
        _weaponManager?.ExecuteReload();
    }

    /// <summary>근접 공격 스윙 시작 이벤트. 히트박스를 활성화합니다.</summary>
    public void OnSwingStart()
    {
        _weaponManager?.EnableMeleeHitbox();
    }

    /// <summary>근접 공격 스윙 종료 이벤트. 히트박스를 비활성화합니다.</summary>
    public void OnSwingEnd()
    {
        _weaponManager?.DisableMeleeHitbox();
    }
}
