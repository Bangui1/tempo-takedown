using UnityEngine;

public class AmpSpawner : MonoBehaviour
{
    public GameObject ampPrefab;   // The amp prefab you created
    public Vector2[] spawnPositions;  // Positions where you want the amps
    public int numberOfAmps = 10;
    public Vector2 minPos = new Vector2(-10, -10);
    public Vector2 maxPos = new Vector2(10, 10);

    void Start()
    {
        SpawnAmps();
    }


    void SpawnAmps()
    {
        for (int i = 0; i < numberOfAmps; i++)
        {
            Vector2 spawnPos;
            bool validPos = false;
    
            int attempts = 0;
            do
            {
                float x = Random.Range(minPos.x, maxPos.x);
                float y = Random.Range(minPos.y, maxPos.y);
                spawnPos = new Vector2(x, y);
    
                // Check if another amp is too close
                validPos = !Physics2D.OverlapCircle(spawnPos, 0.5f); 
                // 0.5f is the radius – adjust depending on sprite size
    
                attempts++;
            }
            while (!validPos && attempts < 20);
    
            if (validPos)
                Instantiate(ampPrefab, spawnPos, Quaternion.identity);
        }
    }
}