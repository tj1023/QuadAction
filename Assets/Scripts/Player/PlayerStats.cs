using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int maxHp = 100;
    private int _currentHp;
    private int _currentMoney;
    
    // UI나 다른 시스템에서 구독(Subscribe)할 이벤트들
    public event Action<int, int> OnHpChanged;
    public event Action<int> OnMoneyChanged;
    public event Action OnPlayerDeath;
    
    public int CurrentHp => _currentHp;
    public int CurrentMoney => _currentMoney;
    
    private void Awake()
    {
        _currentHp = maxHp;
    }
    
    private void Start()
    {
        // 시작 시 UI 초기화를 위해 이벤트 한 번 호출
        OnHpChanged?.Invoke(_currentHp, maxHp);
        OnMoneyChanged?.Invoke(_currentMoney);
    }
    
    // --- HP 관련 메서드 ---
    public void TakeDamage(int damage)
    {
        if (_currentHp <= 0) return;

        _currentHp -= damage;
        _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);

        OnHpChanged?.Invoke(_currentHp, maxHp);

        if (_currentHp <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (_currentHp <= 0) return;

        _currentHp += amount;
        _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);

        OnHpChanged?.Invoke(_currentHp, maxHp);
    }
    
    private void Die()
    {
        Debug.Log("플레이어 사망!");
        OnPlayerDeath?.Invoke();
        // 사망 애니메이션 재생, 게임 오버 처리 등 추가
    }
    
    // --- 경제(돈) 관련 메서드 ---
    public void AddMoney(int amount)
    {
        _currentMoney += amount;
        OnMoneyChanged?.Invoke(_currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (_currentMoney >= amount)
        {
            _currentMoney -= amount;
            OnMoneyChanged?.Invoke(_currentMoney);
            return true; // 구매 성공
        }
        
        Debug.Log("돈이 부족합니다.");
        return false; // 구매 실패
    }
}
