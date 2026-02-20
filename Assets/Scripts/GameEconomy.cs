using UnityEngine;

public class GameEconomy : MonoBehaviour
{
    public static GameEconomy Instance { get; private set; }

    public static event System.Action<int> OnPointsChanged;
    public static event System.Action<int> OnLivesChanged;

    [Header("Starting Resources")]
    public int startingPoints = 100;
    public int startingLives = 20;

    public int CurrentPoints { get; private set; }
    public int CurrentLives { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CurrentPoints = startingPoints;
        CurrentLives = startingLives;
    }

    void OnEnable()
    {
        EnemyWalker.OnEnemyDied += HandleEnemyDied;
        EnemyWalker.OnEnemyReachedEnd += HandleEnemyReachedEnd;
    }

    void OnDisable()
    {
        EnemyWalker.OnEnemyDied -= HandleEnemyDied;
        EnemyWalker.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
    }

    void HandleEnemyDied(EnemyWalker enemy)
    {
        AddPoints(enemy.rewardPoints);
    }

    void HandleEnemyReachedEnd(EnemyWalker enemy)
    {
        LoseLife();
    }

    public void AddPoints(int amount)
    {
        CurrentPoints += amount;
        OnPointsChanged?.Invoke(CurrentPoints);
    }

    public bool SpendPoints(int amount)
    {
        if (CurrentPoints < amount) return false;
        CurrentPoints -= amount;
        OnPointsChanged?.Invoke(CurrentPoints);
        return true;
    }

    public bool CanAfford(int cost)
    {
        return CurrentPoints >= cost;
    }

    public void LoseLife()
    {
        CurrentLives--;
        OnLivesChanged?.Invoke(CurrentLives);
        Debug.Log($"Lost a life! Lives remaining: {CurrentLives}");

        if (CurrentLives <= 0)
        {
            Debug.Log("Game Over!");
        }
    }
}
