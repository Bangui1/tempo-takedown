# Pathfinding System Setup Guide

## 🎯 **Quick Setup (Recommended)**

### **Option 1: All-in-One Manager (Easiest)**
1. Create an empty GameObject named "**MazeSystemManager**"
2. Add the **`MazeSystemManager`** script to it
3. The script will auto-find all components, or you can manually assign them:
   - **Simple Maze Generator** (or Wall Line Generator)
   - **Point Spawner** (any of the point spawners)
   - **Maze Pathfinder** (or Pathfinder)
4. Press **Play** - everything will generate automatically!

### **Option 2: Manual Setup**
1. **Maze Generation**: Use `SimpleMazeGenerator` or `WallLineGenerator`
2. **Point Spawning**: Use `MazePointSpawner` (works best with mazes)
3. **Pathfinding**: Use `MazePathfinder` (more advanced) or `Pathfinder` (simpler)

## 🔧 **Component Details**

### **MazePathfinder** (Recommended)
- **Advanced A* pathfinding** with wall avoidance
- **Optimal visiting order** using TSP approximation
- **Visual path rendering** with LineRenderer
- **Debug visualization** with waypoints and grid
- **Integration** with all maze generators

### **Pathfinder** (Simpler Alternative)
- Basic A* pathfinding
- Simple nearest-neighbor path ordering
- Visual path rendering
- Good for simple setups

## ⚙️ **Key Settings**

### **MazePathfinder Settings:**
- **Grid Resolution**: `0.25` (higher = more precise, slower)
- **Path Width**: `0.2` (thickness of path line)
- **Path Color**: `Cyan` (color of path)
- **Path Smoothing**: `0.3` (how smooth the path is)
- **Max Iterations**: `1000` (pathfinding limit)

### **Visual Settings:**
- **Show Debug Path**: `true` (shows path in Scene view)
- **Show Grid**: `false` (shows pathfinding grid)
- **Show Waypoints**: `true` (shows debug spheres at points)

## 🎮 **How It Works**

1. **Finds all points** with names starting with "Point_"
2. **Sorts points** by name (START, 1, 2, 3, 4, END)
3. **Calculates optimal order** using TSP approximation
4. **Finds path** between each consecutive point using A*
5. **Renders visual path** using LineRenderer
6. **Avoids all walls** using physics collision detection

## 🚀 **Usage**

### **Automatic Generation:**
- Just press Play - everything generates automatically!

### **Manual Control:**
```csharp
// Generate complete maze system
mazeSystemManager.GenerateCompleteMaze();

// Regenerate just the path
mazeSystemManager.RegeneratePath();

// Clear everything
mazeSystemManager.ClearAll();
```

### **Individual Components:**
```csharp
// Generate path only
mazePathfinder.GeneratePath();

// Clear path
mazePathfinder.ClearPath();
```

## 🎨 **Customization**

### **Path Appearance:**
- Change **Path Color** for different visual style
- Adjust **Path Width** for thicker/thinner lines
- Use custom **Path Material** for special effects

### **Pathfinding Behavior:**
- Increase **Grid Resolution** for more precise paths
- Adjust **Path Smoothing** for smoother/straighter paths
- Modify **Max Iterations** for complex mazes

### **Debug Features:**
- Enable **Show Debug Path** to see path in Scene view
- Enable **Show Grid** to see pathfinding grid
- Enable **Show Waypoints** to see debug spheres

## 🔍 **Troubleshooting**

### **No Path Generated:**
- Check that points are named "Point_START", "Point_1", etc.
- Ensure walls have colliders
- Check LayerMask settings
- Increase Max Iterations for complex mazes

### **Path Goes Through Walls:**
- Check Wall LayerMask is set correctly
- Ensure walls have colliders
- Increase Grid Resolution for more precision

### **Path Looks Jagged:**
- Increase Path Smoothing value
- Increase Grid Resolution
- Check for overlapping walls

## 📋 **Requirements**

- Points must be named "Point_START", "Point_1", "Point_2", "Point_3", "Point_4", "Point_END"
- Walls must have colliders
- Points should be placed in valid (non-wall) positions
- At least 2 points required for pathfinding

## 🎯 **Best Practices**

1. **Use MazePathfinder** for best results
2. **Set appropriate Grid Resolution** (0.25 is usually good)
3. **Ensure proper point naming** (Point_START, Point_1, etc.)
4. **Test with different maze densities** to find optimal settings
5. **Use debug features** to troubleshoot pathfinding issues
