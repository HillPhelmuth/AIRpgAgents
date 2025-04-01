using System.Text;
using AIRpgAgents.GameEngine.Rules;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;
using CosmosIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using System;
using System.Text.Json.Serialization;
using System.Reflection.Emit;
using System.Security.Claims;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class CharacterSheet
{
    private int _armorClass = 10;

    // Basic Info
    [CosmosName("id")]
    public string Id => $"{PlayerName}-{CharacterName}";

    public string PlayerName { get; set; } = "";

    public string CharacterName { get; set; } = "";

    public Race Race { get; set; } = new();
    public CharacterClass Class { get; set; } = new();
    public List<ClassAbility> ActiveClassAbilities
    {
        get
        {
           return Class.Abilities.Where(x => x.LevelGained <= Level).ToList();
        }
        
    }

    public int Level { get; set; } = 1;
    public AlignmentValue Alignment { get; set; } = new();
    public string Deity { get; set; } = "";

    // Stats
    [CosmosIgnore]
    [JsonIgnore]
    public AttributeSet AttributeSet { get; set; } = new();

    public int Might
    {
        get => AttributeSet[RpgAttribute.Might];
        set => AttributeSet[RpgAttribute.Might] = value;
    }

    public int Agility
    {
        get => AttributeSet[RpgAttribute.Agility];
        set => AttributeSet[RpgAttribute.Agility] = value;
    }
    public int Wits
    {
        get => AttributeSet[RpgAttribute.Wits];
        set => AttributeSet[RpgAttribute.Wits] = value;
    }
    public int Vitality
    {
        get => AttributeSet[RpgAttribute.Vitality];
        set => AttributeSet[RpgAttribute.Vitality] = value;
    }
    public int Presence
    {
        get => AttributeSet[RpgAttribute.Presence];
        set => AttributeSet[RpgAttribute.Presence] = value;
    }
    public List<Skill> Skills { get; set; } = [];

    // Combat Stats
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int CurrentMP { get; set; }
    public int MaxMP { get; set; }

    public int ArmorClass
    {
        get
        {
            return _armorClass + Armor.Where(x => x.IsEquipped).Sum(x => x.ArmorBonus);
        }
        set => _armorClass = value;
    }

    public int InitiativeBonus => AttributeSystem.CalculateModifier(Wits) + AttributeSystem.CalculateModifier(Agility);
    public int Speed { get; set; } = 30;

    // Equipment
    public List<Weapon> Weapons { get; set; } = [];
    [JsonIgnore]
    [CosmosIgnore]
    public List<InventoryItem> Inventory
    {
        get
        {
            var result = new List<InventoryItem>();
            result.AddRange(Weapons);
            result.AddRange(Armor);
            result.AddRange(Gear);
            return result;
        }
    }

    public List<Armor> Armor { get; set; } = [];
    public List<AdventuringGear> Gear { get; set; } = [];

    // Spellcasting
    public SpellcastingInfo? Spellcasting { get; set; }

    // Currency
    public int GoldCoins { get; set; }
    public int SilverCoins { get; set; } = 100;
    public int CopperCoins { get; set; }

    // Background Info
    public string PersonalityTraits { get; set; } = "";
    public string Ideals { get; set; } = "";
    public string Bonds { get; set; } = "";
    public string Flaws { get; set; } = "";
    public string Notes { get; set; } = "";

    // Calculate attribute modifiers
    public int GetAttributeModifier(RpgAttribute attributeName)
    {
        return AttributeSystem.CalculateModifier(AttributeSet[attributeName]);
        //int score = GetAttributeScore(attributeName);
        //return (int)Math.Floor((score - 10) / 2.0);
    }

    public int GetAttributeScore(RpgAttribute attributeName)
    {
        return AttributeSet[attributeName];

    }

    // Calculate saving throws
    public int GetSavingThrow(RpgAttribute attributeName)
    {
        return GetAttributeModifier(attributeName);
        // Add proficiency or other bonuses if needed
    }

}