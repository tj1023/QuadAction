using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    private int _damage;
    private float _knockbackForce;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Initialize(int damage, float speed, Vector3 direction, float knockbackForce)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
        _rb.useGravity = false;
        _rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;

        Vector3 hitDirection = _rb.linearVelocity.normalized;

        if (other.TryGetComponent(out EnemyStats enemyStats))
            enemyStats.OnHit(_damage, hitDirection, _knockbackForce);
        else if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(_damage);

        Destroy(gameObject);
    }
}
