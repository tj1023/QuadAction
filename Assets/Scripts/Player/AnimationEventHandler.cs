using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerWeaponManager _weaponManager;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
		_weaponManager = GetComponentInParent<PlayerWeaponManager>();
    }
    
    public void OnDodgeEnd()
    {
        _playerController?.EndDodge();
    }

	public void OnReloadComplete()
    {
        _weaponManager?.ExecuteReload();
    }

    public void OnSwingStart()
    {
        _weaponManager?.EnableMeleeHitbox();
    }

    public void OnSwingEnd()
    {
        _weaponManager?.DisableMeleeHitbox();
    }
}
