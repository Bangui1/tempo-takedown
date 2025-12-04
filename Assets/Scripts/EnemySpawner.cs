using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 3f;
    public float enemyScale = 2f;
    public float spawnInterval = 5f;
    public bool autoSpawn = false;
    
    [Header("Walk Animation Sprites")]
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
        
        GameObject enemy = new GameObject("Enemy_" + Time.time);
        enemy.transform.position = new Vector3(path[0].x, path[0].y, 0f);
        enemy.transform.localScale = Vector3.one * enemyScale;
        
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20;
        
        EnemyWalker walker = enemy.AddComponent<EnemyWalker>();
        walker.moveSpeed = moveSpeed;
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
        
        Debug.Log($"Spawned enemy at {enemy.transform.position}");
    }
}

