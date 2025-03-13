using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SkPluginComponents.Models;
using System.Diagnostics;
using Markdig;
using SkPluginComponents.Components;

namespace SkPluginComponents
{
    public partial class RollDiceWindow
    {
        [Parameter]
        public bool IsOpen { get; set; }
        [Parameter]
        public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter]
        public Location ModalLocation { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }
        [Inject]
        private RollDiceService AskUserService { get; set; } = default!;

        private Type? ComponentType { get; set; } = typeof(DieRoller);
        private RollDiceParameters? RollDiceParameters { get; set; }
        private RollDiceWindowOptions RollDiceWindowsOptions { get; set; } = new();

        public void Reset()
        {
            ChildContent = null;
            ComponentType = null;
        }
        protected override Task OnInitializedAsync()
        {
            Debug.WriteLine("Modal Initialized");
            AskUserService.OnOpenComponent += Open;
            AskUserService.OnOpenFragment += OpenFragment;
            AskUserService.OnModalClose += Close;
            return base.OnInitializedAsync();
        }

        private void OpenFragment(RenderFragment<RollDiceService> childContent, RollDiceParameters? parameters = null,
            RollDiceWindowOptions? options = null)
        {
            Console.WriteLine("ModalService OnOpenFragment handled in Modal.razor");
            Reset();
            ChildContent = childContent.Invoke(AskUserService);
            RollDiceParameters = parameters;
            RollDiceWindowsOptions = options ?? new RollDiceWindowOptions();
            IsOpen = true;
            InvokeAsync(StateHasChanged);
        }
        private void Open(Type type, RollDiceParameters? parameters = null, RollDiceWindowOptions? options = null)
        {
            Console.WriteLine("ModalService OnOpenComponent handled in Modal.razor");
            Reset();
            ComponentType = type;
            RollDiceParameters = parameters;
            RollDiceWindowsOptions = options ?? new RollDiceWindowOptions();
            IsOpen = true;
            InvokeAsync(StateHasChanged);
        }

        private void Close(RollDiceResults? results = null)
        {
            Console.WriteLine("ModalService OnClose handled in Modal.razor");
            IsOpen = false;
            InvokeAsync(StateHasChanged);
        }
        private void CloseSelf()
        {
            AskUserService.CloseSelf();
        }
        private string AsHtml(string? text)
        {
            if (text == null) return "";
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var result = Markdown.ToHtml(text, pipeline);
            return result;

        }
        
        private void OutClick()
        {
            if (RollDiceWindowsOptions.CloseOnOuterClick)
            {
                CloseSelf();
            }
        }

        private void Close()
        {
            IsOpen = false;
            IsOpenChanged.InvokeAsync(IsOpen);
        }
    }
}
