using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 100;
    [SerializeField] private float flashDuration = 0.15f;

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
    
    public void TakeDamage(int damage)
    {
        if (_currentHp <= 0) return;

        _currentHp -= damage;
        _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);
        
        EventManager.OnHpChanged?.Invoke(_currentHp, maxHp);
        EventManager.OnPlayerHit?.Invoke();
        StartCoroutine(FlashYellow());

        if (_currentHp <= 0)
            Die();
    }
    
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
            r.material.color = Color.yellow;

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
    
    // --- 경제(돈) 관련 메서드 ---
    public void AddMoney(int amount)
    {
        _currentMoney += amount;
        EventManager.OnMoneyChanged?.Invoke(_currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (_currentMoney >= amount)
        {
            _currentMoney -= amount;
            EventManager.OnMoneyChanged?.Invoke(_currentMoney);
            return true; // 구매 성공
        }
        
        Debug.Log("돈이 부족합니다.");
        return false; // 구매 실패
    }
}
