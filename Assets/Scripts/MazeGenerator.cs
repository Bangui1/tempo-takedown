using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public GameObject wallPrefab;  // Your amp prefab
    public int mazeWidth = 20;
    public int mazeHeight = 20;
    public float cellSize = 1.0f;
    public Vector2 mazeOffset = Vector2.zero;
    
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
            for (int y = 0; y < mazeHeight; y++)
            {
                if (maze[x, y]) // If it's a wall
                {
                    Vector3 position = new Vector3(
                        x * cellSize + mazeOffset.x,
                        y * cellSize + mazeOffset.y,
                        0
                    );
                    
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
                    wall.name = $"Wall_{x}_{y}";
                    
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
    
    // Method to check if a position is a wall
    public bool IsWall(Vector2 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - mazeOffset.x) / cellSize);
        int y = Mathf.RoundToInt((worldPos.y - mazeOffset.y) / cellSize);
        
        if (x >= 0 && x < mazeWidth && y >= 0 && y < mazeHeight)
        {
            return maze[x, y];
        }
        
        return true; // Outside maze bounds is considered a wall
    }
    
    // Method to get a random empty position in the maze
    public Vector2 GetRandomEmptyPosition()
    {
        List<Vector2> emptyPositions = new List<Vector2>();
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (!maze[x, y]) // If it's not a wall
                {
                    Vector2 pos = new Vector2(
                        x * cellSize + mazeOffset.x,
                        y * cellSize + mazeOffset.y
                    );
                    emptyPositions.Add(pos);
                }
            }
        }
        
        if (emptyPositions.Count > 0)
        {
            return emptyPositions[Random.Range(0, emptyPositions.Count)];
        }
        
        return Vector2.zero; // Fallback
    }
}
