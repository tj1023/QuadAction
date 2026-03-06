using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 체력·돈을 관리하는 컴포넌트.
/// IDamageable을 구현하여 적 공격으로부터 데미지를 받습니다.
/// 
/// <para><b>피격 피드백</b>: 피격 시 노란색 플래시 + 카메라 흔들림(EventManager.OnPlayerHit)으로
/// 시각적 타격감을 제공합니다.</para>
/// </summary>
public class PlayerStats : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 100;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private AudioClip hitSound;

    private int _currentHp;
    private int _currentMoney;
    private Renderer[] _renderers;
    private Color[] _originalColors;

    private void Awake()
    {
        _currentHp = maxHp;
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }
    
    private void Start()
    {
        EventManager.OnHpChanged?.Invoke(_currentHp, maxHp);
        EventManager.OnMoneyChanged?.Invoke(_currentMoney);
    }
    
    /// <inheritdoc/>
    public void TakeDamage(int damage)
    {
        if (_currentHp <= 0) return;

        _currentHp -= damage;
        _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);
        
        EventManager.OnHpChanged?.Invoke(_currentHp, maxHp);
        EventManager.OnPlayerHit?.Invoke();
        StartCoroutine(FlashYellow());

        if (hitSound != null)
            SoundManager.Instance.PlaySfx(hitSound);

        if (_currentHp <= 0)
            Die();
    }
    
    /// <summary>HP를 회복합니다. 최대 HP를 초과하지 않습니다.</summary>
    public void Heal(int amount)
    {
        if (_currentHp <= 0) return;

        _currentHp += amount;
        _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);
        
        EventManager.OnHpChanged?.Invoke(_currentHp, maxHp);
    }

    private IEnumerator FlashYellow()
    {
        foreach (var r in _renderers)
        {
            if (r != null) r.material.color = Color.yellow;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
                _renderers[i].material.color = _originalColors[i];
        }
    }
    
    private void Die()
    {
        EventManager.OnPlayerDeath?.Invoke();

        if (TryGetComponent(out PlayerAnimator animator))
            animator.TriggerDeath();
    }
    
    #region Economy

    /// <summary>소지금을 증가시킵니다.</summary>
    public void AddMoney(int amount)
    {
        _currentMoney += amount;
        EventManager.OnMoneyChanged?.Invoke(_currentMoney);
    }

    /// <summary>
    /// 소지금에서 지정된 금액을 차감합니다.
    /// </summary>
    /// <returns>구매 성공 여부. 돈이 부족하면 false를 반환합니다.</returns>
    public bool SpendMoney(int amount)
    {
        if (_currentMoney >= amount)
        {
            _currentMoney -= amount;
            EventManager.OnMoneyChanged?.Invoke(_currentMoney);
            return true;
        }
        
        return false;
    }

    #endregion
}
