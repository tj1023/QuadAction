using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dodgeAction;
    [SerializeField] private InputActionReference interactAction;
    
    [Header("Movement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 10f;
    
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDuration = 0.575f;
    [SerializeField] private float dodgeCooldown = 1.2f;
    [SerializeField] private float dodgeSpeedMultiplier = 2.5f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private Camera _mainCamera;
    private PlayerInteraction _playerInteraction;
    
    private float _nextDodgeTime;
    private float _dodgeTimer;
    private bool _isDodging;
    private Vector3 _dodgeDirection; 

    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _dodgeHash = Animator.StringToHash("DoDodge");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _mainCamera = Camera.main;
        _playerInteraction = GetComponent<PlayerInteraction>();
        
        _agent.speed = speed;
        _agent.acceleration = 200f;
        _agent.angularSpeed = 1000f;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        moveAction.action.performed += OnClickMove;

        dodgeAction.action.Enable();
        dodgeAction.action.performed += OnDodge;
        
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
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
            // 목표 지점까지 걷게 하는 대신, 지정된 방향으로 매 프레임 강제 이동시킵니다.
            float currentDodgeSpeed = speed * dodgeSpeedMultiplier;
            _agent.Move(_dodgeDirection * (currentDodgeSpeed * Time.deltaTime));

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
            // 1. 회피 방향 = 마우스 방향
            _dodgeDirection = (mousePos - transform.position).normalized;
            _dodgeDirection.y = 0; // y축으로 파고드는 것을 방지
            
            // 2. 회피 방향으로 즉시 캐릭터를 회전
            if (_dodgeDirection != Vector3.zero)
                transform.forward = _dodgeDirection;

            // 3. 기존의 이동 명령과 남아있는 속도 초기화
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;

            StartDodge();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if(_playerInteraction == null) return;
        _playerInteraction.PickupClosestItem();
    }
    
    private void StartDodge()
    {
        _isDodging = true;
        _nextDodgeTime = Time.time + dodgeCooldown;
        _dodgeTimer = dodgeDuration;
        
        _animator.SetTrigger(_dodgeHash);
    }

    private void EndDodge()
    {
        _isDodging = false;
        
        // 회피가 끝난 직후 미끄러지지 않도록 정지
        _agent.ResetPath(); 
        _agent.velocity = Vector3.zero; 
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
        // _agent.velocity.magnitude는 Move() 메서드를 사용할 때 정확하지 않을 수 있으므로
        // dodge 중일 때는 강제로 스피드 값을 넣어 애니메이션이 멈추지 않도록 보완
        float currentSpeed = _isDodging ? (speed * dodgeSpeedMultiplier) : _agent.velocity.magnitude;
        _animator.SetFloat(_speedHash, currentSpeed);
    }
}