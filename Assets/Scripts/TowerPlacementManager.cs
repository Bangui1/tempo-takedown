using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("References")]
    public MazeGenerator mazeGenerator;
    public SimpleMazeGenerator simpleMazeGenerator;
    public NavMeshPathfinder pathfinder;

    [Header("Towers")]
    public GameObject[] towerPrefabs;
    public TowerStats[] towerStats; // Optional stats for each tower type
    public int selectedTowerIndex = 0;
    public int towerLayer = 0; // resolved at runtime if 0
    public string towerLayerName = "Tower";
    
    [Header("Combat")]
    public GameObject defaultProjectilePrefab;
    public LayerMask enemyLayer;

    [Header("Preview")]
    public Color validColor = new Color(0f, 1f, 0f, 0.6f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.6f);
    public LayerMask placementBlockMask;
    public float placementRadiusMultiplier = 0.45f;
    public bool placementModeActive = true;
    public string[] layersToIgnore = new string[] { "Default", "Ground", "Ignore Raycast", "UI", "Water" };

    private GameObject previewInstance;

    void Start()
    {
        if (mazeGenerator == null) mazeGenerator = FindFirstObjectByType<MazeGenerator>();
        if (simpleMazeGenerator == null) simpleMazeGenerator = FindFirstObjectByType<SimpleMazeGenerator>();
        if (pathfinder == null) pathfinder = FindFirstObjectByType<NavMeshPathfinder>();

        if (towerLayer == 0 && !string.IsNullOrEmpty(towerLayerName))
        {
            int resolved = LayerMask.NameToLayer(towerLayerName);
            if (resolved != -1) towerLayer = resolved;
        }

        GameObject wallPrefab = GetWallPrefab();
        if (towerLayer == 0 && wallPrefab != null)
        {
            towerLayer = wallPrefab.layer;
        }

        ConfigurePlacementBlockMask();
        CreatePreview();
    }

    GameObject GetWallPrefab()
    {
        if (simpleMazeGenerator != null && simpleMazeGenerator.wallPrefab != null)
            return simpleMazeGenerator.wallPrefab;
        if (mazeGenerator != null && mazeGenerator.wallPrefab != null)
            return mazeGenerator.wallPrefab;
        return null;
    }

    float GetCellSize()
    {
        if (simpleMazeGenerator != null) return simpleMazeGenerator.cellSize;
        if (mazeGenerator != null) return mazeGenerator.cellSize;
        return 1f;
    }

    Vector2 GetGridOffset()
    {
        if (simpleMazeGenerator != null) return simpleMazeGenerator.gridOffset;
        if (mazeGenerator != null) return new Vector2(mazeGenerator.mazeOffset.x, mazeGenerator.mazeOffset.z);
        return Vector2.zero;
    }

    void ConfigurePlacementBlockMask()
    {
        int ignoreMask = 0;
        foreach (string layerName in layersToIgnore)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
            {
                ignoreMask |= (1 << layer);
            }
        }
        
        GameObject wallPrefab = GetWallPrefab();
        if (wallPrefab != null)
        {
            int wallLayer = wallPrefab.layer;
            placementBlockMask = (1 << wallLayer);
            
            if (towerLayer != 0 && towerLayer != wallLayer)
            {
                placementBlockMask |= (1 << towerLayer);
            }
        }
        else
        {
            placementBlockMask = ~ignoreMask;
        }
    }

    void Update()
    {
        // Cancel placement mode with Escape or Right Click
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            SetPlacementMode(false);
            return;
        }

        if (!placementModeActive)
        {
            if (previewInstance != null) previewInstance.SetActive(false);
            return;
        }

        if (towerPrefabs == null || towerPrefabs.Length == 0) return;
        if (previewInstance == null) CreatePreview();

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                if (previewInstance != null) previewInstance.SetActive(false);
                return;
            }
        }

        // Check if mouse is over UI first
        if (IsPointerOverUI())
        {
            if (previewInstance != null) previewInstance.SetActive(false);
            return;
        }

        Vector3 mouseWorld = Vector3.zero;
        bool gotValidPosition = false;
        
        if (cam.orthographic)
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(cam.transform.position.z);
            mouseWorld = cam.ScreenToWorldPoint(mouseScreenPos);
            mouseWorld.z = 0f;
            gotValidPosition = true;
        }
        else
        {
            // For perspective cameras, use raycast
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            
            if (groundPlane.Raycast(ray, out distance) && distance > 0 && distance < 1000f)
            {
                mouseWorld = ray.GetPoint(distance);
                gotValidPosition = true;
            }
        }
        
        if (gotValidPosition)
        {
            Vector3 gridPos = SnapToGrid(mouseWorld);
            
            if (previewInstance != null)
            {
                if (!previewInstance.activeSelf)
                {
                    previewInstance.SetActive(true);
                }
                
                previewInstance.transform.position = new Vector3(gridPos.x, gridPos.y, 0f);
                previewInstance.transform.rotation = Quaternion.identity;
                
                bool valid = IsValidPlacement(gridPos);
                SetPreviewColor(valid ? validColor : invalidColor);
            }
            else
            {
                CreatePreview();
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (IsValidPlacement(gridPos))
                {
                    PlaceTower(gridPos);
                }
            }
        }
        else
        {
            // Hide preview when mouse is not over the game view
            if (previewInstance != null)
            {
                previewInstance.SetActive(false);
            }
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            CycleTower((int)Mathf.Sign(Input.mouseScrollDelta.y));
        }
    }

    public void SetPlacementMode(bool active)
    {
        placementModeActive = active;
        if (active)
        {
            // Ensure preview is created when placement mode is activated
            if (previewInstance == null && towerPrefabs != null && towerPrefabs.Length > 0)
            {
                CreatePreview();
                Debug.Log("Created preview when placement mode activated");
            }
        }
        else
        {
            if (previewInstance != null)
            {
                previewInstance.SetActive(false);
            }
        }
        Debug.Log($"Placement mode: {(active ? "ON" : "OFF")}");
    }

    public void CreatePreview()
    {
        if (previewInstance != null) DestroyImmediate(previewInstance);
        GameObject prefab = towerPrefabs != null && towerPrefabs.Length > 0 ? towerPrefabs[Mathf.Clamp(selectedTowerIndex, 0, towerPrefabs.Length - 1)] : null;
        if (prefab == null)
        {
            Debug.LogWarning("Cannot create preview - prefab is null!");
            return;
        }
        previewInstance = Instantiate(prefab);
        previewInstance.name = "TowerPreview";
        // Start hidden and positioned far away until we have a valid mouse position
        previewInstance.SetActive(false);
        previewInstance.transform.position = new Vector3(0, -1000, 0);
        previewInstance.transform.localScale = Vector3.one;
        previewInstance.transform.rotation = Quaternion.identity;
        foreach (Collider c in previewInstance.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (Collider2D c in previewInstance.GetComponentsInChildren<Collider2D>()) c.enabled = false;
        
        foreach (SpriteRenderer sr in previewInstance.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.sortingOrder = 100; // High sorting order to ensure visibility
            sr.enabled = true;
        }
        
        SetPreviewColor(invalidColor);
        Debug.Log($"Preview created from prefab: {prefab.name}, active: {previewInstance.activeSelf}, position: {previewInstance.transform.position}");
    }

    void SetPreviewColor(Color c)
    {
        if (previewInstance == null) return;
        foreach (SpriteRenderer sr in previewInstance.GetComponentsInChildren<SpriteRenderer>())
        {
            Color baseCol = sr.color;
            sr.color = new Color(c.r, c.g, c.b, c.a);
        }
    }

    void CycleTower(int delta)
    {
        selectedTowerIndex = (selectedTowerIndex + delta + towerPrefabs.Length) % towerPrefabs.Length;
        CreatePreview();
    }

    Vector3 SnapToGrid(Vector3 world)
    {
        float size = GetCellSize();
        Vector2 off = GetGridOffset();
        int gx = Mathf.RoundToInt((world.x - off.x) / size);
        int gy = Mathf.RoundToInt((world.y - off.y) / size);
        return new Vector3(gx * size + off.x, gy * size + off.y, 0f);
    }

    bool IsValidPlacement(Vector3 pos)
    {
        Vector2 pos2D = new Vector2(pos.x, pos.y);
        
        if (simpleMazeGenerator != null && simpleMazeGenerator.IsWall(pos2D))
        {
            return false;
        }
        
        float cell = GetCellSize();
        float radius2D = Mathf.Max(0.1f, cell * placementRadiusMultiplier);
        
        Collider2D[] hits2D = Physics2D.OverlapCircleAll(pos2D, radius2D, placementBlockMask);
        foreach (Collider2D c in hits2D)
        {
            if (c != null && !c.name.Contains("Preview") && c.gameObject != previewInstance)
            {
                return false;
            }
        }
        
        Vector3 pos3D = new Vector3(pos.x, pos.y, 0f);
        Collider[] hits3D = Physics.OverlapBox(pos3D, new Vector3(cell * 0.3f, cell * 0.3f, 0.5f), Quaternion.identity, placementBlockMask);
        foreach (Collider c in hits3D)
        {
            if (c != null && !c.name.Contains("Preview") && c.gameObject != previewInstance)
            {
                if (c.name.Contains("Amp") || c.name.Contains("Wall") || c.name.Contains("Tower") || c.name.Contains("Guitar"))
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    void PlaceTower(Vector3 pos)
    {
        Vector3 finalPos = new Vector3(pos.x, pos.y, 0f);
        
        GameObject towerRoot = new GameObject($"Tower_{Time.time}");
        towerRoot.transform.position = finalPos;
        
        if (towerLayer != 0)
        {
            towerRoot.layer = towerLayer;
        }
        
        GameObject towerVisual = Instantiate(towerPrefabs[selectedTowerIndex], finalPos, Quaternion.identity);
        towerVisual.name = "Visual";
        towerVisual.transform.SetParent(towerRoot.transform);
        towerVisual.transform.localPosition = Vector3.zero;
        
        Swing swing = towerVisual.GetComponent<Swing>();
        if (swing != null)
        {
            Destroy(swing);
        }
        
        foreach (SpriteRenderer sr in towerVisual.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.sortingOrder = 10;
        }
        
        Collider existingCollider3D = towerVisual.GetComponent<Collider>();
        if (existingCollider3D != null)
        {
            existingCollider3D.enabled = false;
        }
        
        NavMeshObstacle visualObstacle = towerVisual.GetComponent<NavMeshObstacle>();
        if (visualObstacle != null)
        {
            Destroy(visualObstacle);
        }
        
        float cell = GetCellSize();
        
        BoxCollider2D box2D = towerRoot.AddComponent<BoxCollider2D>();
        box2D.size = new Vector2(1.5f, 4f);
        
        NavMeshObstacle obstacle = towerRoot.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;
        obstacle.carveOnlyStationary = false;
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.size = new Vector3(2f, 4f, 10f);
        obstacle.center = Vector3.zero;
        obstacle.enabled = true;
        
        // Add TowerShooter component for combat
        TowerShooter shooter = towerRoot.AddComponent<TowerShooter>();
        shooter.visualTransform = towerVisual.transform;
        shooter.firePoint = towerRoot.transform;
        
        // Assign stats if available
        if (towerStats != null && selectedTowerIndex < towerStats.Length && towerStats[selectedTowerIndex] != null)
        {
            shooter.stats = towerStats[selectedTowerIndex];
        }
        else
        {
            // Use default values if no stats assigned
            shooter.projectilePrefab = defaultProjectilePrefab;
        }
        
        // Set enemy layer for targeting
        if (enemyLayer != 0)
        {
            shooter.enemyLayer = enemyLayer;
        }
        
        Debug.Log($"Placed tower at {finalPos} with shooter component");
        
        StartCoroutine(RegeneratePathAfterPlacement(towerRoot));
    }
    
    System.Collections.IEnumerator RegeneratePathAfterPlacement(GameObject placedTower)
    {
        yield return new WaitForSeconds(0.5f);
        
        if (pathfinder != null)
        {
            pathfinder.RegeneratePath();
            
            yield return null;
            
            EnemyWalker[] enemies = FindObjectsByType<EnemyWalker>(FindObjectsSortMode.None);
            foreach (EnemyWalker enemy in enemies)
            {
                if (enemy != null && enemy.IsWalking)
                {
                    enemy.RefreshPath();
                }
            }
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}


