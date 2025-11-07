# NavMesh Setup Guide

Your project is now configured to use Unity's NavMesh for pathfinding. Here's how to set it up:

## Step 1: Install AI Navigation Package

1. Open **Window > Package Manager**
2. Click the **+** button (top left)
3. Select **Add package by name**
4. Type: `com.unity.ai.navigation`
5. Click **Add**

## Step 2: Create Ground Plane for NavMesh

You need a walkable surface for NavMesh to bake on:

### Option A: Use a Plane
1. Create **GameObject > 3D Object > Plane**
2. Name it "NavMeshGround"
3. Position at Y=0, scale to cover your maze area
4. Set Layer to "Walkable" or "Default"

### Option B: Use Existing Floor
If you have floor tiles/sprites, add a **MeshCollider** or **BoxCollider** to them.

## Step 3: Setup NavMeshSurface

1. Create an empty GameObject named "NavMeshManager"
2. Add Component: **Nav Mesh Surface** (from AI Navigation package)
3. Configure:
   - **Agent Type**: Humanoid
   - **Collect Objects**: All
   - **Include Layers**: Select layers with your floor/ground
   - **Use Geometry**: Physics Colliders
4. Click **Bake** button in Inspector

## Step 4: Mark Walls as Not Walkable

1. Select all your amp/wall GameObjects
2. In Inspector, check **Navigation Static**
3. Set **Navigation Area**: Not Walkable

## Step 5: Setup NavMeshPathfinder

1. Create empty GameObject named "NavMeshPathfinder"
2. Add Component: **NavMeshPathfinder** script
3. Assign **Nav Mesh Surface**: drag the NavMeshManager object

## Step 6: Setup Tower Placement

1. Select your TowerPlacementManager GameObject
2. Assign **Pathfinder**: drag the NavMeshPathfinder object
3. Set **Tower Layer**: create and assign "Tower" layer
4. Configure **Placement Block Mask** to include walls and towers

## Step 7: Configure Towers

Each tower prefab needs:
1. **BoxCollider** (3D, not 2D)
2. **NavMeshObstacle** component (auto-added on placement, but you can pre-add):
   - Carving: **Enabled**
   - Shape: **Box**
   - Size: match your cell size (e.g., 1×0.5×1)

## Step 8: Camera Setup

Position your camera to look down at the XZ plane:
- Position: (0, 20, 0) or higher
- Rotation: (90, 0, 0)
- Projection: Orthographic (recommended for top-down)

## How It Works

### Path Generation
- `NavMeshPathfinder.GeneratePath()` finds all Point_* objects
- Uses `NavMesh.CalculatePath()` between each sequential pair
- Draws cyan LineRenderer showing the path

### Tower Placement
- Towers get a `NavMeshObstacle` with carving enabled
- When placed, NavMesh is rebaked: `navMeshSurface.BuildNavMesh()`
- Path is recalculated; if no valid path exists, tower is removed

### Advantages Over Grid A*
- ✅ Native Unity system, well-optimized
- ✅ Handles complex geometry automatically
- ✅ Dynamic obstacle carving with NavMeshObstacle
- ✅ Works with 3D colliders and terrain

### Disadvantages
- ❌ Rebaking is slower than grid updates (~100-500ms)
- ❌ Requires 3D colliders and proper layer setup
- ❌ More setup complexity

## Troubleshooting

### "No path found"
- Check NavMesh was baked (blue overlay in Scene view)
- Ensure points are on/near the NavMesh surface
- Verify walls are marked Not Walkable

### Towers don't block path
- Ensure tower has NavMeshObstacle with carving enabled
- Check NavMesh rebakes after placement (console log)
- Verify obstacle size covers at least one cell

### Path goes through walls
- Mark walls as Navigation Static + Not Walkable
- Rebake NavMesh after adding walls
- Check wall colliders are on included layers

## Scripts Reference

- **NavMeshPathfinder.cs**: Uses NavMesh.CalculatePath() for pathfinding
- **TowerPlacementManager.cs**: Adds NavMeshObstacle and rebakes on placement
- **MazeGenerator.cs**: XZ-compatible maze generation
- **MazePointSpawner.cs**: XZ-compatible point spawning
- **MazeSystemManager.cs**: Orchestrates maze, points, and pathfinding

## Next Steps

1. Install AI Navigation package
2. Create ground plane
3. Add NavMeshSurface component and bake
4. Create NavMeshPathfinder GameObject
5. Wire references in TowerPlacementManager and MazeSystemManager
6. Test path generation and tower placement


