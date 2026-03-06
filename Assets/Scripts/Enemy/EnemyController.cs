using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 일반 적 AI를 제어하는 FSM 기반 컨트롤러.
/// Idle → Chase → (Dash →) Attack 상태를 거리 기반으로 전환합니다.
/// 
/// <para><b>설계 의도</b>: State 열거형 + switch문으로 간결한 FSM을 구현합니다.
/// 상태 전환 시 ChangeState()에서 진입/퇴장 로직을 일괄 처리하여
/// Update()의 복잡도를 줄이고 상태 전환 버그를 방지합니다.</para>
/// 
/// <para><b>대시 공격</b>: 일부 적은 useDash=true일 때 일정 거리에서
/// 순간적으로 가속하여 플레이어에게 돌진합니다.
/// acceleration을 극대화하여 즉각적인 가속 효과를 만듭니다.</para>
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Dash }

    private const float DashAcceleration = 2000f;
    private const float DeathReleaseDelay = 2f;
    private const float RagdollReleaseDelay = 5f;
    private const float RagdollUpwardForce = 3f;

    [Header("Melee Setup")]
    [SerializeField] private EnemyMeleeHitbox meleeHitbox;
    
    [Header("Ranged Setup")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform firePoint;
    
    private NavMeshAgent _agent;
    private EnemyStats _stats;
    private EnemyAnimator _enemyAnimator;
    private Transform _player;

    private State _currentState = State.Idle;
    private float _lastAttackTime;
    private float _attackLockUntil;
    private float _originalAcceleration;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _stats = GetComponent<EnemyStats>();
        _enemyAnimator = GetComponent<EnemyAnimator>();
        _originalAcceleration = _agent.acceleration;
    }

    private void OnEnable()
    {
        _currentState = State.Idle;
        _lastAttackTime = 0f;
        _attackLockUntil = 0f;
        
        DisableHitbox();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        if (_stats != null && _stats.Data != null && _agent != null)
        {
            _agent.speed = _stats.Data.MoveSpeed;
            _agent.acceleration = _originalAcceleration;
            _agent.enabled = true;
            _agent.isStopped = false;
            
            // 적끼리 겹치지 않도록 회피 우선순위를 랜덤 분배
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _agent.avoidancePriority = Random.Range(30, 70);
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

        float distance = Vector3.Distance(transform.position, _player.position);
        EnemyData data = _stats.Data;

        // 공격 잠금 중에는 상태 전환 금지
        if (Time.time < _attackLockUntil)
            return;

        // 거리 기반 상태 전환
        if (distance <= data.AttackRange)
            ChangeState(State.Attack);
        else if (data.UseDash && distance <= data.DashRange)
            ChangeState(State.Dash);
        else if (distance <= data.ChaseRange)
            ChangeState(State.Chase);
        else
            ChangeState(State.Idle);

        // 상태별 행동
        switch (_currentState)
        {
            case State.Idle:
                _agent.ResetPath();
                break;

            case State.Chase:
            case State.Dash:
                _agent.SetDestination(_player.position);
                break;

            case State.Attack:
                _agent.ResetPath();
                LookAtPlayer();
                TryAttack(data);
                break;
        }
    }

    /// <summary>
    /// 상태 전환을 처리합니다. 상태 진입/퇴장 시 필요한 파라미터 변경을 일괄 수행합니다.
    /// </summary>
    private void ChangeState(State newState)
    {
        if (_currentState == newState) return;
        
        EnemyData data = _stats.Data;

        // Dash 상태 퇴장 시 속도 복원
        if (_currentState == State.Dash && newState != State.Dash)
        {
            _agent.speed = data.MoveSpeed;
            _agent.acceleration = _originalAcceleration;
        }
        
        _currentState = newState;

        // Dash 상태 진입 시 가속도 극대화로 즉각 최고 속도 도달
        if (_currentState == State.Dash)
        {
            _agent.speed = data.DashSpeed;
            _agent.acceleration = DashAcceleration;
        }

        bool isMoving = (newState == State.Chase || newState == State.Dash);
        _enemyAnimator?.SetMoving(isMoving);
    }

    private void LookAtPlayer()
    {
        if (_player == null) return;
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.forward = dir;
    }

    private void TryAttack(EnemyData data)
    {
        if (Time.time - _lastAttackTime < data.AttackRate) return;
        _lastAttackTime = Time.time;
        _attackLockUntil = Time.time + data.AttackRate;
        _enemyAnimator?.TriggerAttack();
        
        if (data.AttackSound != null)
            SoundManager.Instance.PlaySfx(data.AttackSound);
    }

    /// <summary>근접 히트박스를 활성화합니다. 애니메이션 이벤트에서 호출됩니다.</summary>
    public void EnableHitbox()
    {
        if (meleeHitbox != null)
            meleeHitbox.Activate(_stats.Data.AttackPower);
    }

    /// <summary>근접 히트박스를 비활성화합니다. 애니메이션 이벤트에서 호출됩니다.</summary>
    public void DisableHitbox()
    {
        if (meleeHitbox != null)
            meleeHitbox.Deactivate();
    }
    
    /// <summary>원거리 투사체를 발사합니다. 애니메이션 이벤트에서 호출됩니다.</summary>
    public void Fire()
    {
        if (!_stats.Data.IsRanged || missilePrefab == null || firePoint == null || _player == null) return;

        var missileObj = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(missilePrefab, firePoint.position, transform.rotation)
            : Instantiate(missilePrefab, firePoint.position, transform.rotation);
        
        Vector3 targetPos = _player.position;
        targetPos.y = firePoint.position.y;
        Vector3 dir = (targetPos - firePoint.position).normalized;

        if (missileObj.TryGetComponent(out EnemyMissile enemyMissile))
            enemyMissile.Initialize(_player, dir);
    }
    
    /// <summary>
    /// 사망 처리. NavMeshAgent를 정지시키고 애니메이션을 재생한 뒤 풀에 반환합니다.
    /// </summary>
    /// <param name="willRagdoll">래그돌 물리를 적용할지 여부.</param>
    public void OnDeath(bool willRagdoll = false)
    {
        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        _currentState = State.Idle;
        _enemyAnimator?.SetMoving(false);

        DisableHitbox();
        
        if (!willRagdoll)
        {
            _enemyAnimator?.TriggerDeath();

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

    /// <summary>
    /// 수류탄 등 폭발에 의한 래그돌 물리를 적용합니다.
    /// NavMeshAgent를 비활성화하고 Rigidbody에 물리 힘을 가합니다.
    /// </summary>
    public void LaunchRagdoll(Vector3 explosionPos, float force)
    {
        if (_agent.isOnNavMesh && _agent.enabled)
        {
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
        }
        _agent.enabled = false;

        StopAllCoroutines();
        if (_stats != null && _stats.IsDead)
            StartCoroutine(ReleaseRoutine(RagdollReleaseDelay));

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false;

        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.None;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 reactVec = (transform.position - explosionPos).normalized;
            reactVec += Vector3.up * RagdollUpwardForce;

            rb.AddForce(reactVec * force, ForceMode.Impulse);
            rb.AddTorque(reactVec * force * RagdollUpwardForce, ForceMode.Impulse);
        }
    }
}
