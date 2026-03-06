using UnityEngine;

/// <summary>
/// 적 캐릭터의 기획 데이터를 담는 ScriptableObject.
/// Inspector에서 프리셋을 생성하여 프리팹에 할당하면,
/// 코드 수정 없이 적의 스탯·AI·드롭을 조정할 수 있습니다.
/// 
/// <para><b>설계 의도</b>: 하드코딩된 수치를 data-driven으로 분리하여
/// 기획자가 코드 변경 없이 밸런싱할 수 있도록 합니다.
/// 또한 프리팹별로 SO를 공유하므로 메모리 효율이 높습니다.</para>
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private int difficulty = 1;
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int attackPower = 10;
    [SerializeField] private float attackRate = 1.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("AI")]
    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float attackRange = 2f;

    [Header("Hit")]
    [SerializeField] private float knockbackResistance;
    
    [Header("Dash Attack")]
    [SerializeField] private bool useDash;
    [SerializeField] private float dashRange = 30f;
    [SerializeField] private float dashSpeed = 30f;
    
    [Header("Ranged Attack")]
    [SerializeField] private bool isRanged;

    [Header("Boss Settings")]
    [SerializeField] private bool isBoss;
    [SerializeField] private float meleeAttackRange = 4f;

    [Header("Coin Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField][Range(0f, 1f)] private float bonusCoinChance = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;
    
    [Header("Boss Audio")]
    [SerializeField] private AudioClip bossJumpAttackSound;
    [SerializeField] private AudioClip bossMissileAttackSound;
    [SerializeField] private AudioClip bossRockAttackSound;

    #region Read-Only Properties

    /// <summary>스테이지 스폰 예산에 사용되는 난이도 비용.</summary>
    public int Difficulty => difficulty;

    /// <summary>최대 체력.</summary>
    public int MaxHp => maxHp;

    /// <summary>기본 공격력.</summary>
    public int AttackPower => attackPower;

    /// <summary>공격 속도(초). 낮을수록 빠릅니다.</summary>
    public float AttackRate => attackRate;

    /// <summary>이동 속도.</summary>
    public float MoveSpeed => moveSpeed;

    /// <summary>플레이어를 감지하여 추적을 시작하는 범위.</summary>
    public float ChaseRange => chaseRange;

    /// <summary>공격 가능 범위.</summary>
    public float AttackRange => attackRange;

    /// <summary>넉백 저항값. effectiveForce = knockbackForce - knockbackResistance.</summary>
    public float KnockbackResistance => knockbackResistance;

    /// <summary>대시 공격 사용 여부.</summary>
    public bool UseDash => useDash;

    /// <summary>대시 공격 범위.</summary>
    public float DashRange => dashRange;

    /// <summary>대시 시 이동 속도.</summary>
    public float DashSpeed => dashSpeed;

    /// <summary>원거리 공격 여부.</summary>
    public bool IsRanged => isRanged;

    /// <summary>보스 여부. 보스는 HP 바 이벤트를 추가로 발행합니다.</summary>
    public bool IsBoss => isBoss;

    /// <summary>보스 근접 공격 범위.</summary>
    public float MeleeAttackRange => meleeAttackRange;

    /// <summary>사망 시 드롭하는 코인 프리팹.</summary>
    public GameObject CoinPrefab => coinPrefab;

    /// <summary>보너스 코인 드롭 확률 (난이도와 곱연산).</summary>
    public float BonusCoinChance => bonusCoinChance;

    /// <summary>공격 효과음.</summary>
    public AudioClip AttackSound => attackSound;

    /// <summary>피격 효과음.</summary>
    public AudioClip HitSound => hitSound;

    /// <summary>보스 점프 공격 효과음.</summary>
    public AudioClip BossJumpAttackSound => bossJumpAttackSound;

    /// <summary>보스 미사일 공격 효과음.</summary>
    public AudioClip BossMissileAttackSound => bossMissileAttackSound;

    /// <summary>보스 바위 공격 효과음.</summary>
    public AudioClip BossRockAttackSound => bossRockAttackSound;

    #endregion
}
