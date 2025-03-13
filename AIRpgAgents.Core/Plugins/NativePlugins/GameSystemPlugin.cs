using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.GameEngine.Extensions;
using AIRpgAgents.GameEngine.Rules;
using Microsoft.SemanticKernel;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

public class GameSystemPlugin
{
    [KernelFunction, Description("Get information about the RPG Attribute system")]
    public string GetAttributeSystemInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("AttributeSystem.md");
        return content;
    }

    [KernelFunction, Description("Get information about the RPG Alignment system")]
    public string GetAlignmentSystemInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("AlignmentSystem.md");
        return content;
    }

    [KernelFunction, Description("Get information about the RPG Character Classes")]
    public string GetCharacterClassesInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("CharacterClasses.md");
        return content;
    }

    [KernelFunction, Description("Get information about the RPG Character Races")]
    public string GetCharacterRacesInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("CharacterRaces.md");
        return content;
    }

    [KernelFunction, Description("Get information about the RPG Combat system")]
    public string GetCombatSystemInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("CombatSystem.md");
        return content;
    }

    [KernelFunction, Description("Get information about the RPG Equipment system")]
    public string GetEquipmentSystemInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("EquipmentSystem.md");
        return content;
    }

    [KernelFunction, Description("Get information about the RPG Magic systems")]
    public string GetMagicSystemsInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("MagicSystems.md");
        return content;
    }

    [KernelFunction, Description("Get information about the RPG Skill system")]
    public string GetSkillSystemInfo()
    {
        var content = FileHelper.ExtractFromAssembly<string>("SkillSystem.md");
        return content;
    }
}