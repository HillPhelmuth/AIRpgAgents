using System.Text.Json;
using AIRpgAgents.Components.ChatComponents;
using AIRpgAgents.Core.Services;
using AIRpgAgents.Core;
using Microsoft.AspNetCore.Components;
using SkPluginComponents.Models;
using AIRpgAgents.ChatModels;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AIRpgAgents.GameEngine.World;
using AIRpgAgents.Core.Agents;

namespace AIRpgAgents.Components.AgentComponents;
public partial class BuildWorld
{
    private ChatView _chatView;
   
    [Inject]
    private CosmosService CosmosService { get; set; } = default!;
    [Inject]
    private ICharacterCreationService CharacterCreationService { get; set; } = default!;
    [Inject]
    private RollDiceService RollDiceService { get; set; } = default!;
    private CreateWorldAgent? CreateWorldAgent { get; set; }
    private bool _isBusy;

    private CancellationTokenSource _cts = new();
    private WorldState? _worldState;
   
    private void HandleWorldCreated(WorldState obj)
    {
        _worldState = obj;
        Console.WriteLine($"\n\nWorld: {JsonSerializer.Serialize(obj)}\n\n");
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isBusy = true;
            StateHasChanged();
            await Task.Delay(1);
            var history = new ChatHistory();
            history.AddUserMessage("Go on. Ask me for my damn ideas.");
            CreateWorldAgent = new CreateWorldAgent(CosmosService);
            var worldAgent = await CosmosService.GetAgent("WorldAgent");
            if (worldAgent == null)
                await CosmosService.SaveAgent(CreateWorldAgent.ToAgentData());
            CreateWorldAgent.WorldCreated += HandleWorldCreated;
            await foreach (var response in CreateWorldAgent.InvokeStreamingAsync(history))
            {
                _chatView.ChatState.UpsertAssistantMessage(response);
                StateHasChanged();
            }
            _isBusy = false;
            StateHasChanged();
        }


        await base.OnAfterRenderAsync(firstRender);
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
        await foreach (var response in CreateWorldAgent.InvokeStreamingAsync(histpry))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }
}
