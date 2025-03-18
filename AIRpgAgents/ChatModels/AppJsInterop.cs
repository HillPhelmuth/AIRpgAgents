using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AIRpgAgents.ChatModels
{
    public class AppJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./appJsInterop.js").AsTask());

        public async ValueTask ScrollDown(ElementReference elementReference)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("scrollDown", elementReference);
                Console.WriteLine("Scrolled Down");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on ScrollDown: {ex.Message}");
            }
        }

        public async ValueTask<bool> RequiresScroll(ElementReference elementReference)
        {
            try
            {
                var module = await _moduleTask.Value;
                var requiresScroll = await module.InvokeAsync<bool>("requiresScroll", elementReference);
                if (requiresScroll)
                    Console.WriteLine("Requires Scroll");
                return requiresScroll;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on RequiresScroll: {ex.Message}");
                return false;
            }
        }

        public async ValueTask AddCodeStyle(ElementReference element)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("addCodeStyle", element);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on AddCodeStyle: {ex.Message}");
            }
        }
        
        public async ValueTask ShowJsonViewer(ElementReference element, object jsonObj)
        {
            try
            {
                await (await _moduleTask.Value).InvokeVoidAsync("showJsonViewer", element, jsonObj);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on ShowJsonViewer: {ex.Message}");
            }
        }
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_moduleTask.IsValueCreated)
                {
                    var module = await _moduleTask.Value;
                    await module.DisposeAsync();
                }
            }
            catch (JSDisconnectedException ex)
            {
                Console.WriteLine($"Error on DisposeAsync: {ex.Message}");
            }
        }
    }
}