using System.Collections.Generic;
using UnityEngine;

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

    public void Release(GameObject obj)
    {
        obj.SetActive(false);

        int key = obj.GetOrAddPoolID().PrefabID;
        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        _pools[key].Enqueue(obj);
    }
}

public class PoolID : MonoBehaviour
{
    public int PrefabID;
}

public static class PoolExtensions
{
    public static PoolID GetOrAddPoolID(this GameObject obj)
    {
        if (!obj.TryGetComponent(out PoolID poolID))
            poolID = obj.AddComponent<PoolID>();
        return poolID;
    }
}
