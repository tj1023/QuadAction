using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 이동·회피·공격·상호작용 입력을 처리하는 메인 컨트롤러.
/// Unity Input System의 InputActionReference를 사용하여 입력을 바인딩합니다.
/// 
/// <para><b>설계 의도</b>: 입력 처리(이 클래스) → 무기 로직(PlayerWeaponManager) →
/// 애니메이션(PlayerAnimator)으로 책임을 분리하여 각 컴포넌트가 독립적으로 변경 가능합니다.
/// 이벤트 구독/해제를 OnEnable/OnDisable에서 엄격하게 관리하여 메모리 누수를 방지합니다.</para>
/// 
/// <para><b>회피(Dodge)</b>: WASD 입력 방향으로 순간 이속 증가 이동을 수행하며,
/// 쿨타임 기반으로 제한됩니다. 회피 중 상체 애니메이션 레이어를 비활성화하여
/// 하체 회피 모션만 재생되도록 합니다.</para>
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshObstacle))]
public class PlayerController : MonoBehaviour
{
    private const float GroundingForce = -2f;
    private const float InputDeadzone = 0.01f;
    private const float MouseRayMaxDistance = 100f;
    private const float FootstepVolumeScale = 0.4f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dodgeAction;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference swapAction;
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private InputActionReference reLoadAction;

    [Header("Weapon Slots")]
    [SerializeField] private InputActionReference equipSlot1Action;
    [SerializeField] private InputActionReference equipSlot2Action;
    [SerializeField] private InputActionReference equipSlot3Action;

    [Header("Movement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeCooldown = 1.2f;
    [SerializeField] private float dodgeSpeedMultiplier = 2.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.4f;
    [SerializeField] private AudioClip dodgeSound;

    private Camera _mainCamera;
    private CharacterController _controller;
    private PlayerAnimator _animator;
    private PlayerInteraction _playerInteraction;
    private PlayerWeaponManager _weaponManager;
    
    private Vector3 _dodgeDirection;
    private float _nextDodgeTime;
    private bool _isDodging;
    private bool _wasMoving;
    private bool _isAttacking;
    private bool _isDead;
    private float _verticalVelocity;
    private bool _inputEnabled = true;
    private float _nextFootstepTime;

    /// <summary>
    /// 회피 쿨타임 진행률. 0 = 준비 완료, 1 = 방금 사용.
    /// UIDodge에서 쿨다운 게이지를 표시하는 데 사용됩니다.
    /// </summary>
    public float DodgeCooldownRatio => Mathf.Clamp01((_nextDodgeTime - Time.time) / dodgeCooldown);

    /// <summary>플레이어 입력을 활성화/비활성화합니다. 상점·설정 UI에서 사용됩니다.</summary>
    public void SetInputEnabled(bool inputEnabled) => _inputEnabled = inputEnabled;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<PlayerAnimator>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _weaponManager = GetComponent<PlayerWeaponManager>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();

        dodgeAction.action.Enable();
        dodgeAction.action.performed += OnDodge;
        
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
        
        swapAction.action.Enable();
        swapAction.action.performed += OnSwapWeapon;
        
        equipSlot1Action.action.Enable();
        equipSlot1Action.action.performed += OnEquipSlot1;
        
        equipSlot2Action.action.Enable();
        equipSlot2Action.action.performed += OnEquipSlot2;
        
        equipSlot3Action.action.Enable();
        equipSlot3Action.action.performed += OnEquipSlot3;

        fireAction.action.Enable();
        fireAction.action.performed += OnFire;
        fireAction.action.canceled += OnFireCanceled;
        
        reLoadAction.action.Enable();
        reLoadAction.action.performed += OnReLoad;

        EventManager.OnPlayerDeath += OnDeath;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        
        dodgeAction.action.Disable();
        dodgeAction.action.performed -= OnDodge;
        
        interactAction.action.Disable();
        interactAction.action.performed -= OnInteract;
        
        swapAction.action.Disable();
        swapAction.action.performed -= OnSwapWeapon;
        
        equipSlot1Action.action.Disable();
        equipSlot1Action.action.performed -= OnEquipSlot1;

        equipSlot2Action.action.Disable();
        equipSlot2Action.action.performed -= OnEquipSlot2;

        equipSlot3Action.action.Disable();
        equipSlot3Action.action.performed -= OnEquipSlot3;
        
        fireAction.action.Disable();
        fireAction.action.performed -= OnFire;
        fireAction.action.canceled -= OnFireCanceled;
        
        reLoadAction.action.Disable();
        reLoadAction.action.performed -= OnReLoad;

        EventManager.OnPlayerDeath -= OnDeath;
    }

    private void Update()
    {
        // 중력 처리 (사망 후에도 바닥 유지를 위해 항상 적용)
        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = GroundingForce;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        if (_isDead)
        {
            _controller.Move(new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));
            return;
        }

        if (!_inputEnabled)
        {
            _controller.Move(new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));
            return;
        }

