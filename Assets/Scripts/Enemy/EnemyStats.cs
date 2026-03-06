using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적의 HP·피격·사망·코인 드롭을 관리하는 컴포넌트.
/// IDamageable 인터페이스를 구현하여 총알·미사일 등 다양한 소스에서 데미지를 받습니다.
/// 
/// <para><b>데미지 처리 흐름</b>:
/// 단순 데미지 → <see cref="TakeDamage"/> (IDamageable, 넉백 없음)
/// 넉백·래그돌 → <see cref="OnHit"/> (총알/근접 무기에서 호출)
/// 양쪽 모두 내부적으로 <see cref="ApplyDamage"/>를 호출하여 중복 로직을 방지합니다.</para>
/// </summary>
public class EnemyStats : MonoBehaviour, IDamageable
{
    private const float CoinDropHeight = 1.5f;
    private const float BonusCoinOffsetRange = 0.5f;

    [SerializeField] private EnemyData data;
    [SerializeField] private float flashDuration = 0.15f;

    /// <summary>현재 할당된 적 데이터(ScriptableObject).</summary>
    public EnemyData Data => data;

    /// <summary>사망 상태 여부.</summary>
    public bool IsDead { get; private set; }

    private Renderer[] _renderers;
    private Color[] _originalColors;
    private NavMeshAgent _agent;
    private int _currentHp;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        // 렌더러 + 원본 색상 캐싱 (피격 플래시에 사용)
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }

    private void OnEnable()
    {
        IsDead = false;
        StopAllCoroutines();

        if (data != null)
        {
            _currentHp = data.MaxHp;

            if (data.IsBoss)
                EventManager.OnBossAppeared?.Invoke(data.MaxHp);
        }

        // 원본 색상 복원 (풀에서 재사용될 때를 대비)
        RestoreOriginalColors();
    }

    /// <summary>
    /// IDamageable 구현. 넉백 없이 순수 데미지만 적용합니다.
    /// EnemyMissile 등 투사체가 파괴될 때 사용됩니다.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        ApplyDamage(damage);

        if (_currentHp <= 0)
            Die();
    }

    /// <summary>
    /// 넉백·래그돌을 포함한 피격 처리.
    /// 총알(Bullet)이나 근접 무기(MeleeHitbox)에서 호출됩니다.
    /// </summary>
    /// <param name="damage">적용할 데미지.</param>
    /// <param name="hitDirection">넉백 방향.</param>
    /// <param name="knockbackForce">넉백 힘.</param>
    /// <param name="willRagdoll">사망 시 래그돌 적용 여부(수류탄 등).</param>
    public void OnHit(int damage, Vector3 hitDirection, float knockbackForce, bool willRagdoll = false)
    {
        if (IsDead) return;

        ApplyDamage(damage);

        if (_currentHp <= 0)
        {
            Die(willRagdoll);
            return;
        }

        StartCoroutine(FlashRed());

        float effectiveForce = knockbackForce - data.KnockbackResistance;
        if (effectiveForce > 0f)
            ApplyKnockback(hitDirection, effectiveForce);
    }

    /// <summary>
    /// HP 감소, 보스 HP 이벤트, 히트 사운드를 통합 처리합니다.
    /// TakeDamage와 OnHit 양쪽에서 호출되어 중복 로직을 제거합니다.
    /// </summary>
    private void ApplyDamage(int damage)
    {
        _currentHp -= damage;
        _currentHp = Mathf.Max(_currentHp, 0);

        if (data != null && data.IsBoss)
            EventManager.OnBossHpChanged?.Invoke(_currentHp, data.MaxHp);

        if (data != null && data.HitSound != null)
            SoundManager.Instance.PlaySfx(data.HitSound);
    }

    private IEnumerator FlashRed()
    {
        foreach (var r in _renderers)
        {
            if (r != null) r.material.color = Color.red;
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreOriginalColors();
    }

    /// <summary>원본 머티리얼 색상을 복원합니다.</summary>
    private void RestoreOriginalColors()
    {
        if (_renderers == null || _originalColors == null) return;

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

    private void Die(bool willRagdoll = false)
    {
        IsDead = true;
        StopAllCoroutines();

        EventManager.OnEnemyDied?.Invoke();

        if (data != null && data.IsBoss)
            EventManager.OnBossDied?.Invoke();

        DropCoin();

        foreach (var r in _renderers)
        {
            if (r != null)
                r.material.color = Color.gray;
        }

        if (TryGetComponent(out EnemyController controller))
            controller.OnDeath(willRagdoll);
        else if (TryGetComponent(out BossController bossController))
            bossController.OnDeath(willRagdoll);
    }

    /// <summary>
    /// 코인 드롭. Object Pool을 활용하여 GC 부담을 줄입니다.
    /// 보너스 코인은 난이도(difficulty) × bonusCoinChance 확률로 추가 드롭됩니다.
    /// </summary>
    private void DropCoin()
    {
        if (data == null || data.CoinPrefab == null) return;

        Vector3 dropPos = transform.position + Vector3.up * CoinDropHeight;

        SpawnCoin(dropPos);

        // 난이도 × bonusCoinChance 확률로 추가 코인 드롭
        float bonusRoll = Random.Range(0f, 1f);
        if (bonusRoll < data.Difficulty * data.BonusCoinChance)
        {
            Vector3 offset = new Vector3(
                Random.Range(-BonusCoinOffsetRange, BonusCoinOffsetRange),
                0f,
                Random.Range(-BonusCoinOffsetRange, BonusCoinOffsetRange));
            SpawnCoin(dropPos + offset);
        }
    }

    /// <summary>코인을 Object Pool에서 꺼내거나 새로 생성합니다.</summary>
    private void SpawnCoin(Vector3 position)
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Get(data.CoinPrefab, position, Quaternion.identity);
        else
            Instantiate(data.CoinPrefab, position, Quaternion.identity);
    }
}
