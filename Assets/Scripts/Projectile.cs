using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float damage = 25f;
    public float maxLifetime = 5f;
    public float rotationSpeed = 360f;
    
    private Vector3 targetPosition;
    private Vector3 direction;
    private float lifetimeTimer = 0f;
    private bool hasTarget = false;
    private Transform targetTransform;
    
    void Start()
    {
        lifetimeTimer = 0f;
        
        // Add Rigidbody2D if not present (needed for trigger collisions)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true; // Kinematic so physics doesn't affect it
            rb.gravityScale = 0f;
        }
        
        // Ensure we have a collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        // If we have a live target, update direction to track it
        if (targetTransform != null && hasTarget)
        {
            Vector3 targetPos2D = new Vector3(targetTransform.position.x, targetTransform.position.y, 0f);
            direction = (targetPos2D - transform.position).normalized;
        }
        
        // Move the projectile
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate the projectile for visual effect
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
    
    public void SetTarget(Vector3 target)
    {
        targetPosition = new Vector3(target.x, target.y, 0f);
        Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, 0f);
        direction = (targetPosition - currentPos).normalized;
        hasTarget = true;
    }
    
    public void SetTarget(Transform target)
    {
        if (target != null)
        {
            targetTransform = target;
            Vector3 targetPos2D = new Vector3(target.position.x, target.position.y, 0f);
            Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, 0f);
            direction = (targetPos2D - currentPos).normalized;
            hasTarget = true;
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Projectile collided with: {collision.gameObject.name}");
        
        EnemyWalker enemy = collision.GetComponent<EnemyWalker>();
        if (enemy != null && !enemy.IsDead)
        {
            Debug.Log($"Projectile hit enemy! Dealing {damage} damage.");
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (enemy != null && enemy.IsDead)
        {
            Debug.Log("Hit enemy but it's already dead, destroying projectile.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"Collided with {collision.gameObject.name} but it's not an enemy.");
        }
    }
}

