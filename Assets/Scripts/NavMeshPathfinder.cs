using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class NavMeshPathfinder : MonoBehaviour
{
    [Header("Path Settings")]
    public float pathWidth = 0.1f;
    public Color pathColor = Color.cyan;
    public Material pathMaterial;
    
    [Header("NavMesh")]
    public NavMeshSurface navMeshSurface;
    
    [Header("Debug")]
    public bool showDebugPath = true;
    
    private List<GameObject> pathLines = new List<GameObject>();
    private List<Vector3> currentPath = new List<Vector3>();
    
    void Start()
    {
        if (navMeshSurface == null)
        {
            navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
        }
    }
    
    public void GeneratePath()
    {
        if (navMeshSurface == null)
        {
            navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                Debug.LogError("NavMeshSurface not found! Cannot generate path.");
                return;
            }
        }
        
        // Ensure NavMesh is baked
        if (!NavMesh.SamplePosition(Vector3.zero, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            Debug.LogWarning("NavMesh not baked! Attempting to bake now...");
            navMeshSurface.BuildNavMesh();
        }
        
        GameObject[] points = FindAllPoints();
        if (points.Length < 2)
        {
            Debug.LogWarning($"Not enough points found for pathfinding! Found {points.Length} points.");
            return;
        }
        
        ClearPath();
        
        List<Vector3> fullPath = new List<Vector3>();
        int successfulSegments = 0;
        
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 start = points[i].transform.position;
            Vector3 end = points[i + 1].transform.position;
            
            // Try to find nearest NavMesh point
            NavMeshHit startHit, endHit;
            bool startOnNavMesh = NavMesh.SamplePosition(start, out startHit, 5f, NavMesh.AllAreas);
            bool endOnNavMesh = NavMesh.SamplePosition(end, out endHit, 5f, NavMesh.AllAreas);
            
            if (!startOnNavMesh)
            {
                Debug.LogWarning($"{points[i].name} at {start} is not on NavMesh!");
            }
            if (!endOnNavMesh)
            {
                Debug.LogWarning($"{points[i + 1].name} at {end} is not on NavMesh!");
            }
            
            NavMeshPath segmentPath = new NavMeshPath();
            Vector3 pathStart = startOnNavMesh ? startHit.position : start;
            Vector3 pathEnd = endOnNavMesh ? endHit.position : end;
            
            if (NavMesh.CalculatePath(pathStart, pathEnd, NavMesh.AllAreas, segmentPath))
            {
                if (segmentPath.status == NavMeshPathStatus.PathComplete)
                {
                    successfulSegments++;
                    for (int j = 0; j < segmentPath.corners.Length; j++)
                    {
                        if (fullPath.Count == 0 || Vector3.Distance(fullPath[fullPath.Count - 1], segmentPath.corners[j]) > 0.01f)
                        {
                            fullPath.Add(segmentPath.corners[j]);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Path incomplete between {points[i].name} and {points[i + 1].name}: {segmentPath.status}");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to calculate path between {points[i].name} and {points[i + 1].name}");
            }
        }
        
        currentPath = fullPath;
        CreateVisualPath(currentPath);
        
        Debug.Log($"Generated NavMesh path: {successfulSegments}/{points.Length - 1} segments successful, {currentPath.Count} waypoints");
    }
    
    GameObject[] FindAllPoints()
    {
        List<GameObject> allPoints = new List<GameObject>();
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Point_"))
            {
                allPoints.Add(obj);
            }
        }
        
        GameObject start = null;
        GameObject end = null;
        List<(int num, GameObject go)> numbered = new List<(int, GameObject)>();
        
        foreach (GameObject p in allPoints)
        {
            if (p == null) continue;
            
            if (p.name.Equals("Point_START"))
                start = p;
            else if (p.name.Equals("Point_END"))
                end = p;
            else
            {
                string tag = p.name.Substring("Point_".Length);
                if (int.TryParse(tag, out int n))
                {
                    numbered.Add((n, p));
                }
            }
        }
        
        numbered.Sort((a, b) => a.num.CompareTo(b.num));
        
        List<GameObject> ordered = new List<GameObject>();
        if (start != null) ordered.Add(start);
        foreach (var tup in numbered) ordered.Add(tup.go);
        if (end != null) ordered.Add(end);
        
        return ordered.ToArray();
    }
    
    void CreateVisualPath(List<Vector3> path)
    {
        if (path.Count < 2) return;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            CreatePathSegment(path[i], path[i + 1]);
        }
    }
    
    void CreatePathSegment(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("PathSegment");
        line.transform.SetParent(transform);
        
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start + Vector3.up * 0.1f);
        lr.SetPosition(1, end + Vector3.up * 0.1f);
        
        lr.startWidth = pathWidth;
        lr.endWidth = pathWidth;
        lr.material = pathMaterial != null ? pathMaterial : CreateDefaultMaterial();
        lr.material.color = pathColor;
        
        pathLines.Add(line);
    }
    
    Material CreateDefaultMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = pathColor;
        return mat;
    }
    
    public void ClearPath()
    {
        foreach (GameObject line in pathLines)
        {
            if (line != null)
            {
                DestroyImmediate(line);
            }
        }
        pathLines.Clear();
        currentPath.Clear();
    }
    
    public void RegeneratePath()
    {
        ClearPath();
        GeneratePath();
    }
    
    public bool HasValidPath()
    {
        return currentPath != null && currentPath.Count >= 2;
    }
    
    public List<Vector3> GetPath()
    {
        return new List<Vector3>(currentPath);
    }
    
    public void RebakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh rebaked");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugPath || currentPath.Count < 2) return;
        
        Gizmos.color = pathColor;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
    }
}


