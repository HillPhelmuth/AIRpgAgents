using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Plugins.NativePlugins;
using AIRpgAgents.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SkPluginComponents;
using SkPluginComponents.Models;

namespace AIRpgAgents.Core.Agents;

public class CreateCharacterAgent : RpgAgent
{
    private readonly AppState _appState;
    private readonly CosmosService _cosmosService;
    private readonly CharacterCreationState _characterCreationState;
    private readonly ICharacterCreationService _charCreateService;
    private readonly RollDiceService _rollDiceService;
    private readonly AutoInvokeFilter _autoInvokeFilter = new();
    
    public CreateCharacterAgent(AppState appState, CosmosService cosmosService, ICharacterCreationService charCreateService, RollDiceService rollDiceService) : base("CharacterAgent", "Bad Ass Character Creator", PromptHelper.ExtractPromptFromFile("CreateCharacterPrompt.md"), new OpenAIPromptExecutionSettings() { Temperature = 0.9, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    {
        RequiredArguments = new KernelArguments() { ["name"] = $"{Name} a {Description}" };
        _appState = appState;
        _cosmosService = cosmosService;
        _characterCreationState = new CharacterCreationState(appState.Player.Name ?? Guid.NewGuid().ToString());
        _charCreateService = charCreateService;
        _rollDiceService = rollDiceService;
    }
    public CreateCharacterAgent(AppState appState, CosmosService cosmosService, ICharacterCreationService charCreateService, RollDiceService rollDiceService, string promptTemplate) : base("CharacterAgent", "Bad Ass Character Creator", promptTemplate, new OpenAIPromptExecutionSettings() { Temperature = 0.9, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    {
        RequiredArguments = new KernelArguments() { ["name"] = Name };
        _appState = appState;
        _cosmosService = cosmosService;
        _characterCreationState = new CharacterCreationState(appState.Player.Name ?? Guid.NewGuid().ToString()); ;
        _charCreateService = charCreateService;
        _rollDiceService = rollDiceService;
    }


    protected override Kernel CreateKernel()
    {
        var client = DelegateHandlerFactory.GetHttpClientWithHandler<LoggingHandler>();
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o", Config.OpenAIApiKey!);
        kernelBuilder.Services.AddSingleton(_characterCreationState);
        kernelBuilder.Services.AddSingleton(_appState);
        kernelBuilder.Services.AddSingleton(_charCreateService);
        kernelBuilder.Services.AddSingleton(_rollDiceService);
        //kernelBuilder.Services.AddScoped<ICharacterService, CharacterService>();
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<DieRollerPlugin>();
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        var createCharacterPlugin =
            new CreateCharacterPlugin(kernel.Services.GetService<ICharacterCreationService>()!, _cosmosService, _appState);
        kernel.ImportPluginFromObject(createCharacterPlugin, nameof(CreateCharacterPlugin));
        kernel.ImportPluginFromType<GameSystemPlugin>();
        return kernel;
    }

    public override async IAsyncEnumerable<string> InvokeStreamingAsync(ChatHistory chatHistory)
    {
        var chatCompletion = Kernel.GetRequiredService<IChatCompletionService>();
        var templateConfig = new PromptTemplateConfig(PromptTemplate);
        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var systemPrompt = await promptTemplateFactory.Create(templateConfig).RenderAsync(Kernel, RequiredArguments);
        Console.WriteLine($"Hydrated System Prompt\n----------------------------------------------------------------------------\n{systemPrompt}\n----------------------------------------------------------------------------\n");
        chatHistory.AddSystemMessage(systemPrompt);
        await foreach (var response in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, ExecutionSettings, Kernel))
        {
            if (string.IsNullOrEmpty(response?.Content)) continue;
            yield return response.Content;
        }
    }
}