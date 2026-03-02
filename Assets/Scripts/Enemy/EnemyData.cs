using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public int maxHp = 100;
    public int attackPower = 10;
    public float attackRate = 1.5f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("AI")]
    public float chaseRange = 15f;
    public float attackRange = 2f;

    [Header("Hit")]
    public float knockbackResistance;
    
    [Header("Dash Attack")]
    public bool useDash;
    public float dashRange = 30f;
    public float dashSpeed = 30f;
}
