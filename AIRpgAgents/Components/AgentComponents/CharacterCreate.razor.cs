using AIRpgAgents.ChatModels;
using AIRpgAgents.Components.ChatComponents;
using AIRpgAgents.Core.Services;
using AIRpgAgents.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using SkPluginComponents.Models;
using AIRpgAgents.Core.Agents;

namespace AIRpgAgents.Components.AgentComponents;
public partial class CharacterCreate
{
    private ChatView _chatView;
    [Inject]
    private AppState AppState { get; set; } = default!;
    [Inject]
    private CosmosService CosmosService { get; set; } = default!;
    [Inject]
    private ICharacterCreationService CharacterCreationService { get; set; } = default!;
    [Inject]
    private RollDiceService RollDiceService { get; set; } = default!;
    private CreateCharacterAgent? CharacterAgent { get; set; }
    private bool _isBusy;
    
    private CancellationTokenSource _cts = new();
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isBusy = true;
            StateHasChanged();
            await Task.Delay(1);
            var history = new ChatHistory();
            history.AddUserMessage("Introduce yourself in a flamboyant way and explain the character creation process. Then Begin the first step.");
            CharacterAgent ??= new CreateCharacterAgent(AppState, CosmosService, CharacterCreationService, RollDiceService);
            var characterAgent = await CosmosService.GetAgent("CharacterAgent");
            if (characterAgent == null)
            {
                await CosmosService.SaveAgent(CharacterAgent.ToAgentData());
            }
            
            await foreach (var response in CharacterAgent.InvokeStreamingAsync(history))
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
        await foreach (var response in CharacterAgent.InvokeStreamingAsync(histpry))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }
}
