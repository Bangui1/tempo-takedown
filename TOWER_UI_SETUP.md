# Tower Selection UI Setup Guide

This guide will help you set up a UI system for selecting and placing towers.

## Quick Setup

### Step 1: Create the UI Canvas

1. In Unity, go to **GameObject → UI → Canvas**
2. This creates a Canvas with an EventSystem
3. The Canvas should be set to **Screen Space - Overlay** (default)

### Step 2: Add Tower Selection UI

1. Create an empty GameObject as a child of the Canvas
2. Name it "TowerSelectionUI"
3. Add the **TowerSelectionUI** script component to it
4. In the Inspector:
   - **Placement Manager**: Drag your GameObject with `TowerPlacementManager` component
   - **Button Parent**: Leave empty (will be created automatically) OR create a Panel and assign it
   - **Tower Button Prefab**: Leave empty (buttons will be created automatically) OR create a custom button prefab

### Step 3: Configure TowerPlacementManager

1. Make sure your `TowerPlacementManager` GameObject has:
   - **Tower Prefabs** array assigned with your tower prefabs
   - All other settings configured

### Step 4: Test It!

1. Press Play
2. You should see a panel on the left side with buttons for each tower
3. Click a button to select that tower
4. Move your mouse over the maze - you'll see the preview
5. Click to place the tower
6. Press **Escape** or **Right-Click** to cancel placement mode

## Features

### UI Buttons
- **Automatic Creation**: Buttons are created automatically for each tower prefab
- **Visual Feedback**: Selected button is highlighted in green
- **Labels**: Each button shows the tower's name

### Keyboard Shortcuts
- Press **1, 2, 3, etc.** to quickly select towers
- Press **Escape** or **Right-Click** to cancel placement mode

### Mouse Controls
- **Left Click** on UI button → Select tower and enter placement mode
- **Mouse Move** → Preview follows cursor
- **Left Click** on valid position → Place tower
- **Right Click** or **Escape** → Cancel placement mode
- **Mouse Scroll** → Cycle through towers (still works)

## Customization

### Button Appearance
You can customize the buttons by:
1. Creating a custom button prefab
2. Assigning it to **Tower Button Prefab** in TowerSelectionUI
3. The prefab should have:
   - A `Button` component
   - An `Image` component (for background)
   - Optionally a `Text` child for labels

### Button Colors
In `TowerSelectionUI` Inspector:
- **Selected Color**: Color when button is selected (default: light green)
- **Normal Color**: Default button color (default: white)

### Button Size and Spacing
- **Button Size**: Size of each button (default: 100x100)
- **Button Spacing**: Space between buttons (default: 10)

## Advanced: Custom Button Prefab

If you want to create a custom button prefab:

1. Create a UI Button: **GameObject → UI → Button**
2. Customize the appearance (colors, images, text)
3. Save it as a prefab
4. Assign it to **Tower Button Prefab** in TowerSelectionUI
5. The script will use your custom design but still handle selection logic

## Troubleshooting

**Buttons don't appear:**
- Check that TowerPlacementManager has tower prefabs assigned
- Check Console for errors
- Make sure Canvas is set to Screen Space - Overlay

**Buttons don't respond:**
- Make sure EventSystem exists in the scene (should be created with Canvas)
- Check that TowerPlacementManager is assigned in TowerSelectionUI

**Preview doesn't show:**
- Make sure a tower is selected (button is highlighted)
- Check that placement mode is active
- Verify camera can see the ground plane

**Can't place towers:**
- Make sure you're clicking on valid NavMesh positions
- Check that preview is green (valid placement)
- Verify no obstacles are blocking placement

