using System;

namespace AIRpgAgents.Core.Models;

/// <summary>
/// Represents a participant in combat (either a player character or monster)
/// </summary>
public class Combatant
{
    /// <summary>
    /// Represents a participant in combat (either a player character or monster)
    /// </summary>
    public Combatant(string id)
    {
        Id = id;
    }

    /// <summary>
    /// Name of the combatant
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Initiative roll result
    /// </summary>
    public int Initiative { get; set; }
    
    /// <summary>
    /// Indicates if this combatant is a player character (true) or monster (false)
    /// </summary>
    public bool IsPlayerCharacter { get; set; }
    
    /// <summary>
    /// Index of the combatant in their respective collection (party members or monsters)
    /// This is kept for backward compatibility
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// Unique identifier of the combatant (player character ID or monster ID)
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Indicates if the combatant has taken their turn this round
    /// </summary>
    public bool HasTakenTurn { get; set; }
}