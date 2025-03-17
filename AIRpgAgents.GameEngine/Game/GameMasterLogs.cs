using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIRpgAgents.GameEngine.Game
{
    public class GameMasterLogs
    {
    }
    public partial class GmResponse
    {
        [JsonPropertyName("scene_planning")]
        public ScenePlanning ScenePlanning { get; set; }
    }

    public partial class ScenePlanning
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("narrative")]
        public string Narrative { get; set; }

        [JsonPropertyName("gm_response")]
        public string GmResponse { get; set; }
    }
}
