using System.Text;
using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Rules;
using CosmosName = Newtonsoft.Json.JsonPropertyAttribute;
using CosmosIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

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
    public int Level { get; set; } = 1;
    public AlignmentValue Alignment { get; set; } = new();
    public string Deity { get; set; } = "";
        
    // Stats
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
    public Dictionary<string, Skill> Skills { get; set; } = new();
        
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

    /// <summary>
    /// Creates a markdown representation of the character's primary details in a hierarchical format.
    /// </summary>
    /// <returns>A string containing the markdown representation of the character sheet.</returns>
    public string PrimaryDetailsMarkdown()
    {
        var markdown = new StringBuilder();

        // Character name and basic info
        markdown.AppendLine($"# {CharacterName}");
        markdown.AppendLine($"**Level {Level} {Race.Type} {Class.Type}**");
        
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
        
        if (!string.IsNullOrEmpty(Race.Description))
        {
            markdown.AppendLine($"* **Description:** {Race.Description}");
        }
        
        if (Race.AttributeModifiers?.Count > 0)
        {
            markdown.AppendLine("* **Attribute Modifiers:**");
            foreach (var modifier in Race.AttributeModifiers)
            {
                markdown.AppendLine($"  * {modifier.Key}: {(modifier.Value >= 0 ? "+" : "")}{modifier.Value}");
            }
        }
        
        if (Race.Traits?.Count > 0)
        {
            markdown.AppendLine("* **Racial Traits:**");
            foreach (var trait in Race.Traits)
            {
                markdown.AppendLine($"  * {trait.Name}");
            }
        }
        
        // Class details
        markdown.AppendLine();
        markdown.AppendLine("## Class");
        markdown.AppendLine($"* **Type:** {Class.Type}");
        
        if (!string.IsNullOrEmpty(Class.Description))
        {
            markdown.AppendLine($"* **Description:** {Class.Description}");
        }
        
        if (Class.PrimaryAttributes?.Count > 0)
        {
            markdown.AppendLine($"* **Primary Attributes:** {string.Join(", ", Class.PrimaryAttributes)}");
        }
        
        // Attributes
        markdown.AppendLine();
        markdown.AppendLine("## Attributes");
        foreach (RpgAttribute attribute in Enum.GetValues(typeof(RpgAttribute)))
        {
            int value = AttributeSet[attribute];
            int modifier = GetAttributeModifier(attribute);
            string modifierStr = modifier >= 0 ? $"+{modifier}" : modifier.ToString();
            
            markdown.AppendLine($"* **{attribute}:** {value} ({modifierStr})");
        }
        
        // Skills
        if (Skills?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("## Skills");
            
            foreach (var skill in Skills)
            {
                markdown.AppendLine($"* **{skill.Key}:** Rank {skill.Value.Rank}");
                markdown.AppendLine($"  * Associated Attribute: {skill.Value.AssociatedAttribute}");
                if (skill.Value.Bonus != 0)
                {
                    string bonusStr = skill.Value.Bonus >= 0 ? $"+{skill.Value.Bonus}" : skill.Value.Bonus.ToString();
                    markdown.AppendLine($"  * Bonus: {bonusStr}");
                }
            }
        }
        
        return markdown.ToString();
    }
}