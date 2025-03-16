using Microsoft.AspNetCore.Components;
using SkPluginComponents.Models;

namespace SkPluginComponents.Components;
public partial class DieRoller
{
    public int DieVal { get; set; } = 20;
    [Parameter]
    public DieType DieType { get; set; } = DieType.D20;

    [Parameter]
    public int CurrentValue { get; set; } = 1;
    [Parameter]
    public EventCallback<int> CurrentValueChanged { get; set; }
    [Parameter]
    public bool IsManual { get; set; }

    private string _diceClass = "stopped";
    private string _rollingCss = "";
    private int _currentFace = 1;
    private Timer _rollTimer;
    protected override void OnParametersSet()
    {
        DieVal = DieType switch
        {
            DieType.D4 => 4,
            DieType.D6 => 6,
            DieType.D8 => 8,
            DieType.D10 => 10,
            DieType.D20 => 20,
            _ => DieVal
        };
        base.OnParametersSet();
    }
    protected override void OnInitialized()
    {

    }
    private bool _rollStarted;
    private async Task RollMany()
    {
        if (_rollStarted)
            return;
        _rollStarted = true;
        var random = new Random();
        for (var i = 0; i < 10; i++)
        {
            _currentFace = random.Next(1, DieVal + 1);
            StateHasChanged();
            await Task.Delay(300);
        }
        CurrentValue = _currentFace;
        await CurrentValueChanged.InvokeAsync(_currentFace);
        _rollStarted = false;
    }

    public async Task ManualRoll()
    {
        if (!IsManual) return;
        await RollDice();
    }
    public async Task RollDice()
    {
        Console.WriteLine($"Roll {DieType} Initiated");
        if (_rollingCss == "rolling")
            return;
        _diceClass = "rolling";
        StateHasChanged();
        await Task.Delay(1);
        var nextValue = new Random().Next(1, DieVal + 1);
        //await connection.SendAsync("SendRolling", _rollingCss);
        await Task.Delay(3000);
        //SharedResult = DieRollService.Result;
        //await connection.SendAsync("SendRoll", SharedResult);
        _currentFace = nextValue;
        CurrentValue = _currentFace;
        await CurrentValueChanged.InvokeAsync(_currentFace);
        _diceClass = "stopped";
        StateHasChanged();
    }
}
