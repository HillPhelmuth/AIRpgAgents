using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Enums;
using Newtonsoft.Json.Converters;

namespace AIRpgAgents.GameEngine.World;

public class NPC
{
    public string? Id => Name;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CurrentLocationId { get; set; }
    public NPCState State { get; set; } = NPCState.Idle;
    public NPCDisposition Disposition { get; set; } = NPCDisposition.Neutral;
    
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum NPCDisposition
{
    Friendly,
    Neutral,
    Hostile
}
public class Relationship
{
    public string NpcName { get; set; }
    public NPCDisposition Value { get; set; }
}
