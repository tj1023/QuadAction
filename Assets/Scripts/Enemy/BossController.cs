using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class BossController : MonoBehaviour
{
    private enum State { Idle, Attack }
    private enum AttackType { Missile = 0, Rock = 1, MeleeJump = 2 }

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
            _agent.speed = _stats.Data.moveSpeed;
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
        
        if (_isAttacking) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        EnemyData data = _stats.Data;
        
        ChangeState(distance <= data.chaseRange ? State.Attack : State.Idle);
        
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
        if (Time.time - _lastAttackTime < data.attackRate) return;
        _lastAttackTime = Time.time;
        _isAttacking = true;

        AttackType selectedAttack = SelectAttack(data, distance);

        Debug.Log($"[Boss] Attack: {selectedAttack}, Distance: {distance:F1}");

        // 점프 공격
        if (selectedAttack == AttackType.MeleeJump)
        {
            LookAtPlayer();
            _agent.updateRotation = false;
            _agent.speed = _stats.Data.moveSpeed;
            _agent.SetDestination(_player.position);
                        
            if (data.bossJumpAttackSound)
                SoundManager.Instance.PlaySfx(data.bossJumpAttackSound);
        }
        else if (selectedAttack == AttackType.Missile && data.bossMissileAttackSound)
        {
            SoundManager.Instance.PlaySfx(data.bossMissileAttackSound);
        }
        else if (selectedAttack == AttackType.Rock && data.bossRockAttackSound)
        {
            SoundManager.Instance.PlaySfx(data.bossRockAttackSound);
        }
        else if (data.attackSound)
        {
            SoundManager.Instance.PlaySfx(data.attackSound);
        }

        _bossAnimator?.SetAttackIndex((int)selectedAttack);
        _bossAnimator?.TriggerAttack();
    }

    private AttackType SelectAttack(EnemyData data, float distance)
    {
        // 원거리 공격 범위 안이면 미사일 or 바위
        if (distance <= data.attackRange && distance > data.meleeAttackRange)
            return Random.value < 0.5f ? AttackType.Missile : AttackType.Rock;

        // 그 외(너무 멀거나 매우 가까우면) → 점프 공격으로 접근 + 공격
        return AttackType.MeleeJump;
    }
    
    public void FireMissile(int side)
    {
        if (missilePrefab == null || _player == null) return;

        Transform firePoint = side == 0 ? firePointLeft : firePointRight;
        if (firePoint == null) return;

        var missileObj = ObjectPool.Instance
            ? ObjectPool.Instance.Get(missilePrefab, firePoint.position, transform.rotation)
            : Instantiate(missilePrefab, firePoint.position, transform.rotation);

        Vector3 targetPos = _player.position;
        targetPos.y = firePoint.position.y;
        Vector3 dir = (targetPos - firePoint.position).normalized;

        if (missileObj.TryGetComponent(out EnemyMissile enemyMissile))
            enemyMissile.Initialize(_player, dir);
    }
    
    public void FireRock()
    {
        if (rockPrefab == null || firePointMouth == null || _player == null) return;

        var rockObj = ObjectPool.Instance
            ? ObjectPool.Instance.Get(rockPrefab, firePointMouth.position, transform.rotation)
            : Instantiate(rockPrefab, firePointMouth.position, transform.rotation);

        Vector3 targetPos = _player.position;
        targetPos.y = firePointMouth.position.y;
        Vector3 dir = (targetPos - firePointMouth.position).normalized;

        if (rockObj.TryGetComponent(out EnemyMissile missile))
            missile.Initialize(_player, dir);
    }
    
    public void EnableMeleeHitbox()
    {
        _agent.ResetPath();
        _agent.speed = _stats.Data.moveSpeed;

        if (meleeHitbox)
            meleeHitbox.Activate(_stats.Data.attackPower);
    }

    public void DisableMeleeHitbox()
    {
        if (meleeHitbox)
            meleeHitbox.Deactivate();
    }
    
    public void OnAttackAnimationEnd()
    {
        ResetAttackState();
    }

    private void ResetAttackState()
    {
        _isAttacking = false;
        DisableMeleeHitbox();
        _agent.ResetPath();
        _agent.speed = _stats.Data.moveSpeed;
        _agent.updateRotation = true;
        LookAtPlayer();
    }
    
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

        StartCoroutine(ReleaseRoutine(willRagdoll ? 5f : 3f));
    }

    private System.Collections.IEnumerator ReleaseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ObjectPool.Instance)
            ObjectPool.Instance.Release(gameObject);
        else
            Destroy(gameObject);
    }
}
