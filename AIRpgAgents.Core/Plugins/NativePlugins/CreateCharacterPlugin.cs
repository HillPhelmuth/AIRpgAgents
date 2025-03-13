using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.Enums;
using AIRpgAgents.GameEngine.Extensions;
using AIRpgAgents.GameEngine.Helpers;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

public class CreateCharacterPlugin
{
    private readonly ICharacterCreationService _characterCreationService;
    private readonly CosmosService _cosmosService;
    private CharacterCreationState? _characterCreationState;
    public CreateCharacterPlugin(ICharacterCreationService characterCreationService, CosmosService cosmosService, AppState appState)
    {
        _characterCreationService = characterCreationService;
        _cosmosService = cosmosService;
        //_characterCreationService.InitializeCharacterCreation(appState.Player.Name, new CharacterCreationState(appState.Player.Name));
    }

    #region Character Creation Steps

    [KernelFunction, Description("Start a new character creation process with basic information")]
    public async Task<string> StartCharacterCreation(Kernel kernel,
        [Description("Character name")] string characterName,
        [Description("Player name")] string playerName,
        [Description("Character race (e.g. Human, Elf, Dwarf)")] RaceType race,
        [Description("Character class (e.g. Fighter, Wizard, Rogue)")] ClassType characterClass,
        [Description("Moral alignment (Good, Neutral, Evil)")] MoralAxis moralAlignment = MoralAxis.Neutral,
        [Description("Ethical alignment (Passionate, Neutral, Reasonable)")] EthicalAxis ethicalAlignment = EthicalAxis.Neutral,
        [Description("Character deity or higher power")] string deity = "")
    {
        var alignment = new AlignmentValue(moralAlignment, ethicalAlignment);
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        //_characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        //var createdState = _characterCreationState;
        var charClass = CharacterClasses.AllClasses.Find(x => x.Type == characterClass) ?? CharacterClasses.Warrior;
        await creationService.InitializeCharacterCreationAsync(characterName, appState.Player.Name, race.GetDetails(), charClass, alignment, deity);
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        return $"Character creation step {createdState.CurrentStep} completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Get the current character creation state")]
    public async Task<string> GetCharacterCreationState(Kernel kernel)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        return $"Character creation step {createdState.CurrentStep} completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Player has the option to set character attribute scores using standard array or rolling. Once decided, use this method to set character attributes")]
    public async Task<string> SelectAttributeScoreMethod(Kernel kernel, [Description("Method for generating attributes (RollAttributes or StandardArray)")] AttributeGenerationMethod method)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        var values = await _characterCreationService.SelectAttributeScoreTypeAsync(createdState.Id, method);
        if (method == AttributeGenerationMethod.StandardArray)
            return $"User can assign the following values to the attribute of their choice:{string.Join(", ", values)}";
        return
            "Invoke the `RollDice` for 4 6-sided dice. Then drop the lowest roll. Do this 5 times. Provide the 5 totals for the user to attribute to the character attributes";

    }
    [KernelFunction, Description("Set character attribute scores for each attribute")]
    public async Task<string> SetAttributeScores(Kernel kernel, [Description("Might score")] int might, [Description("Wits score")] int wits, [Description("Agility score")] int agility, [Description("Vitality score")] int vitality, [Description("Presence score")] int presence)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        //if (createdState.RolledScores.Count == 0)
        //{
        //    return "Attribute scores have not been determined yet. Use the SelectAttributeScoreMethod method to generate scores first.";
        //}
        var attributeScores = new Dictionary<RpgAttribute, int>
        {
            {RpgAttribute.Might, might},
            {RpgAttribute.Wits, wits},
            {RpgAttribute.Agility, agility},
            {RpgAttribute.Vitality, vitality},
            {RpgAttribute.Presence, presence}
        };
        await _characterCreationService.AssignAttributeScores(createdState.Id, attributeScores);
        return $"Character creation step SetAttributeScores completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Apply +1 bonus attribute for human characters (only if Human is the selected Race)")]
    public async Task<string> ApplyHumanBonusAttribute(Kernel kernel,

        [Description("Attribute to apply the +1 bonus to (Might, Agility, etc.)")] RpgAttribute attribute)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        await _characterCreationService.ApplyHumanBonusAttributeAsync(createdState.Id, attribute);
        return $"Character creation step {createdState.CurrentStep} completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Get available skills for this character based on class")]
    public string GetAvailableSkills()
    {
        return JsonSerializer.Serialize(Skill.AllSkills, new JsonSerializerOptions() { WriteIndented = true });
    }

    [KernelFunction, Description("Select skills for the character")]
    public async Task<string> SelectSkills(Kernel kernel, [Description("List of 5 skill names to select")] List<string> selectedSkills)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        await _characterCreationService.SelectSkillsAsync(createdState.Id, selectedSkills);
        return $"Character creation step {createdState.CurrentStep} completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Get available spells for spellcasting classes")]
    public async Task<string> GetAvailableSpells(Kernel kernel,
        [Description("Magic Type")] MagicTradition magicType)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        //Console.WriteLine(JsonSerializer.Serialize(_characterCreationState.DraftCharacter, new JsonSerializerOptions(){WriteIndented = true}));
        return JsonSerializer.Serialize(Spells.GetSpells.Traditions.Find(x => x.Name == magicType)?.Spells.Where(x => x.Band == Band.Lower).ToList()!, new JsonSerializerOptions() { WriteIndented = true });
    }

    [KernelFunction, Description("Select spells for spellcasting classes")]
    public async Task<string> SelectSpells(Kernel kernel, [Description("List of spells to select")] List<Spell> selectedSpells)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        await _characterCreationService.SelectSpellsAsync(createdState.Id, selectedSpells);
        return $"Character creation step {createdState.CurrentStep} completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Get available equipment options based on class")]
    public async Task<string> GetAvailableEquipment(Kernel kernel)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var availableEquipment = JsonSerializer.Serialize(EquipmentDatabase.EquipmentData,
            new JsonSerializerOptions() { WriteIndented = true });
        var availableCopper = _characterCreationState.CopperCoins + _characterCreationState.SilverCoins * 10 + _characterCreationState.GoldCoins * 100;
        return $"Available Coins: {availableCopper} Copper \nEquipment for purchace:\n{availableEquipment}";
    }

    [KernelFunction, Description("Select starting equipment for the character")]
    public async Task<string> SelectEquipment(
        Kernel kernel,
        [Description("List of weapons to select")] List<Weapon>? selectedWeapons = null,
        [Description("Armor to select (can be null)")] List<Armor>? selectedArmor = null,
        [Description("List of inventory items to select")] List<AdventuringGear>? selectedItems = null)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        var remainingCopper = await _characterCreationService.SelectEquipmentAsync(
            createdState.Id,
            selectedWeapons,
            selectedArmor,
            selectedItems);
        if (remainingCopper < 0)
        {
            return "Not enough coins to purchase selected equipment.";
        }
        return $"Equipment purchase sucessful. Remaining copper pieces: {remainingCopper}";
    }

    [KernelFunction, Description("Set character background information")]
    public async Task<string> SetBackgroundInfo(
        Kernel kernel,
        [Description("Personality traits")] string personalityTraits,
        [Description("Character ideals")] string ideals,
        [Description("Character bonds")] string bonds,
        [Description("Character flaws")] string flaws)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        await _characterCreationService.SetBackgroundInfoAsync(
            createdState.Id,
            personalityTraits,
            ideals,
            bonds,
            flaws);
        return $"Character creation step {createdState.CurrentStep} completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Advance to the next step of character creation")]
    public async Task<string> AdvanceToNextStep(Kernel kernel)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        await _characterCreationService.AdvanceToNextStepAsync(createdState.Id);
        return $"Character creation step {createdState.CurrentStep} completed for {createdState.Id}.";
    }

    [KernelFunction, Description("Complete the character creation process and generate final character sheet")]
    public async Task<string> CompleteCharacterCreation(Kernel kernel)
    {
        var creationService = kernel.Services.GetRequiredService<ICharacterCreationService>();
        var appState = kernel.Services.GetRequiredService<AppState>();
        _characterCreationState ??= await creationService.GetCharacterCreationStateAsync(appState.Player.Name!);
        var createdState = _characterCreationState;
        var sheet = await _characterCreationService.CompleteCharacterCreationAsync(createdState.Id);
        await _cosmosService.SaveCharacterAsync(appState.Player.Id, sheet);
        return $"Character creation completed for {createdState.Id}. Character sheet saved. Tell the player they're ready for an adventure! Fuck yeah!";
    }
    [KernelFunction]
    public string CurrentStep(Kernel kernel)
    {
        var createdState = _characterCreationState;
        var currentStep = createdState?.CurrentStep ?? CharacterCreationStep.BasicInfo;
        return $"**{currentStep}**: {currentStep.GetDescription()}";
    }

    #endregion

    #region Legacy Methods

    //[KernelFunction, Description("Create a complete character in one step (legacy method)")]
    //public async Task<CharacterSheet> CreateCharacter(
    //    [Description("Character name")] string characterName,
    //    [Description("Player name")] string playerName,
    //    [Description("Character race (e.g. Human, Elf, Dwarf)")] Race race,
    //    [Description("Character class (e.g. Fighter, Wizard, Rogue)")] CharacterClass characterClass,
    //    [Description("Moral alignment (Good, Neutral, Evil)")] MoralAxis moralAlignment = MoralAxis.Neutral,
    //    [Description("Ethical alignment (Passionate, Neutral, Reasonable)")] EthicalAxis ethicalAlignment = EthicalAxis.Neutral,
    //    [Description("Character deity or higher power")] string deity = "")
    //{
    //    var alignment = new AlignmentValue(moralAlignment, ethicalAlignment);

    //    return await _characterService.CreateCharacterAsync(
    //        characterName, 
    //        playerName, 
    //        race, 
    //        characterClass,
    //        alignment,
    //        deity);
    //}

    //[KernelFunction, Description("Get an existing character sheet")]
    //public async Task<CharacterSheet> GetCharacter(
    //    [Description("Character ID")] string characterId)
    //{
    //    return await _characterService.GetCharacterAsync(characterId);
    //}

    //[KernelFunction, Description("Level up a character")]
    //public async Task<CharacterSheet> LevelUpCharacter(
    //    [Description("Character ID")] string characterId)
    //{
    //    var character = await _characterService.GetCharacterAsync(characterId);
    //    character.Level += 1;

    //    // Update derived stats based on level
    //    int vitalityMod = character.GetAttributeModifier("vitality");
    //    int hpGain = Math.Max(1, DieType.D8.RollDie() + vitalityMod);
    //    character.MaxHP += hpGain;
    //    character.CurrentHP = character.MaxHP;

    //    return await _characterService.UpdateCharacterAsync(character);
    //}

    //[KernelFunction, Description("Add currency to character")]
    //public async Task<CharacterSheet> AddCurrency(
    //    [Description("Character ID")] string characterId,
    //    [Description("Gold coins")] int gold = 0,
    //    [Description("Silver coins")] int silver = 0,
    //    [Description("Copper coins")] int copper = 0)
    //{
    //    var character = await _characterService.GetCharacterAsync(characterId);
    //    character.GoldCoins += gold;
    //    character.SilverCoins += silver;
    //    character.CopperCoins += copper;

    //    // Handle currency conversion (e.g., 100 copper = 1 silver, 100 silver = 1 gold)
    //    int copperOverflow = character.CopperCoins / 100;
    //    character.CopperCoins %= 100;
    //    character.SilverCoins += copperOverflow;

    //    int silverOverflow = character.SilverCoins / 100;
    //    character.SilverCoins %= 100;
    //    character.GoldCoins += silverOverflow;

    //    return await _characterService.UpdateCharacterAsync(character);
    //}

    #endregion
}