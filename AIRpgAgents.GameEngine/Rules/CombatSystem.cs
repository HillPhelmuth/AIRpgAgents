using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.Rules;

public static class CombatSystem
{
    // Attack types
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [CosmosConverter(typeof(StringEnumConverter))]
    public enum AttackType
    {
        Melee,
        Ranged,
        Spell,
        Special
    }

    // Damage types
    
        
    // Attack modifiers based on circumstances
    public static readonly Dictionary<string, int> AttackCircumstanceModifiers = new Dictionary<string, int>
    {
        { "Advantage", 0 }, // Roll twice, take the best
        { "Disadvantage", 0 }, // Roll twice, take the worst
        { "Flanking", 2 },
        { "Higher Ground", 1 },
        { "Prone Target", 2 }, // For melee attacks
        { "Prone Attacker", -2 },
        { "Concealed Target", -2 }
    };
        
    // Standard combat actions
    public static readonly List<string> StandardActions =
    [
        "Attack",
        "Cast Spell",
        "Dash",
        "Disengage",
        "Dodge",
        "Help",
        "Hide",
        "Ready",
        "Search",
        "Use Item"
    ];
        
    // Bonus actions (based on circumstance, class, etc.)
    public static readonly List<string> BonusActions =
    [
        "Off-hand Attack",
        "Cast Bonus Action Spell",
        "Use Class Feature"
    ];
        
    // Reactions
    public static readonly List<string> ReactionActions =
    [
        "Opportunity Attack",
        "Readied Action",
        "Shield Spell",
        "Parry"
    ];

    
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum DamageType
{
    Slashing,
    Piercing,
    Bludgeoning,
    Fire,
    Cold,
    Lightning,
    Acid,
    Poison,
    Necrotic,
    Radiant,
    Force,
    Psychic
}