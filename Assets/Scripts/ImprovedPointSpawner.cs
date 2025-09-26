using UnityEngine;
using UnityEngine.UI;

public class ImprovedPointSpawner : MonoBehaviour
{
    public GameObject pointPrefab;
    public Vector2 minPos = new Vector2(-8, -8);
    public Vector2 maxPos = new Vector2(8, 8);
    public float minDistanceFromAmps = 2.0f;  // Increased default distance
    public float minDistanceBetweenPoints = 2.5f;  // Increased default distance
    public int maxAttempts = 100;  // More attempts to find valid positions
    
    private string[] pointLabels = { "START", "1", "2", "3", "4", "END" };
    private GameObject[] spawnedPoints = new GameObject[6];
    
    void Start()
    {
        SpawnPoints();
    }
    
    void SpawnPoints()
    {
        for (int i = 0; i < pointLabels.Length; i++)
        {
            Vector2 spawnPos;
            bool validPos = false;
            int attempts = 0;
            
            do
            {
                float x = Random.Range(minPos.x, maxPos.x);
                float y = Random.Range(minPos.y, maxPos.y);
                spawnPos = new Vector2(x, y);
                
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
        // Check for collision with amps using a larger radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, minDistanceFromAmps);
        foreach (Collider2D col in colliders)
        {
            // Check if it's an amp (you can also check by tag or layer)
            if (col.gameObject.name.Contains("amp") || col.gameObject.name.Contains("Amp"))
            {
                return false;
            }
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
    
    // Method to respawn points (useful for testing)
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
