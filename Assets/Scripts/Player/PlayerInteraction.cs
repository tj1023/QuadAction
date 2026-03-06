using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 주변의 상호작용 가능한 오브젝트를 관리합니다.
/// 트리거 범위에 들어온 IInteractable 목록 중 가장 가까운 오브젝트와 상호작용합니다.
/// 
/// <para><b>안전성</b>: 파괴된 MonoBehaviour를 감지하여 리스트에서 자동 제거합니다.
/// Unity의 == null 연산자 오버로딩을 활용하여 파괴된 오브젝트를 식별합니다.</para>
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();

    /// <summary>상호작용 가능 목록에 추가합니다. OnTriggerEnter에서 호출됩니다.</summary>
    public void AddInteractable(IInteractable interactable)
    {
        if (!_nearbyInteractables.Contains(interactable))
            _nearbyInteractables.Add(interactable);
    }

    /// <summary>상호작용 가능 목록에서 제거합니다. OnTriggerExit에서 호출됩니다.</summary>
    public void RemoveInteractable(IInteractable interactable)
    {
        _nearbyInteractables.Remove(interactable);
    }

    /// <summary>
    /// 가장 가까운 IInteractable에게 Interact를 호출합니다.
    /// 역순 순회로 파괴된 오브젝트를 안전하게 제거합니다.
    /// </summary>
    public void PickupClosestItem()
    {
        if (_nearbyInteractables.Count == 0) return;
        
        IInteractable closestInteractable = null;
        float minDist = float.MaxValue;

        for (int i = _nearbyInteractables.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = _nearbyInteractables[i];
            
            // 파괴된 MonoBehaviour 감지 (Unity의 == null 오버로딩 활용)
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
            closestInteractable.Interact(gameObject);
            _nearbyInteractables.Remove(closestInteractable);
        }
    }
}