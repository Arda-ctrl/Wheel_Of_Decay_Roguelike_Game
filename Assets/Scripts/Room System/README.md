# Room System

This system handles procedural dungeon generation for the Wheel of Decay roguelike game.

## Closed Graph Algorithm

The room generation system now implements a "closed graph" algorithm that ensures all doorways are properly connected or capped. This prevents issues with open doorways that lead nowhere.

### Key Components

1. **RoomGenerator.cs**: The main class for dungeon generation, now with added functionality to handle open connections.
2. **DoorCap.cs**: A component that can be attached to prefabs used to visually block off unused doorways.

### How It Works

1. After dungeon generation, the system analyzes all rooms to find unconnected doorways.
2. For each open connection, it:
   - First tries to connect it to a neighboring room
   - If that fails, tries to place a new room
   - If that fails, caps the doorway with the assigned door cap prefab

### Special Boss Room Handling

The boss room (end room) can be configured to be a dead-end with only one entrance. This creates a more natural flow where the player enters the boss room and doesn't have other exits.

## Setup Instructions

1. Create a door cap prefab:
   - Create a new empty GameObject
   - Add a visual component (like a sprite or mesh) that represents a blocked doorway
   - Add the `DoorCap` component
   - Adjust the collider as needed to prevent player passage
   - Save as a prefab

2. Assign the door cap prefab in the RoomGenerator:
   - Open your scene with the RoomGenerator
   - In the inspector, find the "Connection Closure" section
   - Assign your door cap prefab to the "Door Cap Prefab" field
   - Set "Allow Boss Dead End" to true if you want the boss room to be a dead-end

3. Test the dungeon generation:
   - Run the game
   - Check that all doorways are either connected to another room or properly capped
   - Verify that the boss room has only one entrance if the setting is enabled 