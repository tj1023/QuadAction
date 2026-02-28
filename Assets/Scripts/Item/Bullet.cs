using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    private int _damage;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Initialize(int damage, float speed, Vector3 direction)
    {
        _damage = damage;
        _rb.useGravity = false;
        _rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 자기 자신(플레이어)과 충돌 무시
        if (other.CompareTag("Player")) return;

        // TODO: 대상에게 데미지
        // if (other.TryGetComponent(out IDamageable target))
        //     target.TakeDamage(_damage);

        Destroy(gameObject);
    }
}
