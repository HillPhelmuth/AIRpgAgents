using System.Text.Json.Serialization;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;
using CosmosIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace AIRpgAgents.GameEngine.WorldState;

public class RpgWorldState
{
    [CosmosName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<Location> Locations { get; set; } = [];
    public List<NPC> NPCs { get; set; } = [];
    [CosmosIgnore]
    [JsonIgnore]
    public List<GameObject> Objects { get; set; } = [];
    public List<WorldEvent> Events { get; set; } = [];
    public List<Quest> Quests { get; set; } = [];
    //public Dictionary<string, object> GlobalVariables { get; set; } = new Dictionary<string, object>();
        
    // Game time tracking
    //public GameTime CurrentTime { get; set; } = new GameTime();
}