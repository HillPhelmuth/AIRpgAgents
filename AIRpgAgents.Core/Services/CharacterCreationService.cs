using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Models.Events;
using AIRpgAgents.GameEngine.Enums;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using SkPluginComponents.Models;

namespace AIRpgAgents.Core.Services;

public interface ICharacterCreationService
{
    /// <summary>
    /// Retrieves the current state of character creation for a given ID.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <returns>The current character creation state.</returns>
    Task<CharacterCreationState> GetCharacterCreationStateAsync(string creationStateId);

    /// <summary>
    /// Updates the current character creation state.
    /// </summary>
    /// <param name="creationState">The updated character creation state.</param>
    void UpdateCharacterCreationStateAsync(CharacterCreationState creationState);

    /// <summary>
    /// Selects the method for generating attribute scores and returns the generated scores.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <param name="method">The method to use for generating attribute scores.</param>
    /// <returns>A list of generated attribute scores.</returns>
    Task<List<int>> SelectAttributeScoreTypeAsync(string creationStateId, AttributeGenerationMethod method);

    /// <summary>
    /// Applies the human race bonus attribute selection.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <param name="attribute">The attribute to receive the human bonus.</param>
    Task ApplyHumanBonusAttributeAsync(string creationStateId, RpgAttribute attribute);

    /// <summary>
    /// Selects the initial skills for the character.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <param name="selectedSkills">The list of selected skill names.</param>
    Task SelectSkillsAsync(string creationStateId, List<string> selectedSkills);

    /// <summary>
    /// Selects spells for spellcasting classes.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <param name="selectedSpells">The list of selected spells.</param>
    Task SelectSpellsAsync(string creationStateId, List<Spell> selectedSpells);

    /// <summary>
    /// Selects starting equipment for the character.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <param name="selectedWeapons">List of selected weapons.</param>
    /// <param name="selectedArmor">List of selected armor.</param>
    /// <param name="selectedItems">List of selected adventuring gear.</param>
    /// <returns>The remaining currency in copper pieces.</returns>
    Task<int> SelectEquipmentAsync(string creationStateId, List<Weapon>? selectedWeapons, List<Armor>? selectedArmor, List<AdventuringGear>? selectedItems);

    /// <summary>
    /// Sets the character's background information.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <param name="personalityTraits">The character's personality traits.</param>
    /// <param name="ideals">The character's ideals.</param>
    /// <param name="bonds">The character's bonds.</param>
    /// <param name="flaws">The character's flaws.</param>
    Task SetBackgroundInfoAsync(string creationStateId, string personalityTraits, string ideals, string bonds, string flaws);

    /// <summary>
    /// Completes the character creation process and validates all required information.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <returns>The completed character sheet.</returns>
    Task<CharacterSheet> CompleteCharacterCreationAsync(string creationStateId);

    /// <summary>
    /// Advances the character creation process to the next step.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    Task AdvanceToNextStepAsync(string creationStateId);

    /// <summary>
    /// Assigns attribute scores to specific attributes.
    /// </summary>
    /// <param name="creationStateId">The unique identifier for the character creation state.</param>
    /// <param name="attributeAssignments">Dictionary mapping attributes to their assigned scores.</param>
    Task AssignAttributeScores(string creationStateId, Dictionary<RpgAttribute, int> attributeAssignments);

    /// <summary>
    /// Event that fires when the character creation state changes.
    /// </summary>
    event CharacterCreationEventHandler? CharacterCreationStateChanged;

    /// <summary>
    /// Initializes character creation with an existing creation state.
    /// </summary>
    /// <param name="playerName">The name of the player.</param>
    /// <param name="creationState">The initial character creation state.</param>
    void InitializeCharacterCreation(string playerName, CharacterCreationState creationState);

    /// <summary>
    /// Initializes a new character creation process.
    /// </summary>
    /// <param name="characterName">The name of the character.</param>
    /// <param name="playerName">The name of the player.</param>
    /// <param name="race">The character's race.</param>
    /// <param name="characterClass">The character's class.</param>
    /// <param name="alignment">The character's alignment.</param>
    /// <param name="deity">Optional deity selection.</param>
    Task InitializeCharacterCreationAsync(string characterName, string playerName, Race race, CharacterClass characterClass, AlignmentValue alignment, string? deity = null);
}

/// <summary>
/// Service implementation for managing the character creation process.
/// </summary>
public class CharacterCreationService : ICharacterCreationService
{
    private readonly ConcurrentDictionary<string, CharacterCreationState> _creationStates = new();

    /// <summary>
    /// Event that fires when the character creation state changes.
    /// </summary>
    public event CharacterCreationEventHandler? CharacterCreationStateChanged;

