using Microsoft.AspNetCore.Components.Routing;

namespace AIRpgAgents.Components;
public partial class Routes
{
    private string _title = "AI Rpg Agents Home";

    private void HandleNavigation(NavigationContext navigationContext)
    {
        _title = navigationContext.Path switch
        {
            "narrative" => "Narrative Builder Agent",
            "createCharacter" => "Character Builder Agent",
            "buildWorld" => "World Builder Agent",
            "documents" => "Relevant Documents",
            "player" => "Player Dashboard",
            "worlds" => "Worlds Dashboard",
            _ => "AI Rpg Agents Home"
        };
        StateHasChanged();
    }
}
