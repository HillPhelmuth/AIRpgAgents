using Microsoft.AspNetCore.Components;

namespace SkPluginComponents.Models;

public class RollDiceService
{
    private readonly List<object> _modals = [];
    private readonly List<AskUserReference> _askUserReferences = [];
    public bool IsOpen { get; set; }
    public RollDiceParameters? Parameters { get; set; }
    public event Action<RollDiceResults?>? OnModalClose;
    public event Action? OnOpen;
    public event Action<Type, RollDiceParameters?, RollDiceWindowOptions?>? OnOpenComponent;
    public event Action<RenderFragment<RollDiceService>, RollDiceParameters?, RollDiceWindowOptions?>? OnOpenFragment;

    public void Open()
    {
        IsOpen = true;
        OnOpen?.Invoke();
    }   
    public void Open<T>(RollDiceParameters? parameters = null, RollDiceWindowOptions? options = null) where T : ComponentBase
    {
        _askUserReferences.Add(new AskUserReference());
        OnOpenComponent?.Invoke(typeof(T), parameters, options);
    }

    public void Open(RenderFragment<RollDiceService> renderFragment, RollDiceParameters? parameters = null, RollDiceWindowOptions? options = null)
    {
        _modals.Add(new object());
        OnOpenFragment?.Invoke(renderFragment, parameters, options);
    }
    public Task<RollDiceResults?> OpenAsync<T>(RollDiceParameters? parameters = null, RollDiceWindowOptions? options = null) where T : ComponentBase
    {
        TaskCompletionSource<RollDiceResults?> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        AskUserReference modalRef = new() { TaskCompletionSource = taskCompletionSource };
        _askUserReferences.Add(modalRef);
        OnOpenComponent?.Invoke(typeof(T), parameters, options);
        return modalRef.TaskCompletionSource.Task;

    }

    public void Close(bool success)
    {
        Close(RollDiceResults.Empty(success));
    }
    public void CloseSelf(RollDiceResults? results = null)
    {
        Close(results);
    }
   
    public void Close(RollDiceResults? result)
    {
        var modalRef = _askUserReferences.LastOrDefault();
        if (modalRef != null)
        {
            IsOpen = false;
            _askUserReferences.Remove(modalRef);
            OnClose(result);
        }
        var taskCompletion = modalRef.TaskCompletionSource;
        if (taskCompletion == null || taskCompletion.Task.IsCompleted) return;
        taskCompletion.SetResult(result);
    }
    private void OnClose(RollDiceResults? results)
    {
        OnModalClose?.Invoke(results);
    }   
}
internal class AskUserReference
{
    internal Guid Id { get; set; } = Guid.NewGuid();
    internal TaskCompletionSource<RollDiceResults?> TaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public enum Location
{
    Center, Left, Right, TopLeft, TopRight, Bottom
}
