using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public GameObject wallPrefab;  // Your amp prefab
    public int mazeWidth = 20;
    public int mazeHeight = 20;
    public float cellSize = 1.0f;
    public Vector3 mazeOffset = Vector3.zero;
    
    [Header("Maze Generation")]
    public bool generateOnStart = true;
    public bool clearExistingWalls = true;
    
    [Header("Wall Types")]
    public GameObject[] wallSprites;  // Different wall sprites for variety
    
    private bool[,] maze;
    private List<GameObject> spawnedWalls = new List<GameObject>();
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateMaze();
        }
    }
    
    public void GenerateMaze()
    {
        if (clearExistingWalls)
        {
            ClearExistingWalls();
        }
        
        InitializeMaze();
        CreateMaze();
        BuildWalls();
    }
    
    void InitializeMaze()
    {
        maze = new bool[mazeWidth, mazeHeight];
        
        // Initialize all cells as walls (true = wall, false = path)
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = true;
            }
        }
    }
    
    void CreateMaze()
    {
        // Start from a random cell
        int startX = Random.Range(1, mazeWidth - 1);
        int startY = Random.Range(1, mazeHeight - 1);
        
        // Use recursive backtracking to create the maze
        List<Vector2Int> stack = new List<Vector2Int>();
        stack.Add(new Vector2Int(startX, startY));
        maze[startX, startY] = false; // Make it a path
        
        while (stack.Count > 0)
        {
            Vector2Int current = stack[stack.Count - 1];
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);
            
            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];
                
                // Remove wall between current and next
                Vector2Int wall = new Vector2Int(
                    (current.x + next.x) / 2,
                    (current.y + next.y) / 2
                );
                maze[wall.x, wall.y] = false;
                maze[next.x, next.y] = false;
                
                stack.Add(next);
            }
            else
            {
                stack.RemoveAt(stack.Count - 1);
            }
        }
        
        // Add some random openings for more interesting paths
        AddRandomOpenings();
    }
    
    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        
        // Check all 4 directions
        Vector2Int[] directions = {
            new Vector2Int(0, 2),   // Up
            new Vector2Int(2, 0),   // Right
            new Vector2Int(0, -2),  // Down
            new Vector2Int(-2, 0)   // Left
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            
            if (IsValidCell(neighbor) && maze[neighbor.x, neighbor.y])
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    bool IsValidCell(Vector2Int cell)
    {
        return cell.x > 0 && cell.x < mazeWidth - 1 && 
               cell.y > 0 && cell.y < mazeHeight - 1;
    }
    
    void AddRandomOpenings()
    {
        // Add some random openings to make the maze less predictable
        int openings = Random.Range(3, 8);
        
        for (int i = 0; i < openings; i++)
        {
            int x = Random.Range(1, mazeWidth - 1);
            int y = Random.Range(1, mazeHeight - 1);
            maze[x, y] = false;
        }
    }
    
    void BuildWalls()
    {
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                if (maze[x, z]) // If it's a wall
                {
                    Vector3 position = new Vector3(
                        x * cellSize + mazeOffset.x,
                        0f, // Always at ground level for NavMesh
                        z * cellSize + mazeOffset.z
                    );
                    
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.Euler(90, 0, 0));
                    wall.name = $"Wall_{x}_{z}";
                    
                    // Ensure wall has a collider for NavMesh
                    Collider wallCollider = wall.GetComponent<Collider>();
                    if (wallCollider == null)
                    {
                        BoxCollider box = wall.AddComponent<BoxCollider>();
                        box.size = new Vector3(cellSize, 0.5f, cellSize);
                    }
                    
                    // Mark wall to be excluded from NavMesh baking
                    // Walls will block paths via NavMeshObstacle carving instead
                    wall.layer = LayerMask.NameToLayer("Default");
                    
                    // Add NavMeshObstacle so wall blocks pathfinding
                    NavMeshObstacle obstacle = wall.GetComponent<NavMeshObstacle>();
                    if (obstacle == null)
                    {
                        obstacle = wall.AddComponent<NavMeshObstacle>();
                    }
                    obstacle.carving = true;
                    obstacle.shape = NavMeshObstacleShape.Box;
                    obstacle.size = new Vector3(cellSize, 0.5f, cellSize);
                    obstacle.center = Vector3.zero;
                    obstacle.enabled = true;
                    
                    // Verify obstacle is set up correctly
                    if (!obstacle.carving)
                    {
                        Debug.LogWarning($"Wall {wall.name} NavMeshObstacle carving is disabled!");
                    }
                    
                    // Add some variety to wall sprites
                    if (wallSprites != null && wallSprites.Length > 0)
                    {
                        SpriteRenderer spriteRenderer = wall.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            GameObject randomWallSprite = wallSprites[Random.Range(0, wallSprites.Length)];
                            SpriteRenderer randomSpriteRenderer = randomWallSprite.GetComponent<SpriteRenderer>();
                            if (randomSpriteRenderer != null)
                            {
                                spriteRenderer.sprite = randomSpriteRenderer.sprite;
                            }
                        }
                    }
                    
                    spawnedWalls.Add(wall);
                }
            }
        }
        
        // Rebake NavMesh after all walls are placed (wait a frame for obstacles to register)
        StartCoroutine(RebakeNavMeshDelayed());
    }
    
    System.Collections.IEnumerator RebakeNavMeshDelayed()
    {
        yield return new WaitForFixedUpdate();
        yield return null; // Wait one more frame for obstacles to fully register
        
        // Verify obstacles are set up
        int obstaclesWithCarving = 0;
        foreach (GameObject wall in spawnedWalls)
        {
            if (wall != null)
            {
                NavMeshObstacle obs = wall.GetComponent<NavMeshObstacle>();
                if (obs != null && obs.carving)
                {
                    obstaclesWithCarving++;
                }
            }
        }
        Debug.Log($"Found {obstaclesWithCarving} walls with NavMeshObstacle carving enabled");
        
        RebakeNavMesh();
        
        // Wait for NavMesh to update after rebake
        yield return new WaitForFixedUpdate();
        Debug.Log("NavMesh updated with obstacles");
    }
    
    void ClearExistingWalls()
    {
        foreach (GameObject wall in spawnedWalls)
        {
            if (wall != null)
            {
                DestroyImmediate(wall);
            }
        }
        spawnedWalls.Clear();
    }
    
    public void ClearMaze()
    {
        ClearExistingWalls();
    }
    
    public void RegenerateMaze()
    {
        ClearMaze();
        GenerateMaze();
    }
    
    public void RebakeNavMesh()
    {
        NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface != null)
        {
            Debug.Log($"Rebaking NavMesh with {spawnedWalls.Count} walls as NavMeshObstacles...");
            Debug.Log("Note: Configure NavMeshSurface in Inspector to exclude wall layers from baking to avoid 'excessive tiles' error");
            surface.BuildNavMesh();
            Debug.Log("NavMesh rebaked after maze generation");
        }
        else
        {
            Debug.LogError("NavMeshSurface not found! Please add a NavMeshSurface component to a GameObject in your scene.");
        }
    }
    
    // Method to check if a position is a wall
    public bool IsWall(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - mazeOffset.x) / cellSize);
        int z = Mathf.RoundToInt((worldPos.z - mazeOffset.z) / cellSize);
        
        if (x >= 0 && x < mazeWidth && z >= 0 && z < mazeHeight)
        {
            return maze[x, z];
        }
        
        return true; // Outside maze bounds is considered a wall
    }
    
    // Method to get a random empty position in the maze
    public Vector3 GetRandomEmptyPosition()
    {
        List<Vector3> emptyPositions = new List<Vector3>();
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                if (!maze[x, z]) // If it's not a wall
                {
                    Vector3 pos = new Vector3(
                        x * cellSize + mazeOffset.x,
                        0f, // Always at ground level for NavMesh
                        z * cellSize + mazeOffset.z
                    );
                    emptyPositions.Add(pos);
                }
            }
        }
        
        if (emptyPositions.Count > 0)
        {
            return emptyPositions[Random.Range(0, emptyPositions.Count)];
        }
        
        return Vector3.zero; // Fallback
    }
}
