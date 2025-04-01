using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Extensions;

namespace AIRpgAgents.GameEngine.Rules;

public class Spells
{
    [JsonPropertyName("Traditions")]
    public List<SpellTradition> Traditions { get; set; }

    private static Spells? _spells;
    public static Spells GetSpells => _spells ??= FileHelper.ExtractFromAssembly<Spells>("Spells.json");

    
}