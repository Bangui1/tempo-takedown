using UnityEngine;
using System.Collections.Generic;

public class EnemyPathFollower : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 180.0f;
    public float waypointReachDistance = 0.1f;
    public bool lookAtDirection = true;
    
    [Header("Path Integration")]
    public MazePathfinder mazePathfinder;
    public bool findPathfinderAutomatically = true;
    
    [Header("Enemy Settings")]
    public float health = 100f;
    public bool destroyAtEnd = true;
    public bool loopPath = false;
    
    [Header("Visual Feedback")]
    public bool showDebugPath = true;
    public Color debugPathColor = Color.red;
    public float debugPathWidth = 0.05f;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnReachEnd;
    public UnityEngine.Events.UnityEvent OnDestroyed;
    
    // Private variables (exposed for debugging)
    [SerializeField] private List<Vector2> currentPath = new List<Vector2>();
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool hasReachedEnd = false;
    
    // Public getters for debugging
    public List<Vector2> CurrentPath => currentPath;
    public int CurrentWaypointIndex => currentWaypointIndex;
    public bool IsMoving => isMoving;
    public bool HasReachedEnd => hasReachedEnd;
    
    // Components
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        // Get components
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-find pathfinder if needed
        if (findPathfinderAutomatically && mazePathfinder == null)
        {
            mazePathfinder = FindFirstObjectByType<MazePathfinder>();
        }
        
        // Initialize enemy
        InitializeEnemy();
    }
    
    void InitializeEnemy()
    {
        Debug.Log($"Initializing enemy: {gameObject.name}");
        
        // Set up Rigidbody2D if it exists
        if (rb2D != null)
        {
            rb2D.gravityScale = 0f; // No gravity for top-down movement
            rb2D.linearDamping = 5f; // Some drag for smooth stopping
            Debug.Log("Rigidbody2D configured for top-down movement");
        }
        else
        {
            Debug.LogWarning("No Rigidbody2D found on enemy!");
        }
        
        // Get the path and start moving
        GetPathFromPathfinder();
        
        if (currentPath.Count > 0)
        {
            // Position enemy at the start of the path
            Vector2 startPos = currentPath[0];
            transform.position = startPos;
            currentWaypointIndex = 1; // Start moving towards the second waypoint
            isMoving = true;
            
            Debug.Log($"Enemy {gameObject.name} initialized with {currentPath.Count} waypoints");
            Debug.Log($"Starting at {startPos}, moving to waypoint {currentWaypointIndex}: {(currentWaypointIndex < currentPath.Count ? currentPath[currentWaypointIndex].ToString() : "NONE")}");
        }
        else
        {
            Debug.LogError($"Enemy {gameObject.name}: No path found! Enemy will not move.");
            Debug.LogError("Make sure MazePathfinder has generated a path before spawning enemies.");
        }
    }
    
    void GetPathFromPathfinder()
    {
        if (mazePathfinder == null)
        {
            Debug.LogError($"Enemy {gameObject.name}: No MazePathfinder assigned! Cannot get path.");
            return;
        }
        
        // Get the optimal path from the pathfinder
        if (mazePathfinder.HasValidPath())
        {
            currentPath = mazePathfinder.GetOptimalPath();
            Debug.Log($"Enemy {gameObject.name}: Successfully retrieved path with {currentPath.Count} waypoints");
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name}: Pathfinder has no generated path. Attempting to generate path...");
            
            // Try to generate path if it doesn't exist
            mazePathfinder.GeneratePath();
            
            // Wait a frame and try again
            StartCoroutine(RetryPathRetrieval());
        }
    }
    
    System.Collections.IEnumerator RetryPathRetrieval()
    {
        // Wait a few frames for path generation to complete
        for (int i = 0; i < 5; i++)
        {
            yield return null; // Wait one frame
            
            if (mazePathfinder.HasValidPath())
            {
                currentPath = mazePathfinder.GetOptimalPath();
                Debug.Log($"Enemy {gameObject.name}: Retrieved path after retry with {currentPath.Count} waypoints");
                
                // Restart initialization now that we have a path
                if (currentPath.Count > 0)
                {
                    Vector2 startPos = currentPath[0];
                    transform.position = startPos;
                    currentWaypointIndex = 1;
                    isMoving = true;
                    hasReachedEnd = false;
                    
                    Debug.Log($"Enemy {gameObject.name}: Reinitialized with path, starting at {startPos}");
                }
                yield break;
            }
        }
        
        // If we still don't have a path after retries, try one more time with a longer wait
        Debug.LogWarning($"Enemy {gameObject.name}: Still no path after retries, waiting longer...");
        yield return new WaitForSeconds(1.0f);
        
        if (mazePathfinder.HasValidPath())
        {
            currentPath = mazePathfinder.GetOptimalPath();
            Debug.Log($"Enemy {gameObject.name}: Retrieved path after long wait with {currentPath.Count} waypoints");
            
            if (currentPath.Count > 0)
            {
                Vector2 startPos = currentPath[0];
                transform.position = startPos;
                currentWaypointIndex = 1;
                isMoving = true;
                hasReachedEnd = false;
                
                Debug.Log($"Enemy {gameObject.name}: Reinitialized with path, starting at {startPos}");
            }
        }
        else
        {
            Debug.LogError($"Enemy {gameObject.name}: Failed to get path even after extended retries!");
        }
    }
    
    void Update()
    {
        if (isMoving && !hasReachedEnd && currentPath.Count > 0)
        {
            MoveAlongPath();
        }
        else if (currentPath.Count == 0)
        {
            // Only log this once per enemy
            if (Time.frameCount % 120 == 0) // Every 2 seconds at 60fps
            {
                Debug.LogWarning($"Enemy {gameObject.name}: No path to follow!");
            }
        }
    }
    
    void MoveAlongPath()
    {
        if (currentWaypointIndex >= currentPath.Count)
        {
            // Reached the end of the path
            ReachEnd();
            return;
        }
        
        Vector2 currentPos = transform.position;
        Vector2 targetPos = currentPath[currentWaypointIndex];
        
        // Calculate direction and distance
        Vector2 direction = (targetPos - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, targetPos);
        
        // Check if we've reached the current waypoint
        if (distance <= waypointReachDistance)
        {
            currentWaypointIndex++;
            
            // If we haven't reached the end, continue to next waypoint
            if (currentWaypointIndex < currentPath.Count)
            {
                Debug.Log($"Reached waypoint {currentWaypointIndex - 1}, moving to waypoint {currentWaypointIndex}");
            }
            return;
        }
        
        // Move towards the target waypoint
        Vector2 newPosition = currentPos + direction * moveSpeed * Time.deltaTime;
        
        // Use Rigidbody2D if available, otherwise use Transform
        if (rb2D != null)
        {
            rb2D.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
        
        // Rotate to face movement direction
        if (lookAtDirection && direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
                rotationSpeed * Time.deltaTime);
        }
    }
    
    void ReachEnd()
    {
        if (hasReachedEnd) return;
        
        hasReachedEnd = true;
        isMoving = false;
        
        Debug.Log("Enemy reached the end of the path!");
        
        // Invoke end event
        OnReachEnd?.Invoke();
        
        if (loopPath)
        {
            // Restart the path
            RestartPath();
        }
        else if (destroyAtEnd)
        {
            // Destroy the enemy
            DestroyEnemy();
        }
    }
    
    public void RestartPath()
    {
        if (currentPath.Count > 0)
        {
            currentWaypointIndex = 1;
            hasReachedEnd = false;
            isMoving = true;
            transform.position = currentPath[0];
            
            Debug.Log("Enemy restarted path");
        }
    }
    
    public void DestroyEnemy()
    {
        Debug.Log("Enemy destroyed");
        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            DestroyEnemy();
        }
    }
    
    public void SetPath(List<Vector2> newPath)
    {
        if (newPath != null && newPath.Count > 0)
        {
            currentPath = new List<Vector2>(newPath);
            currentWaypointIndex = 1;
            hasReachedEnd = false;
            isMoving = true;
            transform.position = currentPath[0];
            
            Debug.Log($"Enemy path set manually with {currentPath.Count} waypoints");
        }
    }
    
    public void PauseMovement()
    {
        isMoving = false;
    }
    
    public void ResumeMovement()
    {
        if (!hasReachedEnd && currentPath.Count > 0)
        {
            isMoving = true;
        }
    }
    
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    public void RetryGetPath()
    {
        Debug.Log($"Enemy {gameObject.name}: Manually retrying path retrieval...");
        GetPathFromPathfinder();
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugPath || currentPath.Count < 2) return;
        
        Gizmos.color = debugPathColor;
        
        // Draw the path
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
        
        // Draw waypoints
        for (int i = 0; i < currentPath.Count; i++)
        {
            if (i == currentWaypointIndex)
            {
                // Current target waypoint
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentPath[i], 0.2f);
            }
            else if (i < currentWaypointIndex)
            {
                // Passed waypoints
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentPath[i], 0.1f);
            }
            else
            {
                // Future waypoints
                Gizmos.color = debugPathColor;
                Gizmos.DrawWireSphere(currentPath[i], 0.1f);
            }
        }
        
        // Draw current position and direction
        if (isMoving && currentWaypointIndex < currentPath.Count)
        {
            Gizmos.color = Color.cyan;
            Vector2 currentPos = transform.position;
            Vector2 targetPos = currentPath[currentWaypointIndex];
            Vector2 direction = (targetPos - currentPos).normalized;
            
            Gizmos.DrawRay(currentPos, direction * 0.5f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw additional debug info when selected
        if (currentPath.Count > 0)
        {
            Gizmos.color = Color.white;
            
            // Draw waypoint numbers
            for (int i = 0; i < currentPath.Count; i++)
            {
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(currentPath[i], i.ToString());
                #endif
            }
        }
    }
}
