using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrownGrenade : MonoBehaviour
{
    private int _damage;
    private float _explosionRadius;
    private float _knockbackForce;
    private Rigidbody _rb;
    private ParticleSystem _effect;
    private bool _exploded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _effect = GetComponentInChildren<ParticleSystem>();
        if (_effect) _effect.Stop();
    }

    public void Initialize(int damage, float knockbackForce, float explosionRadius, Vector3 velocity)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
        _explosionRadius = explosionRadius;
        _rb.linearVelocity = velocity;
    }

    private void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        // 이펙트 재생
        if (_effect)
        {
            _effect.transform.SetParent(null);
            _effect.Play();
            Destroy(_effect.gameObject, _effect.main.duration + 1f);
        }

        // 범위 내 대상에 데미지
        Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius);
        HashSet<GameObject> damaged = new();

        foreach (var hit in hits)
        {
            if (damaged.Contains(hit.gameObject)) continue;
            damaged.Add(hit.gameObject);

            if (hit.CompareTag("Player")) continue;

            Vector3 hitDir = (hit.transform.position - transform.position).normalized;

            if (hit.TryGetComponent(out EnemyStats enemyStats))
            {
                enemyStats.OnHit(_damage, hitDir, _knockbackForce);

                // 죽은 적은 날아가게
                if (enemyStats.IsDead && hit.TryGetComponent(out EnemyController controller))
                    controller.LaunchRagdoll(transform.position, _knockbackForce);
            }
            else if (hit.TryGetComponent(out IDamageable target))
                target.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥이나 벽에 부딪히면 폭발
        if (!collision.collider.CompareTag("Player"))
            Explode();
    }
}
