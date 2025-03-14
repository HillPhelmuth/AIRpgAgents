namespace AIRpgAgents.GameEngine.PlayerCharacter;

/// <summary>
/// Represents the character's level of exhaustion
/// </summary>
public enum ExhaustionLevel
{
    None = 0,
    
    // Level 1: Disadvantage on ability checks
    Tired = 1,
    
    // Level 2: Speed halved
    Fatigued = 2,
    
    // Level 3: Disadvantage on attack rolls and saving throws
    Exhausted = 3,
    
    // Level 4: Hit point maximum halved
    Weakened = 4,
    
    // Level 5: Speed reduced to 0
    Immobile = 5,
    
    // Level 6: Death
    Dead = 6
}
