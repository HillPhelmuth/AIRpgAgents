using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace AIRpgAgents.GameEngine.Rules;

/// <summary>
/// Represents the core character attributes in the game system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum RpgAttribute
{
    Might,    // Physical strength, endurance, and power
    Agility,  // Reflexes, balance, and coordination
    Vitality, // Toughness, health, and resilience
    Wits,     // Quick thinking, perception, and problem-solving
    Presence  // Force of personality, leadership, and social influence
}
    
/// <summary>
/// Extension methods for the Attributes enum.
/// </summary>
public static class AttributesExtensions
{
    /// <summary>
    /// Gets a description of what the attribute represents.
    /// </summary>
    public static string GetDescription(this RpgAttribute attribute) => attribute switch
    {
        RpgAttribute.Might => "Physical strength, endurance, and the ability to perform feats of power",
        RpgAttribute.Agility => "Reflexes, balance, and coordination, affecting actions requiring speed or precision",
        RpgAttribute.Vitality => "Toughness, health, and overall resilience to damage and fatigue",
        RpgAttribute.Wits => "Quick thinking, perception, and problem-solving skills in dynamic situations",
        RpgAttribute.Presence => "Force of personality, leadership, and social influence",
        _ => throw new ArgumentOutOfRangeException(nameof(attribute))
    };
        
    /// <summary>
    /// Gets the primary gameplay uses for this attribute.
    /// </summary>
    public static string GetPrimaryUses(this RpgAttribute attribute) => attribute switch
    {
        RpgAttribute.Might => "Melee attack rolls, strength-based checks, carrying capacity",
        RpgAttribute.Agility => "Dodge, ranged attack rolls, stealth, acrobatics",
        RpgAttribute.Vitality => "Health points, endurance-based saving throws, resistance to fatigue",
        RpgAttribute.Wits => "Initiative rolls, perception checks, traps, puzzles",
        RpgAttribute.Presence => "Persuasion, intimidation, leadership, social interactions",
        _ => throw new ArgumentOutOfRangeException(nameof(attribute))
    };
        
    /// <summary>
    /// Calculates the modifier for a given attribute score.
    /// </summary>
    /// <param name="attribute">The attribute (unused but kept for method chaining)</param>
    /// <param name="score">The attribute score (3-20)</param>
    /// <returns>The attribute modifier</returns>
    public static int CalculateModifier(this RpgAttribute attribute, int score)
    {
        return AttributeSystem.CalculateModifier(score);
    }
}