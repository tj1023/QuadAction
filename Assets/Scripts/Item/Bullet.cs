using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

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

    public void Initialize(int damage, float speed, Vector3 direction, float knockbackForce)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
        _rb.useGravity = false;
        _rb.linearVelocity = direction.normalized * speed;
        _deactivateTime = Time.time + lifeTime;
        _trailRenderer?.Clear();
    }

    private void Update()
    {
        if (Time.time >= _deactivateTime)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;

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
        if (ObjectPool.Instance)
            ObjectPool.Instance.Release(gameObject);
        else
            gameObject.SetActive(false);
    }
}
