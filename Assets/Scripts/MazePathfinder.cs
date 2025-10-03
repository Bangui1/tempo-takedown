using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MazePathfinder : MonoBehaviour
{
    [Header("Path Settings")]
    public GameObject pathLinePrefab;
    public float pathWidth = 0.2f;
    public Color pathColor = Color.cyan;
    public Material pathMaterial;
    
    [Header("Pathfinding Settings")]
    public float gridResolution = 0.1f; // Much higher resolution for better wall avoidance
    public LayerMask wallLayerMask = -1;
    public float pathSmoothing = 0.8f; // Much more smoothing for better curves
    public int maxPathfindingIterations = 5000; // Many more iterations for complex mazes
    
    [Header("Integration")]
    public SimpleMazeGenerator mazeGenerator;
    public WallLineGenerator wallGenerator;
    public MazeGenerator traditionalMazeGenerator;
    
    [Header("Debug")]
    public bool showDebugPath = true;
    public bool showGrid = false;
    public bool showWaypoints = false; // Disabled by default to prevent errors
    
    private List<GameObject> pathLines = new List<GameObject>();
    private List<Vector2> waypoints = new List<Vector2>();
    private List<Vector2> optimalPath = new List<Vector2>();
    private List<GameObject> debugWaypoints = new List<GameObject>();
    
    [Header("Lifecycle")]
    public bool generateOnStart = false;

    void Start()
    {
        // Auto-find maze generators if not assigned
        if (mazeGenerator == null)
            mazeGenerator = FindFirstObjectByType<SimpleMazeGenerator>();
        if (wallGenerator == null)
            wallGenerator = FindFirstObjectByType<WallLineGenerator>();
        if (traditionalMazeGenerator == null)
            traditionalMazeGenerator = FindFirstObjectByType<MazeGenerator>();
        
        if (generateOnStart)
        {
            // Generate path after a short delay to ensure everything is spawned
            Invoke(nameof(GeneratePath), 0.5f);
        }
    }
    
    public void GeneratePath()
    {
        // Find all points in the scene
        GameObject[] points = FindAllPoints();
        if (points.Length < 2)
        {
            Debug.LogWarning("Not enough points found for pathfinding! Found: " + points.Length);
            return;
        }
        
        // Clear existing path
        ClearPath();
        
        // Get waypoints from points
        waypoints = GetWaypointsFromPoints(points);
        
        // Find optimal visiting order using TSP approximation
        List<int> visitOrder = FindOptimalVisitOrder(waypoints);
        
        // Generate path through all points in optimal order
        optimalPath = GeneratePathThroughPoints(waypoints, visitOrder);
        
        // Create visual path
        CreateVisualPath(optimalPath);
        
        // Create debug waypoints
        if (showWaypoints)
        {
            CreateDebugWaypoints(waypoints);
        }
        
        Debug.Log($"Generated path through {waypoints.Count} points with {optimalPath.Count} waypoints");
    }
    
    GameObject[] FindAllPoints()
    {
        List<GameObject> points = new List<GameObject>();
        
        // Find points by name pattern
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Point_"))
            {
                points.Add(obj);
            }
        }
        
        // Sort by name to get proper order
        points.Sort((a, b) => a.name.CompareTo(b.name));
        
        return points.ToArray();
    }
    
    List<Vector2> GetWaypointsFromPoints(GameObject[] points)
    {
        // Strict ordering: START -> numeric ascending -> END
        Dictionary<string, GameObject> nameToObj = new Dictionary<string, GameObject>();
        List<(int num, GameObject go)> numbered = new List<(int, GameObject)>();
        GameObject start = null, end = null;

        foreach (GameObject p in points)
        {
            if (p == null) continue;
            nameToObj[p.name] = p;
            if (p.name.Equals("Point_START")) start = p;
            else if (p.name.Equals("Point_END")) end = p;
            else if (p.name.StartsWith("Point_"))
            {
                string tag = p.name.Substring("Point_".Length);
                if (int.TryParse(tag, out int n))
                {
                    numbered.Add((n, p));
                }
            }
        }

        numbered.Sort((a, b) => a.num.CompareTo(b.num));

        List<Vector2> ordered = new List<Vector2>();
        if (start != null) ordered.Add(GetPointWorldPosition(start));
        foreach (var tup in numbered) ordered.Add(GetPointWorldPosition(tup.go));
        if (end != null) ordered.Add(GetPointWorldPosition(end));

        Debug.Log("Ordered points:");
        for (int i = 0; i < ordered.Count; i++) Debug.Log($"[{i}] {ordered[i]}");

        return ordered;
    }

    Vector2 GetPointWorldPosition(GameObject point)
    {
        // Prefer the visible bounds center if available (handles labels/sprites)
        Renderer r = point.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            Vector3 c = r.bounds.center;
            return new Vector2(c.x, c.y);
        }

        RectTransform rt = point.GetComponentInChildren<RectTransform>();
        if (rt != null)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            return new Vector2(center.x, center.y);
        }

        return point.transform.position;
    }
    
    List<int> FindOptimalVisitOrder(List<Vector2> points)
    {
        if (points.Count <= 2) return new List<int> { 0, 1 };
        
        // For the specific case of START, 1, 2, 3, 4, END, use sequential order
        // This ensures the path goes START → 1 → 2 → 3 → 4 → END
        List<int> order = new List<int>();
        
        Debug.Log($"Total points found: {points.Count}");
        
        // Always start with START (index 0)
        order.Add(0);
        Debug.Log($"Added START (index 0)");
        
        // Add numbered points in sequence (1, 2, 3, 4)
        for (int i = 1; i < points.Count - 1; i++)
        {
            order.Add(i);
            Debug.Log($"Added point {i} (index {i})");
        }
        
        // Always end with END (last index)
        order.Add(points.Count - 1);
        Debug.Log($"Added END (index {points.Count - 1})");
        
        Debug.Log($"Final visit order: {string.Join(" → ", order.Select(i => i == 0 ? "START" : i == points.Count - 1 ? "END" : i.ToString()))}");
        
        return order;
    }
    
    float GetPathfindingDistance(Vector2 start, Vector2 end)
    {
        // Use A* to find actual path distance
        List<Vector2> path = FindPath(start, end);
        if (path.Count < 2) return Vector2.Distance(start, end);
        
        float totalDistance = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalDistance += Vector2.Distance(path[i], path[i + 1]);
        }
        
        return totalDistance;
    }
    
    List<Vector2> GeneratePathThroughPoints(List<Vector2> points, List<int> visitOrder)
    {
        List<Vector2> fullPath = new List<Vector2>();
        
        for (int i = 0; i < visitOrder.Count - 1; i++)
        {
            Vector2 start = points[visitOrder[i]];
            Vector2 end = points[visitOrder[i + 1]];
            
            List<Vector2> segmentPath = FindPath(start, end);
            if (segmentPath.Count > 0)
            {
                // Add start point if it's the first segment
                if (i == 0)
                {
                    fullPath.Add(start);
                }
                
                // Add path points (skip first to avoid duplicates)
                for (int j = 1; j < segmentPath.Count; j++)
                {
                    fullPath.Add(segmentPath[j]);
                }
            }
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
        
        if (gridPath.Count == 0)
        {
            Debug.LogWarning($"No path found from {start} to {end}");
            return new List<Vector2> { start, end }; // Fallback to direct line
        }
        
        // Smooth the path (corner-preserving + spline)
        return SmoothPathFromGrid(gridPath);
    }
    
    List<Vector2Int> AStarPathfinding(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        
        List<Vector2Int> openSet = new List<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        
        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);
        
        int iterations = 0;
        
        while (openSet.Count > 0 && iterations < maxPathfindingIterations)
        {
            iterations++;
            
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
                Debug.Log($"Path found with {path.Count} waypoints after {iterations} iterations");
                break;
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            // Check neighbors (8-directional movement with better wall avoidance)
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
        
        if (iterations >= maxPathfindingIterations)
        {
            Debug.LogWarning($"Pathfinding reached maximum iterations! Path length: {path.Count}");
        }
        
        if (path.Count == 0)
        {
            Debug.LogWarning("No path found! Attempting simplified pathfinding...");
            // Try a simpler pathfinding approach
            path = FindSimplePath(start, end);
            
            if (path.Count == 0)
            {
                Debug.LogWarning("Simplified pathfinding also failed! Using direct line as last resort.");
                path.Add(start);
                path.Add(end);
            }
        }
        
        return path;
    }
    
    List<Vector2Int> FindSimplePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        
        // Try to find a path by going in a straight line with small deviations
        Vector2Int current = start;
        path.Add(current);
        
        Vector2Int direction = new Vector2Int(
            end.x > start.x ? 1 : end.x < start.x ? -1 : 0,
            end.y > start.y ? 1 : end.y < start.y ? -1 : 0
        );
        
        int maxSteps = Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y) + 10;
        int steps = 0;
        
        while (current != end && steps < maxSteps)
        {
            steps++;
            
            // Try to move in the direction of the target
            Vector2Int next = current + direction;
            
            if (IsWalkable(next))
            {
                current = next;
                path.Add(current);
            }
            else
            {
                // Try alternative directions
                Vector2Int[] alternatives = {
                    new Vector2Int(direction.x, 0),
                    new Vector2Int(0, direction.y),
                    new Vector2Int(1, 0), new Vector2Int(-1, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, -1)
                };
                
                bool foundAlternative = false;
                foreach (Vector2Int alt in alternatives)
                {
                    Vector2Int altNext = current + alt;
                    if (IsWalkable(altNext))
                    {
                        current = altNext;
                        path.Add(current);
                        foundAlternative = true;
                        break;
                    }
                }
                
                if (!foundAlternative)
                {
                    // If no alternative found, try to move closer to target
                    Vector2Int closer = new Vector2Int(
                        current.x + (end.x > current.x ? 1 : end.x < current.x ? -1 : 0),
                        current.y + (end.y > current.y ? 1 : end.y < current.y ? -1 : 0)
                    );
                    
                    if (IsWalkable(closer))
                    {
                        current = closer;
                        path.Add(current);
                    }
                    else
                    {
                        break; // Give up
                    }
                }
            }
        }
        
        if (current != end)
        {
            path.Add(end); // Add end point even if we couldn't reach it
        }
        
        Debug.Log($"Simple path found with {path.Count} waypoints");
        return path;
    }
    
    bool IsWalkable(Vector2Int gridPos)
    {
        Vector2 worldPos = GridToWorld(gridPos);
        
        // Use a reasonable radius for wall detection
        float checkRadius = gridResolution * 0.6f;
        Collider2D hit = Physics2D.OverlapCircle(worldPos, checkRadius, wallLayerMask);
        
        if (hit != null)
        {
            return false;
        }
        
        // Check maze generators for wall detection
        if (mazeGenerator != null && mazeGenerator.IsWall(worldPos))
            return false;
        if (wallGenerator != null && wallGenerator.IsWall(worldPos))
            return false;
        if (traditionalMazeGenerator != null && traditionalMazeGenerator.IsWall(worldPos))
            return false;
            
        return true;
    }
    
    float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Prefer Manhattan to reduce diagonal cutting near obstacles
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
    
    List<Vector2> SmoothPathFromGrid(List<Vector2Int> gridPath)
    {
        // 1) Simplify by keeping only corners (direction changes)
        List<Vector2Int> corners = new List<Vector2Int>();
        if (gridPath.Count == 0) return new List<Vector2>();
        corners.Add(gridPath[0]);
        for (int i = 1; i < gridPath.Count - 1; i++)
        {
            Vector2Int a = gridPath[i - 1];
            Vector2Int b = gridPath[i];
            Vector2Int c = gridPath[i + 1];
            Vector2Int d1 = new Vector2Int(Mathf.Clamp(b.x - a.x, -1, 1), Mathf.Clamp(b.y - a.y, -1, 1));
            Vector2Int d2 = new Vector2Int(Mathf.Clamp(c.x - b.x, -1, 1), Mathf.Clamp(c.y - b.y, -1, 1));
            if (d1 != d2)
            {
                corners.Add(b);
            }
        }
        if (gridPath.Count > 1) corners.Add(gridPath[gridPath.Count - 1]);

        // Convert to world points
        List<Vector2> ctrl = new List<Vector2>();
        foreach (var p in corners) ctrl.Add(GridToWorld(p));
        if (ctrl.Count <= 2) return ctrl;

        // 2) Catmull-Rom spline through control points, sample densely
        List<Vector2> sampled = new List<Vector2>();
        sampled.Add(ctrl[0]);
        int samplesPerSegment = Mathf.Max(6, Mathf.RoundToInt(12 * pathSmoothing));
        for (int i = 0; i < ctrl.Count - 1; i++)
        {
            Vector2 p0 = i == 0 ? ctrl[i] : ctrl[i - 1];
            Vector2 p1 = ctrl[i];
            Vector2 p2 = ctrl[i + 1];
            Vector2 p3 = i + 2 < ctrl.Count ? ctrl[i + 2] : ctrl[i + 1];
            for (int s = 1; s <= samplesPerSegment; s++)
            {
                float t = s / (float)samplesPerSegment;
                Vector2 q = CatmullRom(p0, p1, p2, p3, t);
                // Collision-aware: ensure we don't cut through walls between last and q
                Vector2 prev = sampled[sampled.Count - 1];
                if (!LineBlocked(prev, q))
                {
                    sampled.Add(q);
                }
                else
                {
                    // Try incremental binary search to get closest non-blocked point
                    Vector2 lo = prev, hi = q, mid;
                    for (int k = 0; k < 5; k++)
                    {
                        mid = (lo + hi) * 0.5f;
                        if (LineBlocked(prev, mid)) hi = mid; else lo = mid;
                    }
                    sampled.Add(lo);
                }
            }
        }
        return sampled;
    }

    Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    bool LineBlocked(Vector2 a, Vector2 b)
    {
        RaycastHit2D hit = Physics2D.Linecast(a, b, wallLayerMask);
        if (hit.collider != null) return true;
        // Also sample midpoints for safety
        Vector2 mid = (a + b) * 0.5f;
        if (!IsWalkable(WorldToGrid(mid))) return true;
        return false;
    }
    
    void CreateVisualPath(List<Vector2> path)
    {
        if (path.Count < 2) return;
        
        // Single smooth LineRenderer for the whole path
        GameObject line = new GameObject("Path");
        line.transform.SetParent(transform);
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lr.SetPosition(i, new Vector3(path[i].x, path[i].y, -0.1f));
        }
        lr.startWidth = pathWidth;
        lr.endWidth = pathWidth;
        Material mat = pathMaterial != null ? pathMaterial : CreateDefaultMaterial();
        if (mat != null)
        {
            lr.material = mat;
            lr.material.color = pathColor;
        }
        lr.sortingOrder = 1;
        lr.useWorldSpace = true;
        pathLines.Add(line);
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
        
        // Create material safely
        Material mat = pathMaterial != null ? pathMaterial : CreateDefaultMaterial();
        if (mat != null)
        {
            lr.material = mat;
            lr.material.color = pathColor;
        }
        
        lr.sortingOrder = 1;
        lr.useWorldSpace = true;
        
        pathLines.Add(line);
    }
    
    void CreateDebugWaypoints(List<Vector2> waypoints)
    {
        // Clear existing debug waypoints
        foreach (GameObject wp in debugWaypoints)
        {
            if (wp != null) DestroyImmediate(wp);
        }
        debugWaypoints.Clear();
        
        // Only create debug waypoints if enabled
        if (!showWaypoints) return;
        
        // Create new debug waypoints
        for (int i = 0; i < waypoints.Count; i++)
        {
            GameObject wp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wp.name = $"DebugWaypoint_{i}";
            wp.transform.position = new Vector3(waypoints[i].x, waypoints[i].y, -0.2f);
            wp.transform.localScale = Vector3.one * 0.3f;
            
            Renderer renderer = wp.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = i == 0 ? Color.green : (i == waypoints.Count - 1 ? Color.red : Color.yellow);
            }
            
            // Remove collider
            Collider collider = wp.GetComponent<Collider>();
            if (collider != null) DestroyImmediate(collider);
            
            debugWaypoints.Add(wp);
        }
    }
    
    Material CreateDefaultMaterial()
    {
        try
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.color = pathColor;
                return mat;
            }
            else
            {
                // Fallback to a basic material
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = pathColor;
                return mat;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to create default material: {e.Message}");
            return null;
        }
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
        
        foreach (GameObject wp in debugWaypoints)
        {
            if (wp != null) DestroyImmediate(wp);
        }
        debugWaypoints.Clear();
        
        optimalPath.Clear();
    }
    
    public void RegeneratePath()
    {
        ClearPath();
        GeneratePath();
    }
    
    [ContextMenu("Force Regenerate Path")]
    public void ForceRegeneratePath()
    {
        Debug.Log("Force regenerating path with improved settings...");
        ClearPath();
        
        // Temporarily increase resolution for better pathfinding
        float originalResolution = gridResolution;
        gridResolution = 0.05f; // Very high resolution
        
        // Temporarily increase smoothing for better curves
        float originalSmoothing = pathSmoothing;
        pathSmoothing = 0.9f; // Maximum smoothing
        
        // Temporarily increase iterations
        int originalIterations = maxPathfindingIterations;
        maxPathfindingIterations = 10000; // Many more iterations
        
        GeneratePath();
        
        // Restore original settings
        gridResolution = originalResolution;
        pathSmoothing = originalSmoothing;
        maxPathfindingIterations = originalIterations;
    }
    
    [ContextMenu("Update Inspector Values")]
    public void UpdateInspectorValues()
    {
        Debug.Log("Updating Inspector values to match code defaults...");
        // This will update the Inspector to show the new default values
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    [ContextMenu("Clear Console and Regenerate")]
    public void ClearConsoleAndRegenerate()
    {
        Debug.Log("=== CLEARING CONSOLE AND REGENERATING PATH ===");
        
        // Clear existing path
        ClearPath();
        
        // Wait a frame to clear any pending operations
        StartCoroutine(RegenerateAfterDelay());
    }
    
    System.Collections.IEnumerator RegenerateAfterDelay()
    {
        yield return null; // Wait one frame
        GeneratePath();
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugPath || optimalPath.Count < 2) return;
        
        Gizmos.color = pathColor;
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
