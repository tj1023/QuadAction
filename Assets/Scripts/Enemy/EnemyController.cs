using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Dash }
    
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

        // 씬에서 플레이어 자동 탐색
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        if (_stats != null && _stats.Data != null && _agent != null)
        {
            _agent.speed = _stats.Data.moveSpeed;
            _agent.acceleration = _originalAcceleration;
            _agent.enabled = true;
            _agent.isStopped = false;
            
            // 적끼리 겹치지 않도록 회피 설정
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
        if (anim)
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

        // 상태 전환
        if (distance <= data.attackRange)
            ChangeState(State.Attack);
        else if (data.useDash && distance <= data.dashRange)
            ChangeState(State.Dash);
        else if (distance <= data.chaseRange)
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

    private void ChangeState(State newState)
    {
        if (_currentState == newState) return;
        
        EnemyData data = _stats.Data;

        // 이전 상태를 빠져나올 때의 처리 (Dash 해제 시 속도 원상복구)
        if (_currentState == State.Dash && newState != State.Dash)
        {
            _agent.speed = data.moveSpeed;
            _agent.acceleration = _originalAcceleration;
        }
        
        _currentState = newState;

        // 새로운 상태 진입 시의 처리 (Dash 진입 시 속도 증가)
        if (_currentState == State.Dash)
        {
            _agent.speed = data.dashSpeed;
            _agent.acceleration = 2000f; // 순간적으로 최고 속도에 도달하도록 가속도 대폭 증가
        }

        // Chase와 Dash 상태 모두 이동 애니메이션(뛰기)을 재생하도록 처리
        bool isMoving = (newState == State.Chase || newState == State.Dash);
        _enemyAnimator?.SetMoving(isMoving);
    }

    private void LookAtPlayer()
    {
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.forward = dir;
    }

    private void TryAttack(EnemyData data)
    {
        if (Time.time - _lastAttackTime < data.attackRate) return;
        _lastAttackTime = Time.time;
        _attackLockUntil = Time.time + data.attackRate;
        _enemyAnimator?.TriggerAttack();
    }

    public void EnableHitbox()
    {
        if (meleeHitbox)
            meleeHitbox.Activate(_stats.Data.attackPower);
    }

    public void DisableHitbox()
    {
        if (meleeHitbox)
            meleeHitbox.Deactivate();
    }
    
    public void Fire()
    {
        if (!_stats.Data.isRanged || missilePrefab == null || firePoint == null || _player == null) return;

        var missileObj = ObjectPool.Instance ? ObjectPool.Instance.Get(missilePrefab, firePoint.position, transform.rotation) : Instantiate(missilePrefab, firePoint.position, transform.rotation);
        
        Vector3 targetPos = _player.position;
        targetPos.y = firePoint.position.y;
        Vector3 dir = (targetPos - firePoint.position).normalized;

        if (missileObj.TryGetComponent(out EnemyMissile enemyMissile))
            enemyMissile.Initialize(_player, dir);
    }
    
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

        StartCoroutine(ReleaseRoutine(willRagdoll ? 5f : 2f));
    }

    private System.Collections.IEnumerator ReleaseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ObjectPool.Instance)
            ObjectPool.Instance.Release(gameObject);
        else
            Destroy(gameObject);
    }

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
            StartCoroutine(ReleaseRoutine(5f));

        Animator anim = GetComponentInChildren<Animator>();
        if (anim) anim.enabled = false;

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
            reactVec += Vector3.up * 3f;

            rb.AddForce(reactVec * force, ForceMode.Impulse);
            rb.AddTorque(reactVec * force * 3f, ForceMode.Impulse);
        }
    }
}
