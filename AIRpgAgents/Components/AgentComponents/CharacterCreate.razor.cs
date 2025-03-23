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
using AIRpgAgents.GameEngine.PlayerCharacter;

namespace AIRpgAgents.Components.AgentComponents;
public partial class CharacterCreate
{
    private ChatView _chatView;
    
    [Inject]
    private ICharacterCreationService CharacterCreationService { get; set; } = default!;
    [Inject]
    private RollDiceService RollDiceService { get; set; } = default!;
    private CreateCharacterAgent? CharacterAgent { get; set; }
    private bool _isBusy;
    private bool _isStarted;
    private CancellationTokenSource _cts = new();
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetOrCreateCharacterAgent();
        }


        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task GetOrCreateCharacterAgent()
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var characterAgent = await CosmosService.GetAgent("CharacterAgent");
        if (characterAgent == null)
        {
            CharacterAgent ??= new CreateCharacterAgent(AppState, CosmosService, CharacterCreationService, RollDiceService);
            await CosmosService.SaveAgent(CharacterAgent.ToAgentData());
        }
        else
        {
            CharacterAgent = new CreateCharacterAgent(AppState, CosmosService, CharacterCreationService,
                RollDiceService, characterAgent.PromptTemplate!);
        }
        _isBusy = false;
        StateHasChanged();
    }

    private async Task StartAgent()
    {
        if (_isBusy) return;
        _isBusy = true;
        _isStarted = true;
        StateHasChanged();
        await Task.Delay(1);

        var history = new ChatHistory();
        history.AddUserMessage("Introduce yourself in a flamboyant way and explain the character creation process. Then Begin the first step.");
        await foreach (var response in CharacterAgent.InvokeStreamingAsync(history))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }
    private async Task SaveCharacterSheet(CharacterSheet characterSheet)
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        await CharacterCreationService.UpdateDraftCharacterSheet(characterSheet, AppState.Player.Id);


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
        await foreach (var response in CharacterAgent.InvokeStreamingAsync(histpry))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }
}
