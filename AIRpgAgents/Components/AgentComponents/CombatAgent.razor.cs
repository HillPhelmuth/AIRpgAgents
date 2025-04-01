using System.Text.Json;
using AIRpgAgents.ChatModels;
using AIRpgAgents.Components.ChatComponents;
using AIRpgAgents.Core.Agents;
using AIRpgAgents.Core.Models;
using AIRpgAgents.Core.Services;
using AIRpgAgents.GameEngine.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using SkPluginComponents.Models;
using AIRpgAgents.GameEngine.Monsters;
using AIRpgAgents.GameEngine.PlayerCharacter;
using Microsoft.SemanticKernel.Connectors.OpenAI;
#pragma warning disable SKEXP0010

namespace AIRpgAgents.Components.AgentComponents;
public partial class CombatAgent
{
    private bool _isBusy;
    private bool _isStarted;
    private bool _isReady;
    private CancellationTokenSource _cts = new();
    private ChatView _chatView;

    private RpgCampaign RpgCampaign => RpgState.ActiveCampaign;
    [Inject]
    private CombatService CombatService { get; set; } = default!;
    [Inject]
    private RollDiceService RollDiceService { get; set; } = default!;

    private CombatEncounterAgent? CombatEncounterAgent { get; set; }
    private CombatEncounter? _combatEncounter;
    private async Task<CombatEncounter> CreateCombatEncounter()
    {
        var monsterEncounter = await DndMonsterService.CreateRandomMonsterEncounter(2, 0.25, 0.5, 1.0);
        RpgState.ActiveCampaign.PendingMonsterEncounter = monsterEncounter;
        var testPlayers = FileHelper.ExtractFromAssembly<List<CharacterState>>("TestCharacters.json");
        var activeCampaignParty = new RpgParty() { PartyMembers = testPlayers, Name = "Badass Testing Party" };
        RpgState.ActiveCampaign.Party = activeCampaignParty;
        _isReady = true;
        StateHasChanged();
        Console.WriteLine("Combat Encounter Created");
        return new CombatEncounter()
            { MonsterEncounter = monsterEncounter, PlayerParty = activeCampaignParty, IsActive = true };
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _combatEncounter = await CreateCombatEncounter();
            var worldAgent = await CosmosService.GetAgent("CombatEncounterAgent");
            if (worldAgent == null)
            {
                CombatEncounterAgent = new CombatEncounterAgent(CombatService, RollDiceService, RpgState, _combatEncounter);
                await CosmosService.SaveAgent(CombatEncounterAgent.ToAgentData());
            }
            else
            {
                PromptHelper.PromptDictionary["CombatAgentPrompt.md"] = worldAgent.PromptTemplate!;
                CombatEncounterAgent = new CombatEncounterAgent(CombatService, RollDiceService, RpgState, _combatEncounter);
            }
            StateHasChanged();
        }
        //GetUpdatedCombatState();

        await base.OnAfterRenderAsync(firstRender);
    }

    private void HandleUpdateSummaries(string arg1, string arg2)
    {
        Console.WriteLine("Updating Combat State");
        GetUpdatedCombatState(arg1, arg2);
    }

    private async Task StartAgent()
    {
        _isStarted = true;
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var history = new ChatHistory();
        history.AddUserMessage("Introduce yourself. explain the process, and then start combat.");
        await foreach (var response in CombatEncounterAgent.InvokeStreamingAsync(history))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }

    private void GetUpdatedCombatState(string summary, string combatants)
    {
        //var summary = RpgState.ActiveCampaign.CombatEncounters.Find(x => x.IsActive)?.GetCombatSummary();
        //var combatants = RpgState.ActiveCampaign.CombatEncounters.Find(x => x.IsActive)?.GetCombatantsInfo();
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
        await foreach (var response in CombatEncounterAgent.InvokeStreamingAsync(histpry))
        {
            _chatView.ChatState.UpsertAssistantMessage(response);
            StateHasChanged();
        }
        _isBusy = false;
        StateHasChanged();
    }
}
