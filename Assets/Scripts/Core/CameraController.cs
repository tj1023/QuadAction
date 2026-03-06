using UnityEngine;

/// <summary>
/// 쿼터뷰 카메라 컨트롤러.
/// 플레이어 추적 + 마우스 리드 + 카메라 흔들림을 지원합니다.
/// 
/// <para><b>마우스 리드</b>: 마우스 커서 방향으로 카메라를 약간 치우쳐서
/// 플레이어가 조준하는 방향의 시야를 넓혀줍니다.
/// 쿼터뷰 액션 게임에서 전방 가시성을 향상시킵니다.</para>
/// 
/// <para><b>카메라 흔들림</b>: EventManager.OnPlayerHit 이벤트를 구독하여
/// 피격 시 화면 흔들림으로 타격감을 강화합니다.</para>
/// </summary>
public class CameraController : MonoBehaviour
{
    private const float MaxRayDistance = 200f;

    [Header("Target & Offset")]
    [SerializeField] private Transform target;
    [Tooltip("플레이어와 카메라 사이의 기본 간격입니다.")]
    [SerializeField] private Vector3 offset = new Vector3(20f, 30f, 20f);
    
    [Header("Camera Settings")]
    [Tooltip("카메라가 따라가는 속도입니다. 높을수록 즉각적으로 따라갑니다.")]
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Mouse Lead")]
    [Tooltip("마우스 방향으로 카메라가 치우치는 정도 (0 = 없음, 1 = 마우스까지 전부)")]
    [SerializeField][Range(0f, 1f)] private float leadFactor = 0.3f;
    [Tooltip("최대 치우침 거리")]
    [SerializeField] private float maxLeadDistance = 8f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Camera Shake")]
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 0.2f;

    private Camera _cam;
    private float _shakeTimer;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        EventManager.OnPlayerHit += Shake;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerHit -= Shake;
    }

    /// <summary>
    /// LateUpdate에서 카메라 위치를 갱신하여 모든 캐릭터 이동이 끝난 후 촬영합니다.
    /// Lerp 기반 부드러운 추적 → 흔들림 오프셋 순서로 적용합니다.
    /// </summary>
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 leadOffset = GetMouseLeadOffset();
        Vector3 desiredPosition = target.position + offset + leadOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 카메라 흔들림 (Lerp 이후 직접 적용하여 원래 위치를 교란)
        if (_shakeTimer > 0f)
        {
            float ratio = _shakeTimer / shakeDuration;
            Vector3 shakeOffset = Random.insideUnitSphere * (shakeIntensity * ratio);
            shakeOffset.y = 0;
            transform.position += shakeOffset;
            _shakeTimer -= Time.deltaTime;
        }
    }

    /// <summary>카메라 흔들림을 시작합니다. EventManager.OnPlayerHit에 바인딩됩니다.</summary>
    public void Shake()
    {
        _shakeTimer = shakeDuration;
    }

    /// <summary>
    /// 마우스 커서가 가리키는 바닥 좌표를 기준으로 리드 오프셋을 계산합니다.
    /// maxLeadDistance로 최대 치우침을 제한하여 화면이 과도하게 밀리지 않도록 합니다.
    /// </summary>
    private Vector3 GetMouseLeadOffset()
    {
        if (_cam == null) return Vector3.zero;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance, groundLayer))
        {
            Vector3 toMouse = hit.point - target.position;
            toMouse.y = 0;

            if (toMouse.magnitude > maxLeadDistance)
                toMouse = toMouse.normalized * maxLeadDistance;

            return toMouse * leadFactor;
        }

        return Vector3.zero;
    }
}