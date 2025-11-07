using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshObstacleVerifier : MonoBehaviour
{
    [ContextMenu("Verify Obstacles Block Paths")]
    public void VerifyObstacles()
    {
        Debug.Log("=== NavMeshObstacle Verification ===");
        
        NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface == null)
        {
            Debug.LogError("NavMeshSurface not found!");
            return;
        }
        
        GameObject[] allWalls = GameObject.FindGameObjectsWithTag("Untagged");
        int wallCount = 0;
        int obstacleCount = 0;
        int carvingCount = 0;
        
        foreach (GameObject obj in allWalls)
        {
            if (obj.name.StartsWith("Wall_"))
            {
                wallCount++;
                NavMeshObstacle obs = obj.GetComponent<NavMeshObstacle>();
                if (obs != null)
                {
                    obstacleCount++;
                    if (obs.carving)
                    {
                        carvingCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"Wall {obj.name} has NavMeshObstacle but carving is FALSE!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Wall {obj.name} has NO NavMeshObstacle component!");
                }
            }
        }
        
        Debug.Log($"Total walls: {wallCount}");
        Debug.Log($"Walls with NavMeshObstacle: {obstacleCount}");
        Debug.Log($"Walls with carving enabled: {carvingCount}");
        
        if (carvingCount < wallCount)
        {
            Debug.LogError($"Only {carvingCount}/{wallCount} walls have carving enabled! This is why paths go through walls.");
        }
        
        // Test if a path goes through a wall
        if (wallCount > 0)
        {
            GameObject testWall = GameObject.Find("Wall_10_10");
            if (testWall != null)
            {
                Vector3 wallPos = testWall.transform.position;
                Vector3 testStart = wallPos + Vector3.left * 2f;
                Vector3 testEnd = wallPos + Vector3.right * 2f;
                
                NavMeshPath testPath = new NavMeshPath();
                if (NavMesh.CalculatePath(testStart, testEnd, NavMesh.AllAreas, testPath))
                {
                    bool goesThroughWall = false;
                    foreach (Vector3 corner in testPath.corners)
                    {
                        if (Vector3.Distance(corner, wallPos) < 0.5f)
                        {
                            goesThroughWall = true;
                            break;
                        }
                    }
                    
                    if (goesThroughWall)
                    {
                        Debug.LogError($"Path goes through wall at {wallPos}! NavMeshObstacle carving is not working.");
                    }
                    else
                    {
                        Debug.Log($"Path correctly avoids wall at {wallPos}. Obstacles are working!");
                    }
                }
            }
        }
        
        Debug.Log("=== Verification Complete ===");
    }
}

