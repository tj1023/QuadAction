using UnityEngine;

// 상호작용 키를 눌러야 작동하는 오브젝트용
public interface IInteractable
{
    void Interact(GameObject interactor);
    
    Vector3 GetPosition(); 
}

// 닿기만 해도 자동 획득되는 소모품용
public interface ICollectible
{
    void Collect(GameObject collector);
}