using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();

    public void AddInteractable(IInteractable interactable)
    {
        if(!_nearbyInteractables.Contains(interactable))
            _nearbyInteractables.Add(interactable);
    }

    public void RemoveInteractable(IInteractable interactable)
    {
        _nearbyInteractables.Remove(interactable);
    }

    public void PickupClosestItem()
    {
        if (_nearbyInteractables.Count == 0) return;
        
        IInteractable closestInteractable = null;
        float minDist = float.MaxValue;

        // 리스트를 역순으로 순회하며 파괴된 오브젝트를 안전하게 걸러냄
        for (int i = _nearbyInteractables.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = _nearbyInteractables[i];
            
            // 오브젝트가 파괴되었는지 검사
            if (interactable as MonoBehaviour == null)
            {
                _nearbyInteractables.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(transform.position, interactable.GetPosition());
            if (dist < minDist)
            {
                minDist = dist;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable != null)
        {
            // 구체적인 타입 검사 없이 다형성을 이용해 상호작용 실행
            closestInteractable.Interact(gameObject);
            _nearbyInteractables.Remove(closestInteractable);
        }
    }
}