using AIRpgAgents.ChatModels;
using AIRpgAgents.Components.AgentComponents;
using AIRpgAgents.Components.ChatComponents;
using AIRpgAgents.Components.Game;
using AIRpgAgents.Core;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SkPluginComponents.Models;

namespace AIRpgAgents.Components.Pages;
public partial class Home
{
    
    [Inject]
    private AppState AppState { get; set; } = default!;
    
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }
    private AuthenticationState? _authState;

    private Dictionary<string, Type> _componentOptions => new()
        { ["Dice Playground"] = typeof(DicePlayground), ["Create Character Agent"] = typeof(CharacterCreate), ["World Builder Agent"] = typeof(BuildWorld) }; 
    private string _selectedComponent = "Dice Playground";

    private void SelectComponent(string name)
    {
        _selectedComponent = name;
        StateHasChanged();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _authState ??= await AuthenticationState!;
        var identityName = _authState.User.Identity?.Name;
        //Console.WriteLine($"Identity Name: {identityName}");
        AppState.Player.Name ??= identityName;
       
        
        await base.OnAfterRenderAsync(firstRender);
    }

   
    
}
