using System.Text.Json;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Plugins.NativePlugins;
using AIRpgAgents.GameEngine.Extensions;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.World;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIRpgAgents.Core.Agents;

public class NarrativeAgent : RpgAgent
{
    private readonly AppState _appState;
    public NarrativeAgent(AppState appState) : base("NarrativeAgent", "The Narrative Agent is responsible for orchestrating the game narrative", PromptHelper.ExtractPromptFromFile("NarrativeAgentPrompt.md"), new OpenAIPromptExecutionSettings() { Temperature = 0.9, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    {
        _appState = appState;
        RequiredArguments["GAME_SETTING"] = JsonSerializer.Deserialize<WorldState>(FileHelper.ExtractFromAssembly<string>("TestWorld.json")).ToMarkdown();
        var characters = JsonSerializer.Deserialize<List<CharacterState>>(FileHelper.ExtractFromAssembly<string>("TestCharacters.json"));
        RequiredArguments["ACTIVE_CHARACTERS"] = string.Join("\n", characters.Select(x => x.CharacterSheet.PrimaryDetailsMarkdown()));
    }
    public NarrativeAgent(string promptTemplate, AppState appState) : base("NarrativeAgent", "The Narrative Agent is responsible for orchestrating the game narrative", promptTemplate, new OpenAIPromptExecutionSettings() { Temperature = 0.9, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    {
        RequiredArguments["GAME_SETTING"] = JsonSerializer.Deserialize<WorldState>(FileHelper.ExtractFromAssembly<string>("TestWorld.json")).ToMarkdown();
        var characters = JsonSerializer.Deserialize<List<CharacterState>>(FileHelper.ExtractFromAssembly<string>("TestCharacters.json"));
        RequiredArguments["ACTIVE_CHARACTERS"] = string.Join("\n", characters.Select(x => x.CharacterSheet.PrimaryDetailsMarkdown()));
        _appState = appState;
    }
    protected override Kernel CreateKernel()
    {
        var grokEndpoint = Config.GrokEndpoint;
        var grokApiKey = Config.GrokApiKey;
        var hasGrok = !string.IsNullOrEmpty(grokEndpoint) && !string.IsNullOrEmpty(grokApiKey);
        var kernelBuilder = Kernel.CreateBuilder();
        if (hasGrok)
        {
            kernelBuilder.AddOpenAIChatCompletion("grok-2-latest", new Uri(grokEndpoint), grokApiKey);
        }
        else
        {
            kernelBuilder.AddOpenAIChatCompletion("gpt-4o", Config.OpenAIApiKey!);

        }
        kernelBuilder.Services.AddSingleton(_appState);
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<NarrativePlugin>();
        kernel.ImportPluginFromType<GameSystemPlugin>();
        return kernel;
    }
}