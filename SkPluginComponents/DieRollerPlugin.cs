using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using SkPluginComponents.Components;
using SkPluginComponents.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkPluginComponents;

public class DieRollerPlugin(RollDiceService modalService)
{
    [KernelFunction, Description("Rolls a die with the specified number of sides.")]
    [return: Description("The result of the die roll.")]
    public async Task<int> RollDie([Description("Type of the die based on number of sides")] DieType dieType, [Description("Value to add (or subtract, if negative) to the roll result")] int modifier = 0)
    {
        var windowOptions = new RollDiceWindowOptions() { Title = $"Roll 1{dieType}", Location = Location.Center, Style = "width:max-content;min-width:50vw;height:max-content" };
        var parameters = new RollDiceParameters() { ["DieType"] = dieType, ["NumberOfRolls"] = 1 };
        var result = await modalService.OpenAsync<DiceRoller>(parameters, windowOptions);
        var returnItem = result?.Parameters.Get<int>("Total");
        return returnItem.GetValueOrDefault() + modifier;
    }
    [KernelFunction, Description("Rolls a die with the specified number of sides a specified number of times. (ex: 3D6 is D6 die rolled 3 times)")]
    [return: Description("The total of the results of the die rolls.")]
    public async Task<DieRollResults> RollDice([Description("Type of the die based on number of sides")] DieType dieType, [Description("Number of roles of the indicated die")] int numberOfRolls, [Description("Value to add (or subtract, if negative) to the roll result")] int modifier = 0)
    {
        var windowOptions = new RollDiceWindowOptions() { Title = $"Roll {numberOfRolls}{dieType}", Location = Location.Center, Style = "width:max-content;min-width:50vw;height:max-content" };
        var parameters = new RollDiceParameters() { ["DieType"] = dieType, ["NumberOfRolls"] = numberOfRolls };
        var result = await modalService.OpenAsync<DiceRoller>(parameters, windowOptions);
        var total = result?.Parameters.Get<int>("Total");
        var rolls = result?.Parameters.Get<List<int>>("Rolls");
        return new DieRollResults(rolls ?? [], total.GetValueOrDefault() + modifier);
    }
}
[TypeConverter(typeof(GenericTypeConverter<DieRollResults>))]
public record DieRollResults(List<int> RollResults, int Total);
internal class GenericTypeConverter<T> : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => true;

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        Console.WriteLine($"Converting {value} to {typeof(T)}");
        return JsonSerializer.Deserialize<T>(value.ToString());
    }
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        Console.WriteLine($"Converting {typeof(T)} to {value}");
        return JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
    }
}