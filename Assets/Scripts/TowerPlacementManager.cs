using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.EventSystems;
using System.Collections;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("References")]
    public MazeGenerator mazeGenerator;
    public NavMeshPathfinder pathfinder;
    public NavMeshSurface navMeshSurface;

    [Header("Towers")]
    public GameObject[] towerPrefabs;
    public int selectedTowerIndex = 0;
    public int towerLayer = 0; // resolved at runtime if 0
    public string towerLayerName = "Tower";

    [Header("Preview")]
    public Color validColor = new Color(0f, 1f, 0f, 0.6f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.6f);
    public LayerMask placementBlockMask = ~0; // layers that block placement (amps/walls/towers)
    public float placementRadiusMultiplier = 0.45f; // portion of cell size used for collision test
    public bool placementModeActive = true; // Can be toggled to disable placement

    private GameObject previewInstance;

    void Start()
    {
        if (mazeGenerator == null) mazeGenerator = FindFirstObjectByType<MazeGenerator>();
        if (pathfinder == null) pathfinder = FindFirstObjectByType<NavMeshPathfinder>();
        if (navMeshSurface == null) navMeshSurface = FindFirstObjectByType<NavMeshSurface>();

        // Resolve tower layer by name if not set
        if (towerLayer == 0 && !string.IsNullOrEmpty(towerLayerName))
        {
            int resolved = LayerMask.NameToLayer(towerLayerName);
            if (resolved != -1) towerLayer = resolved;
            else Debug.LogWarning($"TowerPlacementManager: Layer '{towerLayerName}' not found. Set it in Project Settings > Tags and Layers.");
        }

        // Fallback: if still not set, mirror the maze wall layer so towers count as obstacles
        if (towerLayer == 0 && mazeGenerator != null && mazeGenerator.wallPrefab != null)
        {
            towerLayer = mazeGenerator.wallPrefab.layer;
            Debug.Log($"TowerPlacementManager: Using wall prefab layer '{LayerMask.LayerToName(towerLayer)}' for towers.");
        }

        CreatePreview();
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

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        
        if (groundPlane.Raycast(ray, out distance) && distance > 0 && distance < 1000f)
        {
            Vector3 mouseWorld = ray.GetPoint(distance);
            
            // Only update if we got a valid world position (not at origin by default)
            if (Vector3.Distance(mouseWorld, Vector3.zero) > 0.1f)
            {
                Vector3 snapped = SnapToNavMesh(mouseWorld);
                
                // Only show preview if snapped position is valid (not default origin)
                if (Vector3.Distance(snapped, Vector3.zero) > 0.1f || Vector3.Distance(mouseWorld, Vector3.zero) > 1f)
                {
                    if (previewInstance != null)
                    {
                        // Only show preview when we have a valid mouse position
                        if (!previewInstance.activeSelf)
                        {
                            previewInstance.SetActive(true);
                        }
                        previewInstance.transform.position = new Vector3(snapped.x, 0f, snapped.z);
                        
                        bool valid = IsValidPlacement(snapped);
                        SetPreviewColor(valid ? validColor : invalidColor);
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 placementPos = SnapToNavMesh(mouseWorld);
                        if (IsValidPlacement(placementPos))
                        {
                            PlaceTower(placementPos);
                        }
                    }
                }
                else
                {
                    // Invalid position, hide preview
                    if (previewInstance != null) previewInstance.SetActive(false);
                }
            }
            else
            {
                // Mouse position too close to origin, hide preview
                if (previewInstance != null) previewInstance.SetActive(false);
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
        if (!active && previewInstance != null)
        {
            previewInstance.SetActive(false);
        }
        Debug.Log($"Placement mode: {(active ? "ON" : "OFF")}");
    }

    public void CreatePreview()
    {
        if (previewInstance != null) DestroyImmediate(previewInstance);
        GameObject prefab = towerPrefabs != null && towerPrefabs.Length > 0 ? towerPrefabs[Mathf.Clamp(selectedTowerIndex, 0, towerPrefabs.Length - 1)] : null;
        if (prefab == null) return;
        previewInstance = Instantiate(prefab);
        previewInstance.name = "TowerPreview";
        // Start hidden and positioned far away until we have a valid mouse position
        previewInstance.SetActive(false);
        previewInstance.transform.position = new Vector3(0, -1000, 0); // Hide far below ground
        previewInstance.transform.localScale = Vector3.one; // Ensure scale is correct
        // Disable colliders so preview doesn't interfere
        foreach (Collider c in previewInstance.GetComponentsInChildren<Collider>()) c.enabled = false;
        // Remove NavMeshObstacle from preview if it has one
        NavMeshObstacle previewObs = previewInstance.GetComponent<NavMeshObstacle>();
        if (previewObs != null) DestroyImmediate(previewObs);
        SetPreviewColor(invalidColor);
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

    Vector3 SnapToNavMesh(Vector3 world)
    {
        // First snap to grid
        float size = mazeGenerator != null ? mazeGenerator.cellSize : 1f;
        Vector3 off = mazeGenerator != null ? mazeGenerator.mazeOffset : Vector3.zero;
        int gx = Mathf.RoundToInt((world.x - off.x) / size);
        int gz = Mathf.RoundToInt((world.z - off.z) / size);
        Vector3 gridPos = new Vector3(gx * size + off.x, 0f, gz * size + off.z);
        
        // Then snap to nearest NavMesh position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(gridPos, out hit, 2f, NavMesh.AllAreas))
        {
            return new Vector3(hit.position.x, 0f, hit.position.z);
        }
        
        // If not on NavMesh, return grid position at Y=0
        return new Vector3(gridPos.x, 0f, gridPos.z);
    }

    bool IsValidPlacement(Vector3 pos)
    {
        // Must be on NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(pos, out hit, 2f, NavMesh.AllAreas))
        {
            return false; // Not on NavMesh
        }
        
        // Reject if maze logical cell is a wall
        if (mazeGenerator != null && mazeGenerator.IsWall(pos)) return false;
        
        // Reject if colliding with any blocking collider (walls, other towers, etc.)
        float cell = mazeGenerator != null ? mazeGenerator.cellSize : 1f;
        float radius = Mathf.Max(0.05f, cell * placementRadiusMultiplier);
        Collider[] hits = Physics.OverlapSphere(pos, radius, placementBlockMask);
        
        // Also check for NavMeshObstacles at this position
        foreach (Collider col in hits)
        {
            if (col.GetComponent<NavMeshObstacle>() != null)
            {
                return false; // There's already an obstacle here
            }
        }
        
        return hits.Length == 0;
    }

    void PlaceTower(Vector3 pos)
    {
        // Ensure position is on NavMesh
        NavMeshHit hit;
        Vector3 finalPos = pos;
        if (NavMesh.SamplePosition(pos, out hit, 2f, NavMesh.AllAreas))
        {
            finalPos = new Vector3(hit.position.x, 0f, hit.position.z);
        }
        else
        {
            Debug.LogWarning($"Tower placement position {pos} is not on NavMesh! Placing anyway at Y=0.");
            finalPos = new Vector3(pos.x, 0f, pos.z);
        }
        
        GameObject tower = Instantiate(towerPrefabs[selectedTowerIndex], finalPos, Quaternion.Euler(90, 0, 0));
        if (towerLayer != 0)
        {
            tower.layer = towerLayer;
        }

        // Ensure the tower has a 3D collider
        Collider collider = tower.GetComponent<Collider>();
        float cell = mazeGenerator != null ? mazeGenerator.cellSize : 1f;
        if (collider == null)
        {
            SpriteRenderer sr = tower.GetComponentInChildren<SpriteRenderer>();
            BoxCollider box = tower.AddComponent<BoxCollider>();
            Vector3 size = sr != null && sr.sprite != null ? sr.sprite.bounds.size : new Vector3(cell, 0.5f, cell);
            size.x = Mathf.Max(size.x, cell * 0.9f);
            size.y = 0.5f;
            size.z = Mathf.Max(size.z, cell * 0.9f);
            box.size = size;
            box.center = Vector3.zero;
        }
        else if (collider is BoxCollider existing)
        {
            Vector3 size = existing.size;
            size.x = Mathf.Max(size.x, cell * 0.9f);
            size.z = Mathf.Max(size.z, cell * 0.9f);
            existing.size = size;
        }

        // Add NavMeshObstacle so tower blocks pathfinding
        NavMeshObstacle obstacle = tower.GetComponent<NavMeshObstacle>();
        if (obstacle == null)
        {
            obstacle = tower.AddComponent<NavMeshObstacle>();
        }
        obstacle.carving = true;
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.size = new Vector3(cell, 0.5f, cell);
        obstacle.center = Vector3.zero;
        obstacle.enabled = true;
        
        Debug.Log($"Placed tower at {finalPos} with NavMeshObstacle (carving: {obstacle.carving})");
        
        if (pathfinder != null)
        {
            StartCoroutine(RecalculatePathWithValidation(tower));
        }
    }

    System.Collections.IEnumerator RecalculatePathWithValidation(GameObject placedTower)
    {
        // Wait for NavMeshObstacle to register and carve the NavMesh
        yield return new WaitForFixedUpdate();
        yield return null; // One more frame for obstacle carving to take effect
        
        if (pathfinder != null)
        {
            // NavMeshObstacle carving works at runtime, no rebake needed
            pathfinder.RegeneratePath();
            yield return null;

            // Check if path is still valid after placing tower
            if (!pathfinder.HasValidPath())
            {
                Debug.LogWarning("Tower placement blocks all paths! Removing tower.");
                if (placedTower != null)
                {
                    DestroyImmediate(placedTower);
                }
                // Regenerate path without the tower
                yield return new WaitForFixedUpdate();
                pathfinder.RegeneratePath();
            }
            else
            {
                Debug.Log("Path successfully updated to avoid tower obstacle.");
            }
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}


