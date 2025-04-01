using System.ComponentModel;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.PlayerCharacter;
using Newtonsoft.Json.Converters;
using SkPluginComponents.Models;
using CosmosIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace AIRpgAgents.GameEngine.Rules;

/// <summary>
/// Represents a specific effect that can be applied by a spell.
/// </summary>
public class SpellEffect
{
    /// <summary>
    /// Gets or sets the name of the type of effect (e.g., "Attack", "Heal", "Protect").
    /// </summary>
    public SpellEffectType SpellEffectType { get; set; }

    /// <summary>
    /// Gets or sets the description of how this effect works.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the numeric value of the effect, if applicable.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Gets or sets who the effect targets ("Self", "Ally", "Enemy").
    /// </summary>
    public TargetType Target { get; set; }
    [JsonPropertyName("Range")]
    public string? Range { get; set; }

    [JsonPropertyName("Duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("DamageType")]
    public string? DamageType { get; set; }

    [JsonPropertyName("DiceFormula")] 
    public string DiceFormula { get; set; } = "1D6";
    [JsonIgnore]
    [CosmosIgnore]
    public DieType DamageDie
    {
        get
        {
            // Remove the leading integer from the damage formula
            var dieTypeString = DiceFormula[1..];
            return Enum.TryParse<DieType>(dieTypeString, out var dieType) ? dieType : DieType.D6;
        }
    }
    [JsonIgnore]
    [CosmosIgnore]
    public int DieCount
    {
        get
        {
            // Get the leading integer from the damage formula
            var dieCountString = DiceFormula[..1];
            return int.TryParse(dieCountString.ToUpper(), out var dieCount) ? dieCount : 1;
        }
    }
    [Description("The number of hits the spell makes per cast.")]
    public int DamageCount { get; set; }
    [JsonPropertyName("AreaOfEffect")]
    public string? AreaOfEffect { get; set; }

}
public class SpellTradition
{
    [JsonPropertyName("Name")]
    public MagicTradition Name { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Spells")] 
    public List<Spell> Spells { get; set; } = [];
}

public class Spell
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Level")]
    public int Level { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("ManaCost")]
    public int ManaCost { get; set; }

    [JsonPropertyName("CastTime")]
    public CastTime CastTime { get; set; }

    [JsonPropertyName("Range")]
    public string? Range { get; set; }

    [JsonPropertyName("Duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("SaveType")]
    public SaveType SaveType { get; set; }
    
    [JsonPropertyName("Band")]
    public Band Band { get; set; }

    [JsonPropertyName("Effects")]
    public List<SpellEffect> Effects { get; set; } = [];
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum CastTime { Action, FullRound, Reaction, The1Minute };
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum SaveType { Fortitude, None, Reflex, Special, Will, WillForAttackers };

[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum Band
{
    [Description("Levels 1 - 5")]
    Lower,
    [Description("Levels 6 - 10")]
    Mid,
    [Description("Levels 11 - 15")]
    High
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum SpellEffectType
{
    Damage,
    Heal,
    ArmorClass,
    Protect,
    Condition,
    Stun,
    Blind,
    Paralyze,
    Fear,
    Charm,
    Sleep,
    Slow,
    Haste,
    Dominate,
    Buff,
    Summon,
    Detect,
    Illusion,
    Negate,
    Teleport,
    Barrier
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum TargetType
{
    Self,
    Ally,
    Enemy
}