using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 프리팹 InstanceID를 키로 사용하는 범용 오브젝트 풀(Singleton)
/// 총알·이펙트·적 등 빈번히 생성/파괴되는 GameObject의 GC 부담을 제거
/// 
/// <para><b>설계 의도</b>: Unity의 GetInstanceID()는 프리팹별로 고유하므로,
/// 별도의 문자열 키 없이 프리팹 참조만으로 풀을 자동 분류</para>
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [Header("Pool Setting")]
    [Tooltip("각 프리팹 풀 별 최대 유지 개수. 이 수치를 넘어서면 반환 시 메모리에서 객체를 파괴")]
    public int maxCapacity = 200;

    [Header("PreWarm Setting")]
    [Tooltip("게임 시작 시 미리 생성할 프리팹 리스트")]
    public List<PoolItem> preWarmItems;

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

    private void Start()
    {
        // 인스펙터에 등록된 프리팹들을 자동으로 사전 생성
        if (preWarmItems == null) return;
        
        foreach (var item in preWarmItems)
        {
            if (item.prefab != null && item.count > 0)
            {
                PreWarm(item.prefab, item.count);
            }
        }
    }

    /// <summary>
    /// 사전 생성. 시작 시 다수의 객체를 미리 생성해두어 런타임 프레임 드랍 방지
    /// </summary>
    private void PreWarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        int key = prefab.GetInstanceID();
        if (!_pools.TryGetValue(key, out var queue))
        {
            queue = new Queue<PoolID>(count);
            _pools[key] = queue;
        }

        // 프리팹을 잠시 비활성화하여 생성 시 OnEnable이 호출되는 것을 방지
        bool wasActive = prefab.activeSelf;
        prefab.SetActive(false);

        for (int i = 0; i < count; i++)
        {
            if (queue.Count >= maxCapacity) break;

            GameObject newObj = Instantiate(prefab, transform);
            PoolID poolID = newObj.AddComponent<PoolID>();
            poolID.prefabID = key;

            newObj.SetActive(false);
            queue.Enqueue(poolID);
        }

        // 프리팹 상태 복구
        prefab.SetActive(wasActive);
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼내거나, 부족하면 새로 생성
    /// 꺼낸 오브젝트는 활성화(SetActive(true)) 상태로 반환
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();
        PoolID poolID;

        if (_pools.TryGetValue(key, out var queue))
        {
            while (queue.Count > 0)
            {
                poolID = queue.Dequeue();
                
                // 풀에 있던 객체가 외부 요인으로 파괴되었을 수 있으므로 null 체크
                if (poolID != null && poolID.gameObject != null)
                {
                    GameObject obj = poolID.gameObject;
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    
                    // 활성 목록에 등록 (Release 시 조회용 - GetComponent 회피)
                    _activeObjects[obj.GetInstanceID()] = poolID;
                    return obj;
                }
            }
        }

        GameObject newObj = Instantiate(prefab, position, rotation);
        poolID = newObj.AddComponent<PoolID>();
        poolID.prefabID = key;
        
        _activeObjects[newObj.GetInstanceID()] = poolID;
        return newObj;
    }

    /// <summary>
    /// 씬 전환 시 등 풀 전체를 초기화하고 모든 자원을 해제합니다.
    /// 파괴되지 않고 남아있는 풀링된 객체들도 모두 파괴됩니다.
    /// </summary>
    private void Clear()
    {
        _activeObjects.Clear();
        
        foreach (var kvp in _pools)
        {
            Queue<PoolID> queue = kvp.Value;
            while (queue.Count > 0)
            {
                PoolID poolID = queue.Dequeue();
                if (poolID != null && poolID.gameObject != null)
                {
                    Destroy(poolID.gameObject);
                }
            }
        }
        _pools.Clear();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Clear();
            Instance = null;
        }
    }

    /// <summary>
    /// 오브젝트를 비활성화하고 풀에 반환
    /// </summary>
    public void Release(GameObject obj)
    {
        if (obj == null) return;

        int instanceId = obj.GetInstanceID();

        // 활성화 딕셔너리 O(1) 검색 (가장 큰 문제였던 TryGetComponent 빈번한 호출 제거)
        if (!_activeObjects.TryGetValue(instanceId, out PoolID poolID))
        {
            // 예외적으로 누락된 객체에 대해서만 수행
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

        // 한계 초과 시 메모리 상주를 막기 위해 파괴
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
/// 인스펙터에서 사전 생성 설정을 하기 위한 데이터 구조체
/// </summary>
[System.Serializable]
public struct PoolItem
{
    public GameObject prefab;
    public int count;        // 미리 생성할 개수
}

/// <summary>
/// 풀링된 오브젝트에 자동 부착되어 원본 프리팹 ID를 기억하는 컴포넌트
/// Release 시 맞는 풀 큐에 반환하기 위해 필요
/// </summary>
public class PoolID : MonoBehaviour
{
    [HideInInspector] public int prefabID;
}