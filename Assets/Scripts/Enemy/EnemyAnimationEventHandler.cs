using UnityEngine;

public class EnemyAnimationEventHandler : MonoBehaviour
{
    private EnemyController _controller;

    private void Awake()
    {
        _controller = GetComponentInParent<EnemyController>();
    }
    
    public void OnAttackHitStart()
    {
        _controller?.EnableHitbox();
    }
    
    public void OnAttackHitEnd()
    {
        _controller?.DisableHitbox();
    }

    public void OnFire()
    {
        _controller?.Fire();
    }
}
