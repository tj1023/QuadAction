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

    private void LateUpdate()
    {
        if (!target) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;
        
        // 부드러운 이동 적용 (Lerp)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}