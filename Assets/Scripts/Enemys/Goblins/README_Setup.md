# Goblin System Setup Guide

## Unity Editor'da Kurulum Adımları

### 1. Script Bağlantıları
Her goblin prefab'ı için Unity Editor'da şu scripti bağlayın:

- **DaggerGoblin.prefab** → `DaggerGoblin` script
- **DaggerGoblinBomber.prefab** → `DaggerGoblinBomber` script  
- **TrapperGoblin.prefab** → `TrapperGoblin` script
- **TrapperGoblinBombs.prefab** → `TrapperGoblinBombs` script
- **GoblinTrap.prefab** → `GoblinTrap` script
- **GoblinBomb.prefab** → `GoblinBomb` script

### 2. Prefab Referansları
TrapperGoblin ve TrapperGoblinBombs için:
- `bearTrapPrefab` → GoblinTrap prefab'ını sürükleyin
- `mudBombPrefab` → GoblinBomb prefab'ını sürükleyin

### 3. Sprite Assignment
Her goblin için kendi sprite'ınızı:
- SpriteRenderer component'inde Sprite field'ına sprite'ınızı sürükleyin

### 4. Audio Clips (Opsiyonel)
Goblin script'lerinde audio clip field'larına ses dosyalarınızı ekleyin:
- `attackSound`
- `deathSound` 
- `explosionSound` (bomberlar için)

### 5. Layer Setup
- Player'ı "Player" layer'ına koyun (Layer 8)
- Goblinleri "Enemy" layer'ına koyun

### 6. Room Data Setup
VerdantHallow_BasicRoom.asset'te:
- `possibleEnemies` array'ine goblin prefab'larını sürükleyin
- `minEnemyCount` ve `maxEnemyCount` ayarlayın

### 7. Biom Integration
VerdantHallow.asset'te:
- `roomPool` ve `genericRoomPool`'a room data'ları ekleyin

## Test Etme
1. Bir room'a goblin prefab'ları yerleştirin
2. Play mode'a geçin
3. Goblinlerin AI davranışlarını test edin

## Troubleshooting
- Script null reference hatası alıyorsanız: Prefab'larda script referansları eksik
- Goblinler spawn olmuyorsa: Room data'da enemy array'i boş
- AI çalışmıyorsa: PlayerController.Instance null olabilir

