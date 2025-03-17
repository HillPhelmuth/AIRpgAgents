using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIRpgAgents.Core.Models;

/// <summary>
/// Represents the current state of a character during the creation process
/// </summary>
public class CharacterCreationState(string id)
{
   
    public string Id { get; set; } = id; /*= Guid.NewGuid().ToString();*/
    
    // Character creation state tracking
    public CharacterCreationStep CurrentStep { get; set; } = CharacterCreationStep.BasicInfo;
    public AttributeGenerationMethod AttributeMethod { get; set; } = AttributeGenerationMethod.StandardArray;
    
    // Draft character sheet that will be built up during creation
    public CharacterSheet DraftCharacter { get; set; } = new();
    
    // Creation-specific properties that don't exist in CharacterSheet
    public List<int> RolledScores { get; set; } = [];
    public RpgAttribute? HumanBonusAttribute { get; set; } // For the Human racial +1 bonus choice
    public List<string> AvailableSkills { get; set; } = [];
    public List<Spell> SelectedSpells
    {
        get => DraftCharacter.Spellcasting?.Spells ?? [];
        set
        {
            DraftCharacter.Spellcasting ??= new();
            DraftCharacter.Spellcasting.Spells = value;
        }
    }

    // Equipment being selected during creation (before adding to inventory)
    public List<Weapon> SelectedWeapons
    {
        get => DraftCharacter.Weapons;
        set => DraftCharacter.Weapons = value;
    }

    public List<Armor> SelectedArmor
    {
        get => DraftCharacter.Armor;
        set => DraftCharacter.Armor = value;
    }

    public List<AdventuringGear> SelectedItems
    {
        get => DraftCharacter.Gear;
        set => DraftCharacter.Gear = value;
    }

    // Property to access the completed character
    // This keeps backward compatibility with existing code
    public CharacterSheet CompletedCharacter 
    { 
        get => IsCompleted ? DraftCharacter : null;
        set => DraftCharacter = value;
    }
    
    public bool IsCompleted => CurrentStep == CharacterCreationStep.Completed;
    
    // Helper properties to access commonly used CharacterSheet properties
    // These maintain backward compatibility with existing code
    public string CharacterName 
    { 
        get => DraftCharacter.CharacterName; 
        set => DraftCharacter.CharacterName = value; 
    }
    
    public string PlayerName 
    { 
        get => DraftCharacter.PlayerName; 
        set => DraftCharacter.PlayerName = value; 
    }
    
    public Race Race 
    { 
        get => DraftCharacter.Race; 
        set => DraftCharacter.Race = value; 
    }
    
    public CharacterClass Class 
    { 
        get => DraftCharacter.Class; 
        set => DraftCharacter.Class = value; 
    }
    
    public AlignmentValue Alignment 
    { 
        get => DraftCharacter.Alignment; 
        set => DraftCharacter.Alignment = value; 
    }
    
    public string Deity 
    { 
        get => DraftCharacter.Deity; 
        set => DraftCharacter.Deity = value; 
    }
    
    public AttributeSet AttributeSet 
    { 
        get => DraftCharacter.AttributeSet; 
        set => DraftCharacter.AttributeSet = value; 
    }
    
    public List<Skill> SelectedSkills 
    { 
        get => DraftCharacter.Skills; 
        set => DraftCharacter.Skills = value; 
    }
    
    public SpellcastingInfo SpellcastingInfo 
    { 
        get => DraftCharacter.Spellcasting; 
        set => DraftCharacter.Spellcasting = value; 
    }
    
    public int GoldCoins 
    { 
        get => DraftCharacter.GoldCoins; 
        set => DraftCharacter.GoldCoins = value; 
    }
    
    public int SilverCoins 
    { 
        get => DraftCharacter.SilverCoins; 
        set => DraftCharacter.SilverCoins = value; 
    }
    
    public int CopperCoins 
    { 
        get => DraftCharacter.CopperCoins; 
        set => DraftCharacter.CopperCoins = value; 
    }
    
    public string PersonalityTraits 
    { 
        get => DraftCharacter.PersonalityTraits; 
        set => DraftCharacter.PersonalityTraits = value; 
    }
    
    public string Ideals 
    { 
        get => DraftCharacter.Ideals; 
        set => DraftCharacter.Ideals = value; 
    }
    
    public string Bonds 
    { 
        get => DraftCharacter.Bonds; 
        set => DraftCharacter.Bonds = value; 
    }
    
    public string Flaws 
    { 
        get => DraftCharacter.Flaws; 
        set => DraftCharacter.Flaws = value; 
    }
    
    /// <summary>
    /// Finalizes the character creation by transferring selected equipment to the character sheet
    /// </summary>
    public void FinalizeCharacter()
    {
        // Transfer selected equipment to character inventory
        if (SelectedWeapons.Count > 0)
        {
            DraftCharacter.Weapons.AddRange(SelectedWeapons);
        }
        
        if (SelectedItems.Count > 0)
        {
            DraftCharacter.            Inventory.AddRange(SelectedItems);
        }
        
        if (SelectedArmor != null)
        {
            DraftCharacter.Armor = SelectedArmor;
        }
        
        
        
        CurrentStep = CharacterCreationStep.Completed;
    }
}

/// <summary>
/// Represents the steps in the character creation process
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum CharacterCreationStep
{
    [Description("Name, race, class, alignment")]
    BasicInfo,          // Name, race, class, alignment
    [Description("Roll or assign attribute scores")]
    AttributeScores,    // Roll or assign attribute scores
    [Description("Select starting skills")]
    Skills,             // Select starting skills
    [Description("Select spells (if applicable)")]
    Spells,             // Select spells (if applicable)
    [Description("Select starting equipment")]
    Equipment,          // Select starting equipment
    [Description("Add background information and traits")]
    Background,         // Add background information and traits
    [Description("Review character before finalizing")]
    Review,             // Review character before finalizing
    [Description("Character creation completed")]
    Completed           // Character creation completed
}

/// <summary>
/// Method used for generating attribute scores
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
[CosmosConverter(typeof(StringEnumConverter))]
public enum AttributeGenerationMethod
{
    [Description("Roll 4d6 drop lowest")]
    RollAttributes,     // Roll 4d6 drop lowest
    [Description("Use the standard array (15, 14, 13, 12, 10)")]
    StandardArray       // Use the standard array (15, 14, 13, 12, 10)
}
