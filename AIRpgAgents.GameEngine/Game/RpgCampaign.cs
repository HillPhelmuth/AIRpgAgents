using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.World;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;

namespace AIRpgAgents.GameEngine.Game;

public class RpgCampaign
{
    [CosmosName("id")]
    public string Id => Name ?? Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }
    public RpgParty Party { get; set; } = new();
    public WorldState WorldState { get; set; } = new();
}