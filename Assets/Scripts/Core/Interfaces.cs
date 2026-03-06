using UnityEngine;

/// <summary>
/// 상호작용 키(E키 등)를 눌러 동작하는 오브젝트가 구현하는 인터페이스.
/// PlayerInteraction이 트리거 범위 내 IInteractable 목록을 관리하고,
/// 가장 가까운 오브젝트에 Interact를 호출합니다.
/// </summary>
public interface IInteractable
{
    /// <summary>플레이어가 상호작용 키를 눌렀을 때 호출됩니다.</summary>
    /// <param name="interactor">상호작용을 시도한 GameObject(보통 플레이어).</param>
    void Interact(GameObject interactor);

    /// <summary>거리 비교용 월드 좌표를 반환합니다.</summary>
    Vector3 GetPosition();
}

/// <summary>
/// 트리거 충돌만으로 자동 획득되는 소모품(코인, 탄약, 회복 아이템)용 인터페이스.
/// IInteractable과 분리하여 "키 입력 필요 vs 자동 획득"을 구조적으로 구분합니다.
/// </summary>
public interface ICollectible
{
    /// <summary>수집 로직을 실행합니다.</summary>
    /// <param name="collector">아이템을 수집한 GameObject(보통 플레이어).</param>
    void Collect(GameObject collector);
}

/// <summary>
/// 데미지를 받을 수 있는 모든 오브젝트가 구현하는 인터페이스.
/// 단순 데미지 적용만 필요한 경우(넉백·래그돌 불필요)에 사용하며,
/// EnemyStats처럼 넉백이 필요한 경우 별도의 OnHit 메서드를 추가로 제공합니다.
/// </summary>
public interface IDamageable
{
    /// <summary>지정된 데미지를 적용합니다.</summary>
    /// <param name="damage">적용할 데미지 양.</param>
    void TakeDamage(int damage);
}