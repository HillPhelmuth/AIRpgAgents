using System.Text;
using AIRpgAgents.GameEngine.Rules;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;
using CosmosIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using System;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class CharacterSheet
{
    // Basic Info
    [CosmosName("id")]
    public string Id => $"{PlayerName}-{CharacterName}";

    public string PartitionKey => Id;
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
    public AttributeSet AttributeSet { get; } = new();

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
    public int ArmorClass { get; set; }
    public int Initiative { get; set; }
    public int Speed { get; set; } = 30;

    // Equipment
    public List<Weapon> Weapons { get; set; } = [];
    [System.Text.Json.Serialization.JsonIgnore]
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

    /// <summary>
    /// Creates a markdown representation of the character's primary details in a hierarchical format.
    /// </summary>
    /// <returns>A string containing the markdown representation of the character sheet.</returns>
    public string PrimaryDetailsMarkdown()
    {
        var markdown = new StringBuilder();
        var missingDetail = false;
        // Character name and basic info
        if (!string.IsNullOrEmpty(CharacterName))
        {
            markdown.AppendLine($"# {CharacterName}");
        }
        else
        {
            markdown.AppendLine("# Character Name");
            markdown.AppendLine("No character name selected.");
            markdown.AppendLine("**Begin the character creation process.**");
            missingDetail = true;
        }
        markdown.AppendLine($"**Level {Level}**");

        if (!string.IsNullOrEmpty(Deity))
        {
            markdown.AppendLine($"**Deity:** {Deity}");
        }

        // Alignment
        markdown.AppendLine();
        markdown.AppendLine("## Alignment");
        markdown.AppendLine($"* **Moral Axis:** {Alignment.MoralAlignment}");
        markdown.AppendLine($"* **Ethical Axis:** {Alignment.EthicalAlignment}");

        // Race details
        markdown.AppendLine();
        markdown.AppendLine("## Race");
        markdown.AppendLine($"* **Type:** {Race.Type}");
        if (Race.Type == RaceType.None)
        {
            markdown.AppendLine("No character race selected");
            markdown.AppendLine("**Please ask the player to provide Race selection.**");
        }

        // Class details
        markdown.AppendLine();
        markdown.AppendLine("## Class");
        markdown.AppendLine($"* **Type:** {Class.Type}");
        if (Class.Type == ClassType.None)
        {
            markdown.AppendLine("No character class selected");
            markdown.AppendLine("**Please ask the player to provide Class selection.**");
        }
        // Attributes
        markdown.AppendLine();
        markdown.AppendLine("## Attributes");
        if (AttributeSet.All(x => x.Value == AttributeSystem.MinAttributeValue))
        {
            markdown.AppendLine("No attributes selected.");
            if (!missingDetail)
            {
                markdown.AppendLine("**Please proceed to select attributes before finalizing the character.**");
            }
            missingDetail = true;
        }
        else
        {
            foreach (var item in AttributeSet)
            {
                var value = item.Value;
                int modifier = GetAttributeModifier(item.Key);
                string modifierStr = modifier >= 0 ? $"+{modifier}" : modifier.ToString();
                markdown.AppendLine($"* **{item.Key}:** {value} ({modifierStr})");
            }
        }

        // Skills
        if (Skills?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Skills");

            foreach (var skill in Skills)
            {
                markdown.AppendLine($"* **{skill.Name}:** Rank {skill.Rank}");
                markdown.AppendLine($"  * Associated Attribute: {skill.AssociatedAttribute}");

            }
        }
        else
        {
            markdown.AppendLine();
            markdown.AppendLine("## Skills");
            markdown.AppendLine("No skills selected.");
            if (!missingDetail)
            {
                markdown.AppendLine("**Please proceed select skills before finalizing the character.**");
            }
            missingDetail = true;
        }

        if (Inventory?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Inventory");
            foreach (var item in Inventory)
            {
                markdown.AppendLine($"* **{item.Name}**");
                markdown.AppendLine($"  * **Description:** {item.Description}");
            }
        }
        else
        {
            markdown.AppendLine();
            markdown.AppendLine("## Inventory");
            markdown.AppendLine("No items in inventory.");
            if (!missingDetail)
            {
                markdown.AppendLine("**Please proceed to select starting equipment before finalizing the character.**");
            }
            missingDetail = true;
        }
        return markdown.ToString();
    }
}