using AIRpgAgents.GameEngine.Enums;
using SkPluginComponents.Models;

namespace AIRpgAgents.GameEngine.Rules;

public static class AttributeSystem
{
    // Minimum and maximum attribute values
    public const int MinAttributeValue = 3;
    public const int MaxAttributeValueAtCreation = 18;
    public const int AbsoluteMaxAttributeValue = 20;
    public const int BaseAttributeValue = 10; // Average human attribute value
        
    // Get all attributes as a list
    public static List<RpgAttribute> GetAllAttributes() => 
        Enum.GetValues(typeof(RpgAttribute))
            .Cast<RpgAttribute>()
            .ToList();
        
    // Calculate the modifier for an attribute score
    public static int CalculateModifier(int score)
    {
        return (int)Math.Floor((score - BaseAttributeValue) / 2.0);
    }
        
    // Calculate point cost during character creation
    public static int CalculatePointCost(int score)
    {
        if (score <= 8) return 0;
        if (score <= 13) return score - 8;
        if (score <= 15) return (score - 13) * 2 + 5;
        return -1; // Cannot buy scores above 15 during character creation
    }
    
    // Generate attributes using the 4d6 drop lowest method
    public static int RollAttribute()
    {
        List<int> rolls =
        [
            DieType.D6.RollDie(),
            DieType.D6.RollDie(),
            DieType.D6.RollDie(),
            DieType.D6.RollDie()
        ];
            
        rolls.Sort();
        rolls.RemoveAt(0); // Remove the lowest roll
            
        return rolls.Sum(); // Sum the remaining 3 dice
    }
        
    // Get the standard array of attribute values
    public static int[] GetStandardArray() => [15, 14, 13, 12, 10];
}