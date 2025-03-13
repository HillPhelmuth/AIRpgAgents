using System.Text.Json;
using AIRpgAgents.Core.Plugins.NativePlugins;
using AIRpgAgents.GameEngine.WorldState;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIRpgAgents.Core.Models;

public class RpgAgent(string name, string description, string promptTemplate, PromptExecutionSettings executionSettings)
{
    private Kernel? _kernel;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string PromptTemplate { get; set; } = promptTemplate;
    protected AutoInvokeFilter AutoInvokeFilter { get; set; } = new();
    public Kernel Kernel
    {
        get => _kernel ??= CreateKernel();
        set => _kernel = value;
    }
    public PromptExecutionSettings ExecutionSettings { get; set; } = executionSettings;
    public KernelArguments RequiredArguments { get; set; } = new();

    protected virtual Kernel CreateKernel()
    {
        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o", Config.OpenAIApiKey!);
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<GameSystemPlugin>();
        return kernel;
    }
    public virtual async IAsyncEnumerable<string> InvokeStreamingAsync(ChatHistory chatHistory)
    {
        var chatCompletion = Kernel.GetRequiredService<IChatCompletionService>();
        var templateConfig = new PromptTemplateConfig(PromptTemplate);
        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var systemPrompt = await promptTemplateFactory.Create(templateConfig).RenderAsync(Kernel, RequiredArguments);
        chatHistory.AddSystemMessage(systemPrompt);
        await foreach (var response in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, ExecutionSettings, Kernel))
        {
            if (string.IsNullOrEmpty(response?.Content)) continue;
            yield return response.Content;
        }
    }

}
public class CreateWorldAgent : RpgAgent
{
    private AutoInvokeFilterEvents _autoInvokeFilter;
    public event Action<WorldState>? WorldCreated;
    public CreateWorldAgent() : base("WorldAgent", "Create a new world",PromptHelper.ExtractPromptFromFile("WorldBuilderPrompt.md"), new OpenAIPromptExecutionSettings() { Temperature = 1.1, FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
    {
        RequiredArguments = new KernelArguments() { ["name"] = Name };
        _autoInvokeFilter = new AutoInvokeFilterEvents();
        _autoInvokeFilter.OnCompleted += HandleCompleted;

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
        var kernel = kernelBuilder.Build();
        kernel.ImportPluginFromType<WorldBuilderPlugin>();
        kernel.ImportPluginFromType<GameSystemPlugin>();
        kernel.AutoFunctionInvocationFilters.Add(AutoInvokeFilter);
        kernel.AutoFunctionInvocationFilters.Add(_autoInvokeFilter);
        return kernel;
    }
}