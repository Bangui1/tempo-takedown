using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Stats", menuName = "Tower Defense/Tower Stats")]
public class TowerStats : ScriptableObject
{
    [Header("Tower Identification")]
    public string towerName = "Basic Tower";
    public GameObject towerPrefab;
    public int cost = 50;
    
    [Header("Combat Stats")]
    [Tooltip("Damage dealt per projectile")]
    public float damage = 25f;
    
    [Tooltip("Time between shots in seconds")]
    public float fireRate = 1f;
    
    [Tooltip("Detection range for enemies")]
    public float range = 5f;
    
    [Header("Projectile Settings")]
    [Tooltip("Speed of projectiles")]
    public float projectileSpeed = 10f;
    
    [Tooltip("Prefab of the projectile to spawn")]
    public GameObject projectilePrefab;
    
    [Header("Visual Settings")]
    public Color rangeIndicatorColor = new Color(1f, 1f, 0f, 0.3f);
}

