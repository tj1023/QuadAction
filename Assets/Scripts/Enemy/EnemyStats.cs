using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;
    [SerializeField] private float flashDuration = 0.15f;

    public EnemyData Data => data;
    public int CurrentHp { get; private set; }
    public bool IsDead { get; private set; }

    private Renderer[] _renderers;
    private Color[] _originalColors;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (data != null)
            CurrentHp = data.maxHp;

        // 렌더러 + 원본 색상 캐싱
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        CurrentHp -= damage;
        CurrentHp = Mathf.Max(CurrentHp, 0);

        if (CurrentHp <= 0)
            Die();
    }

    public void OnHit(int damage, Vector3 hitDirection, float knockbackForce)
    {
        TakeDamage(damage);

        if (!IsDead)
        {
            StartCoroutine(FlashRed());

            float effectiveForce = knockbackForce - data.knockbackResistance;
            if (effectiveForce > 0f)
                ApplyKnockback(hitDirection, effectiveForce);
        }
    }

    private IEnumerator FlashRed()
    {
        // 빨간색으로 변경
        foreach (var r in _renderers)
            r.material.color = Color.red;

        yield return new WaitForSeconds(flashDuration);

        // 원본 색상 복원
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
                _renderers[i].material.color = _originalColors[i];
        }
    }

    private void ApplyKnockback(Vector3 direction, float effectiveForce)
    {
        if (_agent == null || !_agent.enabled) return;

        direction.y = 0;
        Vector3 knockback = direction.normalized * effectiveForce;

        // NavMeshAgent 잠깐 끄고 위치 밀어준 뒤 다시 활성화
        StartCoroutine(KnockbackRoutine(knockback));
    }

    private IEnumerator KnockbackRoutine(Vector3 knockback)
    {
        _agent.isStopped = true;

        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            _agent.Move(knockback * (Time.deltaTime / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        _agent.isStopped = false;
    }

    private void Die()
    {
        IsDead = true;
        StopAllCoroutines();

        // 사망 시 회색으로 변경
        foreach (var r in _renderers)
        {
            if (r != null)
                r.material.color = Color.gray;
        }

        if (TryGetComponent(out EnemyController controller))
            controller.OnDeath();
    }
}
