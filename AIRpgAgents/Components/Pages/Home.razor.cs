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
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }
    private AuthenticationState? _authState;
    [Inject]
    private CosmosService CosmosService { get; set; } = default!;
    private Dictionary<string, Type> _componentOptions => new()
    { ["Dice Playground"] = typeof(DicePlayground), ["Create Character Agent"] = typeof(CharacterCreate), ["World Builder Agent"] = typeof(BuildWorld), ["GM/Narrative Agent"] = typeof(BuildNarrative) };
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
        if (_authState.User.Identity?.IsAuthenticated == true)
        {
            AppState.ActiveAgents = await CosmosService.GetAllAgents();
        }

        await base.OnAfterRenderAsync(firstRender);
    }



}
