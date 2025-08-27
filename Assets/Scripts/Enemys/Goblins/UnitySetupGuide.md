# 🎮 Unity Editor Goblin Setup Guide

## Quick Setup Steps

### 1. Prefab Script Assignments

Open each prefab and assign scripts:

#### DaggerGoblin.prefab
1. Select the prefab in Project window
2. Click "Open Prefab" 
3. In Inspector, click script dropdown → assign `DaggerGoblin`
4. Set Player Layer mask to Layer 8 (Player)
5. Apply changes

#### DaggerGoblinBomber.prefab
1. Open prefab
2. Assign `DaggerGoblinBomber` script
3. Apply changes

#### TrapperGoblin.prefab  
1. Open prefab
2. Assign `TrapperGoblin` script
3. Drag `GoblinTrap.prefab` to `Bear Trap Prefab` field
4. Apply changes

#### TrapperGoblinBombs.prefab
1. Open prefab  
2. Assign `TrapperGoblinBombs` script
3. Drag `GoblinTrap.prefab` to `Bear Trap Prefab` field
4. Drag `GoblinBomb.prefab` to `Mud Bomb Prefab` field
5. Apply changes

#### GoblinTrap.prefab
1. Open prefab
2. Assign `GoblinTrap` script  
3. Apply changes

#### GoblinBomb.prefab
1. Open prefab
2. Assign `GoblinBomb` script
3. Apply changes

### 2. Room Data Setup

#### VerdantHallow_BasicRoom.asset
1. Select in Project window
2. In Inspector, expand `Possible Enemies` array
3. Set Size to 4
4. Drag goblin prefabs to elements:
   - Element 0: DaggerGoblin
   - Element 1: DaggerGoblinBomber  
   - Element 2: TrapperGoblin
   - Element 3: TrapperGoblinBombs
5. Set Min Enemy Count: 2
6. Set Max Enemy Count: 4

#### VerdantHallow.asset
1. Select biom asset
2. Expand `Room Pool`
3. Add VerdantHallow_BasicRoom to array
4. Do same for `Generic Room Pool`

### 3. Sprite Assignment (Your Art)
For each goblin prefab:
1. Open prefab
2. In SpriteRenderer component
3. Drag your goblin sprite to `Sprite` field
4. Adjust sprite settings as needed

### 4. Audio Setup (Optional)
In each goblin script component:
1. Drag audio clips to:
   - Attack Sound
   - Death Sound  
   - Explosion Sound (for bombers)

### 5. Test Setup
1. Create test scene
2. Place goblin prefabs in scene
3. Ensure Player object exists with PlayerController
4. Play and test AI behaviors

## Layer Setup Required
- Player: Layer 8 
- Enemies: Default layer or Enemy layer
- Ground: Layer 0

## Common Issues & Solutions

### "Script Missing" Error
- Solution: Manually assign scripts in prefab inspector

### "Null Reference Exception"  
- Check PlayerController.Instance exists
- Verify prefab script assignments
- Ensure layer masks are set correctly

### "Goblins Not Spawning"
- Check room data enemy arrays
- Verify biom integration  
- Ensure room prefab has RoomInitializer script

### "AI Not Working"
- PlayerController must be in scene
- Check detection range settings
- Verify goblin stats are set correctly

