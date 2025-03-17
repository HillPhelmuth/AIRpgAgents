using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.Monsters;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

public class MonsterGeneratorPlugin
{
    
    private const string ChallengeRatingDescription = """
                                                      (Optional)
                                                      The difficulty level of the monster. Can be a single value like "10" or a comma seprated list string like: "4,10" to get multiple difficulty levels
                                                      """;
    [KernelFunction, Description("Get a list of available monsters")]
    public async Task<string> GetAvailableMonsters(Kernel kernel, [Description(ChallengeRatingDescription)]string challengeRating = "")
    {
        var kernelClone = kernel.Clone();
        kernelClone.Plugins.Clear();
        var plugin = await kernelClone.ImportPluginFromOpenApiAsync("DnDMonstersPlugin",
            PromptHelper.ExtractStreamFromFile("DnDApi.json"));
        var args = new KernelArguments() { ["challenge_rating"] = challengeRating };
        var monsterFunction = plugin["Monsters"];
        var result = await monsterFunction.InvokeAsync(kernelClone, args);
        return result.ToString();
    }
    [KernelFunction, Description("Get Monster Details")]
    public async Task<string> GetMonsterDetails(Kernel kernel, string monsterName)
    {
        monsterName = monsterName.Contains(" ") ? monsterName.Replace(" ", "-").ToLower() : monsterName.ToLower();
        var kernelClone = kernel.Clone();
        kernelClone.Plugins.Clear();
        var plugin = await kernelClone.ImportPluginFromOpenApiAsync("DnDMonstersPlugin",
            PromptHelper.ExtractStreamFromFile("DnDApi.json"));
        var args = new KernelArguments() { ["name"] = monsterName };
        var monsterFunction = plugin["Monster"];
        var result = await monsterFunction.InvokeAsync(kernelClone, args);
        var monsterConverterFunction =
            KernelFunctionYaml.FromPromptYaml(PromptHelper.ExtractPromptFromFile("ConvertMonsterFunction.yaml"));
        var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(RpgMonster) };
        var convertedMonster = await monsterConverterFunction.InvokeAsync(kernelClone, new KernelArguments() { ["monster"] = result.ToString() });
        return result.ToString();
    }
}