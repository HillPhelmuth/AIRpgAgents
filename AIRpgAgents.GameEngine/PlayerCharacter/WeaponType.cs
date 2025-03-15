using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.PlayerCharacter;
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum WeaponType
{
    Melee,
    Ranged,
    MeleeAndRanged,
    Spell
}