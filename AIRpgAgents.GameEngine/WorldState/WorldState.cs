using AIRpgAgents.GameEngine.Enums;
using System.ComponentModel;

namespace AIRpgAgents.GameEngine.WorldState;

public class WorldState
{
    [Description("A brief 2-3 sentance description of the current fantasy world.")]
    public string BriefDescription { get; set; }
    [Description("A fully detailed narrative description of the current fantasy world.")]
    public string Description { get; set; }
    [Description("The prominant or important Non-player characters in the world")]
    public List<NPC>? NPCs { get; set; }
    [Description("The potential quests in this fantasy world")]
    public List<Quest> Quests { get; set; } = [];
    [Description("The important or notable landmarks and locations in the fantasy world")]
    public List<Location> Locations { get; set; } = [];
}

public class NpcStart : NPC
{
    //public string? Name { get; set; }
    //public string? Description { get; set; }
    //public string? CurrentLocationId { get; set; }
    //public NPCState State { get; set; } = NPCState.Idle;
}