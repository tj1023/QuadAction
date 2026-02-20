using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dodgeAction;
    
    [Header("Movement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 10f;
    
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDuration = 0.575f;
    [SerializeField] private float dodgeCooldown = 1.2f;
    [SerializeField] private float dodgeSpeedMultiplier = 2.5f; // 회피 시 좀 더 빠릿한 느낌을 위해 상향

    private NavMeshAgent _agent;
    private Animator _animator;
    private Camera _mainCamera;
    
    private float _nextDodgeTime;
    private float _dodgeTimer;
    private bool _isDodging;

    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _dodgeHash = Animator.StringToHash("DoDodge");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _mainCamera = Camera.main;
        
        _agent.speed = speed;
        _agent.acceleration = 200f; // 회피 시 즉각 가속을 위해 매우 높은 값 설정
        _agent.angularSpeed = 1000f; // 회피 방향으로 즉시 회전하도록 설정
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        moveAction.action.performed += OnClickMove;

        dodgeAction.action.Enable();
        dodgeAction.action.performed += OnDodge;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        dodgeAction.action.Disable();
    }

    private void Update()
    {
        UpdateAnimation();

        if (_isDodging)
        {
            _dodgeTimer -= Time.deltaTime;
            if (_dodgeTimer <= 0)
            {
                EndDodge();
            }
        }
    }

    private void OnClickMove(InputAction.CallbackContext context)
    {
        if (_isDodging) return; 

        if (GetMouseGroundPosition(out Vector3 targetPos))
        {
            _agent.SetDestination(targetPos);
        }
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (Time.time < _nextDodgeTime || _isDodging) return;

        if (GetMouseGroundPosition(out Vector3 mousePos))
        {
            // 1. 마우스 방향, dodge 거리 계산
            Vector3 dodgeDir = (mousePos - transform.position).normalized;
            float dodgeDistance = dodgeDuration *  dodgeSpeedMultiplier * _agent.speed;
            
            // 2. 해당 방향으로 즉시 회전 유도 및 목적지 강제 설정
            Vector3 dodgeTarget = transform.position + dodgeDir * dodgeDistance;
            
            // 3. NavMesh 위에서 유효한 위치인지 체크 후 이동
            _agent.SetDestination(
                NavMesh.SamplePosition(dodgeTarget, out NavMeshHit hit, dodgeDistance, NavMesh.AllAreas)
                    ? hit.position
                    : dodgeTarget);

            StartDodge();
        }
    }

    private void StartDodge()
    {
        _isDodging = true;
        _nextDodgeTime = Time.time + dodgeCooldown;
        _dodgeTimer = dodgeDuration;

        _agent.speed = speed * dodgeSpeedMultiplier;
        _animator.SetTrigger(_dodgeHash);
    }

    private void EndDodge()
    {
        _isDodging = false;
        _agent.speed = speed;
    }

    // 마우스 위치를 바닥 좌표로 변환하는 공통 메서드
    private bool GetMouseGroundPosition(out Vector3 position)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            position = hit.point;
            return true;
        }
        position = Vector3.zero;
        return false;
    }

    private void UpdateAnimation()
    {
        float currentSpeed = _agent.velocity.magnitude;
        _animator.SetFloat(_speedHash, currentSpeed);
    }
}