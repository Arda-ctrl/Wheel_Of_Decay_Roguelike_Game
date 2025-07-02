public enum StatusEffectType
{
    None,
    Poisoned,
    Burning,
    Frozen,
    Chilled
}

public interface IStatusEffect
{
    void ApplyStatus(StatusEffectType statusType, float duration);
    void RemoveStatus(StatusEffectType statusType);
    bool HasStatus(StatusEffectType statusType);
} 