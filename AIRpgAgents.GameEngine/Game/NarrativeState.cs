namespace AIRpgAgents.GameEngine.Game;

public class NarrativeState
{
    public List<string> GlobalNarrative { get; set; } = [];
    public List<string> InternalNarrative { get; set; } = [];
    public Dictionary<string, List<string>> CharacterNarratives { get; set; } = new();
}