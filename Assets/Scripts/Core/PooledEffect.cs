using UnityEngine;

/// <summary>
/// ParticleSystem 재생이 끝나면 자동으로 ObjectPool에 반환하는 컴포넌트.
/// 이펙트 프리팹에 부착하면 Instantiate/Destroy 없이 풀링된 이펙트를 운용할 수 있습니다.
/// 
/// <para><b>성능 이점</b>: 피격·폭발 이펙트처럼 짧은 수명의 파티클은
/// 매 프레임 생성/파괴 시 GC Spike를 유발하므로, 풀 반환으로 이를 제거합니다.</para>
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PooledEffect : MonoBehaviour
{
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();

        // 루프 재생이면 IsAlive 기반 자동 반환이 불가능하므로 비활성화
        var main = _ps.main;
        main.loop = false;
    }

    private void OnEnable()
    {
        _ps.Play();
    }

    private void Update()
    {
        // 모든 서브 이미터(withChildren=true)까지 종료됐는지 확인 후 반환
        if (!_ps.IsAlive(true))
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Release(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
