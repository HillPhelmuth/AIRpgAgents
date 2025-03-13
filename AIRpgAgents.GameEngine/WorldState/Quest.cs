using System.ComponentModel;

namespace AIRpgAgents.GameEngine.WorldState;

public class Quest
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LocationName { get; set; }
    public string? QuestGiver { get; set; }
    public string? GetQuestGiverNpc(List<NPC> npcs)
    {
        return npcs.Find(npc => npc.Name == QuestGiver)?.Name;
    }
    [Description("The valuable and/or magical items that the player can receive as a reward for completing the quest.")]
    public List<string> PotentialRewardItems { get; set; } = [];
    public int PotentialRewardCoinsInGold { get; set; }

}