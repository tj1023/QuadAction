using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 근접 공격의 히트박스와 바닥 인디케이터를 관리하는 컴포넌트.
/// 
/// <para><b>인디케이터 시스템</b>: Activate 시 히트박스 크기에 맞는 반투명 Quad를
/// 바닥에 표시하여 플레이어에게 공격 범위를 시각적으로 알려줍니다.
/// 이를 통해 회피 판단의 근거를 제공합니다.</para>
/// 
/// <para><b>다중 히트 방지</b>: HashSet으로 이미 피격된 대상을 추적하여
/// 한 번의 공격 판정에서 같은 대상이 여러 번 피격되지 않도록 합니다.</para>
/// </summary>
public class EnemyMeleeHitbox : MonoBehaviour
{
    [SerializeField] private Collider hitCollider;
    [SerializeField] private Color indicatorColor = new Color(1f, 0f, 0f, 0.35f);

    private int _damage;
    private readonly HashSet<GameObject> _hitTargets = new();
    private GameObject _indicator;

    private void Awake()
    {
        CreateIndicator();
        Deactivate();
    }

    /// <summary>
    /// 런타임에 Quad Primitive를 생성하여 공격 범위 인디케이터로 사용합니다.
    /// BoxCollider의 크기에 맞춰 자동 스케일되므로 Inspector 조정이 최소화됩니다.
    /// </summary>
    private void CreateIndicator()
    {
        _indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _indicator.name = "AttackIndicator";
        _indicator.transform.SetParent(transform);

        // Quad를 바닥에 눕히기 (X축 90도 회전)
        _indicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (hitCollider is BoxCollider box)
        {
            Vector3 size = box.size;
            _indicator.transform.localScale = new Vector3(size.x, size.z, 1f);
            _indicator.transform.localPosition = new Vector3(box.center.x, 0.01f, box.center.z);
        }
        else
        {
            _indicator.transform.localScale = Vector3.one;
            _indicator.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        }

        // Quad에 기본 생성되는 MeshCollider 제거
        Destroy(_indicator.GetComponent<MeshCollider>());

        // 반투명 머티리얼 설정
        var indicatorRenderer = _indicator.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Sprites/Default")) { color = indicatorColor };
        indicatorRenderer.material = mat;
        indicatorRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        indicatorRenderer.receiveShadows = false;

        _indicator.SetActive(false);
    }

    /// <summary>히트박스와 인디케이터를 활성화합니다.</summary>
    public void Activate(int damage)
    {
        _damage = damage;
        _hitTargets.Clear();
        if (hitCollider != null) hitCollider.enabled = true;
        if (_indicator != null) _indicator.SetActive(true);
    }

    /// <summary>히트박스와 인디케이터를 비활성화합니다.</summary>
    public void Deactivate()
    {
        if (hitCollider != null) hitCollider.enabled = false;
        if (_indicator != null) _indicator.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_hitTargets.Add(other.gameObject)) return;

        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(_damage);
    }
}
