using System;
using System.Collections.Generic;
using System.ComponentModel;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine;
using AIRpgAgents.GameEngine.Enums;
using AIRpgAgents.GameEngine.Rules;
using Microsoft.SemanticKernel;
using SkPluginComponents.Models;
using static AIRpgAgents.GameEngine.Rules.CombatSystem;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

/// <summary>
/// A KernelPlugin that provides functionality for resolving combat mechanics as Tool calls for LLMs.
/// </summary>
public class CombatSystemPlugin
{
    private readonly Random _random = new Random();
    private readonly RollDiceService _diceService;
    #region Initiative

    [KernelFunction, Description("Calculates initiative for combat based on a d20 roll plus ability modifiers.")]
    [return: Description("The initiative score that determines turn order in combat.")]
    public async Task<int> CalculateInitiative(
        [Description("Agility modifier of the character.")] int agilityModifier,
        [Description("Wits modifier of the character.")] int witsModifier,
        [Description("If true, uses a previously rolled d20 value instead of rolling a new one.")] bool useProvidedRoll = false,
        [Description("A previously rolled d20 value (1-20) to use if useProvidedRoll is true.")] int providedRoll = 0)
    {
        int d20Roll = useProvidedRoll ? providedRoll : DieType.D20.RollDie();
        return d20Roll + agilityModifier + witsModifier;
    }

    [KernelFunction, Description("Resolves initiative ties between characters.")]
    [return: Description("The ID of the character who acts first. Returns empty if they act simultaneously.")]
    public string ResolveInitiativeTie(
        [Description("First character's ID.")] string character1Id,
        [Description("First character's wits attribute.")] int character1Wits,
        [Description("First character's agility attribute.")] int character1Agility,
        [Description("Second character's ID.")] string character2Id,
        [Description("Second character's wits attribute.")] int character2Wits,
        [Description("Second character's agility attribute.")] int character2Agility)
    {
        if (character1Wits > character2Wits)
            return character1Id;
        if (character2Wits > character1Wits)
            return character2Id;
            
        // If Wits are tied, check Agility
        if (character1Agility > character2Agility)
            return character1Id;
        if (character2Agility > character1Agility)
            return character2Id;
            
        // If all are equal, they act simultaneously
        return string.Empty;
    }
        
    #endregion
        
    #region Attack Resolution
        
    [KernelFunction, Description("Resolves whether an attack hits or misses.")]
    [return: Description("True if the attack hits, false if it misses.")]
    public bool ResolveAttack(
        [Description("The total attack roll (d20 + modifiers).")] int attackRoll,
        [Description("The target's Armor Class (AC).")] int targetAC,
        [Description("The natural d20 roll value (1-20) to check for critical hits/misses.")] int naturalRoll)
    {
        // Critical hit always succeeds
        if (naturalRoll == 20)
            return true;
                
        // Critical miss always fails
        if (naturalRoll == 1)
            return false;
                
        // Normal hit check
        return attackRoll >= targetAC;
    }
        
    [KernelFunction, Description("Calculates the attack roll based on attack type.")]
    [return: Description("The total attack roll value and natural roll as an object.")]
    public AttackRollResult CalculateAttackRoll(
        [Description("Type of attack being made (Melee, Ranged, Spell, Special).")] AttackType attackType,
        [Description("Character's attribute modifier for the attack (based on attack type).")] int attributeModifier,
        [Description("Character's relevant skill rank for the attack type.")] int skillRank)
    {
        int naturalRoll = DieType.D20.RollDie();
        return new AttackRollResult
        {
            TotalRoll = naturalRoll + attributeModifier + skillRank,
            NaturalRoll = naturalRoll,
            IsCriticalHit = naturalRoll == 20,
            IsCriticalMiss = naturalRoll == 1
        };
    }
        
    #endregion
        
    #region Damage Resolution
        
    [KernelFunction, Description("Calculates damage for a successful attack.")]
    [return: Description("The amount of damage dealt.")]
    public int CalculateDamage(
        [Description("The type of die used for damage.")] DieType damageDieType,
        [Description("Number of dice to roll for damage.")] int numberOfDice,
        [Description("Attribute modifier to add to damage.")] int attributeModifier,
        [Description("Whether the attack was a critical hit (doubles damage dice).")] bool isCritical = false,
        [Description("Type of damage being dealt.")] DamageType damageType = DamageType.Slashing)
    {
        int totalDamage = 0;
        int diceToRoll = isCritical ? numberOfDice * 2 : numberOfDice;
            
        for (int i = 0; i < diceToRoll; i++)
        {
            totalDamage += damageDieType.RollDie();
        }
            
        return totalDamage + attributeModifier;
    }
        
