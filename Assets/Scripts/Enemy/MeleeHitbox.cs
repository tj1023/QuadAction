using System.Collections.Generic;
using UnityEngine;

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

    public void Activate(int damage, float knockbackForce)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
        _hitTargets.Clear();

        if (hitCollider) hitCollider.enabled = true;
        if (trail)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    public void Deactivate()
    {
        if (hitCollider) hitCollider.enabled = false;
        if (trail) trail.emitting = false;
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
