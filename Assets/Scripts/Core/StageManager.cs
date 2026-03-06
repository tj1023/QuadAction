using UnityEngine;

/// <summary>
/// 스테이지 진행을 관리하는 싱글톤.
/// Start → Combat → Shop → Combat → Shop 순서로 스테이지를 순환하며,
/// Combat 스테이지에서는 난이도 예산(Budget) 기반으로 적을 스폰합니다.
/// 
/// <para><b>난이도 시스템</b>: 각 적 프리팹의 EnemyData.difficulty를 비용으로 사용하여,
/// 스테이지별 예산 내에서 다양한 조합의 적을 생성합니다.
/// 이를 통해 단순 수량 증가보다 자연스러운 난이도 곡선을 구현합니다.</para>
/// </summary>
public class StageManager : MonoBehaviour
{
    public enum StageType { Start, Combat, Shop }

    public static StageManager Instance { get; private set; }

    private const int MaxRandomAttempts = 20;
    private const float SpawnOffsetRange = 2f;

    [Header("Enemy Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int baseDifficulty = 5;
    [SerializeField] private int difficultyPerStage = 3;

    [Header("Stage Objects")]
    [SerializeField] private NextStageBeacon nextStageBeacon;
    [SerializeField] private GameObject shopObject;

    private int StageNumber { get; set; }
    private StageType CurrentStageType { get; set; }

    private int _remainingEnemies;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        EventManager.OnEnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        EventManager.OnEnemyDied -= OnEnemyDied;
    }

    private void Start()
    {
        StageNumber = 0;
        CurrentStageType = StageType.Start;

        if (shopObject != null) shopObject.SetActive(false);
        if (nextStageBeacon != null) nextStageBeacon.Activate();

        EventManager.OnStageChanged?.Invoke(StageNumber, CurrentStageType);
    }
    
    /// <summary>
    /// NextStageBeacon에서 호출됩니다. 다음 스테이지로 전환합니다.
    /// Start/Shop 이후에는 Combat, Combat 이후에는 Shop으로 진행됩니다.
    /// </summary>
    public void StartNextStage()
    {
        StageNumber++;

        if (CurrentStageType == StageType.Start || CurrentStageType == StageType.Shop)
            BeginCombatStage();
        else
            BeginShopStage();

        EventManager.OnStageChanged?.Invoke(StageNumber, CurrentStageType);
    }

    private void BeginCombatStage()
    {
        CurrentStageType = StageType.Combat;
        
        if (shopObject != null) shopObject.SetActive(false);
        if (nextStageBeacon != null) nextStageBeacon.Deactivate();
        
        SpawnEnemies();
    }

    private void BeginShopStage()
    {
        CurrentStageType = StageType.Shop;
        
        if (shopObject != null) shopObject.SetActive(true);
        if (nextStageBeacon != null) nextStageBeacon.Activate();
    }

    /// <summary>
    /// 난이도 예산 기반 적 스폰.
    /// 예산이 소진될 때까지 랜덤으로 적을 선택하되,
    /// 랜덤 실패 시 순차 탐색으로 폴백하여 무한 루프를 방지합니다.
    /// </summary>
    private void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int combatRound = (StageNumber + 1) / 2;
        int budget = baseDifficulty + (combatRound - 1) * difficultyPerStage;

        // 각 프리팹의 difficulty를 캐싱하여 반복 GetComponent 호출 방지
        int[] difficulties = new int[enemyPrefabs.Length];
        int minDifficulty = int.MaxValue;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            var stats = enemyPrefabs[i].GetComponent<EnemyStats>();
            difficulties[i] = (stats != null && stats.Data != null) ? stats.Data.Difficulty : 1;
            if (difficulties[i] < minDifficulty)
                minDifficulty = difficulties[i];
        }

        _remainingEnemies = 0;
        int spawnIndex = 0;

        while (budget >= minDifficulty)
        {
            // 예산 이하인 적만 후보로 필터링 (랜덤 시도)
            int chosen = -1;
            int attempts = 0;
            while (attempts < MaxRandomAttempts)
            {
                int idx = Random.Range(0, enemyPrefabs.Length);
                if (difficulties[idx] <= budget)
                {
                    chosen = idx;
                    break;
                }
                attempts++;
            }

            // 랜덤 실패 시 순차 탐색으로 폴백
            if (chosen == -1)
            {
                for (int i = 0; i < enemyPrefabs.Length; i++)
                {
                    if (difficulties[i] <= budget)
                    {
                        chosen = i;
                        break;
                    }
                }
            }

            if (chosen == -1) break;

            budget -= difficulties[chosen];

            Transform point = spawnPoints[spawnIndex % spawnPoints.Length];
            Vector3 offset = new Vector3(
                Random.Range(-SpawnOffsetRange, SpawnOffsetRange),
                0f,
                Random.Range(-SpawnOffsetRange, SpawnOffsetRange));
            
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Get(enemyPrefabs[chosen], point.position + offset, Quaternion.identity);
            else
                Instantiate(enemyPrefabs[chosen], point.position + offset, Quaternion.identity);

            _remainingEnemies++;
            spawnIndex++;
        }
    }

    /// <summary>
    /// 적 사망 이벤트 핸들러.
    /// 남은 적이 0이 되면 NextStageBeacon을 활성화하여 다음 스테이지로 진행 가능하게 합니다.
    /// </summary>
    private void OnEnemyDied()
    {
        if (CurrentStageType != StageType.Combat) return;

        _remainingEnemies--;

        if (_remainingEnemies <= 0)
        {
            _remainingEnemies = 0;
            nextStageBeacon?.Activate();
        }
    }
}
