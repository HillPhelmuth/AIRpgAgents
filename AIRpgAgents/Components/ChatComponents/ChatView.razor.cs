﻿using System.ComponentModel;
using AIRpgAgents.ChatModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AIRpgAgents.Components.ChatComponents;

public partial class ChatView : IDisposable, IAsyncDisposable
{
	private ElementReference _chatColumn;
	public ChatState ChatState
	{
		get
		{
			if (string.IsNullOrEmpty(ViewId))
			{
				return ChatStateCollection.CreateChatState(ViewId);
			}
			return ChatStateCollection.TryGetChatState(ViewId, out var chatState) ? chatState! : ChatStateCollection.CreateChatState(ViewId);
		}		
	}
	[Inject]
	private ChatStateCollection ChatStateCollection { get; set; } = default!;

    private AppJsInterop AppJsInterop => new(JsRuntime);
	[Parameter] public string Height { get; set; } = "60vh";
	[Parameter] public bool ResetOnClose { get; set; } = true;
	
	[Parameter]
	public bool Timestamps { get; set; } = true;
	[Parameter]
	public bool AllowRemove { get; set; }

	/// <summary>
	/// Unique Identifier for ChatView instance. If you have multiple ChatView components in your application,
	/// you need to provide unique ViewId for each of them. If left empty, ChatState will not persist when component is disposed.
	/// </summary>
	[Parameter]
	public string ViewId { get; set; } = "";

	private bool _generatedViewId;

	[Inject] private IJSRuntime JsRuntime { get; set; } = default!;

	protected override Task OnParametersSetAsync()
	{
		ChatState.PropertyChanged += ChatState_OnChatStateChanged;
		StateHasChanged();

		return base.OnParametersSetAsync();
	}
	private void HandleRemoveMessage(Message message)
	{
		ChatState.RemoveMessage(message);
		StateHasChanged();
    }
	private void HandleUpdateMessage(Message message)
	{
		ChatState.ChatHistory.Last().Content = message.Content + " ";
		StateHasChanged();
	}
	public List<(string role, string? message)> GetMessageHistory()
	{
		return ChatState.ChatMessages.Select(x => (x.Role.ToString(), x.Content)).ToList();
	}
	public ChatHistory GetChatHistory()
	{
		return ChatState.ChatHistory;
	}

    private bool _requiresScroll;
	private async void ChatState_OnChatStateChanged(object? sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == nameof(ChatState.ChatMessages))
		{
			ChatStateCollection.ChatStates[ViewId] = ChatState;
			await InvokeAsync(StateHasChanged);
            _requiresScroll = await AppJsInterop.RequiresScroll(_chatColumn);
            // Only auto-scroll if we're already at the bottom or it's a new message
            if (!_requiresScroll)
            {
                await AppJsInterop.ScrollDown(_chatColumn);
            }
        }
	}
	public async Task HandleScrollDown()
    {
        await AppJsInterop.ScrollDown(_chatColumn);
        _requiresScroll = false;
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _requiresScroll = await AppJsInterop.RequiresScroll(_chatColumn);
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (ResetOnClose) ChatState.Reset();
			if (_generatedViewId) ChatStateCollection.ChatStates.Remove(ViewId);
			ChatState.PropertyChanged -= ChatState_OnChatStateChanged;
		}
		
	}

    public async ValueTask DisposeAsync()
    {
        await AppJsInterop.DisposeAsync();
        Dispose(true);
    }

    public void Dispose()
    {
        Dispose(true);
    }
}