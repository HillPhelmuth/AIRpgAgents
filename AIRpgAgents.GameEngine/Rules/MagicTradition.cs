using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.Rules;

[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum MagicTradition
{
    Theurgic, // Divine/Clerical magic from deities
    Arcane    // Mage/Wizardly magic from study and manipulation of magical energies
}