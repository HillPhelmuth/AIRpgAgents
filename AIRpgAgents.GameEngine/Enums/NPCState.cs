using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.Enums;
[JsonConverter(typeof(JsonStringEnumConverter))]
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