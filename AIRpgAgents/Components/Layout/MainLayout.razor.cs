using AIRpgAgents.Core;
using AIRpgAgents.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AIRpgAgents.Components.Layout;
public partial class MainLayout
{
    private bool _sidebar1Expanded = false;
    [CascadingParameter(Name = "Title")]
    public string Title { get; set; } = "AI Rpg Agents";
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }
    private AuthenticationState? _authState;
    [Inject]
    private AppState AppState { get; set; } = default!;
    [Inject]
    private CosmosService CosmosService { get; set; } = default!;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //_sidebar1Expanded = false;
            StateHasChanged();
        }
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
