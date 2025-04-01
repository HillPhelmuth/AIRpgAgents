using AIRpgAgents.GameEngine.Game;
using AIRpgAgents.GameEngine.Monsters;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.World;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;

namespace AIRpgAgents.Core.Models;

public class RpgCampaign
{
    [CosmosName("id")]
    public string Id => Name ?? Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }
    public RpgParty Party { get; set; } = new();
    public WorldState WorldState { get; set; } = new();
    public NarrativeState NarrativeState { get; set; } = new();
    public List<CombatEncounter> CombatEncounters { get; set; } = [];
    public MonsterEncounter? PendingMonsterEncounter { get; set; }

}