using System.Text.Json;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Plugins.NativePlugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AIRpgAgents.Core.Agents;

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

    public AgentData ToAgentData()
    {
        var agentData = new AgentData
        {
            Name = Name,
            Description = Description,
            PromptTemplate = PromptTemplate
        };
        var pluginFunctions = Kernel.Plugins.SelectMany(x => x);
        agentData.FunctionInfo = pluginFunctions.Select(fn => new AgentFunctionInfo($"{fn.PluginName}-{fn.Name}", fn.Description,
            fn.Metadata.Parameters.ToDictionary(x => x.Name,
                y => (object)y.Schema!)));
        return agentData;
    }

}

public class AgentData
{
    [JsonProperty("id")]
    public string Id => Name ?? string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? PromptTemplate { get; set; }
    public IEnumerable<AgentFunctionInfo> FunctionInfo { get; set; } = [];

}
public record AgentFunctionInfo(string Name, string Description, Dictionary<string, object> Parameters);