using AIRpgAgents.GameEngine.Enums;
using System.ComponentModel;
using System.Text;

namespace AIRpgAgents.GameEngine.World;

public class WorldState
{
    public string id => Name ?? Guid.NewGuid().ToString();
    [Description("The creative  but unique identifier for the world")]
    public string? Name { get; set; }
    [Description("A brief 2-3 sentance description of the current fantasy world.")]
    public string? BriefDescription { get; set; }
    [Description("A fully detailed narrative description of the current fantasy world.")]
    public string? FullDescription { get; set; }
    [Description("The prominant or important Non-player characters in the world")]
    public List<NPC>? NPCs { get; set; }
    [Description("The potential quests in this fantasy world")]
    public List<Quest> Quests { get; set; } = [];
    [Description("The important or notable landmarks and locations in the fantasy world")]
    public List<Location> Locations { get; set; } = [];
    [Description("The important or notable events that have occured in the fantasy world")]
    public List<WorldEvent> Events { get; set; } = [];
    [Description("Any other noteworthy information on the state of the world")]
    public string? OtherNotes { get; set; }
    
    public string ToMarkdown()
    {
        var markdown = new StringBuilder();

        // World title and descriptions
        markdown.AppendLine($"# {Name ?? "Unnamed World"}");

        if (!string.IsNullOrEmpty(BriefDescription))
        {
            markdown.AppendLine();
            markdown.AppendLine(BriefDescription);
        }

        if (!string.IsNullOrEmpty(FullDescription))
        {
            markdown.AppendLine();
            markdown.AppendLine("## Full Description");
            markdown.AppendLine(FullDescription);
        }

        // Locations section
        if (Locations?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Locations");

            foreach (var location in Locations)
            {
                markdown.AppendLine($"* **{location.Name}**");

                if (!string.IsNullOrEmpty(location.Description))
                {
                    markdown.AppendLine($"  * {location.Description}");
                }

                if (location.Tags?.Count > 0)
                {
                    markdown.AppendLine($"  * Tags: {string.Join(", ", location.Tags)}");
                }

                if (location.IsDiscovered)
                {
                    markdown.AppendLine("  * Status: Discovered");
                }
                else
                {
                    markdown.AppendLine("  * Status: Undiscovered");
                }
            }
        }

        // NPCs section
        if (NPCs?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Non-Player Characters");

            foreach (var npc in NPCs)
            {
                markdown.AppendLine($"* **{npc.Name}**");

                if (!string.IsNullOrEmpty(npc.Description))
                {
                    markdown.AppendLine($"  * {npc.Description}");
                }

                if (!string.IsNullOrEmpty(npc.CurrentLocationId))
                {
                    markdown.AppendLine($"  * Current Location: {npc.CurrentLocationId}");
                }

                markdown.AppendLine($"  * State: {npc.State}");
                markdown.AppendLine($"  * Disposition: {npc.Disposition}");
            }
        }

        // Quests section
        if (Quests?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Quests");

            foreach (var quest in Quests)
            {
                markdown.AppendLine($"* **{quest.Name}**");

                if (!string.IsNullOrEmpty(quest.Description))
                {
                    markdown.AppendLine($"  * {quest.Description}");
                }

                if (!string.IsNullOrEmpty(quest.QuestGiver))
                {
                    markdown.AppendLine($"  * Quest Giver: {quest.QuestGiver}");
                }

                if (!string.IsNullOrEmpty(quest.LocationName))
                {
                    markdown.AppendLine($"  * Location: {quest.LocationName}");
                }

                if (quest.PotentialRewardItems?.Count > 0)
                {
                    markdown.AppendLine($"  * Potential Rewards:");
                    markdown.AppendLine($"    * Items: {string.Join(", ", quest.PotentialRewardItems)}");
                }

                if (quest.PotentialRewardCoinsInGold > 0)
                {
                    markdown.AppendLine($"    * Gold: {quest.PotentialRewardCoinsInGold}");
                }
            }
        }

        // Events section
        if (Events?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## World Events");

            foreach (var worldEvent in Events)
            {
                markdown.AppendLine($"* **{worldEvent.Name}**");

                if (!string.IsNullOrEmpty(worldEvent.Description))
                {
                    markdown.AppendLine($"  * {worldEvent.Description}");
                }

                if (!string.IsNullOrEmpty(worldEvent.LocationId))
                {
                    markdown.AppendLine($"  * Location: {worldEvent.LocationId}");
                }

                if (worldEvent.RelatedEntityIds?.Count > 0)
                {
                    markdown.AppendLine($"  * Related Entities: {string.Join(", ", worldEvent.RelatedEntityIds)}");
                }

                markdown.AppendLine($"  * Status: {(worldEvent.IsResolved ? "Resolved" : "Ongoing")}");
                markdown.AppendLine($"  * Start Time: {worldEvent.StartTime}");

                if (worldEvent.Tags?.Count > 0)
                {
                    markdown.AppendLine($"  * Tags: {string.Join(", ", worldEvent.Tags)}");
                }

                if (!string.IsNullOrEmpty(worldEvent.Notes))
                {
                    markdown.AppendLine($"  * Notes: {worldEvent.Notes}");
                }
            }
        }

        // Other notes
        if (!string.IsNullOrEmpty(OtherNotes))
        {
            markdown.AppendLine();
            markdown.AppendLine("## Other Notes");
            markdown.AppendLine(OtherNotes);
        }

        return markdown.ToString();
    }
}
