using System.ComponentModel;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Extensions;
using AIRpgAgents.GameEngine.PlayerCharacter;
using Newtonsoft.Json.Converters;

namespace AIRpgAgents.GameEngine.Rules;

/// <summary>
/// Represents a specific effect that can be applied by a spell.
/// </summary>
public class SpellEffect
{
    /// <summary>
    /// Gets or sets the name of the effect (e.g., "Stunned", "Blinded", "AC Bonus").
    /// </summary>
    public string Name { get; set; }           // E.g., "Stunned", "Blinded", "AC Bonus"

    /// <summary>
    /// Gets or sets the description of how this effect works.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the numeric value of the effect, if applicable.
    /// </summary>
    public int Value { get; set; }             // Numerical value, if applicable

    /// <summary>
    /// Gets or sets the duration of the effect in rounds.
    /// </summary>
    public int Duration { get; set; }          // In rounds

    /// <summary>
    /// Gets or sets who the effect targets ("Self", "Ally", "Enemy").
    /// </summary>
    public string Target { get; set; }         // "Self", "Ally", "Enemy"

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellEffect"/> class with specified parameters.
    /// </summary>
    /// <param name="name">The name of the effect.</param>
    /// <param name="description">The description of the effect.</param>
    public SpellEffect(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
public class Spells
{
    [JsonPropertyName("Traditions")]
    public List<Tradition> Traditions { get; set; }

    private static Spells? _spells;
    public static Spells GetSpells => _spells ??= FileHelper.ExtractFromAssembly<Spells>("Spells.json");
}

public class Tradition
{
    [JsonPropertyName("Name")]
    public MagicTradition Name { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; }

    [JsonPropertyName("Spells")]
    public List<Spell> Spells { get; set; }
}

public class Spell
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Level")]
    public long Level { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; }

    [JsonPropertyName("ManaCost")]
    public long ManaCost { get; set; }

    [JsonPropertyName("CastTime")]
    public CastTime CastTime { get; set; }

    [JsonPropertyName("Range")]
    public string Range { get; set; }

    [JsonPropertyName("Duration")]
    public string Duration { get; set; }

    [JsonPropertyName("DamageType")]
    public string DamageType { get; set; }

    [JsonPropertyName("DamageFormula")]
    public string DamageFormula { get; set; }

    [JsonPropertyName("AreaOfEffect")]
    public string AreaOfEffect { get; set; }

    [JsonPropertyName("SaveType")]
    public SaveType SaveType { get; set; }
    
    [JsonPropertyName("Band")]
    public Band Band { get; set; }
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