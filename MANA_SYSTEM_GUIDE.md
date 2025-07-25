# 💧 Mana System & ElementalOverflow Guide

## 🔧 Setup Instructions

### 1. Player GameObject Setup
- Player GameObject'ine `PlayerManaController` component'ini ekle
- Inspector'da mana ayarlarını yapılandır:
  - Max Mana: 100
  - Mana Regen Rate: 5 (saniye başına)
  - Mana Regen Delay: 3 (saniye)

### 2. UI Canvas Setup
- UI Canvas'ta mana slider ve text oluştur
- UI_Controller component'ine şu alanları bağla:
  - Mana Slider
  - Mana Text

### 3. ElementalOverflow ScriptableObject
- `Assets/SO/Ability/Ability/Fire/` klasöründe ElementalOverflow SO'su oluştur
- Ayarlar:
  - Ability Type: ElementalOverflow
  - Element Type: Fire  
  - Cooldown Duration: 30
  - **Mana Cost: 100** (İstediğin değeri ayarlayabilirsin)
  - Overflow Stack Amount: 5
  - Overflow Damage: 30

**Not**: Mana Cost artık SO'dan ayarlanabilir! İstediğin değeri verebilirsin.

## 🎮 Test Controls

### Mana System
- **M tuşu**: Mevcut mana durumunu console'da göster
- **Mana otomatik yenileme**: 3 saniye sonra 5/sn hızında

### ElementalOverflow
- **T tuşu**: Fire ElementalOverflow'u tetikle
  - **SO'dan ayarlanan mana** tüketir (varsayılan: 100)
  - Odadaki tüm düşmanlara 5 Fire stack + hasar
  - 30 saniye cooldown

## 💧 Mana System Features

- **Mana Consumption**: Ability'ler mana tüketir
- **Auto Regeneration**: 3 saniye beklemeden sonra otomatik yenilme
- **UI Updates**: Real-time mana bar ve text güncellemesi
- **Smart Controls**: Yetersiz mana durumunda ability kullanılamaz

## 🔥 ElementalOverflow Features

- **Area Effect**: Odadaki tüm düşmanlara etki eder
- **Element Stacks**: Her düşmana 5 stack ekler
- **Damage**: Immediate hasar verir
- **Mana Cost**: SO'dan ayarlanabilir mana tüketimi
- **Cooldown**: 30 saniye (enemy kill ile reset edilebilir)

## 🧪 Testing Steps

1. Unity'de Ability Test sahnesini aç
2. Player'a PlayerManaController ekle
3. UI'da mana slider/text'i bağla
4. Play mode'a geç
5. M tuşu ile mana durumunu kontrol et
6. T tuşu ile Overflow'u test et
7. Console'da mana tüketimi loglarını izle

## 🧪 Test Scenarios

1. **Mana sistem test**: M tuşu ile mana durumu kontrol
2. **Overflow test**: T tuşu ile SO'dan ayarlanan mana tüketimi test
3. **Regen test**: Mana otomatik yenilemeyi gözlemle
4. **Validation test**: Yetersiz mana durumunda hata mesajı
5. **SO Configuration test**: Mana Cost değerini değiştirip farklı tüketim test et

## 🐛 Debug Info

- Console'da mana işlemleri loglanır
- GUI'de real-time mana bilgisi gösterilir
- Overflow saldırısı detayları loglanır
- Mana yetersizliği durumunda warning

---
**Not**: Bu sistemler ElementalBurst ile birlikte çalışır. Her iki sistem de aktif olabilir. 