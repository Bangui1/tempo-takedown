using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Debug script to help troubleshoot enemy path following issues
/// Attach this to any GameObject and it will analyze the scene setup
/// </summary>
public class EnemyDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    public bool runDiagnosticsOnStart = true;
    public bool continuousDebugging = false;
    public float debugInterval = 2.0f;
    
    [Header("Manual Testing")]
    public EnemyPathFollower testEnemy;
    public EnemySpawner testSpawner;
    public MazePathfinder testPathfinder;
    
    void Start()
    {
        if (runDiagnosticsOnStart)
        {
            Invoke(nameof(RunFullDiagnostics), 1.0f); // Wait a bit for everything to initialize
        }
        
        if (continuousDebugging)
        {
            InvokeRepeating(nameof(RunQuickDiagnostics), debugInterval, debugInterval);
        }
    }
    
    [ContextMenu("Run Full Diagnostics")]
    public void RunFullDiagnostics()
    {
        Debug.Log("=== ENEMY SYSTEM DIAGNOSTICS ===");
        
        CheckMazePathfinder();
        CheckPoints();
        CheckEnemySpawner();
        CheckActiveEnemies();
        CheckPathGeneration();
        
        Debug.Log("=== DIAGNOSTICS COMPLETE ===");
    }
    
    [ContextMenu("Run Quick Diagnostics")]
    public void RunQuickDiagnostics()
    {
        Debug.Log("--- Quick Enemy Check ---");
        CheckActiveEnemies();
        CheckPathGeneration();
    }
    
    void CheckMazePathfinder()
    {
        Debug.Log("--- Checking MazePathfinder ---");
        
        MazePathfinder[] pathfinders = FindObjectsByType<MazePathfinder>(FindObjectsSortMode.None);
        Debug.Log($"Found {pathfinders.Length} MazePathfinder(s) in scene");
        
        foreach (MazePathfinder pf in pathfinders)
        {
            Debug.Log($"Pathfinder: {pf.name}");
            Debug.Log($"  - Generate On Start: {pf.generateOnStart}");
            Debug.Log($"  - Has Valid Path: {pf.HasValidPath()}");
            
            if (pf.HasValidPath())
            {
                List<Vector2> path = pf.GetOptimalPath();
                Debug.Log($"  - Path Length: {path.Count} waypoints");
                if (path.Count > 0)
                {
                    Debug.Log($"  - Start Point: {path[0]}");
                    Debug.Log($"  - End Point: {path[path.Count - 1]}");
                }
            }
            else
            {
                Debug.LogWarning($"  - Pathfinder {pf.name} has no valid path!");
            }
        }
    }
    
    void CheckPoints()
    {
        Debug.Log("--- Checking Points ---");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> points = new List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Point_"))
            {
                points.Add(obj);
            }
        }
        
        Debug.Log($"Found {points.Count} points in scene:");
        points.Sort((a, b) => a.name.CompareTo(b.name));
        
        foreach (GameObject point in points)
        {
            Debug.Log($"  - {point.name} at {point.transform.position}");
        }
        
        if (points.Count < 2)
        {
            Debug.LogError("Need at least 2 points for pathfinding! Make sure you have Point_START and Point_END (or Point_1, Point_2, etc.)");
        }
    }
    
    void CheckEnemySpawner()
    {
        Debug.Log("--- Checking EnemySpawner ---");
        
        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        Debug.Log($"Found {spawners.Length} EnemySpawner(s) in scene");
        
        foreach (EnemySpawner spawner in spawners)
        {
            Debug.Log($"Spawner: {spawner.name}");
            Debug.Log($"  - Enemy Prefab: {(spawner.enemyPrefab != null ? spawner.enemyPrefab.name : "NULL")}");
            Debug.Log($"  - Maze Pathfinder: {(spawner.mazePathfinder != null ? spawner.mazePathfinder.name : "NULL")}");
            Debug.Log($"  - Is Spawning: {spawner.IsSpawning()}");
            Debug.Log($"  - Active Enemies: {spawner.GetActiveEnemyCount()}");
            Debug.Log($"  - Spawn On Start: {spawner.spawnOnStart}");
            Debug.Log($"  - Wait For Path Generation: {spawner.waitForPathGeneration}");
            
            if (spawner.enemyPrefab == null)
            {
                Debug.LogError($"  - Spawner {spawner.name} has no enemy prefab assigned!");
            }
            
            if (spawner.mazePathfinder == null)
            {
                Debug.LogWarning($"  - Spawner {spawner.name} has no pathfinder assigned (will try to auto-find)");
            }
        }
    }
    
    void CheckActiveEnemies()
    {
        Debug.Log("--- Checking Active Enemies ---");
        
        EnemyPathFollower[] enemies = FindObjectsByType<EnemyPathFollower>(FindObjectsSortMode.None);
        Debug.Log($"Found {enemies.Length} active enemies in scene");
        
        foreach (EnemyPathFollower enemy in enemies)
        {
            Debug.Log($"Enemy: {enemy.name} at {enemy.transform.position}");
            Debug.Log($"  - Maze Pathfinder: {(enemy.mazePathfinder != null ? enemy.mazePathfinder.name : "NULL")}");
            Debug.Log($"  - Move Speed: {enemy.moveSpeed}");
            Debug.Log($"  - Is Moving: {enemy.IsMoving}");
            Debug.Log($"  - Has Reached End: {enemy.HasReachedEnd}");
            Debug.Log($"  - Current Path Count: {enemy.CurrentPath.Count}");
            Debug.Log($"  - Current Waypoint Index: {enemy.CurrentWaypointIndex}");
            
            if (enemy.mazePathfinder == null)
            {
                Debug.LogError($"  - Enemy {enemy.name} has no pathfinder assigned!");
            }
            
            if (enemy.CurrentPath.Count == 0)
            {
                Debug.LogError($"  - Enemy {enemy.name} has no path to follow!");
            }
            
            if (!enemy.IsMoving)
            {
                Debug.LogWarning($"  - Enemy {enemy.name} is not moving!");
            }
        }
    }
    
    void CheckPathGeneration()
    {
        Debug.Log("--- Checking Path Generation ---");
        
        MazePathfinder pathfinder = FindFirstObjectByType<MazePathfinder>();
        if (pathfinder == null)
        {
            Debug.LogError("No MazePathfinder found in scene!");
            return;
        }
        
        if (!pathfinder.HasValidPath())
        {
            Debug.LogWarning("MazePathfinder has no valid path. Attempting to generate...");
            pathfinder.GeneratePath();
            
            if (pathfinder.HasValidPath())
            {
                Debug.Log("Path generation successful!");
            }
            else
            {
                Debug.LogError("Path generation failed!");
            }
        }
        else
        {
            Debug.Log("MazePathfinder has a valid path.");
        }
    }
    
    [ContextMenu("Force Generate Path")]
    public void ForceGeneratePath()
    {
        MazePathfinder pathfinder = FindFirstObjectByType<MazePathfinder>();
        if (pathfinder != null)
        {
            Debug.Log("Forcing path generation...");
            pathfinder.GeneratePath();
        }
        else
        {
            Debug.LogError("No MazePathfinder found!");
        }
    }
    
    [ContextMenu("Test Enemy Movement")]
    public void TestEnemyMovement()
    {
        if (testEnemy == null)
        {
            testEnemy = FindFirstObjectByType<EnemyPathFollower>();
        }
        
        if (testEnemy != null)
        {
            Debug.Log($"Testing enemy {testEnemy.name}:");
            Debug.Log($"  Position: {testEnemy.transform.position}");
            Debug.Log($"  Path Count: {testEnemy.CurrentPath.Count}");
            Debug.Log($"  Is Moving: {testEnemy.IsMoving}");
            
            // Try to restart the enemy
            testEnemy.RestartPath();
            Debug.Log("Restarted enemy path");
        }
        else
        {
            Debug.LogError("No enemy found to test!");
        }
    }
    
    [ContextMenu("Spawn Test Enemy")]
    public void SpawnTestEnemy()
    {
        if (testSpawner == null)
        {
            testSpawner = FindFirstObjectByType<EnemySpawner>();
        }
        
        if (testSpawner != null)
        {
            GameObject enemy = testSpawner.SpawnEnemy();
            if (enemy != null)
            {
                Debug.Log($"Spawned test enemy: {enemy.name}");
            }
        }
        else
        {
            Debug.LogError("No enemy spawner found!");
        }
    }
}
