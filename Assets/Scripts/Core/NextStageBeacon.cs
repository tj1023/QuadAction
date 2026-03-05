using UnityEngine;

public class NextStageBeacon : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject visualEffect;

    private bool _isActive;

    private void Awake()
    {
        Deactivate();
    }
    
    public void Activate()
    {
        _isActive = true;
        gameObject.SetActive(true);
        if (visualEffect) visualEffect.SetActive(true);
    }

    public void Deactivate()
    {
        _isActive = false;
        if (visualEffect) visualEffect.SetActive(false);
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

    public void Interact(GameObject interactor)
    {
        if (!_isActive) return;
        
        if (interactor.TryGetComponent(out PlayerInteraction player))
            player.RemoveInteractable(this);
        
        Deactivate();
        StageManager.Instance.StartNextStage();
    }

    public Vector3 GetPosition() => transform.position;
}
