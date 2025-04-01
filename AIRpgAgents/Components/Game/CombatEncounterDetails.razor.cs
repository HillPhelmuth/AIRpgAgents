using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.Monsters;
using AIRpgAgents.GameEngine.PlayerCharacter;
using Microsoft.AspNetCore.Components;

namespace AIRpgAgents.Components.Game;

public partial class CombatEncounterDetails
{
    [Parameter]
    public CombatEncounter? CombatEncounter { get; set; }

    private bool ShowInitiativeOrder { get; set; } = true;
    private bool ShowCombatants { get; set; } = true;
    private bool ShowCombatLog { get; set; } = true;
    private object? selectedEntity { get; set; }

    private void ToggleInitiativeOrder() => ShowInitiativeOrder = !ShowInitiativeOrder;
    private void ToggleCombatants() => ShowCombatants = !ShowCombatants;
    private void ToggleCombatLog() => ShowCombatLog = !ShowCombatLog;

    private void ShowDetails(object entity)
    {
        selectedEntity = entity;
    }

    private void CloseDetails()
    {
        selectedEntity = null;
    }
}