    /// <summary>
    /// Standard skill rank given at character creation.
    /// </summary>
    private const int INITIAL_SKILL_RANK = 1;

    /// <summary>
    /// Skill rank for primary class skills.
    /// </summary>
    private const int PRIMARY_SKILL_RANK = 2;

    /// <summary>
    /// Standard array values for attribute generation.
    /// </summary>
    private readonly int[] STANDARD_ARRAY = [15, 14, 12, 11, 9];

    public async Task InitializeCharacterCreationAsync(
        string characterName,
        string playerName,
        Race race,
        CharacterClass characterClass,
        AlignmentValue alignment,
        string? deity = null)
    {
        var creationState = new CharacterCreationState(playerName)
        {
            CharacterName = characterName,
            PlayerName = playerName,
            Race = race,
            Class = characterClass,
            Alignment = alignment,
            Deity = deity,
            CurrentStep = CharacterCreationStep.BasicInfo
        };
        if (characterClass.HasSpellcasting)
        {
            creationState.SpellcastingInfo = new SpellcastingInfo(characterClass, creationState.DraftCharacter.Level)
            {
                MagicTradition = characterClass.SpellcastingTradition,
                SpellcastingAbility = characterClass.SpellcastingTradition switch
                {
                    MagicTradition.Arcane => RpgAttribute.Wits,
                    MagicTradition.Theurgic => RpgAttribute.Presence
                }
                
            };
        }
        // Store the creation state
        _creationStates[creationState.Id] = creationState;
        UpdateCharacterCreationStateAsync(creationState);
        // Advance to attribute scores step
        await AdvanceToNextStepAsync(creationState.Id);
    }
    public void InitializeCharacterCreation(string playerName, CharacterCreationState creationState)
    {
        var id = creationState.Id;
        _creationStates[id] = creationState;
    }

    private void OnCreationStateChanged(CharacterCreationState creationState, string id)
    {
        CharacterCreationStateChanged?.Invoke(this, new CharacterCreationEventArgs(id, creationState));
    }

    public async Task<CharacterCreationState> GetCharacterCreationStateAsync(string creationStateId)
    {
        if (_creationStates.TryGetValue(creationStateId, out var state))
        {
            return state;
        }

        throw new KeyNotFoundException($"Character creation state with ID '{creationStateId}' not found");
    }

    public void UpdateCharacterCreationStateAsync(CharacterCreationState creationState)
    {
        _creationStates[creationState.Id] = creationState;
        OnCreationStateChanged(creationState, creationState.Id);
    }

