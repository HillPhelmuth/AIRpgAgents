using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine;
using AIRpgAgents.GameEngine.Enums;
using AIRpgAgents.GameEngine.Monsters;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using static AIRpgAgents.GameEngine.Rules.CombatSystem;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

/// <summary>
/// A KernelPlugin that provides functionality for resolving combat mechanics as Tool calls for LLMs.
/// </summary>
public class CombatSystemPlugin(CombatService combatService)
{
    private string _encounterId = string.Empty;

    /// <summary>
    /// Starts a new combat encounter between a party and monsters
    /// </summary>
    [KernelFunction, Description("Starts a new combat encounter between the player party and specified monsters")]
    public async Task<string> StartCombat(Kernel kernel,
        [Description("Description of the combat environment")] string environmentDescription)
    {
        var combatEncounter = kernel.Services.GetRequiredService<CombatEncounter>();
        _encounterId = combatEncounter.Id;
        try
        {
            if (combatEncounter.MonsterEncounter.Monsters.Count == 0)
            {
                return "Error: No monsters specified for the encounter.";
            }
            // Start the combat
            var encounter = await combatService.StartCombat(combatEncounter, environmentDescription);
            
            // Return combat narrative
            return $"Combat started! Encounter ID: {encounter.Id}\n\n{await combatService.GetCombatNarrative(encounter)}";
        }
        catch (Exception ex)
        {
            return $"Error starting combat: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Gets the current state of a combat encounter
    /// </summary>
    [KernelFunction, Description("Gets the current state of a combat encounter")]
    public async Task<string> GetCombatState(
        [Description("The ID of the combat encounter")] string encounterId)
    {
        var encounter = combatService.GetEncounter(encounterId);
        _encounterId = encounterId;
        if (encounter == null)
        {
            return "Error: Combat encounter not found.";
        }
        
        return await combatService.GetCombatNarrative(encounter);
    }
    
    /// <summary>
    /// Performs an attack action in combat from a player combatant to a monster
    /// </summary>
    [KernelFunction, Description("Performs an attack action from a player combatant to a monster using their IDs")]
    public async Task<string> PerformPlayerAttack(
        [Description("The ID of the combat encounter")] string encounterId,
        [Description("The ID of the attacker (player)")] string attackerId,
        [Description("The ID of the target (monster)")] string targetId,
        [Description("Name of the specific attack to use (if applicable)")] string? attackName = null)
    {
        var encounter = combatService.GetEncounter(encounterId);
        
        //if (encounter == null)
        //{
        //    return "Error: Combat encounter not found.";
        //}
        //encounter.ActiveCombatant = encounter.InitiativeOrder.FirstOrDefault(x => x.Id == attackerId);
        //if (encounter.ActiveCombatant == null)
        //{
        //    return "Error: It's not this combatant's turn to attack.";
        //}

        try
        {
            var result = await combatService.ProcessPlayerAttackById(encounter, attackerId, targetId, attackName);

            string response = "";
            if (result.IsHit)
            {
                response = result.IsCritical ?
                    $"{result.AttackerName} landed a CRITICAL HIT on {result.TargetName} with {result.AttackName ?? "an attack"} for {result.DamageDealt} damage!" :
                    $"{result.AttackerName} hit {result.TargetName} with {result.AttackName ?? "an attack"} for {result.DamageDealt} damage.";
                response += $" {result.TargetName} has {result.RemainingHP}/{result.MaxHP} HP remaining.";
                if (result.RemainingHP <= 0)
                {
                    response += $" {result.TargetName} has been defeated!";
                }
            }
            else
            {
                response = $"{result.AttackerName} attempted to hit {result.TargetName} with {result.AttackName ?? "an attack"} but missed!";
            }
            return response;
        }
        catch (Exception ex)
        {
            return $"Error processing attack: {ex.Message}";
        }
    }

    /// <summary>
    /// Performs an attack action in combat from a monster combatant to a player
    /// </summary>
    [KernelFunction, Description("Performs an attack action from a monster combatant to a player using their IDs")]
    public async Task<string> PerformMonsterAttack(
        [Description("The ID of the combat encounter")] string encounterId,
        [Description("The ID of the attacker (monster)")] string attackerId,
        [Description("The ID of the target (player)")] string targetId,
        [Description("Name of the specific attack to use (if applicable)")] string? attackName = null)
    {
        var encounter = combatService.GetEncounter(encounterId);
        //if (encounter == null)
        //{
        //    return "Error: Combat encounter not found.";
        //}
        //encounter.ActiveCombatant = encounter.InitiativeOrder.FirstOrDefault(x => x.Id == attackerId);
        //if (encounter.ActiveCombatant == null || encounter.ActiveCombatant.Id != attackerId)
        //{
        //    return "Error: It's not this combatant's turn to attack.";
        //}

        try
        {
            var result = await combatService.ProcessMonsterAttackById(encounter, attackerId, targetId, attackName);

            string response = "";
            if (result.IsHit)
            {
                response = result.IsCritical ?
                    $"{result.AttackerName} landed a CRITICAL HIT on {result.TargetName} with {result.AttackName ?? "an attack"} for {result.DamageDealt} damage!" :
                    $"{result.AttackerName} hit {result.TargetName} with {result.AttackName ?? "an attack"} for {result.DamageDealt} damage.";
                response += $" {result.TargetName} has {result.RemainingHP}/{result.MaxHP} HP remaining.";
                if (result.RemainingHP <= 0)
                {
                    response += $" {result.TargetName} has been defeated!";
                }
            }
            else
            {
                response = $"{result.AttackerName} attempted to hit {result.TargetName} with {result.AttackName ?? "an attack"} but missed!";
            }
            return response;
        }
        catch (Exception ex)
        {
            return $"Error processing attack: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Moves to the next turn in combat
    /// </summary>
    [KernelFunction, Description("Ends the current turn and advances to the next combatant in the initiative order")]
    public async Task<string> NextTurn(
        [Description("The ID of the combat encounter")] string encounterId)
    {
        var encounter = combatService.GetEncounter(encounterId);
        if (encounter == null)
        {
            return "Error: Combat encounter not found.";
        }
        
        var nextCombatant = await combatService.NextTurn(encounter);
        
        if (nextCombatant == null)
        {
            return "Combat round has ended. Starting a new round.";
        }
        
        return $"It's now {nextCombatant.Name}'s turn to act.";
    }
    
    /// <summary>
    /// Gets available actions for the current combatant
    /// </summary>
    [KernelFunction, Description("Gets the available actions for the current combatant")]
    public async Task<string> GetAvailableActions(
        [Description("The ID of the combat encounter")] string encounterId)
    {
        var encounter = combatService.GetEncounter(encounterId);
        var isPlayerCharacter = encounter.ActiveCombatant.IsPlayerCharacter;
        if (encounter == null)
        {
            return "Error: Combat encounter not found.";
        }
        
        if (encounter.ActiveCombatant == null)
        {
            return "Error: No active turn in progress.";
        }
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Available actions for {encounter.ActiveCombatant.Name}:");
        
        // Basic actions available to all
        var actionIndex = 1;
        sb.AppendLine($"{actionIndex++}. Attack - Attack an enemy");
        sb.AppendLine($"{actionIndex++}. Dodge - Take the dodge action (disadvantage to hit you)");
        sb.AppendLine($"{actionIndex++}. Disengage - Move away without provoking opportunity attacks");
        sb.AppendLine($"{actionIndex++}. Help - Help an ally (give advantage on their next action)");
        if (isPlayerCharacter)
        {
            var player = encounter.PlayerParty.PartyMembers.FirstOrDefault(x => x.CharacterName == encounter.ActiveCombatant.Name);
            if (player?.CharacterSheet.Class.HasSpellcasting == true)
            {
                foreach (var spell in player?.CharacterSheet?.Spellcasting?.Spells ?? [])
                {
                    sb.AppendLine($"{actionIndex++}. Cast {spell.Name} - {spell.Description}");
                }
                
            }
        }
        sb.AppendLine($"{actionIndex}. Something else (describe in detail)");
        
        // Add specific attacks for monsters
       
        if (!isPlayerCharacter)
        {
            var monster = encounter.MonsterEncounter.Monsters[encounter.ActiveCombatant.Index];
            if (monster.SpecialAttacks.Count > 0)
            {
                sb.AppendLine("\nSpecial Attacks:");
                foreach (var attack in monster.SpecialAttacks)
                {
                    sb.AppendLine($"- {attack.Name}: {attack.DamageDie} + {attack.DamageBonus} {attack.DamageType} damage (Range: {attack.Range}ft)");
                    if (!string.IsNullOrEmpty(attack.Description))
                    {
                        sb.AppendLine($"  {attack.Description}");
                    }
                }
            }
        }
        
        // Add available targets
        sb.AppendLine("\nAvailable targets:");
        
        if (isPlayerCharacter)
        {
            // Player can target monsters
            foreach (var monster in encounter.MonsterEncounter.Monsters.Where(m => m.CurrentHP > 0))
            {
                sb.AppendLine($"- {monster.Name} (ID: {monster.Id}): {monster.CurrentHP}/{monster.MaxHP} HP");
            }
        }
        else
        {
            // Monster can target players
            foreach (var player in encounter.PlayerParty.PartyMembers.Where(p => p.CurrentHitPoints > 0))
            {
                sb.AppendLine($"- {player.CharacterName} (ID: {player.Id}): {player.CurrentHitPoints}/{player.MaxHitPoints} HP");
            }
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Ends a combat encounter
    /// </summary>
    [KernelFunction, Description("Ends a combat encounter with the specified result")]
    public async Task<string> EndCombat(
        [Description("The ID of the combat encounter")] string encounterId,
        [Description("The result of the combat (PlayerVictory, PlayerDefeat, Fled, Resolved)")] CombatStatus result)
    {
        var encounter = combatService.GetEncounter(encounterId);
        if (encounter == null)
        {
            return "Error: Combat encounter not found.";
        }

        if (result is CombatStatus.NotStarted or CombatStatus.InProgress)
        {
            return "Error: Invalid combat result. Must be one of: PlayerVictory, PlayerDefeat, Fled, Resolved";
        }
        
        await combatService.EndCombat(encounter, result);
        
        return $"Combat has ended. Result: {result}";
    }
    
    /// <summary>
    /// Gets the combat log
    /// </summary>
    [KernelFunction, Description("Gets the log of combat events")]
    public async Task<string> GetCombatLog(
        [Description("The ID of the combat encounter")] string encounterId,
        [Description("The maximum number of log entries to return (default: 10)")] int maxEntries = 10)
    {
        var encounter = combatService.GetEncounter(encounterId);
        if (encounter == null)
        {
            return "Error: Combat encounter not found.";
        }
        
        var log = encounter.CombatLog.TakeLast(maxEntries).ToList();
        
        return string.Join("\n", log.Select((entry, i) => $"{i+1}. {entry}"));
    }
    
    /// <summary>
    /// Casts a spell in combat
    /// </summary>
    [KernelFunction, Description("Casts a spell from a player character to a target using their IDs")]
    public async Task<string> PlayerCastSpell(
        [Description("The ID of the combat encounter")] string encounterId,
        [Description("The ID of the spellcaster (player)")] string casterId,
        [Description("The name of the spell to cast")] string spellName,
        [Description("The ID of the target (optional, some spells don't require targets)")] string? targetId = null)
    {
        try
        {
            var encounter = combatService.GetEncounter(encounterId);
            SpellCastResult result = await combatService.ProcessPlayerCastSpell(encounter, casterId, spellName, targetId);

            if (!result.SuccessfulCast)
            {
                return result.Effect!;
            }

            string response;
            if (!string.IsNullOrEmpty(result.TargetName))
            {
                if (result.DamageDealt > 0)
                {
                    response = $"{result.CasterName} casts {result.SpellName} on {result.TargetName}";
                    response += $", dealing {result.DamageDealt} {result.DamageType} damage!";
                    
                    response += $" {result.TargetName} has {result.RemainingHP}/{result.MaxHP} HP remaining.";
                    if (result.RemainingHP <= 0)
                    {
                        response += $" {result.TargetName} has been defeated!";
                    }
                }
                else if (result.HealingDone > 0)
                {
                    response = $"{result.CasterName} casts {result.SpellName} on {result.TargetName}";
                    response += $", healing for {result.HealingDone} HP.";
                    response += $" {result.TargetName} now has {result.RemainingHP}/{result.MaxHP} HP.";
                }
                else
                {
                    response = $"{result.CasterName} casts {result.SpellName} on {result.TargetName}. {result.Effect}";
                }
            }
            else
            {
                response = $"{result.CasterName} casts {result.SpellName}. {result.Effect}";
            }
            
            response += $" ({result.ManaUsed} mana used, {result.RemainingMana}/{result.MaxMana} remaining)";
            return response;
        }
        catch (Exception ex)
        {
            return $"Error casting spell: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Uses a skill in combat
    /// </summary>
    [KernelFunction, Description("Uses a skill from a player character, optionally targeting another combatant")]
    public async Task<string> PlayerUseSkill(
        [Description("The ID of the combat encounter")] string encounterId,
        [Description("The ID of the skill user (player)")] string userId,
        [Description("The name of the skill to use")] string skillName, [Description("Your best judgement about how difficult the specific use of the skill is on a scale of 1-20")] int difficulty = 12,
        [Description("The ID of the target (optional, some skills don't require targets)")] string? targetId = null)
    {
        try
        {
            var encounter = combatService.GetEncounter(encounterId);
            SkillUseResult result = await combatService.ProcessPlayerUseSkill(encounter, userId, skillName, targetId);

            string response;
            if (!string.IsNullOrEmpty(result.TargetName))
            {
                response = $"{result.UserName} uses {result.SkillName} on {result.TargetName}. ";
                if (result.Success)
                {
                    response += result.Effect;
                }
                else
                {
                    response += $"The attempt fails (rolled {result.SkillCheckRoll}, needed higher).";
                }
            }
            else
            {
                response = $"{result.UserName} uses {result.SkillName}. ";
                if (result.Success)
                {
                    response += result.Effect;
                }
                else
                {
                    response += $"The attempt fails (rolled {result.SkillCheckRoll}, needed higher).";
                }
            }
            
            return response;
        }
        catch (Exception ex)
        {
            return $"Error using skill: {ex.Message}";
        }
    }
}