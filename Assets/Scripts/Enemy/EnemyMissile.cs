using System.Collections;
using UnityEngine;

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
    
    public void Initialize(int damage, float speed, Transform target, Vector3 initialDirection)
    {
        _damage = damage;
        _speed = speed;
        _target = target;
        _currentHp = maxHp;
        
        _rb.useGravity = false;
        _deactivateTime = Time.time + lifeTime;

        // 발사 직후의 초기 방향 및 속도 설정
        transform.rotation = Quaternion.LookRotation(initialDirection);
        _rb.linearVelocity = transform.forward * _speed;
        
        ResetColor();
    }
    
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
        
        if (isHoming && _target && _target.gameObject.activeInHierarchy)
        {
            Vector3 targetPos = _target.position;
            targetPos.y = transform.position.y;

            Vector3 direction = (targetPos - transform.position).normalized;

            // 부드러운 회전 처리
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotateSpeed * Time.fixedDeltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
            
            _rb.linearVelocity = transform.forward * _speed;
        }
    }

    private void Update()
    {
        if (meshTransform && spinSpeed != 0f)
            meshTransform.Rotate(spinAxis * (spinSpeed * Time.deltaTime), Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return; 
        
        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(_damage);

        Explode();
    }

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
        if (mainRenderer)
        {
            mainRenderer.material.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            ResetColor();
        }
    }
    
    private void ResetColor()
    {
        if (mainRenderer)
            mainRenderer.material.color = _originalColor;
    }
    
    private void Explode()
    {
        if (effectPrefab && ObjectPool.Instance)
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
        if (ObjectPool.Instance)
            ObjectPool.Instance.Release(gameObject);
        else
            gameObject.SetActive(false);
    }
}