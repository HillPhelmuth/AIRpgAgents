using Microsoft.AspNetCore.Components;

namespace AIRpgAgents.Components.Layout;
public partial class MainLayout
{
    private bool _sidebar1Expanded = false;
    [CascadingParameter(Name = "Title")]
    public string Title { get; set; } = "AI Rpg Agents";

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _sidebar1Expanded = false;
            StateHasChanged();
        }
        return base.OnAfterRenderAsync(firstRender);
    }
}
