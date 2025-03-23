using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Models.Events;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.Enums;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using Xunit;

namespace AIRpgAgents.Core.Tests.Services;

public class CharacterCreationServiceTests
{
    private readonly ICharacterCreationService _service;
    private string _testCreationStateId;
    private bool _eventFired;

    // Remove the problematic code that tries to get the state ID from the event args
    // Instead, capture the state ID directly in the event handler

    public CharacterCreationServiceTests()
    {
        _service = new CharacterCreationService();
        _service.CharacterCreationStateChanged += (sender, args) =>
        {
            _eventFired = true;
            _testCreationStateId = args.CharacterCreationState.Id; // Use the Id from CharacterCreationState
        };
        _eventFired = false;
    }


    [Fact]
    public async Task InitializeCharacterCreationAsync_SetsBasicInfo_AndAdvancesToAttributeScores()
    {
        // Arrange
        var characterName = "Test Character";
        var playerName = "Test Player";
        var race = new Race { Type = RaceType.Human };
        var characterClass = new CharacterClass
        {
            Type = ClassType.Warrior,
            Name = "Warrior",
            HasSpellcasting = false,
            PrimaryAttributes = ["Swords"]
        };
        var alignment = new AlignmentValue(MoralAxis.Good, EthicalAxis.Passionate);

        // Act
        await _service.InitializeCharacterCreationAsync(
            characterName, playerName, race, characterClass, alignment);

        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.NotNull(state);
        Assert.Equal(characterName, state.CharacterName);
        Assert.Equal(playerName, state.PlayerName);
        Assert.Equal(race.Type, state.Race.Type);
        Assert.Equal(characterClass.Name, state.Class.Name);
        Assert.Equal(alignment.MoralAlignment, state.Alignment.MoralAlignment);
        Assert.Equal(alignment.EthicalAlignment, state.Alignment.EthicalAlignment);
        Assert.Equal(CharacterCreationStep.AttributeScores, state.CurrentStep);
        Assert.True(_eventFired);
    }

    [Fact]
    public async Task SelectAttributeScoreTypeAsync_WithStandardArray_ReturnsCorrectScores()
    {
        // Arrange
        await SetupTestCharacter();

        // Act
        var scores = await _service.SelectAttributeScoreTypeAsync(_testCreationStateId, AttributeGenerationMethod.StandardArray);
        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.NotNull(scores);
        Assert.Equal(5, scores.Count);
        Assert.Contains(15, scores);
        Assert.Contains(14, scores);
        Assert.Contains(12, scores);
        Assert.Contains(11, scores);
        Assert.Contains(9, scores);
        Assert.Equal(AttributeGenerationMethod.StandardArray, state.AttributeMethod);
    }

    [Fact]
    public async Task AssignAttributeScores_AppliesAttributesAndRacialModifiers()
    {
        // Arrange
        await SetupTestCharacter();
        await _service.SelectAttributeScoreTypeAsync(_testCreationStateId, AttributeGenerationMethod.StandardArray);
            
        var attributeAssignments = new Dictionary<RpgAttribute, int>
        {
            { RpgAttribute.Might, 15 },
            { RpgAttribute.Agility, 14 },
            { RpgAttribute.Vitality, 12 },
            { RpgAttribute.Wits, 11 },
            { RpgAttribute.Presence, 9 }
        };

        // Act
        await _service.AssignAttributeScores(_testCreationStateId, attributeAssignments);
        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.Equal(15, state.AttributeSet[RpgAttribute.Might]);
        Assert.Equal(14, state.AttributeSet[RpgAttribute.Agility]);
        Assert.Equal(12, state.AttributeSet[RpgAttribute.Vitality]);
        Assert.Equal(11, state.AttributeSet[RpgAttribute.Wits]);
        Assert.Equal(9, state.AttributeSet[RpgAttribute.Presence]);
    }

