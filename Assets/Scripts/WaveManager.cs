using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    public static event System.Action<int> OnWaveStarted;
    public static event System.Action<int> OnWaveCompleted;
    public static event System.Action<int> OnEnemiesRemainingChanged;

    public enum WaveState { WaitingToStart, Spawning, InProgress, WaveComplete }

    [System.Serializable]
    public class WaveDefinition
    {
        public string waveName;
        public EnemyStats[] enemyTypes;
        public int enemyCount = 5;
        public float spawnInterval = 2f;
        public float healthMultiplier = 1f;
        public float speedMultiplier = 1f;
        public int bonusPointsOnComplete = 25;
    }

    [Header("References")]
    public EnemySpawner enemySpawner;

    [Header("All Enemy Types (for auto-generation)")]
    public EnemyStats[] allEnemyTypes;

    [Header("Hand-Crafted Waves")]
    public WaveDefinition[] definedWaves;

    [Header("Auto-Generation Settings")]
    public int baseEnemyCount = 5;
    public float baseSpawnInterval = 2f;
    public float healthScalePerWave = 0.15f;
    public float speedScalePerWave = 0.05f;
    public float maxSpeedMultiplier = 2.5f;
    public float minSpawnInterval = 0.5f;
    public int baseBonusPoints = 25;
    public int bonusPointsPerWave = 10;

    [Header("Timing")]
    public float delayBetweenWaves = 5f;
    public bool autoStartFirstWave = false;
    public bool autoStartNextWave = false;

    public WaveState CurrentState { get; private set; } = WaveState.WaitingToStart;
    public int CurrentWaveNumber { get; private set; } = 0;
    public int EnemiesRemaining { get; private set; } = 0;

    private List<EnemyWalker> activeEnemies = new List<EnemyWalker>();
    private Coroutine spawnCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (autoStartFirstWave)
            StartNextWave();
    }

    void OnEnable()
    {
        EnemyWalker.OnEnemyDied += HandleEnemyRemoved;
        EnemyWalker.OnEnemyReachedEnd += HandleEnemyRemoved;
    }

    void OnDisable()
    {
        EnemyWalker.OnEnemyDied -= HandleEnemyRemoved;
        EnemyWalker.OnEnemyReachedEnd -= HandleEnemyRemoved;
    }

    void HandleEnemyRemoved(EnemyWalker enemy)
    {
        activeEnemies.Remove(enemy);
        EnemiesRemaining = activeEnemies.Count;
        OnEnemiesRemainingChanged?.Invoke(EnemiesRemaining);

        if (CurrentState == WaveState.InProgress && EnemiesRemaining <= 0)
        {
            WaveCompleted();
        }
    }

    public void StartNextWave()
    {
        if (CurrentState == WaveState.Spawning || CurrentState == WaveState.InProgress)
            return;

        CurrentWaveNumber++;
        CurrentState = WaveState.Spawning;

        WaveDefinition wave = GetWaveDefinition(CurrentWaveNumber);
        OnWaveStarted?.Invoke(CurrentWaveNumber);
        Debug.Log($"=== Wave {CurrentWaveNumber} started! ({wave.enemyCount} enemies) ===");

        spawnCoroutine = StartCoroutine(SpawnWaveEnemies(wave));
    }

    WaveDefinition GetWaveDefinition(int waveNumber)
    {
        int index = waveNumber - 1;
        if (definedWaves != null && index < definedWaves.Length)
            return definedWaves[index];

        return GenerateWave(waveNumber);
    }

    WaveDefinition GenerateWave(int waveNumber)
    {
        WaveDefinition wave = new WaveDefinition();
        wave.waveName = $"Wave {waveNumber}";
        wave.enemyCount = baseEnemyCount + (waveNumber * 2);
        wave.spawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (waveNumber * 0.15f));
        wave.healthMultiplier = 1f + (waveNumber * healthScalePerWave);
        wave.speedMultiplier = Mathf.Min(maxSpeedMultiplier, 1f + (waveNumber * speedScalePerWave));
        wave.bonusPointsOnComplete = baseBonusPoints + (waveNumber * bonusPointsPerWave);

        if (allEnemyTypes != null && allEnemyTypes.Length > 0)
        {
            int typesAvailable = Mathf.Min(allEnemyTypes.Length, 1 + (waveNumber / 2));
            wave.enemyTypes = new EnemyStats[typesAvailable];
            for (int i = 0; i < typesAvailable; i++)
                wave.enemyTypes[i] = allEnemyTypes[i];
        }

        return wave;
    }

    IEnumerator SpawnWaveEnemies(WaveDefinition wave)
    {
        activeEnemies.Clear();
        EnemiesRemaining = wave.enemyCount;
        OnEnemiesRemainingChanged?.Invoke(EnemiesRemaining);

        for (int i = 0; i < wave.enemyCount; i++)
        {
            EnemyStats stats = PickEnemyForWave(wave, i);
            if (stats == null)
            {
                Debug.LogWarning("No enemy stats available for wave spawn!");
                continue;
            }

            EnemyWalker spawned = enemySpawner.SpawnWaveEnemy(stats, wave.healthMultiplier, wave.speedMultiplier);
            if (spawned != null)
                activeEnemies.Add(spawned);

            if (i < wave.enemyCount - 1)
                yield return new WaitForSeconds(wave.spawnInterval);
        }

        CurrentState = WaveState.InProgress;

        // Clean nulls (enemies that died during spawning)
        activeEnemies.RemoveAll(e => e == null || e.IsDead);
        EnemiesRemaining = activeEnemies.Count;
        OnEnemiesRemainingChanged?.Invoke(EnemiesRemaining);

        if (EnemiesRemaining <= 0)
            WaveCompleted();
    }

    EnemyStats PickEnemyForWave(WaveDefinition wave, int index)
    {
        EnemyStats[] pool = wave.enemyTypes;
        if (pool == null || pool.Length == 0)
            pool = allEnemyTypes;
        if (pool == null || pool.Length == 0)
            return null;

        return pool[index % pool.Length];
    }

    void WaveCompleted()
    {
        CurrentState = WaveState.WaveComplete;
        WaveDefinition wave = GetWaveDefinition(CurrentWaveNumber);

        if (GameEconomy.Instance != null)
            GameEconomy.Instance.AddPoints(wave.bonusPointsOnComplete);

        OnWaveCompleted?.Invoke(CurrentWaveNumber);
        Debug.Log($"=== Wave {CurrentWaveNumber} complete! Bonus: {wave.bonusPointsOnComplete} pts ===");

        if (autoStartNextWave)
            StartCoroutine(AutoStartDelay());
    }

    IEnumerator AutoStartDelay()
    {
        yield return new WaitForSeconds(delayBetweenWaves);
        if (CurrentState == WaveState.WaveComplete)
            StartNextWave();
    }
}
