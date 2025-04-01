using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.Monsters;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using SkPluginComponents.Models;

namespace AIRpgAgents.Core.Services;

/// <summary>
/// Service responsible for managing combat encounters in the RPG system
/// </summary>
public class CombatService
{
    private readonly RollDiceService _diceService;
    private readonly RpgState _rpgState;

    public CombatService(RollDiceService diceService, RpgState campaign)
    {
        _diceService = diceService;
        _rpgState = campaign;
    }

    /// <summary>
    /// Creates a new combat encounter between a party and monsters without starting it
    /// </summary>
    public CombatEncounter CreateCombat(CombatEncounter combatEncounter,
        string? environmentDescription = null)
    {
        combatEncounter.EnvironmentDescription = environmentDescription;
        combatEncounter.Status = CombatStatus.InProgress;

        // Add to campaign's combat encounters list if not already there
        if (_rpgState.ActiveCampaign.CombatEncounters.All(x => x.Id != combatEncounter.Id))
            _rpgState.ActiveCampaign.CombatEncounters.Add(combatEncounter);

        // Log the creation of combat
        combatEncounter.CombatLog.Add($"Combat encounter created! {combatEncounter.PlayerParty.Name ?? "The party"} faces {combatEncounter.MonsterEncounter.Monsters.Count} monster(s).");
        if (!string.IsNullOrEmpty(environmentDescription))
        {
            combatEncounter.CombatLog.Add($"Environment: {environmentDescription}");
        }

        return combatEncounter;
    }

    /// <summary>
    /// Starts a new combat encounter between a party and monsters
    /// </summary>
    public async Task<CombatEncounter> StartCombat(CombatEncounter combatEncounter,
        string? environmentDescription = null)
    {
        // Create the combat encounter if it's new
        if (combatEncounter.Status == CombatStatus.NotStarted)
        {
            combatEncounter = CreateCombat(combatEncounter, environmentDescription);
        }

        combatEncounter.Status = CombatStatus.InProgress;
        combatEncounter.IsActive = true;

        // Start the first round
        await StartNextRound(combatEncounter);

        // Log the start of combat
        combatEncounter.CombatLog.Add($"Combat has begun!");
        Console.WriteLine($"CombatEncounter returned after {nameof(CombatService)}.StartCombat:\n{combatEncounter.ToJson()}");
        return combatEncounter;
    }

    /// <summary>
    /// Rolls initiative for all combatants and orders them accordingly
    /// </summary>
    private async Task RollInitiative(CombatEncounter encounter)
    {
        var initiativeList = new List<Combatant>();

        // Roll for party members
        foreach (var character in encounter.PlayerParty.PartyMembers)
        {
            // Get initiative modifier from character
            var initiativeModifier = GetCharacterInitiativeModifier(character);
            var initiativeRoll = await RollD20ForPlayer($"Initiative for {character.CharacterName}", initiativeModifier);

            initiativeList.Add(new Combatant(character.Id)
            {
                Name = character.CharacterName ?? "Unnamed Character",
                Initiative = initiativeRoll,
                IsPlayerCharacter = true,
                Id = character.Id
            });
           

            encounter.CombatLog.Add($"{character.CharacterName} rolls {initiativeRoll} for initiative.");
        }

        // Roll for monsters
        foreach (var monster in encounter.MonsterEncounter.Monsters)
        {
            var initiativeRoll = await RollD20ForNonPlayer($"Initiative for {monster.Name}", monster.InitiativeBonus);

            initiativeList.Add(new Combatant(monster.Id)
            {
                Name = monster.Name ?? "Unnamed Monster",
                Initiative = initiativeRoll,
                IsPlayerCharacter = false,
                Id = monster.Id
            });

            encounter.CombatLog.Add($"{monster.Name} rolls {initiativeRoll} for initiative.");
        }

        // Sort by initiative (highest first)
        encounter.InitiativeOrder = initiativeList.OrderByDescending(c => c.Initiative).ToList();

        // Log initiative order
        encounter.CombatLog.Add("Initiative order: " + string.Join(", ", encounter.InitiativeOrder.Select(c => $"{c.Name} ({c.Initiative})")));
    }

    /// <summary>
    /// Starts the next round of combat, including initiative rolls if it's the first round
    /// </summary>
    public async Task StartNextRound(CombatEncounter encounter)
    {
        encounter.CurrentRound++;

        // Roll initiative if this is the first round
        if (encounter.CurrentRound == 1 || encounter.InitiativeOrder.Count == 0)
        {
            await RollInitiative(encounter);
        }

        // Reset turn tracking for all combatants
        foreach (var combatant in encounter.InitiativeOrder)
        {
            combatant.HasTakenTurn = false;
        }

        // Set the current turn to the first combatant
        encounter.ActiveCombatant = encounter.InitiativeOrder.FirstOrDefault();

        encounter.CombatLog.Add($"Round {encounter.CurrentRound} begins!");
        if (encounter.ActiveCombatant != null)
        {
            encounter.CombatLog.Add($"It's {encounter.ActiveCombatant.Name}'s turn.");
        }
    }

