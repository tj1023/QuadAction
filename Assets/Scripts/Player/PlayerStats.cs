using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 100;
    private int _currentHp;
    private int _currentMoney;
    
    private void Awake()
    {
        _currentHp = maxHp;
    }
    
    private void Start()
    {
        EventManager.OnHpChanged?.Invoke(_currentHp, maxHp);
        EventManager.OnMoneyChanged?.Invoke(_currentMoney);
    }
    
    // --- HP 관련 메서드 ---
    public void TakeDamage(int damage)
    {
        if (_currentHp <= 0) return;

        _currentHp -= damage;
        _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);
        
        EventManager.OnHpChanged?.Invoke(_currentHp, maxHp);

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
