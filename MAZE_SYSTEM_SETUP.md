# Maze Generation System Setup

I've created a complete maze generation system that builds actual connected walls using your amp prefabs! This creates a proper labyrinth for your tower defense game.

## 🏗️ **Files Created:**
1. `MazeGenerator.cs` - Generates connected maze walls using your amp prefab
2. `MazePointSpawner.cs` - Spawns points within the maze paths (not on walls)
3. `MAZE_SYSTEM_SETUP.md` - This setup guide

## 🚀 **Quick Setup:**

### Step 1: Set Up the Maze Generator
1. Create an empty GameObject named "**MazeGenerator**"
2. Add the **`MazeGenerator`** script to it
3. In the inspector, assign your **`amp.prefab`** to the "Wall Prefab" field
4. Configure the maze settings:
   - **Maze Width**: `20` (number of cells wide)
   - **Maze Height**: `20` (number of cells tall)
   - **Cell Size**: `1.0` (size of each cell - adjust based on your amp size)
   - **Maze Offset**: `(0, 0)` (position offset for the maze)

### Step 2: Set Up the Point Spawner
1. Create an empty GameObject named "**MazePointSpawner**"
2. Add the **`MazePointSpawner`** script to it
3. Assign your **point prefab** to the "Point Prefab" field
4. Assign the **MazeGenerator** GameObject to the "Maze Generator" field
5. Set **Min Distance Between Points**: `3.0` (how far apart points should be)

### Step 3: Remove Old Systems
1. **Disable or delete** the old `AmpSpawner` GameObject
2. **Disable or delete** the old `SimplePointSpawner` GameObject

## 🎮 **How It Works:**

### **Maze Generation:**
- Uses **Recursive Backtracking** algorithm to create a proper maze
- All walls are **connected** and form actual pathways
- Adds **random openings** for more interesting paths
- Uses your existing **amp prefab** as wall pieces

### **Point Placement:**
- Points spawn **only in empty maze paths** (not on walls)
- Automatically avoids wall collisions
- Maintains proper distance between points
- **START** (green) → **1, 2, 3, 4** (yellow) → **END** (red)

## ⚙️ **Customization Options:**

### **Maze Settings:**
- **Maze Width/Height**: Larger = bigger maze, more complex
- **Cell Size**: Adjust to match your amp prefab size
- **Maze Offset**: Position the maze in your scene

### **Wall Variety:**
- Add different wall sprites to the **Wall Sprites** array
- The system will randomly choose from them for variety

### **Generation Control:**
- **Generate On Start**: Automatically generate maze when scene starts
- **Clear Existing Walls**: Remove old walls before generating new ones
- Use **Regenerate Maze()** method to create new mazes at runtime

## 🔧 **Advanced Features:**

### **Runtime Controls:**
```csharp
// Get a random empty position in the maze
Vector2 emptyPos = mazeGenerator.GetRandomEmptyPosition();

// Check if a position is a wall
bool isWall = mazeGenerator.IsWall(somePosition);

// Regenerate the entire maze
mazeGenerator.RegenerateMaze();

// Clear all walls
mazeGenerator.ClearMaze();
```

### **Integration with Other Systems:**
- The maze system provides **IsWall()** method for collision detection
- **GetRandomEmptyPosition()** for spawning other objects in empty spaces
- Points automatically avoid walls and spawn in valid paths

## 🎯 **Benefits:**
- **Real maze structure** instead of scattered walls
- **Connected pathways** that create actual navigation challenges
- **Automatic point placement** that respects the maze layout
- **Scalable system** - easily adjust maze size and complexity
- **Performance optimized** - efficient wall generation and collision detection

## 🐛 **Troubleshooting:**
- If points don't spawn: Check that MazeGenerator is assigned to MazePointSpawner
- If maze is too small: Increase Maze Width/Height
- If walls are too big/small: Adjust Cell Size
- If points overlap: Increase Min Distance Between Points

This creates a proper labyrinth tower defense setup with connected walls and strategically placed points!
