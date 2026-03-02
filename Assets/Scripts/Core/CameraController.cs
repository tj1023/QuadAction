using UnityEngine;

public class CameraController : MonoBehaviour
{
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

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 leadOffset = GetMouseLeadOffset();
        Vector3 desiredPosition = target.position + offset + leadOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 카메라 흔들림 (Lerp 이후 직접 적용)
        if (_shakeTimer > 0f)
        {
            float ratio = _shakeTimer / shakeDuration;
            Vector3 shakeOffset = Random.insideUnitSphere * (shakeIntensity * ratio);
            shakeOffset.y = 0;
            transform.position += shakeOffset;
            _shakeTimer -= Time.deltaTime;
        }
    }

    public void Shake()
    {
        _shakeTimer = shakeDuration;
    }

    private Vector3 GetMouseLeadOffset()
    {
        if (!_cam) return Vector3.zero;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
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