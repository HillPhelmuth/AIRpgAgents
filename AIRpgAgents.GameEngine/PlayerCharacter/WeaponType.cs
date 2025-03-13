using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.PlayerCharacter;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WeaponType
{
    Melee,
    Ranged,
    MeleeAndRanged,
    Spell
}