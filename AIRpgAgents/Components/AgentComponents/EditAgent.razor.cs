using AIRpgAgents.Core.Agents;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AIRpgAgents.Components.AgentComponents;
public partial class EditAgent
{
    [Parameter]
    public AgentData Agent { get; set; } = new();
    [Parameter]
    public EventCallback<AgentData> AgentChanged { get; set; }
    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private async Task SaveAgent(AgentData agent)
    {
        await CosmosService.SaveAgent(agent);
        Agent = agent;
        await AgentChanged.InvokeAsync(Agent);
        DialogService.Close(Agent);
    }
}
