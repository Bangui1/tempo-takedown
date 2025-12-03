using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshGroundCreator : MonoBehaviour
{
    [Header("Ground Settings")]
    public bool createOnStart = true;
    public float groundSize = 50f;
    public float groundHeight = 0f;
    
    void Start()
    {
        if (createOnStart)
        {
            CreateGround();
        }
    }
    
    [ContextMenu("Create Ground Plane")]
    public void CreateGround()
    {
        // Check if ground already exists
        GameObject existingGround = GameObject.Find("NavMeshGround");
        if (existingGround != null)
        {
            Debug.Log("Ground plane already exists!");
            return;
        }
        
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "NavMeshGround";
        ground.transform.position = new Vector3(0, groundHeight, 0);
        ground.transform.localScale = new Vector3(groundSize / 10f, 1, groundSize / 10f);
        
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1)
        {
            groundLayer = LayerMask.NameToLayer("Ignore Raycast");
        }
        if (groundLayer != -1)
        {
            ground.layer = groundLayer;
        }
        
        // Mark as Navigation Static (if possible)
        // Note: This requires editor, but collider is enough for NavMesh
        
        Debug.Log($"Created ground plane at Y={groundHeight}, size={groundSize}");
        
        // Try to bake NavMesh if surface exists
        NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
            Debug.Log("NavMesh baked with new ground plane");
        }
    }
}

