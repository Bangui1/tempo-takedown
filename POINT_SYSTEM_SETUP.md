# Point System Setup Instructions

I've created a random point generation system for your labyrinth tower defense game. Here's how to set it up:

## Files Created:
1. `Assets/Scripts/PointSpawner.cs` - TextMeshPro version (requires TextMeshPro package)
2. `Assets/Scripts/SimplePointSpawner.cs` - Unity built-in Text version (works immediately)
3. `Assets/Scripts/PointDisplay.cs` - Helper script for point text management
4. `Assets/Prefabs/point.prefab` - Point prefab with text display

## Quick Setup (Recommended):

### Step 1: Create the Point Spawner
1. In Unity, create an empty GameObject in your scene
2. Name it "PointSpawner"
3. Add the `SimplePointSpawner` script component to it

### Step 2: Create a Simple Point Prefab
Since the prefab I created might have issues, let's create a simple one:

1. Create an empty GameObject in your scene
2. Add a Canvas component to it
3. Set Canvas Render Mode to "World Space"
4. Add a Text component as a child
5. Set the Text properties:
   - Font: Arial (or your downloaded font)
   - Font Size: 24
   - Alignment: Center
   - Color: White
6. Scale the Canvas to 0.01, 0.01, 0.01 (to make it small in world space)
7. Drag this GameObject to your Prefabs folder to create a prefab
8. Delete the instance from the scene

### Step 3: Configure the Spawner
1. Select your PointSpawner GameObject
2. In the SimplePointSpawner component, assign the point prefab you just created
3. Adjust the spawn parameters:
   - Min Pos: Controls the minimum spawn area (default: -8, -8)
   - Max Pos: Controls the maximum spawn area (default: 8, 8)
   - Min Distance From Amps: How far points spawn from walls (default: 1.5)
   - Min Distance Between Points: How far apart points are (default: 2.0)

## Features:
- **Random Generation**: Points spawn randomly within specified bounds
- **Collision Avoidance**: Points avoid spawning too close to amps (walls)
- **Sequential Order**: Points are generated in order: START, 1, 2, 3, 4, END
- **Color Coding**: 
  - START = Green
  - Numbers (1-4) = Yellow  
  - END = Red
- **Distance Management**: Points maintain minimum distance from each other

## Customization:
- Modify `pointLabels` array in the script to change point names
- Adjust spawn area with `minPos` and `maxPos`
- Change colors by modifying the color assignments in `SpawnPoints()`
- Add your downloaded font by replacing the font in the point prefab

## Adding Your Downloaded Font:
1. Import your font file (.ttf or .otf) into Unity
2. Select the font asset in the Project window
3. In the Inspector, set "Rendering Mode" to "Distance Field" for better quality
4. Open the point prefab
5. Select the Text component
6. Change the Font to your imported font
7. Adjust font size and other properties as needed

The system will automatically generate 6 points (START, 1, 2, 3, 4, END) in random positions that don't interfere with your amp walls!