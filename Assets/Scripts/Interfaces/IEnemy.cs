using UnityEngine;

/// <summary>
/// Interface for enemy functionality
/// </summary>
public interface IEnemy
{
    /// <summary>
    /// Gets the detection range of the enemy
    /// </summary>
    float GetDetectionRange();
    
    /// <summary>
    /// Sets the detection range of the enemy
    /// </summary>
    /// <param name="range">The new detection range</param>
    void SetDetectionRange(float range);
    
    /// <summary>
    /// Gets the attack range of the enemy
    /// </summary>
    float GetAttackRange();
    
    /// <summary>
    /// Sets the attack range of the enemy
    /// </summary>
    /// <param name="range">The new attack range</param>
    void SetAttackRange(float range);
}
