using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    private enum State { Idle, Chase, Attack }

    private NavMeshAgent _agent;
    private EnemyStats _stats;
    private EnemyAnimator _animator;
    private Transform _player;

    private State _currentState = State.Idle;
    private float _lastAttackTime;
    private float _attackLockUntil;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _stats = GetComponent<EnemyStats>();
        _animator = GetComponent<EnemyAnimator>();
    }

    private void Start()
    {
        // 씬에서 플레이어 자동 탐색
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        if (_stats.Data != null)
        {
            _agent.speed = _stats.Data.moveSpeed;

            // 적끼리 겹치지 않도록 회피 설정
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _agent.avoidancePriority = Random.Range(30, 70);
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
        _currentState = newState;

        _animator?.SetMoving(newState == State.Chase);
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
        _animator?.TriggerAttack();

        // 플레이어에게 데미지
        if (_player.TryGetComponent(out IDamageable target))
            target.TakeDamage(data.attackPower);
    }

    public void OnDeath()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        _currentState = State.Idle;
        _animator?.SetMoving(false);
        _animator?.TriggerDeath();

        if (TryGetComponent(out Collider col))
            col.enabled = false;

        Destroy(gameObject, 2f);
    }

    public void LaunchRagdoll(Vector3 explosionPos, float force)
    {
        // NavMeshAgent 잔여 속도 제거 후 비활성화
        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
        _agent.enabled = false;

        // 진행 중인 넉백 코루틴 중단
        StopAllCoroutines();

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
