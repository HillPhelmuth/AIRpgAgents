using Microsoft.AspNetCore.Components;
using SkPluginComponents.Components;

namespace SkPluginComponents.Models;

public class RollDiceService
{
    private readonly List<object> _modals = [];
    private readonly List<DieRoleModalReference> _dieRollReferences = [];
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
        _dieRollReferences.Add(new DieRoleModalReference());
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
        DieRoleModalReference modalRef = new() { TaskCompletionSource = taskCompletionSource };
        _dieRollReferences.Add(modalRef);
        OnOpenComponent?.Invoke(typeof(T), parameters, options);
        return modalRef.TaskCompletionSource.Task;

    }
    public Task<RollDiceResults?> OpenDiceWindow(RollDiceParameters? parameters = null, RollDiceWindowOptions? options = null)
    {
        TaskCompletionSource<RollDiceResults?> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        DieRoleModalReference modalRef = new() { TaskCompletionSource = taskCompletionSource };
        _dieRollReferences.Add(modalRef);
        OnOpenComponent?.Invoke(typeof(DiceRoller), parameters, options);
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
        var modalRef = _dieRollReferences.LastOrDefault();
        if (modalRef != null)
        {
            IsOpen = false;
            _dieRollReferences.Remove(modalRef);
            OnClose(result);
        }
        var taskCompletion = modalRef?.TaskCompletionSource;
        if (taskCompletion == null || taskCompletion.Task.IsCompleted) return;
        taskCompletion.SetResult(result);
    }
    private void OnClose(RollDiceResults? results)
    {
        OnModalClose?.Invoke(results);
    }   
}
internal class DieRoleModalReference
{
    internal Guid Id { get; set; } = Guid.NewGuid();
    internal TaskCompletionSource<RollDiceResults?> TaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public enum Location
{
    Center, Left, Right, TopLeft, TopRight, Bottom
}
