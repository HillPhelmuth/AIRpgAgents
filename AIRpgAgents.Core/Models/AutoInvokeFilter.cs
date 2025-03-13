using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AIRpgAgents.Core.Models;

public class AutoInvokeFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"AutoInvokeFilter Invoked: {context.Function.Name}\nArgs: {string.Join(", ", context.Arguments?.Select(x => $"{x.Key} - {x.Value}") ?? [])}");
        try
        {
            await next(context);
            Console.WriteLine($"AutoInvokeFilter Completed: {context.Function.Name}\nResult: {context.Result.ToString()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AutoInvokeFilter: {ex.Message}");
            context.Result = new FunctionResult(context.Function,
                $"An error occurred. Inform the user of the details: {ex}");
        }
    }
}

public class AutoInvokeFilterEvents : IAutoFunctionInvocationFilter
{
    public event EventHandler<AutoFunctionInvocationContext>? OnInvoked;
    public event EventHandler<AutoFunctionInvocationContext>? OnCompleted;
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"AutoInvokeFilterEvents Invoked: {context.Function.Name}\nArgs: {string.Join(", ", context.Arguments?.Select(x => $"{x.Key} - {x.Value}") ?? [])}");
        OnInvoked?.Invoke(this, context);
        try
        {
            await next(context);
            Console.WriteLine($"AutoInvokeFilterEvents Completed: {context.Function.Name}\nResult: {context.Result.ToString()}");
            OnCompleted?.Invoke(this, context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AutoInvokeFilterEvents: {ex.Message}");
            context.Result = new FunctionResult(context.Function,
                $"An error occurred. Inform the user of the details: {ex}");
        }
    }
}