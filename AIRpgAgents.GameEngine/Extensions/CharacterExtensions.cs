using System.Text;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;

namespace AIRpgAgents.GameEngine.Extensions;

public static class CharacterExtensions
{
    /// <summary>
    /// Creates a markdown representation of the character's primary details in a hierarchical format.
    /// </summary>
    /// <returns>A string containing the markdown representation of the character sheet.</returns>
    public static string PrimaryDetailsMarkdown(this CharacterSheet characterSheet)
    {
        var markdown = new StringBuilder();
        var missingDetail = false;
        // Character name and basic info
        markdown.AppendLine($"# Character Id: {characterSheet.Id}");
        if (!string.IsNullOrEmpty(characterSheet.CharacterName))
        {
            markdown.AppendLine($"## {characterSheet.CharacterName}");
        }
        else
        {
            markdown.AppendLine("## Character Name");
            markdown.AppendLine("No character name selected.");
            markdown.AppendLine("**Begin the character creation process.**");
            missingDetail = true;
        }
        markdown.AppendLine($"**Level {characterSheet.Level}**");

        if (!string.IsNullOrEmpty(characterSheet.Deity))
        {
            markdown.AppendLine($"**Deity:** {characterSheet.Deity}");
        }

        // Alignment
        markdown.AppendLine();
        markdown.AppendLine("### Alignment");
        markdown.AppendLine("|Axis|Value|");
        markdown.AppendLine("|---|---|");
        markdown.AppendLine($"|Moral|{characterSheet.Alignment.MoralAlignment}|");
        markdown.AppendLine($"|Ethical|{characterSheet.Alignment.EthicalAlignment}|");

        // Race details
        markdown.AppendLine();
        markdown.AppendLine("### Race");
        markdown.AppendLine("|Type|Description|");
        markdown.AppendLine("|---|---|");
        markdown.AppendLine($"|{characterSheet.Race.Type}|{characterSheet.Race.Description}|");
        if (characterSheet.Race.Type == RaceType.None)
        {
            markdown.AppendLine("No character race selected");
            markdown.AppendLine("**Please ask the player to provide Race selection.**");
        }

        // Class details
        markdown.AppendLine();
        markdown.AppendLine("### Class");
        markdown.AppendLine("|Type|Description|");
        markdown.AppendLine("|---|---|");
        markdown.AppendLine($"|{characterSheet.Class.Type}|{characterSheet.Class.Description}|");
        if (characterSheet.Class.Type == ClassType.None)
        {
            markdown.AppendLine("No character class selected");
            markdown.AppendLine("**Please ask the player to provide Class selection.**");
        }
        
        // Attributes
        markdown.AppendLine();
        markdown.AppendLine("### Attributes");
        if (characterSheet.AttributeSet.All(x => x.Value == AttributeSystem.MinAttributeValue))
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
            markdown.AppendLine("|Attribute|Value|Modifier|");
            markdown.AppendLine("|---|---|---|");
            foreach (var item in characterSheet.AttributeSet)
            {
                var value = item.Value;
                int modifier = characterSheet.GetAttributeModifier(item.Key);
                string modifierStr = modifier >= 0 ? $"+{modifier}" : modifier.ToString();
                markdown.AppendLine($"|{item.Key}|{value}|{modifierStr}|");
            }
        }

        // Skills
        if (characterSheet.Skills?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("### Skills");
            markdown.AppendLine("|Skill|Rank|Associated Attribute|");
            markdown.AppendLine("|---|---|---|");

            foreach (var skill in characterSheet.Skills)
            {
                markdown.AppendLine($"|{skill.Name}|{skill.Rank}|{skill.AssociatedAttribute}|");
            }
        }
        else
        {
            markdown.AppendLine();
            markdown.AppendLine("### Skills");
            markdown.AppendLine("No skills selected.");
            if (!missingDetail)
            {
                markdown.AppendLine("**Please proceed select skills before finalizing the character.**");
            }
            missingDetail = true;
        }

        if (characterSheet.Inventory?.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("### Inventory");
            markdown.AppendLine("|Item|Description|");
            markdown.AppendLine("|---|---|");
            foreach (var item in characterSheet.Inventory)
            {
                markdown.AppendLine($"|{item.Name}|{item.Description}|");
            }
        }
        else
        {
            markdown.AppendLine();
            markdown.AppendLine("### Inventory");
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