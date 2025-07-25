# 🔍 Mana System Debug Guide

## ❌ Problem: UI'da Mana Değişikliği Görünmüyor

### 🔧 Adım Adım Kontrol Listesi

#### 1. PlayerManaController Component Kontrolü
```
✅ Unity'de Player GameObject'ini seç
✅ Inspector'da "PlayerManaController" component'i var mı?
❌ Yoksa: Add Component → PlayerManaController
```

#### 2. UI Slider ve Text Kontrolü
```
✅ Hierarchy'de Canvas'ı bul
✅ Canvas altında Mana Slider var mı?
✅ Canvas altında Mana Text var mı?
❌ Yoksa: Create → UI → Slider ve Text oluştur
```

#### 3. UI_Controller Referans Kontrolü
```
✅ Canvas'ta UI_Controller component'i var mı?
✅ UI_Controller'da "Mana Slider" field'ı bağlı mı?
✅ UI_Controller'da "Mana Text" field'ı bağlı mı?
❌ Değilse: Inspector'da Mana Slider ve Text'i sürükleyip bırak
```

#### 4. Console Log Kontrol
Play mode'da **M tuşu**na bastığında ne görüyorsun?

**Beklenen Loglar:**
```
💧 PlayerManaController initialized: 100/100
💧 PlayerManaController Instance: SET
💧 Current Mana: 100/100 (100.0%)
💧 PlayerManaController found and working!
🧪 Testing 25 mana consumption...
💧 [ConsumeMana] Attempting to consume 25 mana. Current: 100/100
💧 Mana consumed: 25, Changed: 100 → 75/100
💧 [UI] Mana slider updated: 75/100
💧 [UI] Mana text updated: 75 / 100
🧪 Mana consumption result: True
```

**Possible Error Messages:**
```
❌ PlayerManaController not found! Please add PlayerManaController component to Player GameObject!
⚠️ [UI] Mana slider is NULL! Please assign mana slider in UI_Controller.
⚠️ [UI] Mana text is NULL! Please assign mana text in UI_Controller.
❌ [UI] UI_Controller.Instance is NULL!
```

#### 5. T Tuşu ile Overflow Test
**T tuşu**na bastığında ne görüyorsun?

**Beklenen Loglar:**
```
🧪 Testing ElementalOverflow with T key
🎯 ElementalOverflow: Caster is Player with tag 'Player'
🔍 Found 3 total colliders in range
🎯 Found enemy for overflow: Skeleton
🚫 Skipping caster (self): Player  
✅ Final enemy count for overflow: 1
💧 [ConsumeMana] Attempting to consume 50 mana. Current: 100/100
💧 Mana consumed: 50, Changed: 100 → 50/100
💧 [UI] Mana slider updated: 50/100
💧 [UI] Mana text updated: 50 / 100
💧 ElementalOverflow consumed 50 mana (from SO settings)
💥 Applying overflow to enemy: Skeleton
💥 Dealt 30 damage to Skeleton
🔥 Applied 5 Fire stacks to Skeleton
✨ Created overflow VFX for Skeleton (will destroy in 2s)
🔊 Playing overflow SFX (no VFX on player)
```

## 🚨 Yaygın Sorunlar ve Çözümleri

### Problem 1: PlayerManaController Eksik
```
Error: ❌ PlayerManaController not found!
Solution: Player GameObject'ine PlayerManaController component'ini ekle
```

### Problem 2: UI Referansları Eksik
```
Error: ⚠️ [UI] Mana slider is NULL!
Solution: UI_Controller'da Mana Slider field'ını bağla
```

### Problem 3: UI Elemanları Yok
```
Error: UI elemanları hiç oluşturulmamış
Solution: Canvas'ta Mana Slider ve Text oluştur
```

### Problem 4: Mana SO Ayarları
```
Error: Mana tüketimi 0 olarak gözüküyor
Solution: ElementalOverflow SO'sunda Mana Cost değerini kontrol et
```

## 🎯 Hızlı Test

1. **M tuşu** → Mana durumu ve test consumption
2. **T tuşu** → Overflow ile SO'dan mana tüketimi
3. **Console** → Hataları ve başarıları izle
4. **UI** → Slider ve text güncellemelerini gözlemle

## 📝 Hangi Adımda Kaldın?

Lütfen **M tuşu**na basıp console'da gördüğün logları paylaş, böylece hangi adımda problem olduğunu anlayabiliriz! 