    public async Task<List<int>> SelectAttributeScoreTypeAsync(string creationStateId,
        AttributeGenerationMethod method)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);
        state.AttributeMethod = method;

        switch (method)
        {
            case AttributeGenerationMethod.RollAttributes:
                // Roll 4d6, drop the lowest die, for each attribute
                //state.RolledScores.Clear();

                //for (int i = 0; i < 5; i++)
                //{
                //    int[] dice = new int[4];
                //    for (int d = 0; d < 4; d++)
                //    {
                //        dice[d] = DieType.D6.RollDie();
                //    }

                //    // Keep the 3 highest dice
                //    int score = dice.OrderByDescending(d => d).Take(3).Sum();
                //    state.RolledScores.Add(score);
                //}

                break;

            case AttributeGenerationMethod.StandardArray:
                // Apply the standard array based on the attribute assignments
                state.RolledScores = STANDARD_ARRAY.ToList();
                break;
        }

        // Apply racial modifiers again to maintain them


        UpdateCharacterCreationStateAsync(state);
        return state.RolledScores;
    }

    public async Task AssignAttributeScores(string creationStateId, Dictionary<RpgAttribute, int> attributeAssignments)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);
        state.AttributeSet = new AttributeSet(attributeAssignments);
        ApplyRacialModifiers(state);
        UpdateCharacterCreationStateAsync(state);
    }
    private void ApplyRacialModifiers(CharacterCreationState state)
    {
        var race = state.Race;
        //var attributes = state.AttributeSet;

        // Apply racial modifiers based on the race type
        switch (race.Type)
        {
            case RaceType.Human:
                // Humans get player's choice of attribute to improve
                // TODO: Implement player choice for attribute bonus
                // The Human race has "PlayerChoice": 1 in Races.json
                // This will be implemented when UI for attribute selection is created
                break;

            case RaceType.Duskborn:
                state.AttributeSet[RpgAttribute.Agility] += 2;
                state.AttributeSet[RpgAttribute.Might] -= 1;
                break;

            case RaceType.Ironforged:
                state.AttributeSet[RpgAttribute.Vitality] += 2;
                state.AttributeSet[RpgAttribute.Agility] -= 1;
                break;

            case RaceType.Wildkin:
                state.AttributeSet[RpgAttribute.Wits] += 2;
                state.AttributeSet[RpgAttribute.Presence] -= 1;
                break;

            case RaceType.Emberfolk:
                state.AttributeSet[RpgAttribute.Presence] += 2;
                state.AttributeSet[RpgAttribute.Wits] -= 1;
                break;

            case RaceType.Stoneborn:
                state.AttributeSet[RpgAttribute.Might] += 1;
                state.AttributeSet[RpgAttribute.Vitality] += 1;
                state.AttributeSet[RpgAttribute.Agility] -= 2;
                break;
        }


    }
    public async Task ApplyHumanBonusAttributeAsync(string creationStateId, RpgAttribute attribute)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);

        // Check if the character is human
        if (state.Race.Type == RaceType.Human)
        {
            state.HumanBonusAttribute = attribute;
            state.AttributeSet[attribute] += 1;
        }

        UpdateCharacterCreationStateAsync(state);
    }

    public async Task SelectSkillsAsync(string creationStateId, List<string> selectedSkills)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);

        // Per SkillSystem.md, players select 5 skills at character creation
        if (selectedSkills.Count != 5)
        {
            throw new ArgumentException("You must select exactly 5 skills during character creation");
        }

        // Clear previously selected skills
        state.SelectedSkills.Clear();
        state.AvailableSkills = Skill.AllSkills.Select(s => s.Name).ToList();
        // Check if the selected skills are available
        foreach (string skillName in selectedSkills)
        {
            if (!state.AvailableSkills.Contains(skillName))
            {
                throw new ArgumentException($"Skill '{skillName}' is not available for selection");
            }

            // Create the skill with rank
            var skill = new Skill(skillName, Skill.AllSkills.Find(x => x.Name == skillName).AssociatedAttribute)
            {
                Name = skillName,
                Rank = INITIAL_SKILL_RANK
            };

            // If this skill is a class primary skill, increase the rank
            if (state.Class.PrimaryAttributes.Contains(skillName))
            {
                skill.Rank = PRIMARY_SKILL_RANK;
            }

            state.SelectedSkills[skillName] = skill;
        }

        UpdateCharacterCreationStateAsync(state);
    }

    public async Task SelectSpellsAsync(string creationStateId, List<Spell> selectedSpells)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);

        // Check if the class has spellcasting capability
        if (!state.Class.HasSpellcasting)
        {
            throw new InvalidOperationException($"Class {state.Class.Name} does not have spellcasting capabilities");
        }

        // Get the number of starting spells for this class
        var startingSpellCount = state.Class.Type switch
        {
            ClassType.Cleric or ClassType.Wizard => 3,
            ClassType.Paladin or ClassType.WarMage => 2,
            _ => 0
        };

        // Check if the number of selected spells is valid
        if (selectedSpells.Count != startingSpellCount)
        {
            throw new ArgumentException($"A {state.Class.Name} must select exactly {startingSpellCount} spells at character creation");
        }

        // Check that all spells are valid for the character's tradition and level
        //var availableSpells = await GetAvailableSpellsAsync(creationStateId);
        //var availableSpellNames = availableSpells.Select(s => s.Name).ToList();

        //foreach (var spell in selectedSpells)
        //{
        //    if (!availableSpellNames.Contains(spell.Name))
        //    {
        //        throw new ArgumentException($"Spell '{spell.Name}' is not available for this character");
        //    }
        //}

        // Set the selected spells
        state.SelectedSpells = selectedSpells;
        state.SpellcastingInfo.Spells = selectedSpells;

        UpdateCharacterCreationStateAsync(state);
    }

    public async Task<int> SelectEquipmentAsync(string creationStateId,
        List<Weapon>? selectedWeapons,
        List<Armor>? selectedArmor,
        List<AdventuringGear>? selectedItems)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);

        // Get available equipment options
        var equipmentOptions = EquipmentDatabase.EquipmentData;
        var weaponsCost = selectedWeapons.Sum(x => x.Value);
        var armorCost = selectedArmor.Sum(x => x.Value);
        var itemsCost = selectedItems.Sum(x => x.Value);
        var totalCost = weaponsCost + armorCost + itemsCost;
        var availableCopper = state.CopperCoins + state.SilverCoins * 10 + state.GoldCoins * 100;
        var remaining = availableCopper - totalCost;
        
        var (gold, silver, copper) = ConvertCopper((int)remaining);
        state.GoldCoins = gold;
        state.SilverCoins = silver;
        state.CopperCoins = copper;
        
        // Set the selected equipment
        state.SelectedWeapons = selectedWeapons;
        state.SelectedArmor = selectedArmor;
        state.SelectedItems = selectedItems;

        UpdateCharacterCreationStateAsync(state);
        
        return (int)remaining; // Return total remaining copper for potential UI feedback
    }
    private static (int Gold, int Silver, int Copper) ConvertCopper(int totalCopper)
    {
        var gold = totalCopper / 100;
        var remainder = totalCopper % 100;
        var silver = remainder / 10;
        var copper = remainder % 10;
        return (gold, silver, copper);
    }
    public async Task SetBackgroundInfoAsync(
        string creationStateId,
        string personalityTraits,
        string ideals,
        string bonds,
        string flaws)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);

        // Set the background information
        state.PersonalityTraits = personalityTraits;
        state.Ideals = ideals;
        state.Bonds = bonds;
        state.Flaws = flaws;

        UpdateCharacterCreationStateAsync(state);
    }

    public async Task<CharacterSheet> CompleteCharacterCreationAsync(string creationStateId)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);
