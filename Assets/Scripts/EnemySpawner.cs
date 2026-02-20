using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnMode { Random, Sequential }

    [Header("Enemy Types")]
    public EnemyStats[] enemyTypes;
    public SpawnMode spawnMode = SpawnMode.Random;

    [Header("Fallback Enemy Settings")]
    public float moveSpeed = 3f;
    public float enemyScale = 2f;
    public float enemyMaxHealth = 100f;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public bool autoSpawn = false;
    
    [Header("Fallback Walk Animation Sprites")]
    public Sprite[] walkNorth;
    public Sprite[] walkNorthEast;
    public Sprite[] walkEast;
    public Sprite[] walkSouthEast;
    public Sprite[] walkSouth;
    public Sprite[] walkSouthWest;
    public Sprite[] walkWest;
    public Sprite[] walkNorthWest;
    
    [Header("References")]
    public NavMeshPathfinder pathfinder;
    
    private float spawnTimer = 0f;
    private int sequentialIndex = 0;
    
    void Start()
    {
        if (pathfinder == null)
        {
            pathfinder = FindFirstObjectByType<NavMeshPathfinder>();
        }
    }
    
    void Update()
    {
        if (autoSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnEnemy();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnEnemy();
        }
    }
    
    EnemyStats PickEnemyType()
    {
        if (enemyTypes == null || enemyTypes.Length == 0) return null;
        
        if (spawnMode == SpawnMode.Random)
        {
            return enemyTypes[Random.Range(0, enemyTypes.Length)];
        }
        else
        {
            EnemyStats picked = enemyTypes[sequentialIndex % enemyTypes.Length];
            sequentialIndex++;
            return picked;
        }
    }
    
    [ContextMenu("Spawn Enemy")]
    public void SpawnEnemy()
    {
        if (pathfinder == null)
        {
            Debug.LogError("No pathfinder assigned!");
            return;
        }
        
        if (!pathfinder.HasValidPath())
        {
            Debug.LogWarning("No valid path! Regenerating...");
            pathfinder.RegeneratePath();
        }
        
        var path = pathfinder.GetPath();
        if (path == null || path.Count < 2)
        {
            Debug.LogError("Still no valid path after regeneration!");
            return;
        }
        
        EnemyStats picked = PickEnemyType();
        
        if (picked != null)
        {
            SpawnEnemyOfType(picked, path, 1f, 1f);
        }
        else
        {
            SpawnFallbackEnemy(path);
        }
    }
    
    /// Called by WaveManager to spawn a specific enemy type with wave-based stat scaling.
    public EnemyWalker SpawnWaveEnemy(EnemyStats stats, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        if (pathfinder == null)
        {
            Debug.LogError("No pathfinder assigned!");
            return null;
        }
        if (!pathfinder.HasValidPath())
        {
            pathfinder.RegeneratePath();
        }
        var path = pathfinder.GetPath();
        if (path == null || path.Count < 2)
        {
            Debug.LogError("No valid path for wave enemy!");
            return null;
        }
        return SpawnEnemyOfType(stats, path, healthMultiplier, speedMultiplier);
    }

    EnemyWalker SpawnEnemyOfType(EnemyStats stats, System.Collections.Generic.List<Vector3> path, float healthMult = 1f, float speedMult = 1f)
    {
        GameObject enemy = new GameObject("Enemy_" + stats.enemyName + "_" + Time.time);
        enemy.transform.position = new Vector3(path[0].x, path[0].y, 0f);
        
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20;
        
        Rigidbody2D enemyRb = enemy.AddComponent<Rigidbody2D>();
        enemyRb.isKinematic = true;
        enemyRb.gravityScale = 0f;
        
        CircleCollider2D enemyCollider = enemy.AddComponent<CircleCollider2D>();
        enemyCollider.radius = 0.5f;
        enemyCollider.isTrigger = true;
        
        EnemyWalker walker = enemy.AddComponent<EnemyWalker>();
        walker.pathfinder = pathfinder;
        walker.ApplyStats(stats, healthMult, speedMult);
        
        if (stats.walkSouth != null && stats.walkSouth.Length > 0)
        {
            sr.sprite = stats.walkSouth[0];
        }
        sr.color = stats.tintColor;
        
        walker.StartWalking();
        Debug.Log($"Spawned {stats.enemyName} (HP x{healthMult:F1}, Speed x{speedMult:F1}) at {enemy.transform.position}");
        return walker;
    }
    
    void SpawnFallbackEnemy(System.Collections.Generic.List<Vector3> path)
    {
        GameObject enemy = new GameObject("Enemy_" + Time.time);
        enemy.transform.position = new Vector3(path[0].x, path[0].y, 0f);
        enemy.transform.localScale = Vector3.one * enemyScale;
        
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20;
        
        Rigidbody2D enemyRb = enemy.AddComponent<Rigidbody2D>();
        enemyRb.isKinematic = true;
        enemyRb.gravityScale = 0f;
        
        CircleCollider2D enemyCollider = enemy.AddComponent<CircleCollider2D>();
        enemyCollider.radius = 0.5f;
        enemyCollider.isTrigger = true;
        
        EnemyWalker walker = enemy.AddComponent<EnemyWalker>();
        walker.moveSpeed = moveSpeed;
        walker.maxHealth = enemyMaxHealth;
        walker.pathfinder = pathfinder;
        
        walker.walkNorth = walkNorth;
        walker.walkNorthEast = walkNorthEast;
        walker.walkEast = walkEast;
        walker.walkSouthEast = walkSouthEast;
        walker.walkSouth = walkSouth;
        walker.walkSouthWest = walkSouthWest;
        walker.walkWest = walkWest;
        walker.walkNorthWest = walkNorthWest;
        
        if (walkSouth != null && walkSouth.Length > 0)
        {
            sr.sprite = walkSouth[0];
        }
        
        walker.StartWalking();
        Debug.Log($"Spawned fallback enemy at {enemy.transform.position}");
    }
}
