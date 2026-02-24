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
    [SerializeField] private InputActionReference swapAction;

    
    [Header("Movement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 10f;
    
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeCooldown = 1.2f;
    [SerializeField] private float dodgeSpeedMultiplier = 2.5f;

    private Camera _mainCamera;
    private NavMeshAgent _agent;
    private PlayerAnimator _animator;
    private PlayerInteraction _playerInteraction;
    private PlayerWeaponManager _weaponManager;
    
    private Vector3 _dodgeDirection;
    private float _nextDodgeTime;
    private float _dodgeTimer;
    private bool _isDodging;
    private bool _wasMoving;
    private Vector3 _queuedPos;
    private bool _hasQueuedMove;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<PlayerAnimator>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _weaponManager = GetComponent<PlayerWeaponManager>();
        
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
        
        swapAction.action.Enable();
        swapAction.action.performed += OnSwapWeapon;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        moveAction.action.performed -= OnClickMove;
        
        dodgeAction.action.Disable();
        dodgeAction.action.performed -= OnDodge;
        
        interactAction.action.Disable();
        interactAction.action.performed -= OnInteract;
        
        swapAction.action.Disable();
        swapAction.action.performed -= OnSwapWeapon;
    }

    private void Update()
    {
        if (_isDodging)
        {
            // 마우스 방향으로 강제 이동
            float currentDodgeSpeed = speed * dodgeSpeedMultiplier;
            _agent.Move(_dodgeDirection * (currentDodgeSpeed * Time.deltaTime));
        }
        else
        {
            // 멈춤<->이동 상태가 변할 때 1번만 애니메이터 부름
            bool isMoving = _agent.velocity.magnitude > 0.1f;
            if (isMoving != _wasMoving)
            {
                _animator?.SetMoving(isMoving);
                _wasMoving = isMoving;
            }
        }
    }

    private void OnClickMove(InputAction.CallbackContext context)
    {
        if (GetMouseGroundPosition(out Vector3 targetPos))
        {
            // dodge 중이라면 예약 걸어둠
            if (_isDodging)
            {
                _hasQueuedMove = true;
                _queuedPos = targetPos;
            }
            else
            {
                _agent.SetDestination(targetPos);
            }
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
    
    private void OnSwapWeapon(InputAction.CallbackContext context)
    {
        if (_isDodging || _weaponManager == null) return;
        
        float scrollValue = context.ReadValue<Vector2>().y;
        if (scrollValue > 0) _weaponManager.SwapWeapon(1);
        else if (scrollValue < 0) _weaponManager.SwapWeapon(-1);
    }
    
    private void StartDodge()
    {
        _isDodging = true;
        _nextDodgeTime = Time.time + dodgeCooldown;
        _hasQueuedMove = false;

        if (_animator)
        {
            _animator.ResetTriggers();
            _animator.SetUpperBodyWeight(0f);
            _animator.TriggerDodge();
        }
    }

    public void EndDodge()
    {
        _isDodging = false; 
        
        _animator?.SetUpperBodyWeight(1f);
        
        if (_hasQueuedMove)
        {
            _agent.SetDestination(_queuedPos);
            _hasQueuedMove = false;
        }
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
}