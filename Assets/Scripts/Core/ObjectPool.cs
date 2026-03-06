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

    private readonly Dictionary<int, Queue<GameObject>> _pools = new();

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
    /// 풀에서 오브젝트를 꺼내거나, 부족하면 새로 생성합니다.
    /// 꺼낸 오브젝트는 활성화(SetActive(true)) 상태로 반환됩니다.
    /// </summary>
    /// <param name="prefab">원본 프리팹 — InstanceID가 풀 키로 사용됩니다.</param>
    /// <param name="position">배치할 월드 좌표.</param>
    /// <param name="rotation">배치할 회전값.</param>
    /// <returns>풀에서 꺼내거나 새로 생성된 GameObject.</returns>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int key = prefab.GetInstanceID();

        if (_pools.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        GameObject newObj = Instantiate(prefab, position, rotation);
        newObj.GetOrAddPoolID().PrefabID = key;
        return newObj;
    }

    /// <summary>
    /// 오브젝트를 비활성화하고 풀에 반환합니다.
    /// Destroy() 대신 호출하여 재사용을 가능하게 합니다.
    /// </summary>
    /// <param name="obj">반환할 GameObject.</param>
    public void Release(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);

        int key = obj.GetOrAddPoolID().PrefabID;

        if (!_pools.TryGetValue(key, out var queue))
        {
            queue = new Queue<GameObject>();
            _pools[key] = queue;
        }

        queue.Enqueue(obj);
    }
}

/// <summary>
/// 풀링된 오브젝트에 자동 부착되어 원본 프리팹 ID를 기억하는 컴포넌트.
/// Release 시 올바른 풀 큐에 반환하기 위해 필요합니다.
/// </summary>
public class PoolID : MonoBehaviour
{
    [HideInInspector] public int PrefabID;
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
