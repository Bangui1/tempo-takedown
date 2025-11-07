using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshSetupVerifier : MonoBehaviour
{
    void Start()
    {
        VerifySetup();
    }
    
    [ContextMenu("Verify NavMesh Setup")]
    public void VerifySetup()
    {
        Debug.Log("=== NavMesh Setup Verification ===");
        
        NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface == null)
        {
            Debug.LogError("❌ NavMeshSurface not found! Create one and assign it.");
            return;
        }
        else
        {
            Debug.Log("✅ NavMeshSurface found");
        }
        
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Untagged");
        int wallCount = 0;
        int obstacleCount = 0;
        int colliderCount = 0;
        
        foreach (GameObject obj in walls)
        {
            if (obj.name.StartsWith("Wall_"))
            {
                wallCount++;
                if (obj.GetComponent<UnityEngine.AI.NavMeshObstacle>() != null)
                {
                    obstacleCount++;
                    UnityEngine.AI.NavMeshObstacle obs = obj.GetComponent<UnityEngine.AI.NavMeshObstacle>();
                    if (!obs.carving)
                    {
                        Debug.LogWarning($"⚠️ Wall {obj.name} has NavMeshObstacle but carving is disabled!");
                    }
                }
                if (obj.GetComponent<Collider>() != null)
                {
                    colliderCount++;
                }
            }
        }
        
        Debug.Log($"Found {wallCount} walls");
        Debug.Log($"  - {obstacleCount} have NavMeshObstacle");
        Debug.Log($"  - {colliderCount} have Colliders");
        
        if (wallCount > 0 && obstacleCount == 0)
        {
            Debug.LogError("❌ No walls have NavMeshObstacle! Walls won't block paths.");
        }
        else if (wallCount > 0 && obstacleCount < wallCount)
        {
            Debug.LogWarning($"⚠️ Only {obstacleCount}/{wallCount} walls have NavMeshObstacle");
        }
        
        Debug.Log("=== Verification Complete ===");
    }
}

