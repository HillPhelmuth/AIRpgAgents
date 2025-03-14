namespace AIRpgAgents.GameEngine.World;

public class WorldEvent
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LocationId { get; set; } // Where the event is happening
    public List<string> RelatedEntityIds { get; set; } = []; // NPCs, objects involved
    public bool IsResolved { get; set; } = false;
    public string StartTime { get; set; } = DateTime.Now.AddYears(-1000).ToString("D");
    public List<string> Tags { get; set; } = [];
    public string Notes { get; set; } = "";
}