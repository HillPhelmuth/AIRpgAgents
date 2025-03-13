using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.WorldState;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace AIRpgAgents.Core.Plugins.NativePlugins;

public class WorldBuilderPlugin
{
    //private readonly IWorldStateManager _worldStateManager;
    private KernelFunction _createWorldFunction;
    public WorldBuilderPlugin()
    {
        //_worldStateManager = worldStateManager;
        var createWorldyaml = PromptHelper.ExtractPromptFromFile("BuildWorldFunction.yaml");
        _createWorldFunction = KernelFunctionYaml.FromPromptYaml(createWorldyaml);
    }

    [KernelFunction, Description("Build the RPG world for the current game")]
    public async Task<string> BuildWorld(Kernel kernel, [Description("The user input to inspire the world-building process")] string input)
    {
        var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(WorldState) };
        var kernelArgs = new KernelArguments(settings) { ["input"] = input };
        var response = await _createWorldFunction.InvokeAsync(kernel, kernelArgs);
        return response.ToString();
    }
}