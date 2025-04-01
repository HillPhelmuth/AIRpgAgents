using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Plugins.NativePlugins;
using AIRpgAgents.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SkPluginComponents.Models;

namespace AIRpgAgents.Core.Agents;

public class CombatEncounterAgent : RpgAgent
{
    private readonly CombatService _combatService;
    private readonly RpgState _campaign;
    private readonly RollDiceService _rollDiceService;
    public event Action<string, string>? UpdateSummaries;
    public event Action<CombatEncounter>? UpdateCombatEncounter;
    private readonly CombatEncounter _combatEncounter;
    public CombatEncounterAgent(CombatService combatService, RollDiceService rollDiceService, RpgState rpgState, CombatEncounter combatEncounter) : base("CombatEncounterAgent", "Manages encounters between players and monsters", PromptHelper.ExtractPromptFromFile("CombatAgentPrompt.md"), new OpenAIPromptExecutionSettings(){FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
    {
        _combatService = combatService;
        _campaign = rpgState;
        _combatEncounter = combatEncounter;
        _campaign.ActiveCampaign.CombatEncounters.Add(_combatEncounter);
        _rollDiceService = rollDiceService;
    }

    protected override Kernel CreateKernel()
    {
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o", Config.OpenAIApiKey!);
        kernelBuilder.Services.AddSingleton(_combatService);
        kernelBuilder.Services.AddSingleton(_campaign);
        kernelBuilder.Services.AddSingleton(_rollDiceService);
        kernelBuilder.Services.AddSingleton(_combatEncounter);
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<CombatSystemPlugin>();
        kernel.ImportPluginFromType<GameSystemPlugin>();
        kernel.AutoFunctionInvocationFilters.Add(AutoInvokeFilter);
        return kernel;
    }
    public override async IAsyncEnumerable<string> InvokeStreamingAsync(ChatHistory chatHistory)
    {
        var chatCompletion = Kernel.GetRequiredService<IChatCompletionService>();
        var requiredArgument = _campaign.ActiveCampaign.CombatEncounters.LastOrDefault(x => x.IsActive)?.GetCombatSummary();
        RequiredArguments["combatEncounterSummary"] = requiredArgument;
        var combatantsInfo = _campaign.ActiveCampaign.CombatEncounters.LastOrDefault(x => x.IsActive)?.GetCombatantsInfo();
        RequiredArguments["combatants"] = combatantsInfo;
        UpdateSummaries?.Invoke(requiredArgument, combatantsInfo);
        Console.WriteLine($"Update Summaries invoked from {nameof(CombatEncounterAgent)}");
        var templateConfig = new PromptTemplateConfig(PromptTemplate);
        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var systemPrompt = await promptTemplateFactory.Create(templateConfig).RenderAsync(Kernel, RequiredArguments);
        Console.WriteLine($"System Prompt {Name}:\n\n{systemPrompt}\n\n");
        chatHistory.AddSystemMessage(systemPrompt);
        await foreach (var response in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, ExecutionSettings, Kernel))
        {
            if (string.IsNullOrEmpty(response?.Content)) continue;
            yield return response.Content;
        }
    }
}