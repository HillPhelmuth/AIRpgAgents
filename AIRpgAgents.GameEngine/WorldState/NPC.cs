using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Enums;

namespace AIRpgAgents.GameEngine.WorldState;

public class NPC
{
    public string? Id => Name;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CurrentLocationId { get; set; }
    public NPCState State { get; set; } = NPCState.Idle;
    public NPCDisposition Disposition { get; set; } = NPCDisposition.Neutral;
    //public List<Relationship> Relationships { get; set; } = []; // List of relationships
    //public Schedule DailySchedule { get; set; } = new Schedule();
    //public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}
[JsonConverter(typeof(JsonStringEnumConverter))]
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
