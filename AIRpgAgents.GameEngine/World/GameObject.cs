using System.Text;

namespace AIRpgAgents.GameEngine.World;

public class GameObject
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LocationId { get; set; } // Where the object is located
    public string? OwnerId { get; set; } // Who owns/carries this object (if applicable)
    public bool IsPortable { get; set; }
    public List<string> Tags { get; set; } = [];

    public string AsMarkdown()
    {
        // Create a markdown-formatted string for the object
        var sb = new StringBuilder();
        sb.AppendLine($"**{Name}**");
        sb.AppendLine($"*{Description}*");
        sb.AppendLine($"Location: {LocationId}");
        sb.AppendLine($"Is Portable: {IsPortable}");
        sb.AppendLine($"Tags: {string.Join(", ", Tags)}");
        return sb.ToString();
    }
}