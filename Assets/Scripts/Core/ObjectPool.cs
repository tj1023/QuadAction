using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 프리팹 InstanceID를 키로 사용하는 범용 오브젝트 풀(Singleton).
/// 총알·이펙트·적 등 빈번히 생성/파괴되는 GameObject의 GC 부담을 제거합니다.
/// 
/// <para><b>설계 의도</b>: Unity의 GetInstanceID()는 프리팹별로 고유하므로,
/// 별도의 문자열 키 없이 프리팹 참조만으로 풀을 자동 분류할 수 있어
/// 호출부의 코드가 간결해집니다.</para>
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [Header("풀 설정")]
    [Tooltip("각 프리팹 풀 별 최대 유지 개수. 이 수치를 넘어서면 반환 시 메모리에서 객체를 파괴(Destroy)합니다.")]
    public int maxCapacity = 200;

    private readonly Dictionary<int, Queue<PoolID>> _pools = new();
    
    // Release 시 TryGetComponent 호출을 방지하기 위해 Get()에서 캐싱하는 딕셔너리
    private readonly Dictionary<int, PoolID> _activeObjects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 사전 생성(Pre-warm). 로딩 씬이나 맵 시작 시 다수의 객체를 미리 생성해두어 런타임 프레임 드랍(Spike)을 방지합니다.
    /// </summary>
    public void PreWarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        int key = prefab.GetInstanceID();
        if (!_pools.TryGetValue(key, out var queue))
        {
            queue = new Queue<PoolID>(count);
            _pools[key] = queue;
        }

        for (int i = 0; i < count; i++)
        {
            if (queue.Count >= maxCapacity) break; // 최대 유지 개수 초과 방지

            GameObject newObj = Instantiate(prefab, transform);
            PoolID poolID = newObj.AddComponent<PoolID>();
            poolID.prefabID = key;

            newObj.SetActive(false);
            queue.Enqueue(poolID);
        }
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼내거나, 부족하면 새로 생성합니다.
    /// 꺼낸 오브젝트는 활성화(SetActive(true)) 상태로 반환됩니다.
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();
        PoolID poolID = null;

        if (_pools.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            poolID = queue.Dequeue();
            GameObject obj = poolID.gameObject;
            
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
                
                // 활성 목록에 등록 (Release 시 조회용 - GetComponent 회피)
                _activeObjects[obj.GetInstanceID()] = poolID;
                return obj;
            }
        }

        GameObject newObj = Instantiate(prefab, position, rotation);
        poolID = newObj.AddComponent<PoolID>();
        poolID.prefabID = key;
        
        _activeObjects[newObj.GetInstanceID()] = poolID;
        return newObj;
    }

    /// <summary>
    /// 오브젝트를 비활성화하고 풀에 반환합니다.
    /// </summary>
    public void Release(GameObject obj)
    {
        if (obj == null) return;

        int instanceId = obj.GetInstanceID();

        // 1. 활성화 딕셔너리 O(1) 검색 (가장 큰 문제였던 TryGetComponent 빈번한 호출 제거)
        if (!_activeObjects.TryGetValue(instanceId, out PoolID poolID))
        {
            // 예외적으로 누락된 객체에 대해서만 수행 (Fail-safe)
            if (!obj.TryGetComponent(out poolID))
            {
                Destroy(obj);
                return;
            }
        }

        _activeObjects.Remove(instanceId);
        int key = poolID.prefabID;

        if (!_pools.TryGetValue(key, out var queue))
        {
            queue = new Queue<PoolID>();
            _pools[key] = queue;
        }

        // 2. 메모리 상주 한계(Capacity Limit) 체크. 초과 시 메모리 상주를 막기 위해 영구 파괴
        if (queue.Count >= maxCapacity)
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        queue.Enqueue(poolID);
    }
}

/// <summary>
/// 풀링된 오브젝트에 자동 부착되어 원본 프리팹 ID를 기억하는 컴포넌트.
/// Release 시 올바른 풀 큐에 반환하기 위해 필요합니다.
/// </summary>
public class PoolID : MonoBehaviour
{
    [HideInInspector] public int prefabID;
}

/// <summary>
/// GameObject에 PoolID 컴포넌트를 안전하게 가져오거나 추가하는 확장 메서드.
/// </summary>
public static class PoolExtensions
{
    public static PoolID GetOrAddPoolID(this GameObject obj)
    {
        if (!obj.TryGetComponent(out PoolID poolID))
            poolID = obj.AddComponent<PoolID>();
        return poolID;
    }
}
