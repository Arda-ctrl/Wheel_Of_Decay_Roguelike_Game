/// <summary>
/// Ability türleri - 10 temel yetenek tipi
/// </summary>
public enum AbilityType
{
    None,
    // 1. Elemental Strike - Klasik vuruş, elemental stack yerleştirir
    ElementalStrike,
    // 2. Elemental Buff - Düşman üzerinde eşleşen stack varsa hasar 1.5x
    ElementalBuff,
    // 3. Elemental Projectile - Her 3 saldırıda bir projectile
    ElementalProjectile,
    // 4. Elemental Armor - Cooldown ile hasar azaltma
    ElementalArmor,
    // 5. Elemental Area - 5+ stack ile ölen düşman alan hasarı
    ElementalArea,
    // 6. Elemental Overflow - Big, odadaki tüm düşmanlara 5 stack
    ElementalOverflow,
    // 7. Elemental Burst - 3 stack olduğunda patlama
    ElementalBurst,
    // 8. Elemental Aura - Yakındaki düşmanlara sürekli hasar
    ElementalAura,
    // 9. Elemental Orb - Otomatik saldıran küre
    ElementalOrb
} 