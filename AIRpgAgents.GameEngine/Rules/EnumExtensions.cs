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
    public static double GetResistanceMultiplier(this DamageType damageType, string resistance) => 
        resistance.Equals($"Resist{damageType}", StringComparison.OrdinalIgnoreCase) ? 0.5 : 1.0;
        
    /// <summary>
    /// Gets the standard vulnerability multiplier for a damage type.
    /// </summary>
    public static double GetVulnerabilityMultiplier(this DamageType damageType, string vulnerability) =>
        vulnerability.Equals($"Vulnerable{damageType}", StringComparison.OrdinalIgnoreCase) ? 2.0 : 1.0;
}