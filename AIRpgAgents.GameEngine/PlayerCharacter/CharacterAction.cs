using System;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

/// <summary>
/// Represents an action taken by a character
/// </summary>
public class CharacterAction
{
    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Description of what happened
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional ID of the target of the action (NPC, location, item, etc.)
    /// </summary>
    public string? TargetId { get; set; }
    
    /// <summary>
    /// Optional type of action (Attack, Spell, Rest, etc.)
    /// </summary>
    public string? ActionType { get; set; }
    
    /// <summary>
    /// Optional result or outcome of the action
    /// </summary>
    public string? Result { get; set; }
    
    /// <summary>
    /// Optional campaign ID where action occurred
    /// </summary>
    public string? CampaignId { get; set; }
}
