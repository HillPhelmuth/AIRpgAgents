namespace AIRpgAgents.GameEngine.WorldState;

public class WorldEvent
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LocationId { get; set; } // Where the event is happening
    public List<string> RelatedEntityIds { get; set; } = []; // NPCs, objects involved
    public bool IsResolved { get; set; } = false;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}