    /// <summary>
    /// Gets a combat encounter by its ID
    /// </summary>
    public CombatEncounter? GetEncounter(string encounterId)
    {
        // Search for the encounter in the campaign's encounters list instead of _activeEncounters
        var encounter = _rpgState.ActiveCampaign.CombatEncounters.FirstOrDefault(e => e.Id == encounterId);
        if (encounter != null)
        {
            return encounter;
        }

        // If not found by ID, return the first active encounter
        var firstActiveEncounter = _rpgState.ActiveCampaign.CombatEncounters.FirstOrDefault(e => e.IsActive);
        if (firstActiveEncounter != null)
        {
            return firstActiveEncounter;
        }

        throw new ArgumentException("Combat encounter not found");
    }

    public async Task<SpellCastResult> ProcessPlayerCastSpell(CombatEncounter encounter, string casterId, string spellName,
        string? targetId = null)
    {
        // Find caster in combat
        var caster = FindCombatantById(encounter, casterId);
        if (caster is not { IsPlayerCharacter: true })
            throw new ArgumentException($"Caster with ID {casterId} is not a valid player combatant.");

        // Get the player caster
        var playerCaster = encounter.PlayerParty.PartyMembers.FirstOrDefault(p => p.Id.Contains(casterId));
        if (playerCaster == null)
            throw new ArgumentException($"Player character with ID {casterId} not found in party");

        // Find target if specified
        CharacterState? playerTarget = null;
        RpgMonster? monsterTarget = null;
        string? targetName = null;

        if (!string.IsNullOrEmpty(targetId))
        {
            var target = FindCombatantById(encounter, targetId);
            if (target == null)
                throw new ArgumentException($"Target with ID {targetId} not found in combat");

            targetName = target.Name;
            if (target.IsPlayerCharacter)
            {
                playerTarget = encounter.PlayerParty.PartyMembers.FirstOrDefault(p => p.Id.Contains(targetId));
                if (playerTarget == null)
                    throw new ArgumentException($"Player target with ID {targetId} not found in party");
            }
            else
            {
                monsterTarget = encounter.MonsterEncounter.Monsters.FirstOrDefault(m => m.Id.Contains(targetId));
                if (monsterTarget == null)
                    throw new ArgumentException($"Monster target with ID {targetId} not found in encounter");
            }
        }

        // Find the spell in character's spellbook
        var spellcasting = playerCaster.CharacterSheet?.Spellcasting;
        if (spellcasting == null)
            return new SpellCastResult 
            { 
                CasterName = playerCaster.CharacterName,
                SpellName = spellName,
                SuccessfulCast = false,
                Effect = $"{playerCaster.CharacterName} cannot cast spells."
            };

        var spell = spellcasting.Spells.FirstOrDefault(s => 
            s.Name?.Equals(spellName, StringComparison.OrdinalIgnoreCase) == true);
        
        if (spell == null)
            return new SpellCastResult 
            { 
                CasterName = playerCaster.CharacterName,
                SpellName = spellName,
                SuccessfulCast = false,
                Effect = $"{playerCaster.CharacterName} doesn't know the spell {spellName}."
            };

        // Check if player has enough mana
        if (playerCaster.CharacterSheet?.CurrentMP < spell.ManaCost)
            return new SpellCastResult 
            { 
                CasterName = playerCaster.CharacterName,
                SpellName = spellName,
                SuccessfulCast = false,
                Effect = $"{playerCaster.CharacterName} doesn't have enough mana to cast {spellName}."
            };

        var result = new SpellCastResult
        {
            CasterName = playerCaster.CharacterName,
            SpellName = spellName,
            SpellDescription = spell.Description ?? string.Empty,
            SuccessfulCast = true,
            TargetName = targetName,
            ManaUsed = spell.ManaCost
        };

        // Deduct mana cost if character sheet exists
        if (playerCaster.CharacterSheet != null)
        {
            playerCaster.CharacterSheet.CurrentMP -= spell.ManaCost;
            result.RemainingMana = playerCaster.CharacterSheet.CurrentMP;
            result.MaxMana = playerCaster.CharacterSheet.MaxMP;
        }

        // Process each spell effect
        foreach (var effect in spell.Effects)
        {
            switch (effect.SpellEffectType)
            {
                case SpellEffectType.Damage when monsterTarget != null:
                    // Parse dice formula and roll damage
                    var damageRoll = await RollDiceForPlayer($"Spell damage for {spellName}", 
                        effect.DieCount, (int)effect.DamageDie);
                    
                    // Apply multiple hits if specified
                    var hits = Math.Max(1, effect.DamageCount);
                    var casterLevel = playerCaster.CharacterSheet?.Level ?? 0;
                    var totalDamage = (damageRoll + casterLevel) * hits;
                    
                    monsterTarget.CurrentHP = Math.Max(0, monsterTarget.CurrentHP - totalDamage);
                    result.DamageDealt += totalDamage;
                    result.DamageType = effect.DamageType;
                    break;

                case SpellEffectType.Heal when playerTarget != null:
                    // Parse dice formula and roll healing
                    var healingRoll = await RollDiceForPlayer($"Healing for {spellName}", 
                        effect.DieCount, (int)effect.DamageDie);
                    var healerLevel = playerCaster.CharacterSheet?.Level ?? 0;
                    var totalHealing = healingRoll + healerLevel;
                    
                    playerTarget.CurrentHitPoints = Math.Min(playerTarget.MaxHitPoints, 
                        playerTarget.CurrentHitPoints + totalHealing);
                    result.HealingDone += totalHealing;
                    break;

                case SpellEffectType.ArmorClass when playerTarget != null:
                    // Apply AC bonus
                    playerTarget.TemporaryEffects.Add(new TemporaryEffect
                    {
                        Name = $"{spellName} - AC Bonus",
                        Duration = effect.Duration ?? "1 round",
                        Value = effect.Value,
                        Type = EffectType.ArmorClass
                    });
                    result.Effect += $"\nApplied +{effect.Value} AC bonus for {effect.Duration}.";
                    break;

                case SpellEffectType.Protect:
                    var target = playerTarget ?? playerCaster;
                    target.TemporaryEffects.Add(new TemporaryEffect
                    {
                        Name = $"{spellName} - Protection",
                        Duration = effect.Duration ?? "1 round",
                        Value = effect.DieCount > 0 ? effect.DieCount : effect.Value,
                        Type = EffectType.DamageReduction
                    });
                    result.Effect += $"\nApplied protection effect for {effect.Duration}.";
                    break;

                // Add other effect types as needed
                default:
                    result.Effect += $"\n{effect.Description}";
                    break;
            }
        }

        // Log the spell cast
        LogSpellCastResult(encounter, result);
        UpdateCombatStatus(encounter);
        return result;
    }

