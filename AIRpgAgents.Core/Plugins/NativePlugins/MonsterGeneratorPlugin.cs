using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.Monsters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

public class MonsterGeneratorPlugin
{
    
    private const string ChallengeRatingDescription = """
                                                      (Optional)
                                                      The difficulty level of the monster (1-15). Can be a single value like "10" or a comma seprated list string like: "4,10" to get multiple difficulty levels.
                                                      """;
    [KernelFunction, Description("Get a list of available monsters with some basic information")]
    public string GetAvailableMonsters(Kernel kernel, [Description(ChallengeRatingDescription)]string challengeRating = "")
    {
       var monsterMarkdowns = DndMonsterService.GetMonstersBasicInfo(challengeRating);
       return string.Join("\n", monsterMarkdowns);
    }
    [KernelFunction, Description("Get Monster Details")]
    public async Task<string> GetMonsterDetails(Kernel kernel, string monsterName)
    {
        var result = DndMonsterService.GetMonsterByName(monsterName);
        var monsterConverterFunction =
            KernelFunctionYaml.FromPromptYaml(PromptHelper.ExtractPromptFromFile("ConvertMonsterFunction.yaml"));
        var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(RpgMonster) };
        var convertedMonster = await monsterConverterFunction.InvokeAsync(kernel, new KernelArguments(settings) { ["monster"] = result });

        return JsonSerializer.Deserialize<RpgMonster>(convertedMonster.ToString())?.ToMarkdown() ?? "Oooppss!";
    }
    [KernelFunction, Description("Create a unique monster specific to the narrative or campaign")]
    public async Task<string> CreateUniqueMonster(Kernel kernel,[Description("Name of the Monster")] string name, [Description("Detailed Description of the monster")] string description, [Description("Monster Difficulty Level")] int difficultyLevel = 5, [Description("Additional required details (e.g. special attacks)")] string additionalDetails = "")
    {
        var rpgState = kernel.Services.GetRequiredService<RpgCampaign>();
        var kernelClone = kernel.Clone();
        var args = new KernelArguments(new OpenAIPromptExecutionSettings {ResponseFormat = typeof(RpgMonster)}) { ["monsterName"] = name, ["monsterDescription"] = description, ["difficultyLevel"] = difficultyLevel, ["monsterAdditionalDetails"] = additionalDetails };
        var monsterFunction = KernelFunctionYaml.FromPromptYaml(PromptHelper.ExtractPromptFromFile("CreateMonsterFunction.yaml"));
        var result = await monsterFunction.InvokeAsync(kernelClone, args);
        var monster = JsonSerializer.Deserialize<RpgMonster>(result.ToString());
        var combatEncounter = rpgState.CombatEncounters.Find(x => x.IsActive);
        combatEncounter.MonsterEncounter.Monsters.Add(monster);
        return result.ToString();
    }
}