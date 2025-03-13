using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.WorldState;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;

namespace AIRpgAgents.Core.Models;

public class Player
{
    [CosmosName("id")]
    public string Id => Name ?? "";
    public string? Name { get; set; }
    public List<CharacterSheet> Characters { get; set; } = [];
    public RpgWorldState ActiveCampaignState { get; set; } = new();
    public string? Email { get; set; }
}