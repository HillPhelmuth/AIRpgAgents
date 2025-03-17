using AIRpgAgents.ChatModels;
using AIRpgAgents.Components.ChatComponents;
using AIRpgAgents.Core.Agents;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.World;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SkPluginComponents.Models;

namespace AIRpgAgents.Components.AgentComponents;
public partial class BuildNarrative : IDisposable
{
    private ChatView _chatView;

    [Inject]
    private CosmosService CosmosService { get; set; } = default!;
    private NarrativeAgent? GameMasterAgent { get; set; }
    private bool _isBusy;
    private bool _isStarted;

    private CancellationTokenSource _cts = new();

    protected override Task OnInitializedAsync()
    {
        AppState.PropertyChanged += HandlePropertyChanged;
        return base.OnInitializedAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {

            var worldAgent = await CosmosService.GetAgent("NarrativeAgent");
            if (worldAgent == null)
            {
                GameMasterAgent = new NarrativeAgent(AppState);
                await CosmosService.SaveAgent(GameMasterAgent.ToAgentData());
            }
            else
            {
                PromptHelper.PromptDictionary["NarrativeAgent.md"] = worldAgent.PromptTemplate!;
                GameMasterAgent = new NarrativeAgent(appState:AppState,promptTemplate: worldAgent.PromptTemplate!);
            }
            StateHasChanged();
        }


        await base.OnAfterRenderAsync(firstRender);
    }
    private void HandlePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppState.NarrativeState))
        {
            StateHasChanged();
        }
    }
    private async Task StartAgent()
    {
        _isStarted = true;
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var history = new ChatHistory();
        history.AddUserMessage("Go on. Ask me for my damn ideas.");
        await foreach (var response in GameMasterAgent!.InvokeStreamingAsync(history))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }
    private void Cancel()
    {
        _isBusy = false;
        _cts.Cancel();
        _cts = new();
        StateHasChanged();
    }
    private async void HandleRequest(UserInputRequest userInputRequest)
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        if (userInputRequest.FileUploads.Count == 0)
        {
            HandleInput(userInputRequest.ChatInput);
            return;
        }
        var text = new TextContent(userInputRequest.ChatInput);
        //var url = await GetImageUrlFromBlobStorage(userInputRequest.FileUpload!);
        var images = userInputRequest.FileUploads.Select(file => new ImageContent(file.FileBytes, "image/jpeg")).ToList();

        _chatView.ChatState.AddUserMessage(text, [.. images]);
        await SubmitRequest();
        _isBusy = false;
        StateHasChanged();
    }
    private async void HandleInput(string input)
    {

        if (!string.IsNullOrWhiteSpace(input))
            _chatView.ChatState.AddUserMessage(input);

        await SubmitRequest();
    }
    private async Task SubmitRequest()
    {
        var histpry = _chatView.ChatState.ChatHistory;
        await foreach (var response in GameMasterAgent.InvokeStreamingAsync(histpry))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }

    public void Dispose()
    {
        AppState.PropertyChanged -= HandlePropertyChanged;
    }
}
