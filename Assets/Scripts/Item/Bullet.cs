using UnityEngine;

/// <summary>
/// 플레이어가 발사하는 총알.
/// Object Pool에서 관리되며, 수명 종료 또는 충돌 시 풀에 반환됩니다.
/// 
/// <para><b>피격 분기</b>: EnemyStats가 있으면 OnHit(넉백 포함)을,
/// 없으면 IDamageable.TakeDamage(단순 데미지)를 호출합니다.
/// 이를 통해 적뿐 아니라 파괴 가능 오브젝트(미사일 등)에도 대응합니다.</para>
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private const float LifeTime = 1f;

    private int _damage;
    private float _knockbackForce;
    private Rigidbody _rb;
    private float _deactivateTime;
    private TrailRenderer _trailRenderer;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    /// <summary>총알을 초기화합니다. 풀에서 재사용될 때마다 호출됩니다.</summary>
    public void Initialize(int damage, float speed, Vector3 direction, float knockbackForce)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
        _rb.useGravity = false;
        _rb.linearVelocity = direction.normalized * speed;
        _deactivateTime = Time.time + LifeTime;
        _trailRenderer?.Clear();
    }

    private void Update()
    {
        if (Time.time >= _deactivateTime)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Hitbox")) return;

        Vector3 hitDirection = _rb.linearVelocity.normalized;

        if (other.TryGetComponent(out EnemyStats enemyStats))
            enemyStats.OnHit(_damage, hitDirection, _knockbackForce);
        else if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(_damage);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _rb.linearVelocity = Vector3.zero;
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Release(gameObject);
        else
            gameObject.SetActive(false);
    }
}