    [KernelFunction, Description("Applies damage resistance or vulnerability for a target.")]
    [return: Description("The modified damage after applying resistance/vulnerability.")]
    public int ApplyDamageModifiers(
        [Description("Base damage amount.")] int damage,
        [Description("Type of damage being dealt.")] DamageType damageType,
        [Description("List of damage types the target is resistant to (takes half damage).")] List<DamageType> resistances = null,
        [Description("List of damage types the target is vulnerable to (takes double damage).")] List<DamageType> vulnerabilities = null)
    {
        double modifiedDamage = damage;
            
        // Apply resistance (half damage)
        if (resistances != null && resistances.Contains(damageType))
        {
            modifiedDamage *= 0.5;
        }
            
        // Apply vulnerability (double damage)
        if (vulnerabilities != null && vulnerabilities.Contains(damageType))
        {
            modifiedDamage *= 2;
        }
            
        return (int)Math.Round(modifiedDamage);
    }
        
    #endregion
        
    #region Defense
        
    [KernelFunction, Description("Calculates a character's Armor Class (AC).")]
    [return: Description("The character's total AC value.")]
    public int CalculateArmorClass(
        [Description("Character's base AC (typically 10).")] int baseAC,
        [Description("Character's Agility modifier.")] int agilityModifier,
        [Description("Armor bonus to AC (0 for no armor).")] int armorBonus = 0,
        [Description("Shield bonus to AC (0 for no shield).")] int shieldBonus = 0,
        [Description("Any other AC bonuses from magic, abilities, etc.")] int miscBonus = 0)
    {
        return baseAC + agilityModifier + armorBonus + shieldBonus + miscBonus;
    }
        
    [KernelFunction, Description("Resolves a dodge action, calculating the AC bonus until next turn.")]
    [return: Description("The bonus to AC granted by dodging.")]
    public int ResolveDodgeAction()
    {
        // Dodge action grants +2 to AC until the start of the next turn
        return 2;
    }
        
    [KernelFunction, Description("Attempts to parry an incoming attack as a reaction.")]
    [return: Description("True if the parry succeeds, false if it fails.")]
    public bool ResolveParry(
        [Description("The incoming attack roll.")] int attackRoll,
        [Description("Character's Agility modifier.")] int agilityModifier)
    {
        int d20Roll = DieType.D20.RollDie();
        return (d20Roll + agilityModifier) >= attackRoll;
    }
        
    #endregion
        
    #region Hit Points and Death
        
    [KernelFunction, Description("Calculates a character's base hit points at level 1.")]
    [return: Description("The character's maximum hit points.")]
    public int CalculateBaseHitPoints(
        [Description("Character's Vitality modifier.")] int vitalityModifier)
    {
        return 10 + vitalityModifier;
    }
        
    [KernelFunction, Description("Makes a death saving throw for a character at 0 HP.")]
    [return: Description("Result object with success/failure status and whether it was a critical roll.")]
    public DeathSaveResult MakeDeathSavingThrow()
    {
        int d20Roll = DieType.D20.RollDie();
            
        return new DeathSaveResult
        {
            RollValue = d20Roll,
            IsSuccess = d20Roll >= 10,
            IsCriticalSuccess = d20Roll == 20,
            IsCriticalFailure = d20Roll == 1
        };
    }
        
    [KernelFunction, Description("Checks if a character dies instantly from massive damage.")]
    [return: Description("True if the character dies instantly, false otherwise.")]
    public bool CheckInstantDeath(
        [Description("Amount of damage taken.")] int damageTaken,
        [Description("Character's maximum hit points.")] int maxHitPoints)
    {
        return damageTaken >= (maxHitPoints * 2);
    }
        
    #endregion
        
       
}
    
/// <summary>
/// Result of a death saving throw
/// </summary>
public class DeathSaveResult
{
    public int RollValue { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsCriticalSuccess { get; set; } // Natural 20 recovers 1 HP
    public bool IsCriticalFailure { get; set; } // Natural 1 counts as two failures
}
    
/// <summary>
/// Result of an attack roll containing both the total and natural roll values
/// </summary>
public class AttackRollResult
{
    public int TotalRoll { get; set; }
    public int NaturalRoll { get; set; }
    public bool IsCriticalHit { get; set; }
    public bool IsCriticalMiss { get; set; }
}