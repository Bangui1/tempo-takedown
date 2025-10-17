using UnityEngine;

/// <summary>
/// Helper script to automatically set up an enemy prefab with the correct components
/// Attach this to an empty GameObject and it will configure it as an enemy
/// </summary>
public class EnemyPrefabSetup : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public Sprite enemySprite;
    public Color enemyColor = Color.white;
    public Vector2 enemySize = Vector2.one;
    
    [Header("Auto Setup")]
    [SerializeField] private bool setupComplete = false;
    
    void Start()
    {
        if (!setupComplete)
        {
            SetupEnemyPrefab();
        }
    }
    
    [ContextMenu("Setup Enemy Prefab")]
    public void SetupEnemyPrefab()
    {
        Debug.Log("Setting up enemy prefab...");
        
        // Add SpriteRenderer if not present
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Configure sprite renderer
        if (enemySprite != null)
        {
            spriteRenderer.sprite = enemySprite;
        }
        spriteRenderer.color = enemyColor;
        
        // Add Rigidbody2D if not present
        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            rb2D = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for top-down movement
        rb2D.gravityScale = 0f;
        rb2D.linearDamping = 5f;
        rb2D.angularDamping = 5f;
        rb2D.freezeRotation = false; // Allow rotation for direction facing
        
        // Add CircleCollider2D if not present
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        // Configure collider
        collider.radius = Mathf.Min(enemySize.x, enemySize.y) * 0.4f; // Slightly smaller than sprite
        collider.isTrigger = false; // For physics interactions
        
        // Add EnemyPathFollower if not present
        EnemyPathFollower pathFollower = GetComponent<EnemyPathFollower>();
        if (pathFollower == null)
        {
            pathFollower = gameObject.AddComponent<EnemyPathFollower>();
        }
        
        // Configure path follower with good defaults
        pathFollower.moveSpeed = 2.0f;
        pathFollower.rotationSpeed = 180.0f;
        pathFollower.waypointReachDistance = 0.1f;
        pathFollower.lookAtDirection = true;
        pathFollower.health = 100f;
        pathFollower.destroyAtEnd = true;
        pathFollower.loopPath = false;
        pathFollower.showDebugPath = true;
        pathFollower.findPathfinderAutomatically = true;
        
        // Set transform scale
        transform.localScale = new Vector3(enemySize.x, enemySize.y, 1f);
        
        // Set name
        if (gameObject.name == "GameObject")
        {
            gameObject.name = "Enemy";
        }
        
        setupComplete = true;
        
        Debug.Log("Enemy prefab setup complete!");
        Debug.Log("Components added: SpriteRenderer, Rigidbody2D, CircleCollider2D, EnemyPathFollower");
        Debug.Log("Ready to save as prefab and use with EnemySpawner!");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
        #endif
    }
    
    void OnValidate()
    {
        // Update sprite renderer if values change in inspector
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (enemySprite != null)
            {
                spriteRenderer.sprite = enemySprite;
            }
            spriteRenderer.color = enemyColor;
        }
        
        // Update transform scale
        transform.localScale = new Vector3(enemySize.x, enemySize.y, 1f);
        
        // Update collider size
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.radius = Mathf.Min(enemySize.x, enemySize.y) * 0.4f;
        }
    }
    
    [ContextMenu("Reset Setup")]
    public void ResetSetup()
    {
        setupComplete = false;
        Debug.Log("Setup reset. Run Setup Enemy Prefab again to reconfigure.");
    }
}