        if (_isDodging)
        {
            float currentDodgeSpeed = speed * dodgeSpeedMultiplier;
            Vector3 dodgeMove = _dodgeDirection * (currentDodgeSpeed * Time.deltaTime);
            dodgeMove.y = _verticalVelocity * Time.deltaTime;
            _controller.Move(dodgeMove);
        }
        else
        {
            if (_animator != null && _animator.IsPlayingAttackAnimation())
            {
                // 공격 중에는 이동 멈춤, 중력만 적용
                Vector3 gravityOnly = new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f);
                _controller.Move(gravityOnly);

                // 공격 중에도 마우스 방향 회전 유지 (조준 보정)
                if (GetMouseGroundPosition(out Vector3 mousePos))
                {
                    Vector3 lookDir = (mousePos - transform.position).normalized;
                    lookDir.y = 0;
                    if (lookDir != Vector3.zero)
                        transform.forward = lookDir;
                }
            }
            else
            {
                // WASD 이동
                Vector2 input = moveAction.action.ReadValue<Vector2>();
                Vector3 moveDir = GetCameraRelativeDirection(input);

                Vector3 move = moveDir * (speed * Time.deltaTime);
                move.y = _verticalVelocity * Time.deltaTime;
                _controller.Move(move);

                if (moveDir != Vector3.zero)
                    transform.forward = moveDir;
            }

            // 연속 공격 로직은 이동 제한과 독립적으로 매 프레임 검사
            if (_isAttacking && _weaponManager != null && _weaponManager.CanAttackCurrentWeapon())
                StartAttack();

            // 이동↔정지 상태 변화 시에만 애니메이터 호출 (불필요한 SetBool 반복 방지)
            Vector2 currentInput = moveAction.action.ReadValue<Vector2>();
            bool isMoving = currentInput.sqrMagnitude > InputDeadzone
                            && !(_animator != null && _animator.IsPlayingAttackAnimation());
            if (isMoving != _wasMoving)
            {
                _animator?.SetMoving(isMoving);
                _wasMoving = isMoving;
            }

            // 발소리 재생
            if (isMoving && _controller.isGrounded && Time.time >= _nextFootstepTime)
            {
                PlayFootstepSound();
                _nextFootstepTime = Time.time + footstepInterval;
            }
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        SoundManager.Instance.PlaySfx(clip, FootstepVolumeScale);
    }

    /// <summary>
    /// 카메라 기준 입력 방향을 월드 좌표로 변환합니다.
    /// 쿼터뷰 카메라의 Forward/Right를 수평면에 투영하여 사용합니다.
    /// </summary>
    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude < InputDeadzone) return Vector3.zero;

        Transform camTransform = _mainCamera.transform;
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * input.y + camRight * input.x).normalized;
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (!_inputEnabled) return;
        if (Time.time < _nextDodgeTime || _isDodging) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        _dodgeDirection = input.sqrMagnitude > InputDeadzone
            ? GetCameraRelativeDirection(input)
            : transform.forward;
        _dodgeDirection.y = 0;

        if (_dodgeDirection != Vector3.zero)
            transform.forward = _dodgeDirection;

        StartDodge();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!_inputEnabled) return;
        _playerInteraction?.PickupClosestItem();
    }
    
    private void OnSwapWeapon(InputAction.CallbackContext context)
    {
        if (!_inputEnabled) return;
        if (_isDodging || _weaponManager == null) return;
        CancelAttack();
        
        float scrollValue = context.ReadValue<Vector2>().y;
        if (scrollValue > 0) _weaponManager.SwapWeapon(1);
        else if (scrollValue < 0) _weaponManager.SwapWeapon(-1);
    }
    
    private void OnEquipSlot1(InputAction.CallbackContext context) => EquipSlot(0);
    private void OnEquipSlot2(InputAction.CallbackContext context) => EquipSlot(1);
    private void OnEquipSlot3(InputAction.CallbackContext context) => EquipSlot(2);

    private void OnFire(InputAction.CallbackContext context)
    {
        if (!_inputEnabled) return;
        _isAttacking = true;
        
        if (_isDodging) return;
        
        if (_weaponManager != null && _weaponManager.CanAttackCurrentWeapon())
            StartAttack();
    }
    
    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        _isAttacking = false;
    }
    
    private void OnReLoad(InputAction.CallbackContext context)
    {
        if (!_inputEnabled) return;
        CancelAttack();
        _weaponManager?.TryReload();
    }
    
    /// <summary>마우스 위치를 바닥 좌표로 변환합니다.</summary>
    private bool GetMouseGroundPosition(out Vector3 position)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, MouseRayMaxDistance, groundLayer))
        {
            position = hit.point;
            return true;
        }
        position = Vector3.zero;
        return false;
    }

    private void StartDodge()
    {
        _isDodging = true;
        _nextDodgeTime = Time.time + dodgeCooldown;
        CancelAttack();

        if (dodgeSound != null)
            SoundManager.Instance.PlaySfx(dodgeSound);

        if (_animator != null)
        {
            _animator.ResetSwapTrigger();
            _animator.CancelReloadAnimation();
            _animator.SetUpperBodyWeight(0f);
            _animator.TriggerDodge();
        }
    }

    /// <summary>회피 종료. AnimationEventHandler에서 호출됩니다.</summary>
    public void EndDodge()
    {
        _isDodging = false; 
        _animator?.SetUpperBodyWeight(1f);
    }

    private void EquipSlot(int index)
    {
        if (!_inputEnabled || _isDodging || _weaponManager == null) return;
        CancelAttack();
        _weaponManager.EquipWeaponByIndex(index);
    }

    private void StartAttack()
    {
        // 마우스 방향으로 먼저 회전 (총알이 올바른 방향으로 나가도록)
        if (GetMouseGroundPosition(out Vector3 mousePos))
        {
            Vector3 lookDir = (mousePos - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.forward = lookDir;
        }

        _weaponManager.TryAttack();
    }

    private void CancelAttack()
    {
        _isAttacking = false;
        _weaponManager.DisableMeleeHitbox(); 
    }

    private void OnDeath()
    {
        _isDead = true;
        _isAttacking = false;
        _isDodging = false;
        _inputEnabled = false;
        _animator?.SetMoving(false);
    }
}