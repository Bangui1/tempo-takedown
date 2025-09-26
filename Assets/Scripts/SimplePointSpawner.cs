using UnityEngine;
using UnityEngine.UI;

public class SimplePointSpawner : MonoBehaviour
{
    public GameObject pointPrefab;
    public Vector2 minPos = new Vector2(-8, -8);
    public Vector2 maxPos = new Vector2(8, 8);
    public float minDistanceFromAmps = 1.5f;
    public float minDistanceBetweenPoints = 2f;
    
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
            while (!validPos && attempts < 50);
            
            if (validPos)
            {
                GameObject point = Instantiate(pointPrefab, spawnPos, Quaternion.identity);
                point.name = "Point_" + pointLabels[i];
                
                Text textComponent = point.GetComponent<Text>();
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
            }
        }
    }
    
    bool IsValidPosition(Vector2 pos, int pointIndex)
    {
        if (Physics2D.OverlapCircle(pos, minDistanceFromAmps))
            return false;
        
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
}
