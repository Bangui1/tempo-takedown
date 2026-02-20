using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Tower Defense/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Identification")]
    public string enemyName = "Basic Enemy";

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float scale = 2f;

    [Header("Visual")]
    public Color tintColor = Color.white;

    [Header("Walk Animation Sprites")]
    public Sprite[] walkNorth;
    public Sprite[] walkNorthEast;
    public Sprite[] walkEast;
    public Sprite[] walkSouthEast;
    public Sprite[] walkSouth;
    public Sprite[] walkSouthWest;
    public Sprite[] walkWest;
    public Sprite[] walkNorthWest;
}
