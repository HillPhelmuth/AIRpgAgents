using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.Enums;
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum NPCState
{
    Idle,
    Walking,
    Talking,
    Working,
    Sleeping,
    Combat,
    Dead
}