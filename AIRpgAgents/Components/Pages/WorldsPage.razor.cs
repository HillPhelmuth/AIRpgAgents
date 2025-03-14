using AIRpgAgents.Core;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.World;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AIRpgAgents.Components.Pages;
public partial class WorldsPage
{
    [Inject]
    private AppState AppState { get; set; } = default!;
    [Inject]
    private CosmosService CosmosService { get; set; } = default!;
    [Inject]
    private NotificationService NotificationService { get; set; } = default!;
    private List<WorldState> _worlds = new();
    private WorldState? SelectedWorld { get; set; }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _worlds = await CosmosService.GetWorldStatesAsync();
            if (_worlds.Count == 0)
            {
                NotificationService.Notify(NotificationSeverity.Info, "No Worlds Found", "You have no worlds. Please create one using the <strong>World Creation Agent.</strong>", duration: 5000);
            }
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    private void SelectWorld(WorldState world)
    {
        SelectedWorld = world;
        StateHasChanged();
    }
}