    [Fact]
    public async Task ApplyHumanBonusAttributeAsync_IncreasesSelectedAttribute()
    {
        // Arrange
        await SetupTestCharacter();
        await _service.SelectAttributeScoreTypeAsync(_testCreationStateId, AttributeGenerationMethod.StandardArray);
            
        var attributeAssignments = new Dictionary<RpgAttribute, int>
        {
            { RpgAttribute.Might, 15 },
            { RpgAttribute.Agility, 14 },
            { RpgAttribute.Vitality, 12 },
            { RpgAttribute.Wits, 11 },
            { RpgAttribute.Presence, 9 }
        };
        await _service.AssignAttributeScores(_testCreationStateId, attributeAssignments);
            
        // Act
        await _service.ApplyHumanBonusAttributeAsync(_testCreationStateId, RpgAttribute.Might);
        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.Equal(16, state.AttributeSet[RpgAttribute.Might]);
        Assert.Equal(RpgAttribute.Might, state.HumanBonusAttribute);
    }

    [Fact]
    public async Task SelectSkillsAsync_WithValidSkills_AddsSkillsToCharacter()
    {
        // Arrange
        await SetupTestCharacter();
        var skills = new List<string> { "Acrobatics", "Stealth", "Perception", "Persuasion", "Athletics" };

        // Update available skills in state
        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);
        state.AvailableSkills = skills;
        _service.UpdateCharacterCreationState(state);

