using AIRpgAgents.ChatModels;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AIRpgAgents.Components.ChatComponents;

public partial class UserInput : ComponentBase
{
    [Parameter]
    public string HelperText { get; set; } = "";
    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public string ButtonLabel { get; set; } = "Submit";
    [Parameter]
    public string? Message { get; set; }
    
    [Parameter]
    public EventCallback<string> MessageSubmit { get; set; }
    [Parameter]
    public EventCallback<string> MessageChanged { get; set; }
    [Parameter]
    public UserInputType UserInputType { get; set; }
    [Parameter]
    public UserInputFieldType UserInputFieldType { get; set; }
    [Parameter]
    public EventCallback<UserInputRequest> UserInputSubmit { get; set; }
    [Parameter]
    public EventCallback CancelRequest { get; set; }
    [Parameter]
    public bool IsRequired { get; set; } = true;
    [Parameter]
    public string? ImprovedPrompt { get; set; }
    [Parameter]
    public EventCallback<string> ImprovedPromptRequest { get; set; }
    [Inject]
    public DialogService DialogService { get; set; } = default!;


    private bool _isPromptImproveRequested;
    protected override Task OnParametersSetAsync()
    {
        if (_isPromptImproveRequested && !string.IsNullOrEmpty(ImprovedPrompt))
        {
            _requestForm.UserInputRequest.ChatInput = ImprovedPrompt;
        }
        _requestForm.UserInputRequest.UserInputType = UserInputType;
        return base.OnParametersSetAsync();
    }
    
    private bool _isDisabled = false;

    private class RequestForm
    {
        public string? Content { get; set; }
        public bool ShowImageInput { get; set; }
        public UserInputRequest UserInputRequest { get; set; } = new();
    }

    private RequestForm _requestForm = new();
    private void Cancel()
    {
        CancelRequest.InvokeAsync();
    }
    private void ToggleInputType()
    {
        UserInputFieldType = UserInputFieldType == UserInputFieldType.TextBox
            ? UserInputFieldType.TextArea
            : UserInputFieldType.TextBox;
        StateHasChanged();
    }
    private async Task AddImage()
    {
        var files = await OpenImageFileAsync();
        if (files.Count > 0)
        {
            _requestForm.UserInputRequest.FileUploads = [..files.Select(x => new FileUpload {FileName = x.Item1, FileBase64 = Convert.ToBase64String(x.Item2), FileBytes = x.Item2})];
        }
    }
    public async Task<List<(string, byte[])>> OpenImageFileAsync()
    {
        var fileContent = await DialogService.OpenAsync<UploadImageWindow>("", options: new DialogOptions { Draggable = true, ShowClose = true, Resizable = true, ShowTitle = false, CloseDialogOnOverlayClick = true });
        if (fileContent is MultiFileUpload file)
        {
            return file.FileUploads.Select(x => (x.FileName, x.FileBytes)).ToList()!;
        }
        return [];
    }
    private async Task ImprovePrompt()
    {
        var currentPrompt = _requestForm.UserInputRequest.ChatInput;
        _isPromptImproveRequested = true;
        await ImprovedPromptRequest.InvokeAsync(currentPrompt);

    }
    private void SubmitRequest(RequestForm form)
    {
        MessageSubmit.InvokeAsync(form.UserInputRequest.ChatInput);
        UserInputSubmit.InvokeAsync(form.UserInputRequest);
        Message = form.UserInputRequest.ChatInput;
        MessageChanged.InvokeAsync(Message);
        _requestForm = new RequestForm
        {
            UserInputRequest =
            {
                UserInputType = UserInputType
            }
        };
        StateHasChanged();
    }
}