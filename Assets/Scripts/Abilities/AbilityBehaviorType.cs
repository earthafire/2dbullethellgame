/// <summary>
/// Defines the type of behavior an ability should use
/// </summary>
public enum AbilityBehaviorType
{
    /// <summary>
    /// Fires projectiles (Wand, etc.)
    /// </summary>
    Projectile,
    
    /// <summary>
    /// Creates area effects (FrostPulse, WindShield, etc.)
    /// </summary>
    AreaEffect,
    
    /// <summary>
    /// Movement abilities (Dash, etc.)
    /// </summary>
    Movement,
    
    /// <summary>
    /// Melee attacks
    /// </summary>
    Melee,
    
    /// <summary>
    /// Custom behavior defined by script
    /// </summary>
    Custom
}
