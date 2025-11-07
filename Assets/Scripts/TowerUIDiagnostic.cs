using UnityEngine;

/// <summary>
/// Diagnostic script to check if Tower UI setup is correct.
/// Add this to any GameObject and it will check the setup when you press Play.
/// </summary>
public class TowerUIDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Tower UI Diagnostic ===");
        
        // Check for TowerPlacementManager
        TowerPlacementManager placementManager = FindFirstObjectByType<TowerPlacementManager>();
        if (placementManager == null)
        {
            Debug.LogError("❌ TowerPlacementManager not found in scene!");
        }
        else
        {
            Debug.Log($"✅ TowerPlacementManager found: {placementManager.gameObject.name}");
            
            // Check tower prefabs
            if (placementManager.towerPrefabs == null)
            {
                Debug.LogError("❌ Tower prefabs array is NULL!");
            }
            else if (placementManager.towerPrefabs.Length == 0)
            {
                Debug.LogError("❌ Tower prefabs array is EMPTY! Assign tower prefabs in the Inspector.");
            }
            else
            {
                Debug.Log($"✅ Tower prefabs array has {placementManager.towerPrefabs.Length} prefabs");
                for (int i = 0; i < placementManager.towerPrefabs.Length; i++)
                {
                    if (placementManager.towerPrefabs[i] == null)
                    {
                        Debug.LogError($"❌ Tower prefab at index {i} is NULL!");
                    }
                    else
                    {
                        Debug.Log($"  ✅ [{i}] {placementManager.towerPrefabs[i].name}");
                    }
                }
            }
        }
        
        // Check for TowerSelectionUI
        TowerSelectionUI selectionUI = FindFirstObjectByType<TowerSelectionUI>();
        if (selectionUI == null)
        {
            Debug.LogWarning("⚠️ TowerSelectionUI not found. Create one under Canvas if you want UI buttons.");
        }
        else
        {
            Debug.Log($"✅ TowerSelectionUI found: {selectionUI.gameObject.name}");
            
            if (selectionUI.placementManager == null)
            {
                Debug.LogWarning("⚠️ TowerSelectionUI.placementManager is not assigned. It will try to auto-find.");
            }
            else
            {
                Debug.Log($"✅ TowerSelectionUI.placementManager is assigned");
            }
        }
        
        // Check for Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found! Create one: GameObject → UI → Canvas");
        }
        else
        {
            Debug.Log($"✅ Canvas found: {canvas.gameObject.name}");
        }
        
        // Check for EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("⚠️ EventSystem not found. UI buttons might not work. Canvas usually creates one automatically.");
        }
        else
        {
            Debug.Log($"✅ EventSystem found");
        }
        
        Debug.Log("=== End Diagnostic ===");
    }
}

