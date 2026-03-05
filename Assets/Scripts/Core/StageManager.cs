using UnityEngine;

public class StageManager : MonoBehaviour
{
    public enum StageType { Start, Combat, Shop }

    public static StageManager Instance { get; private set; }

    [Header("Enemy Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int baseEnemyCount = 3;
    [SerializeField] private int enemiesPerStage = 2;

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

        // 스테이지 번호에 비례하여 적 수 증가
        // StageNumber는 1부터 시작 (첫 전투 스테이지)
        int combatRound = (StageNumber + 1) / 2; // 1, 2, 3, ...
        int enemyCount = baseEnemyCount + (combatRound - 1) * enemiesPerStage;
        _remainingEnemies = enemyCount;

        for (int i = 0; i < enemyCount; i++)
        {
            // 스폰 포인트 순환
            Transform point = spawnPoints[i % spawnPoints.Length];

            // 스폰 포인트 주변 랜덤 위치
            Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
            Vector3 spawnPos = point.position + offset;

            // 적 프리팹 랜덤 선택
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            Instantiate(prefab, spawnPos, Quaternion.identity);
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
