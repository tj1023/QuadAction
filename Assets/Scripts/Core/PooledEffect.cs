using UnityEngine;

// 이펙트 재생 완료시 풀에 반환
[RequireComponent(typeof(ParticleSystem))]
public class PooledEffect : MonoBehaviour
{
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();

        // 루프 재생이면 자동 반환이 불가능하므로 끈다
        var main = _ps.main;
        main.loop = false;
    }

    private void OnEnable()
    {
        _ps.Play();
    }

    private void Update()
    {
        if (!_ps.IsAlive(true))
        {
            if (ObjectPool.Instance)
                ObjectPool.Instance.Release(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
