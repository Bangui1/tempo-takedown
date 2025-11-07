using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

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
            mazeGenerator = FindFirstObjectByType<MazeGenerator>();
        }
        
        if (mazeGenerator != null)
        {
            // Wait a frame to ensure maze is generated and NavMesh is baked
            StartCoroutine(SpawnPointsDelayed());
        }
        else
        {
            Debug.LogError("MazeGenerator not found! Please assign it in the inspector.");
        }
    }
    
    System.Collections.IEnumerator SpawnPointsDelayed()
    {
        yield return new WaitForFixedUpdate();
        yield return null; // Wait one more frame for NavMesh to be ready
        
        // Verify NavMesh exists before spawning
        NavMeshHit testHit;
        bool navMeshExists = NavMesh.SamplePosition(Vector3.zero, out testHit, 50f, NavMesh.AllAreas);
        if (!navMeshExists)
        {
            Debug.LogError("NavMesh not found! Please bake NavMesh before spawning points. Points will still spawn but may not be on NavMesh.");
        }
        else
        {
            Debug.Log($"NavMesh verified at center. Proceeding to spawn points...");
        }
        
        SpawnPoints();
    }
    
    void SpawnPoints()
    {
        for (int i = 0; i < pointLabels.Length; i++)
        {
            Vector3 spawnPos;
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
                    spawnPos = new Vector3(
                        Random.Range(-8f, 8f),
                        0f,
                        Random.Range(-8f, 8f)
                    );
                }
                
                validPos = IsValidPosition(spawnPos, i);
                attempts++;
            }
            while (!validPos && attempts < maxAttempts);
            
            if (validPos)
            {
                // Ensure point is at ground level (Y=0) for NavMesh
                Vector3 pointPos = new Vector3(spawnPos.x, 0f, spawnPos.z);
                // Use same rotation as walls if points are 2D sprites on XZ plane
                GameObject point = Instantiate(pointPrefab, pointPos, Quaternion.Euler(90, 0, 0));
                point.name = "Point_" + pointLabels[i];
                
                // Force position to ground level after instantiation (in case prefab has offset)
                point.transform.position = new Vector3(pointPos.x, 0f, pointPos.z);
                
                // Verify position is correct
                if (Mathf.Abs(point.transform.position.y) > 0.01f)
                {
                    Debug.LogWarning($"{point.name} Y position is {point.transform.position.y}, forcing to 0");
                    point.transform.position = new Vector3(point.transform.position.x, 0f, point.transform.position.z);
                }
                
                // Ensure a world-space Canvas and Text exist and are centered on the point
                Text textComponent = point.GetComponentInChildren<Text>();
                if (textComponent == null)
                {
                    // Create a Canvas child if missing
                    GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform));
                    canvasGO.transform.SetParent(point.transform, false);
                    Canvas canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.WorldSpace;
                    CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referencePixelsPerUnit = 100f;
                    canvasGO.AddComponent<GraphicRaycaster>();
                    
                    // Create Text under the Canvas
                    GameObject textGO = new GameObject("Text", typeof(RectTransform));
                    textGO.transform.SetParent(canvasGO.transform, false);
                    textComponent = textGO.AddComponent<Text>();
                    textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    textComponent.fontSize = 24;
                }
                
                // Center alignment and zero local offset
                RectTransform textRT = textComponent.rectTransform;
                textRT.anchorMin = new Vector2(0.5f, 0.5f);
                textRT.anchorMax = new Vector2(0.5f, 0.5f);
                textRT.pivot = new Vector2(0.5f, 0.5f);
                textRT.anchoredPosition = Vector2.zero;
                textRT.localPosition = Vector3.zero;
                textComponent.alignment = TextAnchor.MiddleCenter;
                
                // Set label and color
                textComponent.text = pointLabels[i];
                if (i == 0)
                    textComponent.color = Color.green;
                else if (i == pointLabels.Length - 1)
                    textComponent.color = Color.red;
                else
                    textComponent.color = Color.yellow;
                
                spawnedPoints[i] = point;
                
                // Force Y=0 one more time before NavMesh check
                point.transform.position = new Vector3(point.transform.position.x, 0f, point.transform.position.z);
                
                // Try to snap point to NavMesh (try larger radius if first attempt fails)
                NavMeshHit hit;
                bool onNavMesh = NavMesh.SamplePosition(point.transform.position, out hit, 5f, NavMesh.AllAreas);
                
                if (onNavMesh)
                {
                    // Always snap to NavMesh position to ensure it's exactly on the surface
                    point.transform.position = new Vector3(hit.position.x, 0f, hit.position.z);
                    Debug.Log($"{pointLabels[i]} snapped to NavMesh at {point.transform.position}");
                }
                else
                {
                    // Try with even larger radius
                    onNavMesh = NavMesh.SamplePosition(point.transform.position, out hit, 10f, NavMesh.AllAreas);
                    if (onNavMesh)
                    {
                        point.transform.position = new Vector3(hit.position.x, 0f, hit.position.z);
                        Debug.Log($"{pointLabels[i]} snapped to NavMesh (with larger radius) at {point.transform.position}");
                    }
                    else
                    {
                        Debug.LogError($"{point.name} at {point.transform.position} cannot find NavMesh within 10 units! Ensure NavMesh is baked and covers this area.");
                    }
                }
                
                // Final Y=0 enforcement
                point.transform.position = new Vector3(point.transform.position.x, 0f, point.transform.position.z);
                
                Debug.Log($"Spawned {pointLabels[i]} at {point.transform.position} (attempt {attempts}, onNavMesh: {onNavMesh})");
            }
            else
            {
                Debug.LogWarning($"Failed to spawn {pointLabels[i]} after {maxAttempts} attempts!");
            }
        }
    }
    
    bool IsValidPosition(Vector3 pos, int pointIndex)
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
                float distance = Vector3.Distance(pos, spawnedPoints[i].transform.position);
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
