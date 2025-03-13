using Microsoft.AspNetCore.Components;
using SkPluginComponents.Components;
using SkPluginComponents;
using SkPluginComponents.Models;
using System.ComponentModel;

namespace AIRpgAgents.Components.Game;
public partial class DicePlayground
{
    [Inject]
    private RollDiceService RollDiceService { get; set; } = default!;
    private DieRollRequest _dieRollRequest = new();
    private class DieRollRequest
    {
        public DieType DieType { get; set; } = DieType.D6;
        public int NumberOfRolls { get; set; } = 3;
    }

    private DieRollResults? _dieRollResults;

    
    private async Task<DieRollResults> RollDice(DieRollRequest dieRollRequest)
    {
        var numberOfRolls = _dieRollRequest.NumberOfRolls;
        var dieType = _dieRollRequest.DieType;
        var windowOptions = new RollDiceWindowOptions() { Title = $"Roll {numberOfRolls}{dieType}", Location = Location.Center, Style = "width:max-content;min-width:50vw;height:max-content", ShowCloseButton = true, CloseOnOuterClick = true};
        var parameters = new RollDiceParameters() { ["DieType"] = dieType, ["NumberOfRolls"] = numberOfRolls, ["Duration"] = 10000 };
        var result = await RollDiceService.OpenAsync<DiceRoller>(parameters, windowOptions);
        var total = result?.Parameters.Get<int>("Total");
        var rolls = result?.Parameters.Get<List<int>>("Rolls");
        return new DieRollResults(rolls ?? [], total.GetValueOrDefault());
    }
}
