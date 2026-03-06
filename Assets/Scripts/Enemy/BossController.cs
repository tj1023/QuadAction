using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 보스 적 AI 컨트롤러.
/// 미사일·바위·점프 3가지 패턴을 거리 기반으로 선택합니다.
/// 
/// <para><b>공격 선택 로직</b>:
/// - 원거리(attackRange 이내, meleeAttackRange 밖) → 미사일 또는 바위 (50:50)
/// - 그 외(너무 멀거나 매우 가까움) → 점프로 접근 + 공격</para>
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class BossController : MonoBehaviour
{
    private enum State { Idle, Attack }
    private enum AttackType { Missile = 0, Rock = 1, MeleeJump = 2 }

    private const float RangedAttackProbability = 0.5f;
    private const float DeathReleaseDelay = 3f;
    private const float RagdollReleaseDelay = 5f;

    [Header("Melee Setup")]
    [SerializeField] private EnemyMeleeHitbox meleeHitbox;

    [Header("Missile Setup")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;

    [Header("Rock Setup")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform firePointMouth;

    private NavMeshAgent _agent;
    private EnemyStats _stats;
    private BossAnimator _bossAnimator;
    private Transform _player;

    private State _currentState = State.Idle;
    private float _lastAttackTime;
    private bool _isAttacking;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _stats = GetComponent<EnemyStats>();
        _bossAnimator = GetComponent<BossAnimator>();
    }

    private void OnEnable()
    {
        _currentState = State.Idle;
        _lastAttackTime = 0f;
        _isAttacking = false;

        DisableMeleeHitbox();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        if (_stats != null && _stats.Data != null && _agent != null)
        {
            _agent.speed = _stats.Data.MoveSpeed;
            _agent.enabled = true;
            _agent.isStopped = false;
            _agent.updateRotation = true;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.enabled = true;
            anim.Rebind();
            anim.Update(0f);
        }
    }

    private void Update()
    {
        if (_stats.IsDead || _player == null) return;
        
        if (_isAttacking) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        EnemyData data = _stats.Data;
        
        ChangeState(distance <= data.ChaseRange ? State.Attack : State.Idle);
        
        switch (_currentState)
        {
            case State.Idle:
                _agent.ResetPath();
                break;

            case State.Attack:
                _agent.ResetPath();
                LookAtPlayer();
                TryAttack(data, distance);
                break;
        }
    }

    private void ChangeState(State newState)
    {
        _currentState = newState;
    }

    private void LookAtPlayer()
    {
        if (_player == null) return;
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.forward = dir;
    }

    private void TryAttack(EnemyData data, float distance)
    {
        if (Time.time - _lastAttackTime < data.AttackRate) return;
        _lastAttackTime = Time.time;
        _isAttacking = true;

        AttackType selectedAttack = SelectAttack(data, distance);

        // 점프 공격 시 NavMeshAgent로 플레이어 위치까지 이동
        if (selectedAttack == AttackType.MeleeJump)
        {
            LookAtPlayer();
            _agent.updateRotation = false;
            _agent.speed = _stats.Data.MoveSpeed;
            _agent.SetDestination(_player.position);
                        
            if (data.BossJumpAttackSound != null)
                SoundManager.Instance.PlaySfx(data.BossJumpAttackSound);
        }
        else if (selectedAttack == AttackType.Missile && data.BossMissileAttackSound != null)
        {
            SoundManager.Instance.PlaySfx(data.BossMissileAttackSound);
        }
        else if (selectedAttack == AttackType.Rock && data.BossRockAttackSound != null)
        {
            SoundManager.Instance.PlaySfx(data.BossRockAttackSound);
        }
        else if (data.AttackSound != null)
        {
            SoundManager.Instance.PlaySfx(data.AttackSound);
        }

        _bossAnimator?.SetAttackIndex((int)selectedAttack);
        _bossAnimator?.TriggerAttack();
    }

    /// <summary>
    /// 거리 기반 공격 패턴 선택.
    /// 원거리 범위 내에서는 미사일/바위를 랜덤 선택하고,
    /// 그 외에는 점프 공격으로 접근합니다.
    /// </summary>
    private AttackType SelectAttack(EnemyData data, float distance)
    {
        if (distance <= data.AttackRange && distance > data.MeleeAttackRange)
            return Random.value < RangedAttackProbability ? AttackType.Missile : AttackType.Rock;

        return AttackType.MeleeJump;
    }
    
    /// <summary>미사일 투사체를 발사합니다. 애니메이션 이벤트에서 side(0=Left, 1=Right)로 호출됩니다.</summary>
    public void FireMissile(int side)
    {
        if (missilePrefab == null || _player == null) return;

        Transform firePoint = side == 0 ? firePointLeft : firePointRight;
        if (firePoint == null) return;

        var missileObj = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(missilePrefab, firePoint.position, transform.rotation)
            : Instantiate(missilePrefab, firePoint.position, transform.rotation);

        Vector3 targetPos = _player.position;
        targetPos.y = firePoint.position.y;
        Vector3 dir = (targetPos - firePoint.position).normalized;

        if (missileObj.TryGetComponent(out EnemyMissile enemyMissile))
            enemyMissile.Initialize(_player, dir);
    }
    
    /// <summary>바위 투사체를 발사합니다. 애니메이션 이벤트에서 호출됩니다.</summary>
    public void FireRock()
    {
        if (rockPrefab == null || firePointMouth == null || _player == null) return;

        var rockObj = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(rockPrefab, firePointMouth.position, transform.rotation)
            : Instantiate(rockPrefab, firePointMouth.position, transform.rotation);

        Vector3 targetPos = _player.position;
        targetPos.y = firePointMouth.position.y;
        Vector3 dir = (targetPos - firePointMouth.position).normalized;

        if (rockObj.TryGetComponent(out EnemyMissile missile))
            missile.Initialize(_player, dir);
    }
    
    /// <summary>근접 히트박스를 활성화합니다. 애니메이션 이벤트에서 호출됩니다.</summary>
    public void EnableMeleeHitbox()
    {
        _agent.ResetPath();
        _agent.speed = _stats.Data.MoveSpeed;

        if (meleeHitbox != null)
            meleeHitbox.Activate(_stats.Data.AttackPower);
    }

    /// <summary>근접 히트박스를 비활성화합니다.</summary>
    public void DisableMeleeHitbox()
    {
        if (meleeHitbox != null)
            meleeHitbox.Deactivate();
    }
    
    /// <summary>공격 애니메이션 종료 시 호출되어 공격 상태를 리셋합니다.</summary>
    public void OnAttackAnimationEnd()
    {
        ResetAttackState();
    }

    private void ResetAttackState()
    {
        _isAttacking = false;
        DisableMeleeHitbox();
        _agent.ResetPath();
        _agent.speed = _stats.Data.MoveSpeed;
        _agent.updateRotation = true;
        LookAtPlayer();
    }
    
    /// <summary>사망 처리. NavMeshAgent를 멈추고 사망 애니메이션 후 풀에 반환합니다.</summary>
    public void OnDeath(bool willRagdoll = false)
    {
        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        _currentState = State.Idle;
        _isAttacking = false;

        StopAllCoroutines();
        DisableMeleeHitbox();

        if (!willRagdoll)
        {
            _bossAnimator?.TriggerDeath();

            if (TryGetComponent(out Collider col))
                col.enabled = false;
        }

        StartCoroutine(ReleaseRoutine(willRagdoll ? RagdollReleaseDelay : DeathReleaseDelay));
    }

    private System.Collections.IEnumerator ReleaseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Release(gameObject);
        else
            Destroy(gameObject);
    }
}
