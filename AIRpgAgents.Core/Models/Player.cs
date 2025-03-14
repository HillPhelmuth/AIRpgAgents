using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.World;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;

namespace AIRpgAgents.Core.Models;

public class Player
{
    [CosmosName("id")]
    public string Id => Name ?? "";
    public string? Name { get; set; }
    public List<CharacterState> Characters { get; set; } = [];
    public WorldState ActiveCampaignState { get; set; } = new();
    public string? Email { get; set; }
}