using UnityEngine;

public class MazeSystemManager : MonoBehaviour
{
    [Header("Maze Generation")]
    public SimpleMazeGenerator simpleMazeGenerator;
    public WallLineGenerator wallLineGenerator;
    public MazeGenerator traditionalMazeGenerator;
    
    [Header("Point System")]
    public PointSpawner pointSpawner;
    public SimplePointSpawner simplePointSpawner;
    public ImprovedPointSpawner improvedPointSpawner;
    public MazePointSpawner mazePointSpawner;
    
    [Header("Pathfinding")]
    public Pathfinder pathfinder;
    public MazePathfinder mazePathfinder;
    
    [Header("Settings")]
    public bool generateOnStart = true;
    public bool clearExistingOnRegenerate = true;
    public float generationDelay = 0.5f;
    
    void Start()
    {
        // Auto-find components if not assigned
        AutoFindComponents();
        
        if (generateOnStart)
        {
            GenerateCompleteMaze();
        }
    }
    
    void AutoFindComponents()
    {
        if (simpleMazeGenerator == null)
            simpleMazeGenerator = FindFirstObjectByType<SimpleMazeGenerator>();
        if (wallLineGenerator == null)
            wallLineGenerator = FindFirstObjectByType<WallLineGenerator>();
        if (traditionalMazeGenerator == null)
            traditionalMazeGenerator = FindFirstObjectByType<MazeGenerator>();
        
        if (pointSpawner == null)
            pointSpawner = FindFirstObjectByType<PointSpawner>();
        if (simplePointSpawner == null)
            simplePointSpawner = FindFirstObjectByType<SimplePointSpawner>();
        if (improvedPointSpawner == null)
            improvedPointSpawner = FindFirstObjectByType<ImprovedPointSpawner>();
        if (mazePointSpawner == null)
            mazePointSpawner = FindFirstObjectByType<MazePointSpawner>();
        
        if (pathfinder == null)
            pathfinder = FindFirstObjectByType<Pathfinder>();
        if (mazePathfinder == null)
            mazePathfinder = FindFirstObjectByType<MazePathfinder>();
            
        Debug.Log($"Auto-found components: MazeGenerator={simpleMazeGenerator != null}, PointSpawner={simplePointSpawner != null}, Pathfinder={mazePathfinder != null || pathfinder != null}");
    }
    
    public void GenerateCompleteMaze()
    {
        if (clearExistingOnRegenerate)
        {
            ClearAll();
        }
        
        // Generate maze first
        GenerateMaze();
        
        // Wait a bit for maze to be fully generated, then spawn points
        Invoke(nameof(GeneratePoints), generationDelay);
        
        // Wait a bit more for points to be spawned, then generate path
        Invoke(nameof(GeneratePath), generationDelay * 2);
    }
    
    void GenerateMaze()
    {
        // Try different maze generators in order of preference
        if (simpleMazeGenerator != null)
        {
            simpleMazeGenerator.Generate();
            Debug.Log("Generated maze using SimpleMazeGenerator");
        }
        else if (wallLineGenerator != null)
        {
            wallLineGenerator.GenerateWalls();
            Debug.Log("Generated maze using WallLineGenerator");
        }
        else if (traditionalMazeGenerator != null)
        {
            traditionalMazeGenerator.GenerateMaze();
            Debug.Log("Generated maze using MazeGenerator");
        }
        else
        {
            Debug.LogWarning("No maze generator found! Please assign one in the inspector.");
        }
    }
    
    void GeneratePoints()
    {
        // Clear all existing points first
        ClearAllPoints();
        
        // Try different point spawners in order of preference
        if (mazePointSpawner != null)
        {
            // MazePointSpawner works with maze generators
            if (mazePointSpawner.mazeGenerator == null)
            {
                mazePointSpawner.mazeGenerator = traditionalMazeGenerator;
            }
            mazePointSpawner.RespawnPoints();
            Debug.Log("Generated points using MazePointSpawner");
        }
        else if (improvedPointSpawner != null)
        {
            improvedPointSpawner.RespawnPoints();
            Debug.Log("Generated points using ImprovedPointSpawner");
        }
        else if (simplePointSpawner != null)
        {
            simplePointSpawner.RespawnPoints();
            Debug.Log("Generated points using SimplePointSpawner");
        }
        else if (pointSpawner != null)
        {
            pointSpawner.RespawnPoints();
            Debug.Log("Generated points using PointSpawner");
        }
        else
        {
            Debug.LogWarning("No point spawner found! Please assign one in the inspector.");
        }
    }
    
    void ClearAllPoints()
    {
        // Find and destroy all existing points
        GameObject[] allPoints = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allPoints)
        {
            if (obj.name.StartsWith("Point_"))
            {
                DestroyImmediate(obj);
            }
        }
    }
    
    void GeneratePath()
    {
        Debug.Log("Attempting to generate path...");
        
        // Try different pathfinders in order of preference
        if (mazePathfinder != null)
        {
            Debug.Log("Using MazePathfinder for path generation");
            mazePathfinder.GeneratePath();
            Debug.Log("Generated path using MazePathfinder");
        }
        else if (pathfinder != null)
        {
            Debug.Log("Using Pathfinder for path generation");
            pathfinder.GeneratePath();
            Debug.Log("Generated path using Pathfinder");
        }
        else
        {
            Debug.LogWarning("No pathfinder found! Please assign one in the inspector.");
        }
    }
    
    public void ClearAll()
    {
        // Clear maze
        if (simpleMazeGenerator != null)
            simpleMazeGenerator.Clear();
        if (wallLineGenerator != null)
            wallLineGenerator.ClearWalls();
        if (traditionalMazeGenerator != null)
            traditionalMazeGenerator.ClearMaze();
        
        // Clear all points (including duplicates)
        ClearAllPoints();
        
        // Clear path
        if (mazePathfinder != null)
            mazePathfinder.ClearPath();
        if (pathfinder != null)
            pathfinder.ClearPath();
    }
    
    public void RegenerateMaze()
    {
        GenerateCompleteMaze();
    }
    
    public void RegeneratePath()
    {
        if (mazePathfinder != null)
            mazePathfinder.RegeneratePath();
        else if (pathfinder != null)
            pathfinder.RegeneratePath();
    }
    
}
