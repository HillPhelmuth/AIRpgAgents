using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.Extensions;
using AIRpgAgents.GameEngine.Monsters;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;

namespace AIRpgAgents.Core.Services;

public class DndMonsterService
{
    private static List<DndMonster>? _allMonsters;
    private static List<DndMonster> AllDndMonsters => _allMonsters ??=
        JsonSerializer.Deserialize<List<DndMonster>>(FileHelper.ExtractFromAssembly<string>("MonsterDndCache.json")!)!;

    public static DndMonster? GetMonsterByName(string name)
    {
        var monster = AllDndMonsters.FirstOrDefault(m => m.Name?.ToLower() == name.ToLower());
        return monster;
    }
    public static IEnumerable<DndMonster> GetMonstersByChallengeRating(params double[] challengeRatings)
    {
        if (challengeRatings.Length == 0) return AllDndMonsters;
        return AllDndMonsters.Where(m => challengeRatings.Contains(m.ChallengeRating));
    }

    public static IEnumerable<string> GetMonstersBasicInfo(string challengeRating = "")
    {
        var challengeRatings = challengeRating.Split(',').Where(x => double.TryParse(x, out _)).Select(double.Parse).ToArray() ?? [];

        return GetMonstersByChallengeRating(challengeRatings).Select(m => m.ToBasicInfoMarkdown());
    }
    public static async Task<MonsterEncounter> CreateRandomMonsterEncounter(int count, params double[] challengeRatings)
    {
        var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion("gpt-4o-mini", Config.OpenAIApiKey!).Build();
        var monsters = GetMonstersByChallengeRating(challengeRatings);
        var selectedMonsters = monsters.OrderBy(x => Guid.NewGuid()).Take(count).ToList();

        var finalList = new List<RpgMonster>();
        foreach (var monster in selectedMonsters)
        {
            var monsterConverterFunction = KernelFunctionYaml.FromPromptYaml(PromptHelper.ExtractPromptFromFile("ConvertMonsterFunction.yaml"));
            var settings = new OpenAIPromptExecutionSettings() { ResponseFormat = typeof(RpgMonster) };
            var convertedMonster = await monsterConverterFunction.InvokeAsync(kernel, new KernelArguments(settings) { ["monster"] = JsonSerializer.Serialize(monster, new JsonSerializerOptions() { WriteIndented = true }) });
            finalList.Add(JsonSerializer.Deserialize<RpgMonster>(convertedMonster.ToString())!);
        }

        var monsterEncounter = new MonsterEncounter(finalList);
        return monsterEncounter;
    }
}
public class DndMonster
{
    public string ToBasicInfoMarkdown()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## {Name}");
        sb.AppendLine($"**Challenge Rating:** {ChallengeRating}");
        sb.AppendLine($"**Size:** {Size}");
        sb.AppendLine($"**Armor Class:** {ArmorClass.First().Value}");
        sb.AppendLine($"**Hit Points:** {HitPoints}");
        sb.AppendLine($"**XP:** {Xp}");
        sb.AppendLine();
        return sb.ToString();
    }
    [JsonPropertyName("index")]
    public string? Index { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("alignment")]
    public string? Alignment { get; set; }

    [JsonPropertyName("armor_class")]
    public List<ArmorClass>? ArmorClass { get; set; }

    [JsonPropertyName("hit_points")]
    public int HitPoints { get; set; }

    [JsonPropertyName("hit_dice")]
    public string? HitDice { get; set; }

    [JsonPropertyName("hit_points_roll")]
    public string? HitPointsRoll { get; set; }

    [JsonPropertyName("speed")]
    public Speed? Speed { get; set; }

    [JsonPropertyName("strength")]
    public int Strength { get; set; }

    [JsonPropertyName("dexterity")]
    public int Dexterity { get; set; }

    [JsonPropertyName("constitution")]
    public int Constitution { get; set; }

    [JsonPropertyName("intelligence")]
    public int Intelligence { get; set; }

    [JsonPropertyName("wisdom")]
    public int Wisdom { get; set; }

    [JsonPropertyName("charisma")]
    public int Charisma { get; set; }

    [JsonPropertyName("proficiencies")]
    public List<ProficiencyElement>? Proficiencies { get; set; }

    [JsonPropertyName("damage_vulnerabilities")]
    public List<object>? DamageVulnerabilities { get; set; }

    [JsonPropertyName("damage_resistances")]
    public List<object>? DamageResistances { get; set; }

    [JsonPropertyName("damage_immunities")]
    public List<object>? DamageImmunities { get; set; }

    [JsonPropertyName("condition_immunities")]
    public List<object>? ConditionImmunities { get; set; }

    [JsonPropertyName("senses")]
    public Senses? Senses { get; set; }

    [JsonPropertyName("languages")]
    public string? Languages { get; set; }

    [JsonPropertyName("challenge_rating")]
    public double ChallengeRating { get; set; }

    [JsonPropertyName("proficiency_bonus")]
    public int ProficiencyBonus { get; set; }

    [JsonPropertyName("xp")]
    public int Xp { get; set; }

    [JsonPropertyName("special_abilities")]
    public List<SpecialAbility>? SpecialAbilities { get; set; }

    [JsonPropertyName("actions")]
    public List<DndMonsterAction>? Actions { get; set; }

    [JsonPropertyName("legendary_actions")]
    public List<LegendaryAction>? LegendaryActions { get; set; }

    [JsonPropertyName("image")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class DndMonsterAction
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("multiattack_type")]
    public string? MultiattackType { get; set; }

    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("actions")]
    public List<ActionAction>? Actions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("attack_bonus")]
    public int? AttackBonus { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("dc")]
    public Dc? Dc { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("damage")]
    public List<Damage>? Damage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }
}

public class ActionAction
{
    [JsonPropertyName("action_name")]
    public string? ActionName { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class Damage
{
    [JsonPropertyName("damage_type")]
    public DcTypeClass? DamageType { get; set; }

    [JsonPropertyName("damage_dice")]
    public string? DamageDice { get; set; }
}

public class DcTypeClass
{
    [JsonPropertyName("index")]
    public string? Index { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class Dc
{
    [JsonPropertyName("dc_type")]
    public DcTypeClass? DcType { get; set; }

    [JsonPropertyName("dc_value")]
    public int DcValue { get; set; }

    [JsonPropertyName("success_type")]
    public string? SuccessType { get; set; }
}

public class Usage
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("times")]
    public int Times { get; set; }
}

public class ArmorClass
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class LegendaryAction
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("attack_bonus")]
    public int? AttackBonus { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("damage")]
    public List<Damage>? Damage { get; set; }
}

public class ProficiencyElement
{
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("proficiency")]
    public DcTypeClass? Proficiency { get; set; }
}

public class Senses
{
    [JsonPropertyName("darkvision")]
    public string? Darkvision { get; set; }

    [JsonPropertyName("passive_perception")]
    public int PassivePerception { get; set; }
}

public class SpecialAbility
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("dc")]
    public Dc? Dc { get; set; }
}

public class Speed
{
    [JsonPropertyName("walk")]
    public string? Walk { get; set; }

    [JsonPropertyName("swim")]
    public string? Swim { get; set; }
}