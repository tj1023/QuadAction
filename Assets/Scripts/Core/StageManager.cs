using UnityEngine;

public class StageManager : MonoBehaviour
{
    public enum StageType { Start, Combat, Shop }

    public static StageManager Instance { get; private set; }

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
        // 시작 스테이지
        StageNumber = 0;
        CurrentStageType = StageType.Start;

        if (shopObject) shopObject.SetActive(false);
        if (nextStageBeacon) nextStageBeacon.Activate();

        EventManager.OnStageChanged?.Invoke(StageNumber, CurrentStageType);
    }
    
    // NextStageBeacon에서 호출. 다음 스테이지로 전환.
    public void StartNextStage()
    {
        StageNumber++;

        // Start → Combat → Shop → Combat → Shop ...
        if (CurrentStageType == StageType.Start || CurrentStageType == StageType.Shop)
            BeginCombatStage();
        else // Combat
            BeginShopStage();

        EventManager.OnStageChanged?.Invoke(StageNumber, CurrentStageType);
    }

    private void BeginCombatStage()
    {
        CurrentStageType = StageType.Combat;
        
        if (shopObject) shopObject.SetActive(false);
        if (nextStageBeacon) nextStageBeacon.Deactivate();
        
        SpawnEnemies();
    }

    private void BeginShopStage()
    {
        CurrentStageType = StageType.Shop;
        
        if (shopObject) shopObject.SetActive(true);
        if (nextStageBeacon) nextStageBeacon.Activate();
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int combatRound = (StageNumber + 1) / 2;
        int budget = baseDifficulty + (combatRound - 1) * difficultyPerStage;

        // 각 프리팹의 difficulty 캐싱
        int[] difficulties = new int[enemyPrefabs.Length];
        int minDifficulty = int.MaxValue;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            var stats = enemyPrefabs[i].GetComponent<EnemyStats>();
            difficulties[i] = (stats != null && stats.Data != null) ? stats.Data.difficulty : 1;
            if (difficulties[i] < minDifficulty)
                minDifficulty = difficulties[i];
        }

        _remainingEnemies = 0;
        int spawnIndex = 0;

        while (budget >= minDifficulty)
        {
            // 예산 이하인 적만 후보로 필터링
            int chosen = -1;
            int attempts = 0;
            while (attempts < 20)
            {
                int idx = Random.Range(0, enemyPrefabs.Length);
                if (difficulties[idx] <= budget)
                {
                    chosen = idx;
                    break;
                }
                attempts++;
            }

            // 랜덤 실패 시 순차 탐색
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
            Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
            Instantiate(enemyPrefabs[chosen], point.position + offset, Quaternion.identity);

            _remainingEnemies++;
            spawnIndex++;
        }
    }

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
