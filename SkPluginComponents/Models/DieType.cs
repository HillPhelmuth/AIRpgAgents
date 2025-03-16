using System.ComponentModel;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace SkPluginComponents.Models;

/// <summary>
/// Represents the types of dice used in the RPG system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum DieType
{
    [Description("A four-sided die.")]
    D4 = 4,
    [Description("A six-sided die.")]
    D6 = 6,
    [Description("An eight-sided die.")]
    D8 = 8,
    [Description("A ten-sided die.")]
    D10 = 10,
    [Description("A twenty-sided die.")]
    D20 = 20
}