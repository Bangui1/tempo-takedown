using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    public float spawnInterval = 2.0f;
    public bool spawnOnStart = true;
    public bool continuousSpawning = true;
    
    [Header("Enemy Configuration")]
    public float enemySpeed = 2.0f;
    public float enemyHealth = 100f;
    public bool enemyDestroyAtEnd = true;
    public bool enemyLoopPath = false;
    
    [Header("Path Integration")]
    public MazePathfinder mazePathfinder;
    public bool findPathfinderAutomatically = true;
    public bool waitForPathGeneration = true;
    
    [Header("Spawning Control")]
    public bool spawnAtPathStart = true;
    public Vector2 customSpawnPosition = Vector2.zero;
    public float spawnDelay = 0.5f;
    
    [Header("Wave System")]
    public bool useWaveSystem = false;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 10.0f;
    public int maxWaves = 3;
    
    // Private variables
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private int currentWave = 0;
    private int enemiesSpawnedInCurrentWave = 0;
    private Coroutine spawnCoroutine;
    
    // Events
    [System.Serializable]
    public class EnemySpawnerEvents
    {
        public UnityEngine.Events.UnityEvent OnEnemySpawned;
        public UnityEngine.Events.UnityEvent OnEnemyDestroyed;
        public UnityEngine.Events.UnityEvent OnWaveComplete;
        public UnityEngine.Events.UnityEvent OnAllWavesComplete;
    }
    
    public EnemySpawnerEvents events;
    
    void Start()
    {
        // Auto-find pathfinder if needed
        if (findPathfinderAutomatically && mazePathfinder == null)
        {
            mazePathfinder = FindFirstObjectByType<MazePathfinder>();
        }
        
        if (spawnOnStart)
        {
            if (waitForPathGeneration)
            {
                StartCoroutine(WaitForPathAndStartSpawning());
            }
            else
            {
                StartSpawning();
            }
        }
    }
    
    IEnumerator WaitForPathAndStartSpawning()
    {
        Debug.Log("Waiting for path generation before spawning enemies...");
        
        // Wait for pathfinder to generate path
        int attempts = 0;
        while (mazePathfinder == null || !HasValidPath())
        {
            yield return new WaitForSeconds(0.2f);
            attempts++;
            
            // Try to find pathfinder if still null
            if (mazePathfinder == null)
            {
                mazePathfinder = FindFirstObjectByType<MazePathfinder>();
                if (mazePathfinder != null)
                {
                    Debug.Log("Found MazePathfinder, checking for path...");
                }
            }
            
            // Try to generate path if pathfinder exists but no path
            if (mazePathfinder != null && !HasValidPath())
            {
                Debug.Log($"Attempt {attempts}: Pathfinder found but no path, trying to generate...");
                mazePathfinder.GeneratePath();
            }
            
            // Prevent infinite loop
            if (attempts > 50) // 10 seconds at 0.2s intervals
            {
                Debug.LogError("Timeout waiting for path generation! Spawning enemies anyway...");
                break;
            }
        }
        
        if (HasValidPath())
        {
            Debug.Log("Path found, starting enemy spawning");
        }
        else
        {
            Debug.LogWarning("Starting enemy spawning without valid path - enemies may not move!");
        }
        
        yield return new WaitForSeconds(spawnDelay);
        StartSpawning();
    }
    
    bool HasValidPath()
    {
        return mazePathfinder != null && mazePathfinder.HasValidPath();
    }
    
    public void StartSpawning()
    {
        if (isSpawning) return;
        
        isSpawning = true;
        
        if (useWaveSystem)
        {
            StartWaveSpawning();
        }
        else
        {
            StartContinuousSpawning();
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    void StartContinuousSpawning()
    {
        if (continuousSpawning)
        {
            spawnCoroutine = StartCoroutine(ContinuousSpawnCoroutine());
        }
        else
        {
            spawnCoroutine = StartCoroutine(SpawnAllEnemiesCoroutine());
        }
    }
    
    void StartWaveSpawning()
    {
        spawnCoroutine = StartCoroutine(WaveSpawnCoroutine());
    }
    
    IEnumerator ContinuousSpawnCoroutine()
    {
        while (isSpawning)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                yield return new WaitForSeconds(0.5f); // Check again in half a second
            }
        }
    }
    
    IEnumerator SpawnAllEnemiesCoroutine()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            if (!isSpawning) break;
            
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    IEnumerator WaveSpawnCoroutine()
    {
        while (isSpawning && currentWave < maxWaves)
        {
            Debug.Log($"Starting wave {currentWave + 1}");
            enemiesSpawnedInCurrentWave = 0;
            
            // Spawn enemies for this wave
            for (int i = 0; i < enemiesPerWave; i++)
            {
                if (!isSpawning) break;
                
                SpawnEnemy();
                enemiesSpawnedInCurrentWave++;
                yield return new WaitForSeconds(spawnInterval);
            }
            
            events.OnWaveComplete?.Invoke();
            currentWave++;
            
            // Wait between waves (only if not the last wave)
            if (currentWave < maxWaves)
            {
                Debug.Log($"Wave {currentWave} complete. Waiting {timeBetweenWaves} seconds for next wave.");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
        
        if (currentWave >= maxWaves)
        {
            Debug.Log("All waves completed!");
            events.OnAllWavesComplete?.Invoke();
        }
    }
    
    public GameObject SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("No enemy prefab assigned!");
            return null;
        }
        
        Vector2 spawnPos = GetSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        // Configure the enemy
        ConfigureEnemy(enemy);
        
        // Add to active enemies list
        activeEnemies.Add(enemy);
        
        // Set up destruction callback
        EnemyPathFollower pathFollower = enemy.GetComponent<EnemyPathFollower>();
        if (pathFollower != null)
        {
            pathFollower.OnDestroyed.AddListener(() => OnEnemyDestroyed(enemy));
            pathFollower.OnReachEnd.AddListener(() => OnEnemyReachedEnd(enemy));
        }
        
        Debug.Log($"Spawned enemy at {spawnPos}. Active enemies: {activeEnemies.Count}");
        events.OnEnemySpawned?.Invoke();
        
        return enemy;
    }
    
    Vector2 GetSpawnPosition()
    {
        if (spawnAtPathStart && HasValidPath())
        {
            // Get the start position from the path
            List<Vector2> path = mazePathfinder.GetOptimalPath();
            if (path.Count > 0)
            {
                return path[0];
            }
        }
        
        // Fallback to custom spawn position or this object's position
        return customSpawnPosition != Vector2.zero ? customSpawnPosition : transform.position;
    }
    
    void ConfigureEnemy(GameObject enemy)
    {
        EnemyPathFollower pathFollower = enemy.GetComponent<EnemyPathFollower>();
        if (pathFollower != null)
        {
            // Set pathfinder reference
            pathFollower.mazePathfinder = mazePathfinder;
            
            // Configure enemy settings
            pathFollower.moveSpeed = enemySpeed;
            pathFollower.health = enemyHealth;
            pathFollower.destroyAtEnd = enemyDestroyAtEnd;
            pathFollower.loopPath = enemyLoopPath;
        }
        
        // Add any other enemy configuration here
        enemy.name = $"Enemy_{activeEnemies.Count + 1}";
    }
    
    void OnEnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            Debug.Log($"Enemy destroyed. Active enemies: {activeEnemies.Count}");
            events.OnEnemyDestroyed?.Invoke();
        }
    }
    
    void OnEnemyReachedEnd(GameObject enemy)
    {
        Debug.Log($"Enemy {enemy.name} reached the end of the path");
        
        // Remove from active list if it will be destroyed
        if (enemyDestroyAtEnd && activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
    
    public void DestroyAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        Debug.Log("All enemies destroyed");
    }
    
    public void PauseAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyPathFollower pathFollower = enemy.GetComponent<EnemyPathFollower>();
                if (pathFollower != null)
                {
                    pathFollower.PauseMovement();
                }
            }
        }
    }
    
    public void ResumeAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyPathFollower pathFollower = enemy.GetComponent<EnemyPathFollower>();
                if (pathFollower != null)
                {
                    pathFollower.ResumeMovement();
                }
            }
        }
    }
    
    public void SetEnemySpeed(float newSpeed)
    {
        enemySpeed = newSpeed;
        
        // Update existing enemies
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyPathFollower pathFollower = enemy.GetComponent<EnemyPathFollower>();
                if (pathFollower != null)
                {
                    pathFollower.SetSpeed(newSpeed);
                }
            }
        }
    }
    
    // Public getters for external scripts
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public List<GameObject> GetActiveEnemies() => new List<GameObject>(activeEnemies);
    public bool IsSpawning() => isSpawning;
    public int GetCurrentWave() => currentWave;
    
    void OnDrawGizmos()
    {
        // Draw spawn position
        Vector2 spawnPos = GetSpawnPosition();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPos, 0.3f);
        
        // Draw custom spawn position if set
        if (customSpawnPosition != Vector2.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(customSpawnPosition, 0.2f);
        }
    }
}
