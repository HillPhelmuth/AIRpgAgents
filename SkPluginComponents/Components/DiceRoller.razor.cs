using Microsoft.AspNetCore.Components;
using SkPluginComponents.Models;

namespace SkPluginComponents.Components;
public partial class DiceRoller
{
    [Parameter]
    public int NumberOfRolls { get; set; } = 1;

    private int _previousNumberOfRolls;
    [Parameter]
    public DieType DieType { get; set; } = DieType.D20;

    private DieRoller Component { set => _components.Add(value); }

    List<DieRoller> _components = [];

    [Inject] 
    public RollDiceService RollDiceService { get; set; } = default!;    
    
    private List<int> _rolls = [];
    [Parameter]
    public int Duration { get; set; } = 2000;

    [Parameter] 
    public bool IsStandalone { get; set; } = false;
    [Parameter]
    public bool IsManual { get; set; } = false;
    protected override void OnInitialized()
    {
        _previousNumberOfRolls = NumberOfRolls;
        base.OnInitialized();
    }
    protected override Task OnParametersSetAsync()
    {
        if (_previousNumberOfRolls > NumberOfRolls)
        {
            var componentsToRemoveFromEnd = _previousNumberOfRolls - NumberOfRolls;
            for (var i = 0; i < componentsToRemoveFromEnd; i++)
            {
                _components.RemoveAt(_components.Count - 1);
            }
            _previousNumberOfRolls = NumberOfRolls;
        }
        return base.OnParametersSetAsync();
    }

    private async void HandleValueUpdate(int value)
    {
        _rolls.Add(value);
        if (_rolls.Count < NumberOfRolls)
        {
            return;
        }
        //await Task.Delay(Duration);
        //if (!IsStandalone)
        //    RollDiceService.Close(new RollDiceResults(true, new RollDiceParameters() { ["Rolls"] = _rolls, ["Total"] = _rolls.Sum() }));
    }
    private bool _rolling;
    private async Task RollAll()
    {
        if (_rolling)
        {
            return;
        }
        _rolls.Clear();
        _rolling = true;
        var tasks = new List<Task>();
        foreach (var component in _components)
        {
            tasks.Add(component.RollDice());
        }
        await Task.WhenAll(tasks);
        await Task.Delay(Duration);
        if (!IsStandalone)
            RollDiceService.Close(new RollDiceResults(true, new RollDiceParameters() { ["Rolls"] = _rolls, ["Total"] = _rolls.Sum() }));
        _rolling = false;
    }
}