    public async Task<SkillUseResult> ProcessPlayerUseSkill(CombatEncounter encounter, string userId, string skillName,
    string? targetId = null, int difficultyLevel = 10)
    {
        // Find skill user in combat
        var user = FindCombatantById(encounter, userId);
        if (user is not { IsPlayerCharacter: true })
            throw new ArgumentException($"Skill user with ID {userId} is not a valid player combatant.");

        // Get the player character
        var player = encounter.PlayerParty.PartyMembers.FirstOrDefault(p => p.Id.Contains(userId));
        if (player == null)
            throw new ArgumentException($"Player character with ID {userId} not found in party");

        // Find the skill
        var skill = player.CharacterSheet.Skills.FirstOrDefault(s =>
            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

        if (skill == null)
            throw new ArgumentException($"Skill {skillName} not found for character {player.CharacterName}");

        // Find target if specified
        string? targetName = null;
        if (!string.IsNullOrEmpty(targetId))
        {
            var target = FindCombatantById(encounter, targetId);
            if (target == null)
                throw new ArgumentException($"Target with ID {targetId} not found in combat");
            targetName = target.Name;
        }

        var result = new SkillUseResult
        {
            UserName = player.CharacterName,
            SkillName = skill.Name,
            SkillDescription = skill.Name, // Using Name as description since Skill doesn't have a Description property
            TargetName = targetName
        };

        // Calculate skill check modifiers
        var attributeModifier = player.CharacterSheet.GetAttributeModifier(skill.AssociatedAttribute);
        var skillBonus = skill.Rank;

        result.SkillBonus = skillBonus;
        result.AttributeModifier = attributeModifier;
        result.TotalBonus = skillBonus + attributeModifier;

        // Roll skill check
        var skillCheckRoll = await RollD20ForPlayer($"Skill check for {skillName}", result.TotalBonus);
        result.SkillCheckRoll = skillCheckRoll - result.TotalBonus; // Store the natural roll

        // Determine success/failure based on DC
        result.Success = skillCheckRoll >= difficultyLevel;

        // Log the skill use
        LogSkillUseResult(encounter, result);
        return result;
    }
    /// <summary>
    /// Process an attack action from a player combatant to another using combatant IDs
    /// </summary>
    public async Task<AttackResult> ProcessPlayerAttackById(CombatEncounter encounter, string attackerId, string targetId, string? attackName = null)
    {

        Console.WriteLine($"CombatEncounter returned after {nameof(CombatService)}.ProcessPlayerAttackById:\n{encounter.ToJson()}");
        // Find attacker and validate it's a player
        var attacker = FindCombatantById(encounter, attackerId);
        if (attacker == null)
            throw new ArgumentException($"Attacker with ID {attackerId} is not a valid player combatant.");

        // Find target in combat
        var target = FindCombatantById(encounter, targetId);
        if (target == null)
            throw new ArgumentException($"Target with ID {targetId} not found in combat. Available Ids:\n{string.Join("\n", encounter.InitiativeOrder.Select(x => x.Id))}");

        // Get the player attacker and target (target may be player or monster)
        var playerAttacker = encounter.PlayerParty.PartyMembers.FirstOrDefault(p => p.Id.Contains(attackerId));
        if (playerAttacker == null)
            throw new ArgumentException($"Player character with ID {attacker.Id} not found in party");

        CharacterState? playerTarget = null;
        RpgMonster? monsterTarget = null;
        if (target.IsPlayerCharacter)
        {
            playerTarget = encounter.PlayerParty.PartyMembers.FirstOrDefault(p => p.Id.Contains(targetId));
            if (playerTarget == null)
                throw new ArgumentException($"Player character with ID {target.Id} not found in party");
        }
        else
        {
            monsterTarget = encounter.MonsterEncounter.Monsters.FirstOrDefault(m => m.Id.Contains(targetId));
            if (monsterTarget == null)
                throw new ArgumentException($"Monster with ID {target.Id} not found in encounter");
        }

        var result = new AttackResult
        {
            AttackerName = attacker.Name,
            TargetName = target.Name,
            IsHit = false,
            IsCritical = false,
            DamageDealt = 0
        };

        // Roll attack for player
        int attackBonus = GetCharacterAttackBonus(playerAttacker);
        var attackRoll = await RollD20ForPlayer($"Attack on {target.Name}", attackBonus);
        var totalAttackRoll = attackRoll;
        result.AttackRoll = attackRoll - attackBonus;
        result.AttackBonus = attackBonus;
        result.TotalAttack = totalAttackRoll;

        if (attackRoll == 20)
        {
            result.IsHit = true;
            result.IsCritical = true;
        }
        else
        {
            var targetAC = target.IsPlayerCharacter ? GetCharacterArmorClass(playerTarget!) : monsterTarget!.ArmorClass;
            result.TargetAC = targetAC;
            result.IsHit = totalAttackRoll >= targetAC;
        }

        // Process damage if hit
        if (result.IsHit)
        {
            // Use player's equipped weapon or fallback weapon
            var combatAttack = playerAttacker.EquippedWeapon;
            var damageDice = combatAttack.DamageDie;
            var dieCount = combatAttack.DamageDieCount;
            var damageRoll = await RollDiceForPlayer($"Damage against {target.Name}", dieCount, (int)damageDice);
            result.DamageBonus = GetCharacterDamageBonus(playerAttacker, combatAttack.Range);
            result.DamageRoll = result.IsCritical ? damageRoll * 2 : damageRoll;

            var totalDamage = Math.Max(1, result.DamageRoll + result.DamageBonus);
            result.DamageDealt = totalDamage;

            if (target.IsPlayerCharacter)
            {
                playerTarget!.CurrentHitPoints = Math.Max(0, playerTarget.CurrentHitPoints - totalDamage);
                result.RemainingHP = playerTarget.CurrentHitPoints;
                result.MaxHP = playerTarget.MaxHitPoints;
            }
            else
            {
                monsterTarget!.CurrentHP = Math.Max(0, monsterTarget.CurrentHP - totalDamage);
                result.RemainingHP = monsterTarget.CurrentHP;
                result.MaxHP = monsterTarget.MaxHP;
            }
        }

        LogAttackResult(encounter, result);
        UpdateCombatStatus(encounter);
        return result;
    }

    /// <summary>
    /// Process an attack action from a monster combatant to a player using combatant IDs
    /// </summary>
    public async Task<AttackResult> ProcessMonsterAttackById(CombatEncounter encounter, string attackerId, string targetId, string? attackName = null)
    {
        Console.WriteLine($"CombatEncounter returned in {nameof(CombatService)}.ProcessMonsterAttackById:\n{encounter.ToJson()}");
        var monsterAttacker = encounter.MonsterEncounter.Monsters.FirstOrDefault(m => m.Id.Contains(attackerId));
        if (monsterAttacker == null)
            throw new ArgumentException($"Monster with ID {attackerId} not found in encounter");

        var playerTarget = encounter.PlayerParty.PartyMembers.FirstOrDefault(p => p.Id.Contains(targetId));
        if (playerTarget == null)
            throw new ArgumentException("Monsters can only attack players.");

        var result = new AttackResult
        {
            AttackerName = monsterAttacker!.Name!,
            TargetName = playerTarget.CharacterName,
            IsHit = false,
            IsCritical = false,
            DamageDealt = 0
        };

        int attackBonus = 0;
        var monsterAttack = monsterAttacker.SpecialAttacks.FirstOrDefault(a =>
            string.IsNullOrEmpty(attackName) || a.Name == attackName);
        if (monsterAttack != null)
        {
            attackBonus = monsterAttack.AttackBonus;
            result.AttackName = monsterAttack.Name;
        }
        else
        {
            monsterAttack = monsterAttacker.DefaultAttack;
            if (monsterAttack != null)
            {
                attackBonus = monsterAttack.AttackBonus;
                result.AttackName = monsterAttack.Name;
            }
        }

        // Roll attack for non-player
        var attackRoll = await RollD20ForNonPlayer($"Attack on {playerTarget.CharacterName}", attackBonus);
        var totalAttackRoll = attackRoll;
        result.AttackRoll = attackRoll - attackBonus;
        result.AttackBonus = attackBonus;
        result.TotalAttack = totalAttackRoll;

        if (attackRoll == 20)
        {
            result.IsHit = true;
            result.IsCritical = true;
        }
        else
        {
            var targetAC = GetCharacterArmorClass(playerTarget);
            result.TargetAC = targetAC;
            result.IsHit = totalAttackRoll >= targetAC;
        }

        if (result.IsHit)
        {
            // Use monster's special attack or default attack
            monsterAttack = monsterAttacker.SpecialAttacks.FirstOrDefault(a =>
                             string.IsNullOrEmpty(attackName) || a.Name == attackName)
                            ?? monsterAttacker.DefaultAttack 
                            ?? new MonsterAttack() 
                            { 
                                 DamageDie = DieType.D6, 
                                 DamageDieCount = 1, 
                                 Name = "Default Attack", 
                                 Description = "Fallback attack" 
                            };

            var damageRoll = await RollDiceForNonPlayer($"Damage against {playerTarget.CharacterName}", monsterAttack.DamageDieCount, (int)monsterAttack.DamageDie);
            result.DamageRoll = result.IsCritical ? damageRoll * 2 : damageRoll;
            result.DamageBonus = monsterAttack.DamageBonus;
            result.DamageType = monsterAttack.DamageType.ToString();

            var totalDamage = Math.Max(1, result.DamageRoll + result.DamageBonus);
            result.DamageDealt = totalDamage;

            playerTarget.CurrentHitPoints = Math.Max(0, playerTarget.CurrentHitPoints - totalDamage);
            result.RemainingHP = playerTarget.CurrentHitPoints;
            result.MaxHP = playerTarget.MaxHitPoints;
        }

        LogAttackResult(encounter, result);
        UpdateCombatStatus(encounter);
        return result;
    }

    private static void LogAttackResult(CombatEncounter encounter, AttackResult result)
    {
        // Log the attack result
        string attackMessage;
        if (!result.IsHit)
        {
            attackMessage = $"{result.AttackerName} attacks {result.TargetName} with {result.AttackName ?? "an attack"} but misses! (Rolled {result.AttackRoll} + {result.AttackBonus} = {result.TotalAttack} vs AC {result.TargetAC})";
        }
        else if (result.IsCritical)
        {
            attackMessage = $"{result.AttackerName} lands a CRITICAL HIT on {result.TargetName} with {result.AttackName ?? "an attack"} for {result.DamageDealt} damage! (Critical hit!)";
        }
        else
        {
            attackMessage = $"{result.AttackerName} hits {result.TargetName} with {result.AttackName ?? "an attack"} for {result.DamageDealt} damage. (Rolled {result.AttackRoll} + {result.AttackBonus} = {result.TotalAttack} vs AC {result.TargetAC})";
        }

        if (result.RemainingHP <= 0)
        {
            attackMessage += $" {result.TargetName} is defeated!";
        }
        else
        {
            attackMessage += $" {result.TargetName} has {result.RemainingHP}/{result.MaxHP} HP remaining.";
        }

        encounter.CombatLog.Add(attackMessage);
    }

    private void LogSpellCastResult(CombatEncounter encounter, SpellCastResult result)
    {
        string message;
        if (!result.SuccessfulCast)
        {
            message = $"{result.CasterName} attempts to cast {result.SpellName} but fails. {result.Effect}";
        }
        else if (result.DamageDealt > 0)
        {
            message = $"{result.CasterName} casts {result.SpellName} on {result.TargetName}, dealing {result.DamageDealt} {result.DamageType} damage!";
        }
        else if (result.HealingDone > 0)
        {
            message = $"{result.CasterName} casts {result.SpellName} on {result.TargetName}, healing for {result.HealingDone} HP.";
        }
        else
        {
            message = $"{result.CasterName} casts {result.SpellName}. {result.Effect}";
        }

        message += $" ({result.ManaUsed} mana used, {result.RemainingMana}/{result.MaxMana} remaining)";
        encounter.CombatLog.Add(message);
    }

    private void LogSkillUseResult(CombatEncounter encounter, SkillUseResult result)
    {
        string message;
        if (!string.IsNullOrEmpty(result.TargetName))
        {
            message = $"{result.UserName} uses {result.SkillName} on {result.TargetName}. ";
        }
        else
        {
            message = $"{result.UserName} uses {result.SkillName}. ";
        }

        if (result.Success)
        {
            message += result.Effect;
        }
        else
        {
            message += $"The attempt fails (rolled {result.SkillCheckRoll}, needed higher).";
        }

        encounter.CombatLog.Add(message);
    }

    /// <summary>
    /// Helper method to find a combatant by ID in the encounter
    /// </summary>
    private Combatant? FindCombatantById(CombatEncounter encounter, string combatantId)
    {
        return encounter.InitiativeOrder.FirstOrDefault(c => c.Id == combatantId);
    }

    /// <summary>
    /// Advance to the next turn in combat
    /// </summary>
    public async Task<Combatant?> NextTurn(CombatEncounter encounter)
    {
        if (encounter.ActiveCombatant != null)
        {
            // Mark current combatant as having taken their turn
            encounter.ActiveCombatant.HasTakenTurn = true;
        }

        // Find the next combatant who hasn't taken a turn
        var nextCombatant = encounter.InitiativeOrder
            .SkipWhile(c => c != encounter.ActiveCombatant)
            .Skip(1)
            // If no combatant is found, check from the beginning
            .FirstOrDefault(c => !c.HasTakenTurn) ?? encounter.InitiativeOrder.FirstOrDefault(c => !c.HasTakenTurn);

        // If still no combatant is found, start a new round
        if (nextCombatant == null)
        {
            await StartNextRound(encounter);
            nextCombatant = encounter.InitiativeOrder.FirstOrDefault();
        }

        encounter.ActiveCombatant = nextCombatant;

        if (nextCombatant != null)
        {
            encounter.CombatLog.Add($"It's {nextCombatant.Name}'s turn.");
        }

        return nextCombatant;
    }

    /// <summary>
    /// Checks and updates the status of combat
    /// </summary>
    private void UpdateCombatStatus(CombatEncounter encounter)
    {
        // Check if all party members are defeated
        var allPartyDefeated = encounter.PlayerParty.PartyMembers.All(c => c.CurrentHitPoints <= 0);

        // Check if all monsters are defeated
        var allMonstersDefeated = encounter.MonsterEncounter.Monsters.All(m => m.CurrentHP <= 0);

        if (allPartyDefeated)
        {
            encounter.Status = CombatStatus.PlayerDefeat;
            encounter.IsActive = false;
            encounter.CombatLog.Add("The party has been defeated!");
        }
        else if (allMonstersDefeated)
        {
            encounter.Status = CombatStatus.PlayerVictory;
            encounter.IsActive = false;
            encounter.CombatLog.Add("Victory! All enemies have been defeated!");
        }
    }

    /// <summary>
    /// Ends a combat encounter with the specified result
    /// </summary>
    public Task EndCombat(CombatEncounter encounter, CombatStatus status)
    {
        encounter.Status = status;
        encounter.IsActive = false;

        var endMessage = status switch
        {
            CombatStatus.PlayerVictory => "Combat ends in victory for the party!",
            CombatStatus.PlayerDefeat => "Combat ends in defeat for the party.",
            CombatStatus.Fled => "The party has fled from combat.",
            CombatStatus.Resolved => "Combat has been resolved peacefully.",
            _ => "Combat has ended."
        };

        encounter.CombatLog.Add(endMessage);

        // Update the monster encounter end time
        encounter.MonsterEncounter.EncounterEnd = DateTime.Now;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a narrative summary of the combat state
    /// </summary>
    public Task<string> GetCombatNarrative(CombatEncounter encounter)
    {
        var sb = new StringBuilder();

        // Add environment description
        if (!string.IsNullOrEmpty(encounter.EnvironmentDescription))
        {
            sb.AppendLine(encounter.EnvironmentDescription);
            sb.AppendLine();
        }

        // Add combat status
        sb.AppendLine($"Round {encounter.CurrentRound} of combat.");
        sb.AppendLine();

        // Add party status
        sb.AppendLine("Party status:");
        foreach (var member in encounter.PlayerParty.PartyMembers)
        {
            var healthStatus = GetHealthStatusDescription(member.CurrentHitPoints, member.MaxHitPoints);
            sb.AppendLine($"- {member.CharacterName}: {member.CurrentHitPoints}/{member.MaxHitPoints} HP ({healthStatus})");
        }
        sb.AppendLine();

        // Add monster status
        sb.AppendLine("Enemy status:");
        foreach (var monster in encounter.MonsterEncounter.Monsters.Where(m => m.CurrentHP > 0))
        {
            var healthStatus = GetHealthStatusDescription(monster.CurrentHP, monster.MaxHP);
            sb.AppendLine($"- {monster.Name}: {monster.CurrentHP}/{monster.MaxHP} HP ({healthStatus})");
        }
        sb.AppendLine();

        // Add current turn
        if (encounter.ActiveCombatant != null)
        {
            sb.AppendLine($"It's {encounter.ActiveCombatant.Name}'s turn to act.");
        }

        // Add recent log entries (last 5)
        sb.AppendLine("Recent events:");
        foreach (var entry in encounter.CombatLog.TakeLast(5))
        {
            sb.AppendLine($"- {entry}");
        }

        return Task.FromResult(sb.ToString().TrimEnd());
    }

    private string GetHealthStatusDescription(int current, int max)
    {
        var percentage = (double)current / max;

        return percentage switch
        {
            1.0 => "unharmed",
            >= 0.75 => "slightly wounded",
            >= 0.5 => "wounded",
            >= 0.25 => "badly wounded",
            > 0 => "critically wounded",
            _ => "defeated"
        };
    }

    #region Helper Methods for Dice Rolling and Character Stats

    /// <summary>
    /// Rolls a d20 die with the given modifier for a player character using the RollDiceService
    /// </summary>
    private async Task<int> RollD20ForPlayer(string reasonForRolling, int modifier = 0)
    {
        var windowOptions = new RollDiceWindowOptions()
        {
            Title = $"Roll d20 for {reasonForRolling}",
            Location = Location.Center,
            Style = "width:max-content;min-width:50vw;height:max-content"
        };

        var parameters = new RollDiceParameters() { ["DieType"] = DieType.D20, ["NumberOfRolls"] = 1 };
        var result = await _diceService.OpenDiceWindow(parameters, windowOptions);
        return (result?.Parameters.Get<int>("Total") ?? 0) + modifier;
    }

    /// <summary>
    /// Rolls a d20 die with the given modifier for a non-player character using automated rolls
    /// </summary>
    private async Task<int> RollD20ForNonPlayer(string reasonForRolling, int modifier = 0)
    {
        var windowOptions = new RollDiceWindowOptions()
        {
            Title = $"Roll d20 for {reasonForRolling}",
            Location = Location.TopRight,
            Style = "width:max-content;min-width:30vw;height:max-content"
        };

        var parameters = new RollDiceParameters()
        {
            ["DieType"] = DieType.D20,
            ["NumberOfRolls"] = 1,
            ["IsManual"] = false
        };

        var result = await _diceService.OpenDiceWindow(parameters, windowOptions);
        return (result?.Parameters.Get<int>("Total") ?? 0) + modifier;
    }

    /// <summary>
    /// Rolls dice of the specified count and type for a player character using the RollDiceService
    /// </summary>
    private async Task<int> RollDiceForPlayer(string reasonForRolling, int diceCount, int diceType, int damageModifier = 0)
    {
        var dieType = diceType switch
        {
            4 => DieType.D4,
            6 => DieType.D6,
            8 => DieType.D8,
            10 => DieType.D10,
            20 => DieType.D20,
            _ => DieType.D6
        };

        var windowOptions = new RollDiceWindowOptions()
        {
            Title = $"Roll {diceCount}d{diceType} for {reasonForRolling}",
            Location = Location.Center,
            Style = "width:max-content;min-width:50vw;height:max-content"
        };

        var parameters = new RollDiceParameters() { ["DieType"] = dieType, ["NumberOfRolls"] = diceCount };
        var result = await _diceService.OpenDiceWindow(parameters, windowOptions);
        return result?.Parameters.Get<int>("Total") + damageModifier ?? damageModifier;
    }

    /// <summary>
    /// Rolls dice of the specified count and type for a non-player character using automated rolls
    /// </summary>
    private async Task<int> RollDiceForNonPlayer(string reasonForRolling, int diceCount, int diceType)
    {
        var dieType = diceType switch
        {
            4 => DieType.D4,
            6 => DieType.D6,
            8 => DieType.D8,
            10 => DieType.D10,
            20 => DieType.D20,
            _ => DieType.D6
        };

        var windowOptions = new RollDiceWindowOptions()
        {
            Title = $"Roll {diceCount}d{diceType} for {reasonForRolling}",
            Location = Location.TopRight,
            Style = "width:max-content;min-width:30vw;height:max-content"
        };

        var parameters = new RollDiceParameters()
        {
            ["DieType"] = dieType,
            ["NumberOfRolls"] = diceCount,
            ["IsManual"] = false
        };

        var result = await _diceService.OpenDiceWindow(parameters, windowOptions);
        return result?.Parameters.Get<int>("Total") ?? 0;
    }

    /// <summary>
    /// Gets the character's initiative modifier
    /// </summary>
    private int GetCharacterInitiativeModifier(CharacterState character)
    {
        // Use Initiative from CharacterSheet or calculate from attributes
        return character.CharacterSheet.InitiativeBonus;
    }

    /// <summary>
    /// Gets the character's attack bonus
    /// </summary>
    private int GetCharacterAttackBonus(CharacterState character)
    {
        // Use weapon attack bonus if available, otherwise calculate from attributes
        var totalBonus = 0;
        var weapon = character.CharacterSheet?.Weapons?.FirstOrDefault(x => x.IsEquipped);
        if (weapon == null)
            return totalBonus;
        totalBonus += character.CharacterSheet!.Weapons.First().AttackBonus;
        if (weapon.Range == "Melee")
        {
            totalBonus += AttributeSystem.CalculateModifier(character.CharacterSheet?.Might ?? 10);
            if (!character.CharacterSheet!.Skills.Any(x => x.Name.ToLower().Contains("melee"))) return totalBonus;
            var meleeSkill = character.CharacterSheet.Skills.First(x => x.Name.ToLower().Contains("melee"));
            totalBonus += meleeSkill.Rank;

        }
        else
        {
            totalBonus += AttributeSystem.CalculateModifier(character.CharacterSheet?.Agility ?? 10);
            if (!character.CharacterSheet!.Skills.Any(x => x.Name.ToLower().Contains("ranged"))) return totalBonus;
            var rangedSkill = character.CharacterSheet.Skills.First(x => x.Name.ToLower().Contains("ranged"));
            totalBonus += rangedSkill.Rank;

        }

        return totalBonus;
    }

    /// <summary>
    /// Gets the character's damage bonus
    /// </summary>
    private int GetCharacterDamageBonus(CharacterState character, string range)
    {
        // Calculate damage bonus from attributes (typically Strength/Might)
        if (character.CharacterSheet?.Weapons.Find(x => x.IsEquipped)?.Range?.ToLower().Contains("melee") == true)
            return AttributeSystem.CalculateModifier(character.CharacterSheet.Might);
        if (character.CharacterSheet?.Weapons.Find(x => x.IsEquipped)?.Range?.ToLower().Contains("ranged") == true)
            return AttributeSystem.CalculateModifier(character.CharacterSheet.Agility);
        // Fallback to a default value
        return 0;
    }

    /// <summary>
    /// Gets the character's armor class
    /// </summary>
    private int GetCharacterArmorClass(CharacterState character)
    {
        // Get armor class directly from character sheet
        if (character.CharacterSheet != null)
        {
            return character.CharacterSheet.ArmorClass;
        }
        // Fallback to a default value
        return 10;
    }

    #endregion
}

/// <summary>
/// Represents the result of an attack action
/// </summary>
public class AttackResult
{
    public string AttackerName { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public string? AttackName { get; set; }
    public int AttackRoll { get; set; }
    public int AttackBonus { get; set; }
    public int TotalAttack { get; set; }
    public int TargetAC { get; set; }
    public bool IsHit { get; set; }
    public bool IsCritical { get; set; }
    public int DamageRoll { get; set; }
    public int DamageBonus { get; set; }
    public int DamageDealt { get; set; }
    public string? DamageType { get; set; }
    public int RemainingHP { get; set; }
    public int MaxHP { get; set; }
}

/// <summary>
/// Represents the result of a spell casting action
/// </summary>
public class SpellCastResult
{
    public string CasterName { get; set; } = string.Empty;
    public string SpellName { get; set; } = string.Empty;
    public string SpellDescription { get; set; } = string.Empty;
    public bool SuccessfulCast { get; set; }
    public string? TargetName { get; set; }
    public int SpellCheckRoll { get; set; }
    public int DamageDealt { get; set; }
    public string? DamageType { get; set; }
    public int HealingDone { get; set; }
    public int ManaUsed { get; set; }
    public int RemainingMana { get; set; }
    public int MaxMana { get; set; }
    public string? Effect { get; set; }
    public int RemainingHP { get; set; }
    public int MaxHP { get; set; }
}

/// <summary>
/// Represents the result of a skill use action
/// </summary>
public class SkillUseResult
{
    public string UserName { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string SkillDescription { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? TargetName { get; set; }
    public int SkillCheckRoll { get; set; }
    public int SkillBonus { get; set; }
    public int AttributeModifier { get; set; }
    public int TotalBonus { get; set; }
    public string? Effect { get; set; }
}


