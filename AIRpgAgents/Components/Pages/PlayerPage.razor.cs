using System.Text.Json;
using AIRpgAgents.Core;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.PlayerCharacter;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AIRpgAgents.Components.Pages;
public partial class PlayerPage
{
    [Inject]
    private CosmosService CosmosService { get; set; } = default!;
    [Inject]
    private AppState AppState { get; set; } = default!;
    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    private Player? _player;
    private CharacterSheet? _characterSheet;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var player = await CosmosService.GetOrCreatePlayerAsync(AppState.Player.Name);
            if (player.Characters.Count == 0)
            {
                NotificationService.Notify(NotificationSeverity.Info, "No Characters Found", "You have no characters. Please create one using the <strong>Character Creation Agent.</strong>", duration:5000);
            }
            _player = player;
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    private void ViewCharacter(CharacterSheet characterSheet)
    {
        _characterSheet = characterSheet;
        StateHasChanged();
    }
}
