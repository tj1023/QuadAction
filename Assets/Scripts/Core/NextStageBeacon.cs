using UnityEngine;

/// <summary>
/// 다음 스테이지로 이동하기 위한 상호작용 오브젝트(비콘).
/// IInteractable을 구현하여 PlayerInteraction 시스템과 연동됩니다.
/// 
/// <para>StageManager가 전투 종료 시 Activate()를, 다음 스테이지 시작 시 Deactivate()를
/// 호출하여 비콘의 활성 상태를 제어합니다.</para>
/// </summary>
public class NextStageBeacon : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject visualEffect;

    private bool _isActive;

    private void Awake()
    {
        Deactivate();
    }
    
    /// <summary>비콘을 활성화하여 플레이어가 상호작용할 수 있게 합니다.</summary>
    public void Activate()
    {
        _isActive = true;
        gameObject.SetActive(true);
        if (visualEffect != null) visualEffect.SetActive(true);
    }

    /// <summary>비콘을 비활성화하여 상호작용을 차단합니다.</summary>
    public void Deactivate()
    {
        _isActive = false;
        if (visualEffect != null) visualEffect.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;

        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerInteraction player))
            player.AddInteractable(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerInteraction player))
            player.RemoveInteractable(this);
    }

    /// <summary>
    /// 플레이어가 상호작용(E키)하면 비콘을 비활성화하고 다음 스테이지를 시작합니다.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (!_isActive) return;
        
        if (interactor.TryGetComponent(out PlayerInteraction player))
            player.RemoveInteractable(this);
        
        Deactivate();
        StageManager.Instance.StartNextStage();
    }

    /// <inheritdoc/>
    public Vector3 GetPosition() => transform.position;
}