        // Act
        await _service.SelectSkillsAsync(_testCreationStateId, skills);
        state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.Equal(5, state.SelectedSkills.Count);
        foreach (var skill in skills)
        {
            Assert.Contains(state.SelectedSkills, s => s.Name == skill);
        }
    }

    [Fact]
    public async Task SelectSkillsAsync_WithTooFewSkills_ThrowsArgumentException()
    {
        // Arrange
        await SetupTestCharacter();
        var skills = new List<string> { "Acrobatics", "Stealth", "Perception" }; // Only 3 skills

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SelectSkillsAsync(_testCreationStateId, skills));
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_AdvancesCorrectly()
    {
        // Arrange
        await SetupTestCharacter();
        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);
        state.CurrentStep = CharacterCreationStep.AttributeScores;
        _service.UpdateCharacterCreationState(state);

        // Act
        await _service.AdvanceToNextStepAsync(_testCreationStateId);
        state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.Equal(CharacterCreationStep.Skills, state.CurrentStep);

        // Advance again
        await _service.AdvanceToNextStepAsync(_testCreationStateId);
        state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // For non-spellcasting class, should skip to Equipment
        Assert.Equal(CharacterCreationStep.Equipment, state.CurrentStep);
    }

    [Fact]
    public async Task SelectEquipmentAsync_CalculatesRemainingCurrency()
    {
        // Arrange
        await SetupTestCharacter();
        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);
        state.SilverCoins = 100; // 1000 copper
        _service.UpdateCharacterCreationState(state);

        var weapons = new List<Weapon> { new Weapon { Value = 150, Name = "Longsword" } };  // 150 copper
        var armor = new List<Armor> { new Armor { Value = 100, Name = "Leather Armor" } };      // 100 copper
        var items = new List<AdventuringGear> { new AdventuringGear { Value = 50, Name = "Lantern"} };  // 50 copper

        // Act
        var remainingCopper = await _service.SelectEquipmentAsync(_testCreationStateId, weapons, armor, items);
        state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.Equal(700, remainingCopper); // 1000 - 150 - 100 - 50 = 700
        Assert.Equal(7, state.GoldCoins);   // 600 copper = 6 gold
        Assert.Equal(0, state.SilverCoins); // 0 silver remaining
        Assert.Equal(0, state.CopperCoins); // 0 copper remaining
        Assert.Equal(CharacterCreationService.MatchedWeapons(weapons), state.SelectedWeapons);
        Assert.Equal(CharacterCreationService.MatchedArmor(armor), state.SelectedArmor);
        Assert.Equal(CharacterCreationService.AdventuringGears(items), state.SelectedItems);
    }

    [Fact]
    public void ConvertCopper_IsTotalCorrect()
    {
        // Arrange
        var copper = 123;
        // Act
        var (gold, silver, remainingCopper) = CharacterCreationService.ConvertCopper(copper);

        // Assert
        Assert.Equal(1, gold);
        Assert.Equal(2, silver);
        Assert.Equal(3, remainingCopper);
    }

    [Fact]
    public async Task CompleteCharacterCreationAsync_WithMissingInformation_ThrowsValidationException()
    {
        // Arrange
        await SetupTestCharacter();

        // Act & Assert
        await Assert.ThrowsAsync<CharacterCreationService.ValidationException>(() =>
            _service.CompleteCharacterCreationAsync(_testCreationStateId));
    }

    [Fact]
    public async Task CompleteCharacterCreationAsync_WithCompleteInformation_ReturnsCharacterSheet()
    {
        // Arrange
        await SetupTestCharacter();
            
        // Set up a complete character
        var state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);
        state.AttributeSet = new AttributeSet(new Dictionary<RpgAttribute, int>
        {
            { RpgAttribute.Might, 15 },
            { RpgAttribute.Agility, 14 },
            { RpgAttribute.Vitality, 12 },
            { RpgAttribute.Wits, 11 },
            { RpgAttribute.Presence, 9 }
        });
            
        state.SelectedSkills =
        [
            new Skill("Acrobatics", RpgAttribute.Agility),
            new Skill("Stealth", RpgAttribute.Agility),
            new Skill("Perception", RpgAttribute.Wits),
            new Skill("Persuasion", RpgAttribute.Presence),
            new Skill("Athletics", RpgAttribute.Might)
        ];
            
        state.SelectedWeapons = [new Weapon { Name = "Sword" }];
        state.SelectedArmor = [new Armor { Name = "Leather Armor" }];
        state.SelectedItems = [new AdventuringGear { Name = "Backpack" }];
            
        state.PersonalityTraits = "Brave and bold";
        state.Ideals = "Justice for all";
        state.Bonds = "Sworn to protect my homeland";
        state.Flaws = "Too trusting";
            
        // Update the draft character to reflect these selections
        state.DraftCharacter.AttributeSet = state.AttributeSet;
        state.DraftCharacter.Skills = state.SelectedSkills;
        state.DraftCharacter.Weapons = state.SelectedWeapons;
        state.DraftCharacter.Armor = state.SelectedArmor;
        state.DraftCharacter.Gear = state.SelectedItems;
        state.DraftCharacter.PersonalityTraits = state.PersonalityTraits;
        state.DraftCharacter.Ideals = state.Ideals;
        state.DraftCharacter.Bonds = state.Bonds;
        state.DraftCharacter.Flaws = state.Flaws;
            
        _service.UpdateCharacterCreationState(state);

        // Act
        var characterSheet = await _service.CompleteCharacterCreationAsync(_testCreationStateId);
        state = await _service.GetCharacterCreationStateAsync(_testCreationStateId);

        // Assert
        Assert.NotNull(characterSheet);
        Assert.Equal(CharacterCreationStep.Completed, state.CurrentStep);
        Assert.NotNull(state.CompletedCharacter);
        Assert.Equal("Test Character", characterSheet.CharacterName);
    }

    private async Task SetupTestCharacter()
    {
        if (string.IsNullOrEmpty(_testCreationStateId))
        {
            var characterName = "Test Character";
            var playerName = "Test Player";
            var race = new Race { Type = RaceType.Human }; // Remove Name property
            var characterClass = new CharacterClass
            {
                Type = ClassType.Warrior, // Use Warrior instead of Fighter
                Name = "Warrior",
                HasSpellcasting = false,
                PrimaryAttributes = ["Swords"]
            };
            var alignment = new AlignmentValue(MoralAxis.Good, EthicalAxis.Reasonable); // Use Reasonable instead of Lawful

            await _service.InitializeCharacterCreationAsync(
                characterName, playerName, race, characterClass, alignment);

            // Capture the state ID from the event
            _service.CharacterCreationStateChanged += (sender, args) =>
            {
                _testCreationStateId = args.CharacterCreationState.Id; // Use the Id from CharacterCreationState
            };
        }
    }
}