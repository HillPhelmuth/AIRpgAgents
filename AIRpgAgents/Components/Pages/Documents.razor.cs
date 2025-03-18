using AIRpgAgents.GameEngine.Extensions;

namespace AIRpgAgents.Components.Pages;
public partial class Documents
{
    private record MarkdownDocument(string Title, string FileName)
    {
        public string ExpandTitle => $"Expand {Title}";
        public string CollapseTitle => $"Collapse {Title}";
    }

    private readonly List<MarkdownDocument> _documents = new()
    {
        
        new MarkdownDocument("Character Classes", "CharacterClasses.md"),
        new MarkdownDocument("Character Races", "CharacterRaces.md"),
        new MarkdownDocument("Attribute System", "AttributeSystem.md"),
        new MarkdownDocument("Alignment System", "AlignmentSystem.md"),
        new MarkdownDocument("Skills System", "SkillSystem.md"),
        new MarkdownDocument("Magic System", "MagicSystems.md"),
        new MarkdownDocument("Combat System", "CombatSystem.md"),
        new MarkdownDocument("Equipment System", "EquipmentSystem.md")
    };

    private static string GetMarkdownDocument(string docName)
    {
        return FileHelper.ExtractFromAssembly<string>(docName);
    }
}
