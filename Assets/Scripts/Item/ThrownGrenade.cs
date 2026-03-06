using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 투척한 수류탄.
/// 포물선 궤적으로 날아가 충돌 시 폭발하여 범위 데미지 + 래그돌을 적용합니다.
/// 
/// <para><b>폭발 로직</b>: Physics.OverlapSphere로 범위 내 Collider를 탐지하고,
/// HashSet으로 중복 피격을 방지합니다. EnemyStats가 있으면 OnHit(래그돌 포함)을,
/// 없으면 IDamageable.TakeDamage를 호출합니다.</para>
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ThrownGrenade : MonoBehaviour
{
    private const float EffectDestroyExtraTime = 1f;

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
        if (_effect != null) _effect.Stop();
    }

    /// <summary>수류탄을 초기화합니다. 발사 속도를 설정합니다.</summary>
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

        // 이펙트를 부모에서 분리하여 수류탄 파괴 후에도 재생 완료
        if (_effect != null)
        {
            _effect.transform.SetParent(null);
            _effect.Play();
            Destroy(_effect.gameObject, _effect.main.duration + EffectDestroyExtraTime);
        }

        // 범위 내 대상에 데미지 적용
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
                enemyStats.OnHit(_damage, hitDir, _knockbackForce, willRagdoll: true);

                if (enemyStats.IsDead && hit.TryGetComponent(out EnemyController controller))
                    controller.LaunchRagdoll(transform.position, _knockbackForce);
            }
            else if (hit.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(_damage);
            }
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥이나 벽에 부딪히면 폭발 (플레이어 제외)
        if (!collision.collider.CompareTag("Player"))
            Explode();
    }
}
