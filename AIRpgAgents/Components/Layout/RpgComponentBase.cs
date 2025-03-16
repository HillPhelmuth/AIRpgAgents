using AIRpgAgents.Components.AgentComponents;
using AIRpgAgents.Core;
using AIRpgAgents.Core.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AIRpgAgents.Components.Layout;

public class RpgComponentBase : ComponentBase
{
    [Inject] protected AppState AppState { get; set; } = default!;
    [Inject] protected CosmosService CosmosService { get; set; } = default!;
    [Inject] protected DialogService DialogService { get; set; } = default!;

    protected virtual async Task ShowAgent(string agentId)
    {
        var agent = await CosmosService.GetAgent(agentId);
        if (agent != null)
        {
            DialogService.Open<EditAgent>("",new Dictionary<string, object>() { ["Agent"] = agent}, new DialogOptions(){Style = "width:80vw"});
            return;
        }
    }
}