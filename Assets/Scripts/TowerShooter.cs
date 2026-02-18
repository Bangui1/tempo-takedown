using UnityEngine;

public class TowerShooter : MonoBehaviour
{
    [Header("Stats")]
    public TowerStats stats;
    
    [Header("Manual Override (if no stats assigned)")]
    public float damage = 25f;
    public float fireRate = 1f;
    public float range = 5f;
    public float projectileSpeed = 10f;
    public GameObject projectilePrefab;
    
    [Header("Targeting")]
    public LayerMask enemyLayer;
    public Transform firePoint;
    
    [Header("Visual")]
    public bool showRange = true;
    public Transform visualTransform;
    
    private float fireTimer = 0f;
    private Transform currentTarget;
    private EnemyWalker targetEnemy;
    
    void Start()
    {
        // If stats are assigned, use them
        if (stats != null)
        {
            damage = stats.damage;
            fireRate = stats.fireRate;
            range = stats.range;
            projectileSpeed = stats.projectileSpeed;
            if (stats.projectilePrefab != null)
            {
                projectilePrefab = stats.projectilePrefab;
            }
        }
        
        // If no fire point is set, use this transform
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // If no visual transform, try to find one in children
        if (visualTransform == null)
        {
            Transform visual = transform.Find("Visual");
            if (visual != null)
            {
                visualTransform = visual;
            }
            else
            {
                visualTransform = transform;
            }
        }
        
        // Setup enemy layer if not set
        if (enemyLayer == 0)
        {
            enemyLayer = ~0; // All layers
        }
        
        fireTimer = 0f;
    }
    
    void Update()
    {
        // Update fire timer
        fireTimer += Time.deltaTime;
        
        // Find or validate target
        if (currentTarget == null || targetEnemy == null || targetEnemy.IsDead || !IsInRange(currentTarget))
        {
            FindTarget();
        }
        
        // Rotate towards target if we have one
        if (currentTarget != null && visualTransform != null)
        {
            RotateTowardsTarget();
        }
        
        // Shoot if ready
        if (currentTarget != null && fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }
    
    void FindTarget()
    {
        currentTarget = null;
        targetEnemy = null;
        
        // Find all colliders in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        
        float closestDistance = float.MaxValue;
        EnemyWalker closestEnemy = null;
        Transform closestTransform = null;
        
        foreach (Collider2D hit in hits)
        {
            EnemyWalker enemy = hit.GetComponent<EnemyWalker>();
            if (enemy != null && !enemy.IsDead)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                    closestTransform = hit.transform;
                }
            }
        }
        
        if (closestEnemy != null)
        {
            currentTarget = closestTransform;
            targetEnemy = closestEnemy;
        }
    }
    
    bool IsInRange(Transform target)
    {
        if (target == null) return false;
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= range;
    }
    
    void RotateTowardsTarget()
    {
        if (currentTarget == null) return;
        
        Vector3 direction = currentTarget.position - visualTransform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Adjust angle to face right by default (Unity sprites often face right at 0 degrees)
        angle -= 90f;
        
        visualTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    
    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"Tower {gameObject.name} has no projectile prefab!");
            return;
        }
        
        if (currentTarget == null) return;
        
        // Instantiate projectile at fire point
        Vector3 spawnPos = firePoint.position;
        spawnPos.z = 0f;
        
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        // Setup projectile
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = damage;
            projectile.speed = projectileSpeed;
            projectile.SetTarget(currentTarget);
        }
        else
        {
            Debug.LogWarning($"Projectile prefab {projectilePrefab.name} doesn't have a Projectile component!");
            Destroy(projectileObj);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (showRange)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            if (stats != null)
            {
                Gizmos.color = stats.rangeIndicatorColor;
            }
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}

