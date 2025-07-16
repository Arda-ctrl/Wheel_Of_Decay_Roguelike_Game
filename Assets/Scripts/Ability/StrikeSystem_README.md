# ⚡ Strike Sistemi (SO Tabanlı)

Bu sistem, oyuncunun düşmanlarda strike stack'leri oluşturmasını ve bu stack'lere göre hasar vermesini sağlar. **Artık tamamen ScriptableObject tabanlı!**

## 🎯 Sistem Özellikleri

### Strike Stack Sistemi
- Her düşmanın üzerinde `StrikeStack` component'i bulunur
- Maksimum stack sayısı SO'dan alınır (varsayılan: 5)
- Stack'ler SO'dan alınan süre sonra otomatik olarak azalır (varsayılan: 15s)
- Her stack için SO'dan alınan çarpan kadar daha fazla hasar verir (varsayılan: 0.5x)

### Hasar Hesaplama
- **Normal Strike (Buff Yok)**: 
  - 1 stack: SO'dan alınan hasar (varsayılan: 10)
  - 2+ stack: SO'dan alınan hasar + (stack-1) * SO'dan alınan ek hasar
- **Strike Buff Aktif**: 
  - Her stack için SO'dan alınan çarpan kadar daha fazla hasar
  - Örnek: 3 stack = 1 + (3 * 0.5) = 2.5x hasar

## 🎮 Kontroller

### WeaponController Input'ları
- **F Tuşu**: Strike buff'unu toggle eder

### Test Kontrolleri
- **T Tuşu**: Strike ability test (artık SO tabanlı)
- **Y Tuşu**: Strike buff test

## 🔧 Inspector Ayarları

### AbilityData SO Ayarları
- `Has Strike Ability`: Bu ability'nin strike özelliği var mı?
- `Has Strike Buff`: Bu ability'nin strike buff'u var mı?
- `Max Strike Stacks`: Maksimum stack sayısı
- `Strike Stack Decay Time`: Stack'lerin azalma süresi
- `Strike Damage Multiplier Per Stack`: Her stack için hasar çarpanı
- `Normal Strike Damage 1 Stack`: 1 stack için hasar
- `Normal Strike Damage 2 Plus Stacks`: 2+ stack için temel hasar
- `Normal Strike Damage Per Additional Stack`: Her ek stack için ek hasar

### WeaponController Component
- `Is Strike Buff Active`: Strike buff'unun aktif olup olmadığı

## 📊 Debug Bilgileri

### StrikeStack Debug
- Düşmanların üzerinde sarı renkte stack bilgileri görünür
- Stack sayısı, hasar çarpanı ve decay timer gösterilir

### Console Logları
- Strike stack ekleme/kaldırma işlemleri loglanır
- Hasar hesaplamaları detaylı olarak gösterilir
- Ability toggle işlemleri loglanır

## 🧪 Test Sistemi

### StrikeTestController
- Strike sistemini test etmek için kullanılır
- Context menu'den stack ekleme/kaldırma işlemleri yapılabilir
- Test düşmanına otomatik olarak StrikeStack component'i eklenir

## 🔄 Sistem Akışı

1. **Oyuncu ateş eder** → `PlayerBullet` oluşturulur
2. **Strike ability aktifse** → Düşmana `StrikeStack` eklenir
3. **Hasar hesaplanır** → Stack sayısına göre hasar artırılır
4. **Düşmana hasar verilir** → Final hasar uygulanır
5. **Stack decay** → 15 saniye sonra stack azalır

## 📝 Kullanım Örneği

```csharp
// Ability data'yı ayarla (SO tabanlı)
bulletScript.SetAbilityData(strikeAbilityData);

// Strike buff'unu aktifleştir
weaponController.SetStrikeBuff(true);

// Düşmana ateş et
// Otomatik olarak strike stack eklenir ve hasar hesaplanır
```

## ⚠️ Önemli Notlar

- Her düşmanın üzerinde `StrikeStack` component'i olmalıdır
- `EnemyController` otomatik olarak `StrikeStack` ekler
- Strike sistemi `PlayerBullet` ile entegre çalışır
- **Artık tüm ayarlar SO'dan alınır!**
- Debug bilgileri sadece geliştirme sırasında görünür

## 🔧 Geliştirme İpuçları

1. **Yeni hasar formülü eklemek için**: `StrikeStack.CalculateStrikeDamage()` metodunu düzenleyin
2. **Stack limitini değiştirmek için**: SO'dan `Max Strike Stacks` değerini ayarlayın
3. **Decay süresini değiştirmek için**: SO'dan `Strike Stack Decay Time` değerini ayarlayın
4. **Hasar çarpanını değiştirmek için**: SO'dan `Strike Damage Multiplier Per Stack` değerini ayarlayın
5. **Strike ability'sini aktifleştirmek için**: SO'da `Has Strike Ability` = true yapın
6. **Strike buff'unu aktifleştirmek için**: SO'da `Has Strike Buff` = true yapın

## 📁 Oluşturulan SO'lar

- **Strike.asset**: Temel strike ability (hasStrikeAbility = true, hasStrikeBuff = false)
- **StrikeBuff.asset**: Strike buff ability (hasStrikeAbility = true, hasStrikeBuff = true) 