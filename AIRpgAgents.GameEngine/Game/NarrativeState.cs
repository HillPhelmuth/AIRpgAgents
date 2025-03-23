namespace AIRpgAgents.GameEngine.Game;

public class NarrativeState
{
    public List<string> GlobalNarrative { get; set; } = new List<string>();
    public List<string> InternalNarrative { get; set; } = new List<string>();
    public Dictionary<string, List<string>> CharacterNarratives { get; set; } = new();
}