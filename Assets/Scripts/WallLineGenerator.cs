using UnityEngine;
using System.Collections.Generic;

public class WallLineGenerator : MonoBehaviour
{
    [Header("Wall Settings")]
    public GameObject wallPrefab;  // Your amp prefab
    public int numberOfWallLines = 6;
    public float minWallLength = 4f;
    public float maxWallLength = 10f;
    public Vector2 spawnAreaMin = new Vector2(-8, -8);
    public Vector2 spawnAreaMax = new Vector2(8, 8);
    
    [Header("Spacing & Alignment")]
    public float minWallDistance = 3f;
    public bool snapToGrid = true;
    public float gridSnapSize = 1.0f;
    
    [Header("Generation")]
    public bool generateOnStart = true;
    public bool clearExistingWalls = true;
    
    private List<GameObject> spawnedWalls = new List<GameObject>();
    private List<Vector2> wallLineCenters = new List<Vector2>();
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateWalls();
        }
    }
    
    public void GenerateWalls()
    {
        if (clearExistingWalls)
        {
            ClearExistingWalls();
        }
        
        CreateWallLines();
    }
    
    void CreateWallLines()
    {
        for (int i = 0; i < numberOfWallLines; i++)
        {
            CreateRandomWallLine();
        }
    }
    
    void CreateRandomWallLine()
    {
        int attempts = 0;
        int maxAttempts = 30;
        
        while (attempts++ < maxAttempts)
        {
            // Choose random direction (horizontal or vertical)
            bool isHorizontal = Random.Range(0, 2) == 0;
            
            Vector2 startPos;
            Vector2 direction;
            float wallLength = Random.Range(minWallLength, maxWallLength);
            
            if (isHorizontal)
            {
                // Horizontal wall
                startPos = new Vector2(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x - wallLength),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y)
                );
                direction = Vector2.right;
            }
            else
            {
                // Vertical wall
                startPos = new Vector2(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y - wallLength)
                );
                direction = Vector2.up;
            }
            
            // Snap to grid if enabled
            if (snapToGrid)
            {
                startPos.x = Mathf.Round(startPos.x / gridSnapSize) * gridSnapSize;
                startPos.y = Mathf.Round(startPos.y / gridSnapSize) * gridSnapSize;
            }
            
            // Check if this wall line has minimum distance from others
            Vector2 wallCenter = startPos + direction * (wallLength * 0.5f);
            if (HasMinimumDistance(wallCenter))
            {
                // Create wall segments along the line
                int segments = Mathf.RoundToInt(wallLength);
                bool allPositionsClear = true;
                
                for (int i = 0; i < segments; i++)
                {
                    Vector2 segmentPos = startPos + direction * i;
                    if (!IsPositionClear(segmentPos))
                    {
                        allPositionsClear = false;
                        break;
                    }
                }
                
                if (allPositionsClear)
                {
                    // Create all wall segments
                    for (int i = 0; i < segments; i++)
                    {
                        Vector2 segmentPos = startPos + direction * i;
                        CreateWall(segmentPos);
                    }
                    wallLineCenters.Add(wallCenter);
                    return; // Successfully placed wall line
                }
            }
        }
    }
    
    void CreateWall(Vector2 position)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
        wall.name = $"Wall_{spawnedWalls.Count}";
        spawnedWalls.Add(wall);
    }
    
    bool IsPositionClear(Vector2 position)
    {
        // Check if there's already a wall at this position
        Collider2D existingWall = Physics2D.OverlapCircle(position, 0.5f);
        return existingWall == null;
    }
    
    bool HasMinimumDistance(Vector2 wallCenter)
    {
        foreach (Vector2 existingCenter in wallLineCenters)
        {
            float distance = Vector2.Distance(wallCenter, existingCenter);
            if (distance < minWallDistance)
            {
                return false;
            }
        }
        return true;
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
        wallLineCenters.Clear();
    }
    
    public void ClearWalls()
    {
        ClearExistingWalls();
    }
    
    public void RegenerateWalls()
    {
        ClearWalls();
        GenerateWalls();
    }
    
    // Method to check if a position is a wall
    public bool IsWall(Vector2 worldPos)
    {
        Collider2D wall = Physics2D.OverlapCircle(worldPos, 0.5f);
        return wall != null;
    }
    
    // Method to get a random empty position
    public Vector2 GetRandomEmptyPosition()
    {
        Vector2 randomPos;
        int attempts = 0;
        int maxAttempts = 50;
        
        do
        {
            randomPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            attempts++;
        }
        while (IsWall(randomPos) && attempts < maxAttempts);
        
        return randomPos;
    }
}
