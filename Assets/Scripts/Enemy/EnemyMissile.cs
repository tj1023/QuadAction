using System.Collections;
using UnityEngine;

/// <summary>
/// 적이 발사하는 투사체(미사일·바위).
/// IDamageable을 구현하여 플레이어가 투사체를 파괴할 수 있습니다.
/// 
/// <para><b>유도(Homing) 기능</b>: isHoming=true이면 FixedUpdate에서
/// RotateTowards를 사용해 부드러운 유도 회전을 수행합니다.
/// FixedUpdate에서 처리하여 물리 시뮬레이션과 동기화합니다.</para>
/// 
/// <para><b>비주얼 스핀</b>: meshTransform이 할당되면 별도의 축으로
/// 회전시켜 비행 중 시각적 효과를 더합니다(바위 굴러가는 느낌 등).</para>
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyMissile : MonoBehaviour, IDamageable
{
    private static readonly int HashColor = Shader.PropertyToID("_Color");

    [Header("Missile Settings")]
    [SerializeField] private int defaultDamage = 10;
    [SerializeField] private float defaultSpeed = 15f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool isHoming;
    [SerializeField] private float rotateSpeed = 5f;
    
    [Header("Hit Settings")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private MeshRenderer mainRenderer;

    [Header("Visual Spin")]
    [SerializeField] private Transform meshTransform;
    [SerializeField] private Vector3 spinAxis = Vector3.right;
    [SerializeField] private float spinSpeed;
    
    private int _damage;
    private float _speed;
    private Transform _target;
    private Rigidbody _rb;
    private float _deactivateTime;
    private int _currentHp;
    
    private Color _originalColor;
    private Coroutine _flashCoroutine;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
            
        if (mainRenderer != null && mainRenderer.material.HasProperty(HashColor))
            _originalColor = mainRenderer.material.color;
    }
    
    /// <summary>커스텀 데미지·속도로 초기화합니다.</summary>
    public void Initialize(int damage, float speed, Transform target, Vector3 initialDirection)
    {
        _damage = damage;
        _speed = speed;
        _target = target;
        _currentHp = maxHp;
        
        _rb.useGravity = false;
        _deactivateTime = Time.time + lifeTime;

        transform.rotation = Quaternion.LookRotation(initialDirection);
        _rb.linearVelocity = transform.forward * _speed;
        
        ResetColor();
    }
    
    /// <summary>기본 데미지·속도로 초기화합니다. 보스 공격에서 주로 사용됩니다.</summary>
    public void Initialize(Transform target, Vector3 initialDirection)
    {
        Initialize(defaultDamage, defaultSpeed, target, initialDirection);
    }

    private void FixedUpdate()
    {
        if (Time.time >= _deactivateTime)
        {
            Explode();
            return;
        }
        
        if (isHoming && _target != null && _target.gameObject.activeInHierarchy)
        {
            Vector3 targetPos = _target.position;
            targetPos.y = transform.position.y;

            Vector3 direction = (targetPos - transform.position).normalized;

            // RotateTowards로 부드러운 유도 회전 (급격한 방향 전환 방지)
            Vector3 newDirection = Vector3.RotateTowards(
                transform.forward, direction, rotateSpeed * Time.fixedDeltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
            
            _rb.linearVelocity = transform.forward * _speed;
        }
    }

    private void Update()
    {
        if (meshTransform != null && spinSpeed != 0f)
            meshTransform.Rotate(spinAxis * (spinSpeed * Time.deltaTime), Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return; 
        
        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(_damage);

        Explode();
    }

    /// <inheritdoc/>
    public void TakeDamage(int damage)
    {
        _currentHp -= damage;
        if (_currentHp > 0)
        {
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }
        else
        {
            Explode();
        }
    }
    
    private IEnumerator FlashRoutine()
    {
        if (mainRenderer != null)
        {
            mainRenderer.material.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            ResetColor();
        }
    }
    
    private void ResetColor()
    {
        if (mainRenderer != null)
            mainRenderer.material.color = _originalColor;
    }
    
    /// <summary>폭발 이펙트를 생성하고 풀에 반환합니다.</summary>
    private void Explode()
    {
        if (effectPrefab != null && ObjectPool.Instance != null)
            ObjectPool.Instance.Get(effectPrefab, transform.position, Quaternion.identity);
        
        ReturnToPool();
    }
    
    private void ReturnToPool()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }
        ResetColor();
        
        _rb.linearVelocity = Vector3.zero;
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Release(gameObject);
        else
            gameObject.SetActive(false);
    }
}