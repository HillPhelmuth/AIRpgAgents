using System.Text.Json;
using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Extensions;
using Newtonsoft.Json.Converters;

namespace AIRpgAgents.GameEngine.Rules;


public class CharacterClass
{
    public ClassType Type { get; set; }
    public string Name => Type.ToString();
    public string Description { get; set; }
    public List<string> PrimaryAttributes { get; set; } = [];
    public List<ClassAbility> Abilities { get; set; } = [];
    public int HitDie { get; set; }
    public int? MpDie { get; set; }
    public bool HasSpellcasting { get; set; }
    public MagicTradition SpellcastingTradition { get; set; }
        
    //// Hit points at first level
    //public int StartingHP { get; set; }
        
    // Skills that are available to this class
    public List<string> ClassSkills { get; set; } = [];
    public override string ToString()
    {
        return $"{Name} - {Description}";
    }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum ClassType
{
    None,
    Cleric,
    Wizard,
    Warrior,
    Rogue,
    Paladin,
    WarMage
}
public class ClassAbility
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int LevelGained { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        
    // How many times per day/rest this ability can be used
    public int UsesPerDay { get; set; } = -1; // -1 means unlimited
    public bool UsesRemaining { get; set; } = true;
}
    
public static class CharacterClasses
{
    private static readonly List<CharacterClass> _classes = [];
    private static bool _isInitialized = false;
    
    public static List<CharacterClass> AllClasses 
    {
        get
        {
            EnsureInitialized();
            return _classes;
        }
    }

    private static void EnsureInitialized()
    {
        if (_isInitialized) return;
        
        try
        {
            var classes = FileHelper.ExtractFromAssembly<List<CharacterClass>>("CharacterClasses.json");
            if (classes?.Any() == true)
            {
                _classes.AddRange(classes);
            }
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading character classes from JSON: {ex.Message}");
            // Fall back to defaults if needed
        }

        
    }

    // Define getter properties for backward compatibility
    public static CharacterClass Cleric => GetClassByType(ClassType.Cleric);
    public static CharacterClass Wizard => GetClassByType(ClassType.Wizard);
    public static CharacterClass Warrior => GetClassByType(ClassType.Warrior);
    public static CharacterClass Rogue => GetClassByType(ClassType.Rogue);
    public static CharacterClass Paladin => GetClassByType(ClassType.Paladin);
    public static CharacterClass WarMage => GetClassByType(ClassType.WarMage);

    public static CharacterClass GetClassByType(ClassType classType)
    {
        EnsureInitialized();
        
        var characterClass = _classes.FirstOrDefault(c => c.Type == classType);
        
        if (characterClass == null)
        {
            // Return a default class if not found
            return new CharacterClass
            {
                Type = classType,
                Description = $"Default description for {classType}",
                HitDie = 8,
                
            };
        }
        
        return characterClass;
    }
}