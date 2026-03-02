using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeHitbox : MonoBehaviour
{
    [SerializeField] private Collider hitCollider;

    private int _damage;
    private readonly HashSet<GameObject> _hitTargets = new();

    private void Awake()
    {
        Deactivate();
    }

    public void Activate(int damage)
    {
        _damage = damage;
        _hitTargets.Clear();
        if (hitCollider) hitCollider.enabled = true;
    }

    public void Deactivate()
    {
        if (hitCollider) hitCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_hitTargets.Add(other.gameObject)) return;

        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(_damage);
    }
}
