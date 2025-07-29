# Kingdom Enemy System

Bu sistem 4 krallık için kapsamlı bir enemy sistemi sağlar. Her krallığın kendine özgü enemy türleri, boss'ları ve özel yetenekleri vardır.

## Sistem Yapısı

### 1. Temel Sınıflar

#### BaseEnemy
- Tüm enemy'ler için temel superclass
- Health, Movement, Status Effect sistemleri
- AI ve Combat mantığı
- Debug ve Gizmo desteği

#### Kingdom-Specific Base Classes
- **FireEnemy**: Ateş direnci, yanma yetenekleri
- **IceEnemy**: Buz direnci, dondurma yetenekleri  
- **NatureEnemy**: Doğa direnci, iyileştirme yetenekleri
- **ShadowEnemy**: Gölge direnci, gizlenme yetenekleri

### 2. Enemy Türleri

#### Normal Enemies (Her krallık için ~15 adet)
- **Fire Kingdom**: Fire Goblin, Fire Warrior, Fire Mage
- **Ice Kingdom**: Ice Archer, Ice Warrior, Ice Mage  
- **Nature Kingdom**: Nature Druid, Nature Warrior, Nature Shaman
- **Shadow Kingdom**: Shadow Assassin, Shadow Warrior, Shadow Mage

#### Elite Enemies (Her krallık için ~5 adet)
- Daha güçlü versiyonlar
- Özel yetenekler
- Daha iyi ödüller

#### Mini Bosses (Her krallık için 3 adet)
- **Fire Kingdom**: Fire General, Fire Commander, Fire Warlord
- **Ice Kingdom**: Ice General, Ice Commander, Ice Warlord
- **Nature Kingdom**: Nature General, Nature Commander, Nature Warlord  
- **Shadow Kingdom**: Shadow General, Shadow Commander, Shadow Warlord

#### Main Bosses (Her krallık için 1 adet)
- **Fire Kingdom**: Fire King
- **Ice Kingdom**: Ice Queen
- **Nature Kingdom**: Nature Lord
- **Shadow Kingdom**: Shadow Emperor

## Kullanım

### 1. Enemy Data Oluşturma

```csharp
// ScriptableObject olarak EnemyData oluşturun
[CreateAssetMenu(fileName = "Fire Goblin", menuName = "Game/Enemies/Fire Kingdom/Fire Goblin")]
public class FireGoblinData : EnemyData
{
    // Özel ayarlar
}
```

### 2. Enemy Prefab Oluşturma

1. Boş GameObject oluşturun
2. BaseEnemy'den türetilmiş script ekleyin (örn: FireGoblin)
3. EnemyData'yı atayın
4. Gerekli component'leri ekleyin (Rigidbody2D, Collider2D, SpriteRenderer)
5. Tag'ini "Enemy" olarak ayarlayın

### 3. Boss Oluşturma

```csharp
// BossData ScriptableObject oluşturun
// BaseBoss'tan türetilmiş script ekleyin
// Boss abilities tanımlayın
```

## Krallık Özellikleri

### Fire Kingdom 🔥
- **Tema**: Ateş, yıkım, saldırganlık
- **Özellikler**: Yanma hasarı, ateş direnci, yüksek hasar
- **Enemy'ler**: Goblin'ler, Warrior'lar, Mage'ler
- **Boss**: Fire King - Meteor Strike, Fire Storm, Lava Pool

### Ice Kingdom ❄️
- **Tema**: Buz, kontrol, yavaşlatma
- **Özellikler**: Dondurma, yavaşlatma, savunma
- **Enemy'ler**: Archer'lar, Warrior'lar, Mage'ler
- **Boss**: Ice Queen - Frost Nova, Ice Prison, Blizzard

### Nature Kingdom 🌿
- **Tema**: Doğa, iyileştirme, zehir
- **Özellikler**: İyileştirme, zehir hasarı, dayanıklılık
- **Enemy'ler**: Druid'ler, Warrior'lar, Shaman'lar
- **Boss**: Nature Lord - Mass Heal, Poison Storm, Nature's Wrath

### Shadow Kingdom 👤
- **Tema**: Gölge, gizlenme, kritik hasar
- **Özellikler**: Gizlenme, backstab, yüksek kritik hasar
- **Enemy'ler**: Assassin'ler, Warrior'lar, Mage'ler
- **Boss**: Shadow Emperor - Shadow Step, Death Mark, Void Burst

## Sistem Özellikleri

### 1. Status Effect Sistemi
- Poisoned, Burning, Frozen, Chilled
- Her krallık kendi elementine dirençli
- Status effect'ler süre bazlı çalışır

### 2. Projectile Sistemi
- EnemyProjectile base class
- Her krallık kendi projectile'larını kullanır
- Fire: Fireball, Ice: Ice Arrow, Nature: Poison Dart, Shadow: Shadow Dagger

### 3. Boss Sistemi
- Phase-based combat (Phase 1, Phase 2, Enrage)
- Ability-based combat
- Special rewards ve drops

### 4. AI Sistemi
- Player detection
- Movement patterns
- Ability usage
- Kingdom-specific behaviors

## Örnek Kullanım

```csharp
// Fire Goblin oluşturma
public class FireGoblin : FireEnemy
{
    protected override void PerformAttack()
    {
        // Fire-specific attack logic
        ThrowFireball();
    }
    
    protected override void PerformFireSpecialAbility()
    {
        // Fire Burst ability
        CreateFireBurst();
    }
}
```

## Gelecek Geliştirmeler

1. **Enemy Spawning System**: Room-based enemy spawning
2. **Difficulty Scaling**: Level-based enemy scaling
3. **Enemy AI Improvements**: Pathfinding, group tactics
4. **More Abilities**: Her enemy türü için daha fazla ability
5. **Boss Arenas**: Special boss fight areas
6. **Enemy Drops**: Loot system integration
7. **Enemy Animations**: Kingdom-specific animations

## Notlar

- Mevcut EnemyController ve EnemyBullet sistemini koruyoruz
- Yeni sistem paralel olarak çalışacak
- Eski sistem kaldırılmadan önce test edilmeli
- Tüm enemy'ler için EnemyData ScriptableObject'leri oluşturulmalı 