using AIRpgAgents.GameEngine.Enums;
using System.ComponentModel;

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
}