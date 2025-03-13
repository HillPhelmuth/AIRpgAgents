using AIRpgAgents.GameEngine.Extensions;
using AIRpgAgents.GameEngine.Rules;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class Skill
{
    public string Name { get; set; }
    public RpgAttribute AssociatedAttribute { get; set; }
    public int Rank { get; set; }
    public int Bonus { get; set; }
        
    public Skill(string name, RpgAttribute associatedAttribute)
    {
        Name = name;
        AssociatedAttribute = associatedAttribute;
        Rank = 0;
        Bonus = 0;
    }

    private static List<Skill>? _allSkills;
    public static List<Skill> AllSkills => _allSkills ??= FileHelper.ExtractFromAssembly<List<Skill>>("Skills.json");
}