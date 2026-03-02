using System.Collections.Generic;
using UnityEngine;

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

    private void CreateIndicator()
    {
        _indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _indicator.name = "AttackIndicator";
        _indicator.transform.SetParent(transform);

        // Quad를 바닥에 눕히기 (X축 90도 회전)
        _indicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // BoxCollider 크기에 맞게 스케일
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

        // Collider 제거 (Quad에 기본 생성됨)
        Destroy(_indicator.GetComponent<MeshCollider>());

        // 반투명 빨간 머티리얼
        var renderer = _indicator.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = indicatorColor;
        renderer.material = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        _indicator.SetActive(false);
    }

    public void Activate(int damage)
    {
        _damage = damage;
        _hitTargets.Clear();
        if (hitCollider) hitCollider.enabled = true;
        if (_indicator) _indicator.SetActive(true);
    }

    public void Deactivate()
    {
        if (hitCollider) hitCollider.enabled = false;
        if (_indicator) _indicator.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_hitTargets.Add(other.gameObject)) return;

        if (other.TryGetComponent(out IDamageable target))
            target.TakeDamage(_damage);
    }
}
