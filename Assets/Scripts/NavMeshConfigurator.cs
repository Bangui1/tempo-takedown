using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshConfigurator : MonoBehaviour
{
    [ContextMenu("Configure NavMeshSurface")]
    public void ConfigureNavMeshSurface()
    {
        NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface == null)
        {
            Debug.LogError("NavMeshSurface not found!");
            return;
        }
        
        // Set to collect only specific layers (not "All")
        // This prevents the "excessive tiles" error
        surface.collectObjects = CollectObjects.Volume;
        
        // Create or find ground layer
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1)
        {
            Debug.LogWarning("'Ground' layer not found. Please create it in Project Settings > Tags and Layers, or use 'Default' layer.");
            groundLayer = 0; // Use Default layer
        }
        
        // Configure to only include ground layer
        // Note: NavMeshSurface.includeLayers is not directly settable, but we can use CollectObjects.Volume
        // The key is to put ground on a separate layer and configure manually in Inspector
        
        Debug.Log("NavMeshSurface configured. IMPORTANT: In Inspector, set:");
        Debug.Log("1. Collect Objects: 'Volume' or 'All Game Objects'");
        Debug.Log("2. Include Layers: Only select 'Ground' layer (or 'Default' if no Ground layer)");
        Debug.Log("3. This will exclude walls from baking, preventing 'excessive tiles' error");
        Debug.Log("4. Walls will still block paths via NavMeshObstacle carving");
    }
    
    [ContextMenu("Create Ground Layer and Setup")]
    public void CreateGroundAndSetup()
    {
        // Create ground plane
        GameObject ground = GameObject.Find("NavMeshGround");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "NavMeshGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 5f; // 50x50 units
        }
        
        // Try to set to Ground layer (will use Default if Ground doesn't exist)
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1) groundLayer = 0; // Use Default
        ground.layer = groundLayer;
        
        Debug.Log($"Ground plane created/updated at layer {LayerMask.LayerToName(groundLayer)}");
        Debug.Log("Now configure NavMeshSurface in Inspector:");
        Debug.Log("- Collect Objects: 'All Game Objects'");
        Debug.Log($"- Include Layers: Only '{LayerMask.LayerToName(groundLayer)}' (uncheck others)");
    }
}

