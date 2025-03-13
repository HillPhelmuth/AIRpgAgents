using AIRpgAgents.GameEngine.Enums;
using SkPluginComponents.Models;

namespace AIRpgAgents.GameEngine.Rules;

/// <summary>
/// Extension methods for enum types used in the RPG system.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the number of sides for a specific die type.
    /// </summary>
    public static int GetSides(this DieType dieType) => dieType switch
    {
        DieType.D4 => 4,
        DieType.D6 => 6,
        DieType.D8 => 8,
        DieType.D10 => 10,
        DieType.D20 => 20,
        _ => throw new ArgumentOutOfRangeException(nameof(dieType), "Unsupported die type")
    };
    //public static int RollDie(this DieType dieType, int modifier = 0)
    //{
    //    var random = new Random();
    //    var result = dieType switch
    //    {
    //        DieType.D4 => random.Next(1, 5),
    //        DieType.D6 => random.Next(1, 7),
    //        DieType.D8 => random.Next(1, 9),
    //        DieType.D10 => random.Next(1, 11),
    //        DieType.D12 => random.Next(1, 13),
    //        DieType.D20 => random.Next(1, 21),
    //        DieType.D100 => random.Next(1, 101),
    //        _ => throw new ArgumentOutOfRangeException(nameof(dieType), "Unsupported die type")
    //    };
    //    return result + modifier;
    //}

    /// <summary>
    /// Gets the primary attribute associated with an attack type.
    /// </summary>
    public static string GetPrimaryAttribute(this CombatSystem.AttackType attackType) => attackType switch
    {
        CombatSystem.AttackType.Melee => "Might",
        CombatSystem.AttackType.Ranged => "Agility",
        CombatSystem.AttackType.Spell => "Will",
        CombatSystem.AttackType.Special => "Special",
        _ => throw new ArgumentOutOfRangeException(nameof(attackType), "Unsupported attack type")
    };
        
    /// <summary>
    /// Gets the primary skill associated with an attack type.
    /// </summary>
    public static string GetPrimarySkill(this CombatSystem.AttackType attackType) => attackType switch
    {
        CombatSystem.AttackType.Melee => "MeleeCombat",
        CombatSystem.AttackType.Ranged => "RangedCombat",
        CombatSystem.AttackType.Spell => "Spellcasting",
        CombatSystem.AttackType.Special => "Special",
        _ => throw new ArgumentOutOfRangeException(nameof(attackType), "Unsupported attack type")
    };
        
    /// <summary>
    /// Gets the standard resistance multiplier for a damage type.
    /// </summary>
    public static double GetResistanceMultiplier(this CombatSystem.DamageType damageType, string resistance) => 
        resistance.Equals($"Resist{damageType}", StringComparison.OrdinalIgnoreCase) ? 0.5 : 1.0;
        
    /// <summary>
    /// Gets the standard vulnerability multiplier for a damage type.
    /// </summary>
    public static double GetVulnerabilityMultiplier(this CombatSystem.DamageType damageType, string vulnerability) =>
        vulnerability.Equals($"Vulnerable{damageType}", StringComparison.OrdinalIgnoreCase) ? 2.0 : 1.0;
}