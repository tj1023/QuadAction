using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 근접 무기의 히트박스를 관리하는 컴포넌트.
/// EnemyMeleeHitbox와 유사하지만, 적을 대상으로 넉백을 포함한 피격 처리를 수행합니다.
/// 
/// <para><b>EnemyStats vs IDamageable</b>: EnemyStats가 있으면 OnHit()로
/// 넉백을 포함한 처리를, 없으면 IDamageable.TakeDamage()로 단순 데미지를 적용합니다.
/// 이를 통해 파괴 가능한 오브젝트(미사일 등)에도 대응할 수 있습니다.</para>
/// </summary>
public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private Collider hitCollider;
    [SerializeField] private TrailRenderer trail;

    private int _damage;
    private float _knockbackForce;
    private readonly HashSet<GameObject> _hitTargets = new();

    private void Awake()
    {
        Deactivate();
    }

    /// <summary>히트박스를 활성화하고 트레일을 시작합니다.</summary>
    public void Activate(int damage, float knockbackForce)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
        _hitTargets.Clear();

        if (hitCollider != null) hitCollider.enabled = true;
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    /// <summary>히트박스를 비활성화하고 트레일을 중단합니다.</summary>
    public void Deactivate()
    {
        if (hitCollider != null) hitCollider.enabled = false;
        if (trail != null) trail.emitting = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        if (!_hitTargets.Add(other.gameObject)) return;

        if (other.TryGetComponent(out EnemyStats enemyStats))
        {
            Vector3 hitDir = (other.transform.position - transform.root.position).normalized;
            enemyStats.OnHit(_damage, hitDir, _knockbackForce);
        }
        else if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damage);
        }
    }
}
