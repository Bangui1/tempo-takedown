using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Pathfinder : MonoBehaviour
{
    [Header("Path Settings")]
    public GameObject pathLinePrefab;
    public float pathWidth = 0.1f;
    public Color pathColor = Color.blue;
    public Material pathMaterial;
    
    [Header("Pathfinding Settings")]
    public float gridResolution = 0.5f;
    public LayerMask wallLayerMask = -1;
    public float pathSmoothing = 0.5f;
    
    [Header("Debug")]
    public bool showDebugPath = true;
    public bool showGrid = false;
    
    private List<GameObject> pathLines = new List<GameObject>();
    private List<Vector2> waypoints = new List<Vector2>();
    private List<Vector2> optimalPath = new List<Vector2>();
    
    public void GeneratePath()
    {
        // Find all points in the scene
        GameObject[] points = FindAllPoints();
        if (points.Length < 2)
        {
            Debug.LogWarning("Not enough points found for pathfinding!");
            return;
        }
        
        // Clear existing path
        ClearPath();
        
        // Get waypoints from points
        waypoints = GetWaypointsFromPoints(points);
        
        // Find optimal visiting order
        List<int> visitOrder = FindOptimalVisitOrder(waypoints);
        
        // Generate path through all points in optimal order
        optimalPath = GeneratePathThroughPoints(waypoints, visitOrder);
        
        // Create visual path
        CreateVisualPath(optimalPath);
        
        Debug.Log($"Generated path through {waypoints.Count} points with {optimalPath.Count} waypoints");
    }
    
    GameObject[] FindAllPoints()
    {
        List<GameObject> points = new List<GameObject>();
        
        // Find points by name pattern
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Point_"))
            {
                points.Add(obj);
            }
        }
        
        return points.ToArray();
    }
    
    List<Vector2> GetWaypointsFromPoints(GameObject[] points)
    {
        List<Vector2> waypoints = new List<Vector2>();
        
        // Sort points by name to get proper order (START, 1, 2, 3, 4, END)
        System.Array.Sort(points, (a, b) => a.name.CompareTo(b.name));
        
        foreach (GameObject point in points)
        {
            if (point != null)
            {
                waypoints.Add(point.transform.position);
            }
        }
        
        return waypoints;
    }
    
    List<int> FindOptimalVisitOrder(List<Vector2> points)
    {
        if (points.Count <= 2) return new List<int> { 0, 1 };
        
        // For the specific case of START, 1, 2, 3, 4, END, use sequential order
        // This ensures the path goes START → 1 → 2 → 3 → 4 → END
        List<int> order = new List<int>();
        
        // Always start with START (index 0)
        order.Add(0);
        
        // Add numbered points in sequence (1, 2, 3, 4)
        for (int i = 1; i < points.Count - 1; i++)
        {
            order.Add(i);
        }
        
        // Always end with END (last index)
        order.Add(points.Count - 1);
        
        Debug.Log($"Visit order: {string.Join(" → ", order.Select(i => i == 0 ? "START" : i == points.Count - 1 ? "END" : i.ToString()))}");
        
        return order;
    }
    
    List<Vector2> GeneratePathThroughPoints(List<Vector2> points, List<int> visitOrder)
    {
        List<Vector2> fullPath = new List<Vector2>();
        
        for (int i = 0; i < visitOrder.Count - 1; i++)
        {
            Vector2 start = points[visitOrder[i]];
            Vector2 end = points[visitOrder[i + 1]];
            
            List<Vector2> segmentPath = FindPath(start, end);
            fullPath.AddRange(segmentPath);
        }
        
        return fullPath;
    }
    
    List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        // Convert world positions to grid coordinates
        Vector2Int startGrid = WorldToGrid(start);
        Vector2Int endGrid = WorldToGrid(end);
        
        // Use A* pathfinding
        List<Vector2Int> gridPath = AStarPathfinding(startGrid, endGrid);
        
        // Convert grid path back to world coordinates
        List<Vector2> worldPath = new List<Vector2>();
        foreach (Vector2Int gridPos in gridPath)
        {
            worldPath.Add(GridToWorld(gridPos));
        }
        
        // Smooth the path
        return SmoothPath(worldPath);
    }
    
    List<Vector2Int> AStarPathfinding(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        
        // Simple A* implementation
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        
        List<Vector2Int> openSet = new List<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        
        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);
        
        while (openSet.Count > 0)
        {
            // Find node with lowest fScore
            Vector2Int current = openSet[0];
            foreach (Vector2Int node in openSet)
            {
                if (fScore.ContainsKey(node) && fScore[node] < fScore[current])
                {
                    current = node;
                }
            }
            
            if (current == end)
            {
                // Reconstruct path
                path.Add(current);
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Insert(0, current);
                }
                break;
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            // Check neighbors
            Vector2Int[] neighbors = {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 1), new Vector2Int(-1, 1),
                new Vector2Int(1, -1), new Vector2Int(-1, -1)
            };
            
            foreach (Vector2Int neighbor in neighbors)
            {
                Vector2Int next = current + neighbor;
                
                if (closedSet.Contains(next) || !IsWalkable(next))
                    continue;
                
                float tentativeGScore = gScore[current] + Vector2Int.Distance(current, next);
                
                if (!gScore.ContainsKey(next) || tentativeGScore < gScore[next])
                {
                    cameFrom[next] = current;
                    gScore[next] = tentativeGScore;
                    fScore[next] = gScore[next] + Heuristic(next, end);
                    
                    if (!openSet.Contains(next))
                    {
                        openSet.Add(next);
                    }
                }
            }
        }
        
        return path;
    }
    
    bool IsWalkable(Vector2Int gridPos)
    {
        Vector2 worldPos = GridToWorld(gridPos);
        
        // Check for walls using physics overlap
        Collider2D hit = Physics2D.OverlapCircle(worldPos, gridResolution * 0.4f, wallLayerMask);
        return hit == null;
    }
    
    float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }
    
    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / gridResolution),
            Mathf.RoundToInt(worldPos.y / gridResolution)
        );
    }
    
    Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(
            gridPos.x * gridResolution,
            gridPos.y * gridResolution
        );
    }
    
    List<Vector2> SmoothPath(List<Vector2> path)
    {
        if (path.Count <= 2) return path;
        
        List<Vector2> smoothed = new List<Vector2>();
        smoothed.Add(path[0]);
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2 prev = path[i - 1];
            Vector2 current = path[i];
            Vector2 next = path[i + 1];
            
            // Simple smoothing - average with neighbors
            Vector2 smoothedPoint = Vector2.Lerp(current, (prev + next) * 0.5f, pathSmoothing);
            smoothed.Add(smoothedPoint);
        }
        
        smoothed.Add(path[path.Count - 1]);
        return smoothed;
    }
    
    void CreateVisualPath(List<Vector2> path)
    {
        if (path.Count < 2) return;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 start = path[i];
            Vector2 end = path[i + 1];
            
            CreatePathSegment(start, end);
        }
    }
    
    void CreatePathSegment(Vector2 start, Vector2 end)
    {
        GameObject line = new GameObject("PathSegment");
        line.transform.SetParent(transform);
        
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(start.x, start.y, -0.1f));
        lr.SetPosition(1, new Vector3(end.x, end.y, -0.1f));
        
        lr.startWidth = pathWidth;
        lr.endWidth = pathWidth;
        lr.material = pathMaterial != null ? pathMaterial : CreateDefaultMaterial();
        lr.material.color = pathColor;
        lr.sortingOrder = 1;
        
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
        optimalPath.Clear();
    }
    
    public void RegeneratePath()
    {
        ClearPath();
        GeneratePath();
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugPath || optimalPath.Count < 2) return;
        
        Gizmos.color = Color.blue;
        for (int i = 0; i < optimalPath.Count - 1; i++)
        {
            Gizmos.DrawLine(optimalPath[i], optimalPath[i + 1]);
        }
        
        if (showGrid)
        {
            Gizmos.color = Color.gray;
            Vector2 min = new Vector2(-10, -10);
            Vector2 max = new Vector2(10, 10);
            
            for (float x = min.x; x <= max.x; x += gridResolution)
            {
                Gizmos.DrawLine(new Vector3(x, min.y, 0), new Vector3(x, max.y, 0));
            }
            
            for (float y = min.y; y <= max.y; y += gridResolution)
            {
                Gizmos.DrawLine(new Vector3(min.x, y, 0), new Vector3(max.x, y, 0));
            }
        }
    }
}
