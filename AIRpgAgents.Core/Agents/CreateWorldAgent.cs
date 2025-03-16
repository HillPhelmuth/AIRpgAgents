using System.Text.Json;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Plugins.NativePlugins;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.World;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIRpgAgents.Core.Agents;

public class CreateWorldAgent : RpgAgent
{
    private AutoInvokeFilterEvents _autoInvokeFilter;
    public event Action<WorldState>? WorldCreated;
    private readonly CosmosService _cosmos;
    public CreateWorldAgent(CosmosService cosmos) : base("WorldAgent", "Create a new world",PromptHelper.ExtractPromptFromFile("WorldBuilderPrompt.md"), new OpenAIPromptExecutionSettings() { Temperature = 1.1, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
    {
        RequiredArguments = new KernelArguments() { ["name"] = Name };
        _autoInvokeFilter = new AutoInvokeFilterEvents();
        _autoInvokeFilter.OnCompleted += HandleCompleted;
        _cosmos = cosmos;
    }
    public CreateWorldAgent(CosmosService cosmos, string promptTemplate): base("WorldAgent", "Create a new world", promptTemplate, new OpenAIPromptExecutionSettings() { Temperature = 1.1, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    {
        RequiredArguments = new KernelArguments() { ["name"] = Name };
        _autoInvokeFilter = new AutoInvokeFilterEvents();
        _autoInvokeFilter.OnCompleted += HandleCompleted;
        _cosmos = cosmos;
    }

    private void HandleCompleted(object? sender, AutoFunctionInvocationContext e)
    {
        if (e.Function.Name == "BuildWorld")
        {
            var world = JsonSerializer.Deserialize<WorldState>(e.Result.ToString());
            WorldCreated?.Invoke(world);
        }
    }

    protected override Kernel CreateKernel()
    {
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o", Config.OpenAIApiKey!);
        kernelBuilder.Services.AddSingleton(_cosmos);
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<WorldBuilderPlugin>();
        kernel.ImportPluginFromType<GameSystemPlugin>();
        kernel.AutoFunctionInvocationFilters.Add(AutoInvokeFilter);
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        return kernel;
    }
}