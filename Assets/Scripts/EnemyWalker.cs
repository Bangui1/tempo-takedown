using UnityEngine;
using System.Collections.Generic;

public class EnemyWalker : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isDead = false;
    public float fadeOutDuration = 0.5f;
    private bool isFadingOut = false;
    private float fadeTimer = 0f;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float waypointReachDistance = 0.1f;
    
    [Header("Animation")]
    public float frameRate = 10f;
    public Sprite[] walkNorth;
    public Sprite[] walkNorthEast;
    public Sprite[] walkEast;
    public Sprite[] walkSouthEast;
    public Sprite[] walkSouth;
    public Sprite[] walkSouthWest;
    public Sprite[] walkWest;
    public Sprite[] walkNorthWest;
    
    [Header("References")]
    public NavMeshPathfinder pathfinder;
    
    private SpriteRenderer spriteRenderer;
    private List<Vector3> path = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private float animationTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnimationSet;
    private bool isWalking = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingOrder = 20;
        
        if (pathfinder == null)
        {
            pathfinder = FindFirstObjectByType<NavMeshPathfinder>();
        }
        
        if (walkSouth != null && walkSouth.Length > 0)
        {
            currentAnimationSet = walkSouth;
            spriteRenderer.sprite = walkSouth[0];
        }
    }
    
    public void StartWalking()
    {
        if (pathfinder == null)
        {
            Debug.LogError("No pathfinder assigned!");
            return;
        }
        
        path = pathfinder.GetPath();
        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("No valid path to follow!");
            return;
        }
        
        currentWaypointIndex = 0;
        Vector3 startPos = path[0];
        transform.position = new Vector3(startPos.x, startPos.y, 0f);
        isWalking = true;
        
        Debug.Log($"Enemy started walking. Path has {path.Count} waypoints.");
    }
    
    public void StopWalking()
    {
        isWalking = false;
    }
    
    public void RefreshPath()
    {
        if (pathfinder == null || !isWalking) return;
        
        List<Vector3> newPath = pathfinder.GetPath();
        if (newPath == null || newPath.Count < 2) return;
        
        Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, 0f);
        
        int closestIndex = 0;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < newPath.Count; i++)
        {
            Vector3 waypoint2D = new Vector3(newPath[i].x, newPath[i].y, 0f);
            float dist = Vector3.Distance(currentPos, waypoint2D);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }
        
        path = newPath;
        currentWaypointIndex = Mathf.Min(closestIndex + 1, path.Count - 1);
        
        Debug.Log($"Enemy path refreshed. Now at waypoint {currentWaypointIndex}/{path.Count}");
    }
    
    void Update()
    {
        if (isFadingOut)
        {
            HandleFadeOut();
            return;
        }
        
        if (!isWalking || path == null || path.Count == 0) return;
        
        if (currentWaypointIndex >= path.Count)
        {
            OnReachedEnd();
            return;
        }
        
        Vector3 targetWaypoint = path[currentWaypointIndex];
        Vector3 target2D = new Vector3(targetWaypoint.x, targetWaypoint.y, 0f);
        Vector3 current2D = new Vector3(transform.position.x, transform.position.y, 0f);
        
        Vector3 direction = (target2D - current2D).normalized;
        float distance = Vector3.Distance(current2D, target2D);
        
        if (distance < waypointReachDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= path.Count)
            {
                OnReachedEnd();
                return;
            }
        }
        
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        UpdateAnimation(direction);
    }
    
    void UpdateAnimation(Vector3 direction)
    {
        Sprite[] newAnimSet = GetAnimationSetForDirection(direction);
        if (newAnimSet != currentAnimationSet && newAnimSet != null && newAnimSet.Length > 0)
        {
            currentAnimationSet = newAnimSet;
            currentFrame = 0;
        }
        
        if (currentAnimationSet == null || currentAnimationSet.Length == 0) return;
        
        animationTimer += Time.deltaTime;
        if (animationTimer >= 1f / frameRate)
        {
            animationTimer = 0f;
            currentFrame = (currentFrame + 1) % currentAnimationSet.Length;
            spriteRenderer.sprite = currentAnimationSet[currentFrame];
        }
    }
    
    Sprite[] GetAnimationSetForDirection(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        if (angle < 0) angle += 360f;
        
        if (angle >= 337.5f || angle < 22.5f)
            return walkEast;
        else if (angle >= 22.5f && angle < 67.5f)
            return walkNorthEast;
        else if (angle >= 67.5f && angle < 112.5f)
            return walkNorth;
        else if (angle >= 112.5f && angle < 157.5f)
            return walkNorthWest;
        else if (angle >= 157.5f && angle < 202.5f)
            return walkWest;
        else if (angle >= 202.5f && angle < 247.5f)
            return walkSouthWest;
        else if (angle >= 247.5f && angle < 292.5f)
            return walkSouth;
        else
            return walkSouthEast;
    }
    
    void OnReachedEnd()
    {
        isWalking = false;
        Debug.Log("Enemy reached the end!");
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isWalking = false;
        isFadingOut = true;
        fadeTimer = 0f;
        
        Debug.Log("Enemy died!");
    }
    
    void HandleFadeOut()
    {
        fadeTimer += Time.deltaTime;
        float alpha = 1f - (fadeTimer / fadeOutDuration);
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Clamp01(alpha);
            spriteRenderer.color = color;
        }
        
        if (fadeTimer >= fadeOutDuration)
        {
            Destroy(gameObject);
        }
    }
    
    public bool IsWalking => isWalking;
    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public int CurrentWaypoint => currentWaypointIndex;
    public int TotalWaypoints => path?.Count ?? 0;
}

