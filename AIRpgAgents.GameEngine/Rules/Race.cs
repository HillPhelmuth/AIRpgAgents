using System.Text.Json;
using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Extensions;
using Newtonsoft.Json.Converters;

namespace AIRpgAgents.GameEngine.Rules;

public class Race
{
    public RaceType Type { get; set; }
    public string Description { get; set; }
    public Dictionary<string, int> AttributeModifiers { get; set; } = new();
    public List<RacialTrait> Traits { get; set; } = [];
    public int BaseSpeed { get; set; } = 30;
    public List<string> Languages { get; set; } = [];
    public string Size { get; set; } = "Medium";
    public override string ToString()
    {
        return $"{Type.ToString()}";
    }
}
    
public class RacialTrait
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum RaceType
{
    None,
    Human,
    Duskborn,
    Ironforged,
    Wildkin,
    Emberfolk,
    Stoneborn
}

public static class CharacterRaces
{
    private static readonly Dictionary<RaceType, Race> _races = new();
    private static bool _isInitialized = false;
    
    private static void EnsureInitialized()
    {
        if (_isInitialized) return;
        
        try
        {
            // Load races from JSON file
            var races = FileHelper.ExtractFromAssembly<List<Race>>("Races.json");
            if (races?.Any() == true)
            {
                foreach (var race in races)
                {
                    _races[race.Type] = race;
                }
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading races from JSON: {ex.Message}");
            // Fall back to default races if needed
        }

        _isInitialized = true;
    }

    public static Race GetDetails(this RaceType raceType)
    {
        EnsureInitialized();
        
        if (_races.TryGetValue(raceType, out var race))
        {
            return race;
        }
        
        // Return a default race if not found
        return new Race
        {
            Type = raceType,
            Description = $"Default description for {raceType}",
            BaseSpeed = 30,
            Languages = ["Common"]
        };
    }
}