#if DEBUG
        try
        {
            var json = JsonSerializer.Serialize(state.DraftCharacter);
            File.WriteAllText("character.json", json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
#endif
       var characterSheet = state.DraftCharacter;

        // Validate basic character info
        if (string.IsNullOrWhiteSpace(characterSheet.CharacterName))
            throw new ValidationException("Character name is required");
        if (string.IsNullOrWhiteSpace(characterSheet.PlayerName))
            throw new ValidationException("Player name is required");
        if (characterSheet.Race == null || string.IsNullOrEmpty(characterSheet.Race.Type.ToString()))
            throw new ValidationException("Character race is required");
        if (characterSheet.Class == null || string.IsNullOrEmpty(characterSheet.Class.Name))
            throw new ValidationException("Character class is required");
        if (characterSheet.Alignment == null)
            throw new ValidationException("Character alignment is required");

        // Validate attributes
        if (characterSheet.AttributeSet == null)
            throw new ValidationException("Attribute scores are required");
        foreach (var attribute in Enum.GetValues<RpgAttribute>())
        {
            if (characterSheet.AttributeSet[attribute] == 0)
                throw new ValidationException($"Attribute score for {attribute} is required");
        }

        // Validate skills
        if (!characterSheet.Skills.Any())
            throw new ValidationException("At least one skill selection is required");
        var requiredSkillCount = 5; // Per SkillSystem.md
        if (characterSheet.Skills.Count != requiredSkillCount)
            throw new ValidationException($"Exactly {requiredSkillCount} skills must be selected");

        // Validate spellcasting if applicable
        if (characterSheet.Class.HasSpellcasting)
        {
            if (characterSheet.Spellcasting == null)
                throw new ValidationException("Spellcasting information is required for spellcasting classes");
            if (characterSheet.Spellcasting.Spells == null || !characterSheet.Spellcasting.Spells.Any())
                throw new ValidationException("Spell selection is required for spellcasting classes");
        }

        // Validate equipment
        if (!characterSheet.Weapons.Any())
            throw new ValidationException("At least one weapon is required");
        if (!characterSheet.Inventory.Any())
            throw new ValidationException("Basic equipment is required");

        // Validate background information
        if (string.IsNullOrWhiteSpace(characterSheet.PersonalityTraits))
            throw new ValidationException("Personality traits are required");
        if (string.IsNullOrWhiteSpace(characterSheet.Ideals))
            throw new ValidationException("Character ideals are required");
        if (string.IsNullOrWhiteSpace(characterSheet.Bonds))
            throw new ValidationException("Character bonds are required");
        if (string.IsNullOrWhiteSpace(characterSheet.Flaws))
            throw new ValidationException("Character flaws are required");

        // All validations passed, mark as completed
        state.CompletedCharacter = characterSheet;
        state.CurrentStep = CharacterCreationStep.Completed;
        UpdateCharacterCreationStateAsync(state);

        return characterSheet;
    }

    // Add ValidationException class if not already defined elsewhere in the project
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public async Task AdvanceToNextStepAsync(string creationStateId)
    {
        var state = await GetCharacterCreationStateAsync(creationStateId);

        state.CurrentStep = state.CurrentStep switch
        {
            CharacterCreationStep.BasicInfo => CharacterCreationStep.AttributeScores,
            CharacterCreationStep.AttributeScores => CharacterCreationStep.Skills,
            CharacterCreationStep.Skills => state.Class.HasSpellcasting ? CharacterCreationStep.Spells : CharacterCreationStep.Equipment,
            CharacterCreationStep.Spells => CharacterCreationStep.Equipment,
            CharacterCreationStep.Equipment => CharacterCreationStep.Background,
            CharacterCreationStep.Background => CharacterCreationStep.Review,
            _ => state.CurrentStep
        };

        // Advance to the next step based on the current step

        UpdateCharacterCreationStateAsync(state);
    }
}