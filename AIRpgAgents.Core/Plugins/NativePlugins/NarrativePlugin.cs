using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.Game;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

public class NarrativePlugin
{
    private NarrativeState _narrativeState = new();

    [KernelFunction, Description("Add a new narrative event to a specific character's story")]
    public string AddCharacterNarrative(Kernel kernel,
        [Description("The Game Master's internal reasoning")]
        string internalNarrative,
        [Description("The character's name to add narrative for")]
        string characterName,
        [Description("The narrative text to add")]
        string narrativeText,
        [Description("Whether to also display this narrative to all characters")]
        bool displayToAllCharacters = false)
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        _narrativeState = appState.NarrativeState ?? new();
        
        if (!string.IsNullOrEmpty(internalNarrative))
        {
            AddToInternalNarrative($"Character: {characterName}\nReasoning: {internalNarrative}");
        }

        if (!_narrativeState.CharacterNarratives.ContainsKey(characterName))
        {
            _narrativeState.CharacterNarratives[characterName] = new List<string>();
        }
            
        _narrativeState.CharacterNarratives[characterName].Add(narrativeText);
            
        if (displayToAllCharacters)
        {
            AddToNarrative($"[{characterName}] {narrativeText}");
        }
        appState.NarrativeState = _narrativeState;

        return $"Added narrative for {characterName}: {narrativeText}";
    }
        
    [KernelFunction, Description("Get the complete narrative history for a specific character")]
    public string GetCharacterNarrative(Kernel kernel,
        [Description("The Game Master's internal reasoning")]
        string internalNarrative,
        [Description("The character's name to retrieve narrative for")]
        string characterName)
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        _narrativeState = appState.NarrativeState ?? new();

        if (!string.IsNullOrEmpty(internalNarrative))
        {
            AddToInternalNarrative($"Retrieving narrative for {characterName}\nReasoning: {internalNarrative}");
        }

        if (!_narrativeState.CharacterNarratives.ContainsKey(characterName) || 
            !_narrativeState.CharacterNarratives[characterName].Any())
        {
            return $"No narrative exists for character: {characterName}";
        }
            
        var narrativeBuilder = new StringBuilder();
        narrativeBuilder.AppendLine($"# Narrative for {characterName}");
            
        foreach (var entry in _narrativeState.CharacterNarratives[characterName])
        {
            narrativeBuilder.AppendLine($"- {entry}");
        }
        appState.NarrativeState = _narrativeState;
        return narrativeBuilder.ToString();
    }
        
    [KernelFunction, Description("Add a new narrative event to the global game story")]
    public string AddGlobalNarrative(Kernel kernel,
        [Description("The Game Master's internal reasoning")]
        string internalNarrative,
        [Description("The narrative text to add")]
        string narrativeText)
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        _narrativeState = appState.NarrativeState ?? new();

        if (!string.IsNullOrEmpty(internalNarrative))
        {
            AddToInternalNarrative($"Adding global narrative\nReasoning: {internalNarrative}");
        }

        AddToNarrative(narrativeText);
        appState.NarrativeState = _narrativeState;
        return $"Added to global narrative: {narrativeText}";
    }

    private void AddToNarrative(string narrativeText)
    {
        _narrativeState.GlobalNarrative.Add(narrativeText);
    }

    private void AddToInternalNarrative(string internalText)
    {
        _narrativeState.InternalNarrative.Add(internalText);
    }

    [KernelFunction, Description("Get the complete global narrative history")]
    public async Task<string> GetGlobalNarrativeSummary(Kernel kernel,
        [Description("The Game Master's internal reasoning")] string internalNarrative = "")
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        _narrativeState = appState.NarrativeState ?? new();

        if (!string.IsNullOrEmpty(internalNarrative))
        {
            AddToInternalNarrative($"Generating narrative summary\nReasoning: {internalNarrative}");
        }

        if (!_narrativeState.GlobalNarrative.Any())
        {
            return "No global narrative exists yet.";
        }

        string globalNarrativeText = string.Join(Environment.NewLine, _narrativeState.GlobalNarrative);
        var response = await kernel.InvokePromptAsync<string>(
            "Create an outline that summarizes the global narrative:\n\n##Narrative {{ $narrative }}",
            new KernelArguments() { ["narrative"] = globalNarrativeText });
            
        return $"# Global Narrative\n\n{response}";
    }
        
    [KernelFunction, Description("Update or replace the current global narrative")]
    public string UpdateGlobalNarrative(Kernel kernel,
        [Description("The Game Master's internal reasoning")]
        string internalNarrative,
        [Description("The new narrative text to replace the current narrative")]
        string narrativeText)
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        _narrativeState = appState.NarrativeState ?? new();

        if (!string.IsNullOrEmpty(internalNarrative))
        {
            AddToInternalNarrative($"Updating global narrative\nReasoning: {internalNarrative}");
        }

        //_narrativeState.GlobalNarrative.Clear();
        _narrativeState.GlobalNarrative.Add(narrativeText);
        appState.NarrativeState = _narrativeState;
        return "Global narrative updated";
    }

    [KernelFunction, Description("Get the Game Master's internal narrative")]
    public string GetInternalNarrative(Kernel kernel)
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        _narrativeState = appState.NarrativeState ?? new();

        if (!_narrativeState.InternalNarrative.Any())
        {
            return "No internal narrative exists yet.";
        }

        string internalNarrativeText = string.Join(Environment.NewLine + Environment.NewLine, _narrativeState.InternalNarrative);
        return $"# Game Master's Internal Narrative\n\n{internalNarrativeText}";
    }
        
    [KernelFunction, Description("Save the current narrative state to the database")]
    public async Task<string> SaveNarrativeState(Kernel kernel)
    {
        var cosmosService = kernel.Services.GetRequiredService<CosmosService>();
        // await cosmosService.SaveNarrativeStateAsync(_narrativeState);
        return "Narrative state saved";
    }
}

// Define a class to hold narrative state for serialization
public class NarrativeState
{
    public List<string> GlobalNarrative { get; set; } = new List<string>();
    public List<string> InternalNarrative { get; set; } = new List<string>();
    public Dictionary<string, List<string>> CharacterNarratives { get; set; } = new();
}