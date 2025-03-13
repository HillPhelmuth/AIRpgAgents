using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.Rules;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MagicTradition
{
    Theurgic, // Divine/Clerical magic from deities
    Arcane    // Mage/Wizardly magic from study and manipulation of magical energies
}