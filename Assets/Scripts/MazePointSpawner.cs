using UnityEngine;
using UnityEngine.UI;

public class MazePointSpawner : MonoBehaviour
{
    [Header("Point Settings")]
    public GameObject pointPrefab;
    public float minDistanceBetweenPoints = 3f;
    
    [Header("Maze Integration")]
    public MazeGenerator mazeGenerator;
    
    private string[] pointLabels = { "START", "1", "2", "3", "4", "END" };
    private GameObject[] spawnedPoints = new GameObject[6];
    
    void Start()
    {
        if (mazeGenerator == null)
        {
            mazeGenerator = FindObjectOfType<MazeGenerator>();
        }
        
        if (mazeGenerator != null)
        {
            SpawnPoints();
        }
        else
        {
            Debug.LogError("MazeGenerator not found! Please assign it in the inspector.");
        }
    }
    
    void SpawnPoints()
    {
        for (int i = 0; i < pointLabels.Length; i++)
        {
            Vector2 spawnPos;
            bool validPos = false;
            int attempts = 0;
            int maxAttempts = 100;
            
            do
            {
                // Try to get a random empty position from the maze first
                if (mazeGenerator != null)
                {
                    spawnPos = mazeGenerator.GetRandomEmptyPosition();
                }
                else
                {
                    // Fallback to random position
                    spawnPos = new Vector2(
                        Random.Range(-8f, 8f),
                        Random.Range(-8f, 8f)
                    );
                }
                
                validPos = IsValidPosition(spawnPos, i);
                attempts++;
            }
            while (!validPos && attempts < maxAttempts);
            
            if (validPos)
            {
                GameObject point = Instantiate(pointPrefab, spawnPos, Quaternion.identity);
                point.name = "Point_" + pointLabels[i];
                
                // Try to find Text component in children (for Canvas setup)
                Text textComponent = point.GetComponentInChildren<Text>();
                if (textComponent == null)
                    textComponent = point.GetComponent<Text>();
                
                if (textComponent != null)
                {
                    textComponent.text = pointLabels[i];
                    
                    if (i == 0)
                        textComponent.color = Color.green;
                    else if (i == pointLabels.Length - 1)
                        textComponent.color = Color.red;
                    else
                        textComponent.color = Color.yellow;
                }
                
                spawnedPoints[i] = point;
                Debug.Log($"Spawned {pointLabels[i]} at {spawnPos} (attempt {attempts})");
            }
            else
            {
                Debug.LogWarning($"Failed to spawn {pointLabels[i]} after {maxAttempts} attempts!");
            }
        }
    }
    
    bool IsValidPosition(Vector2 pos, int pointIndex)
    {
        // Check if position is not inside a wall
        if (mazeGenerator != null && mazeGenerator.IsWall(pos))
        {
            return false;
        }
        
        // Check distance from other points
        for (int i = 0; i < pointIndex; i++)
        {
            if (spawnedPoints[i] != null)
            {
                float distance = Vector2.Distance(pos, spawnedPoints[i].transform.position);
                if (distance < minDistanceBetweenPoints)
                    return false;
            }
        }
        
        return true;
    }
    
    public GameObject[] GetSpawnedPoints()
    {
        return spawnedPoints;
    }
    
    public void RespawnPoints()
    {
        // Clear existing points
        foreach (GameObject point in spawnedPoints)
        {
            if (point != null)
                DestroyImmediate(point);
        }
        
        // Reset array
        spawnedPoints = new GameObject[6];
        
        // Spawn new points
        SpawnPoints();
    }
}
