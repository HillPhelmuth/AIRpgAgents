using System.Text;

namespace AIRpgAgents.GameEngine.WorldState;

public class Location
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    //public Dictionary<string, string> Exits { get; set; } = new Dictionary<string, string>(); // Direction -> LocationId
    public List<string> Tags { get; set; } = [];
    public bool IsDiscovered { get; set; } = false;

    public string AsMarkdown()
    {
        // Create a markdown representation of the location
        var sb = new StringBuilder();
        sb.AppendLine($"## {Name}");
        sb.AppendLine(Description);
        sb.AppendLine("### Exits");
        //foreach (var exit in Exits)
        //{
        //    sb.AppendLine($"- {exit.Key}: {exit.Value}");
        //}
        sb.AppendLine("### Tags");
        foreach (var tag in Tags)
        {
            sb.AppendLine($"- {tag}");
        }

        sb.AppendLine($"Discovered by players: {IsDiscovered}");
        return sb.ToString();
    }
}