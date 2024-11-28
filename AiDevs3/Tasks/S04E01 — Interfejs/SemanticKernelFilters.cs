using System.Text.Json;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S04E01___Interfejs;

/// <summary>
/// Function filter for observability.
/// </summary>
public sealed class MyFunctionFilter : IFunctionInvocationFilter
{
    private readonly ILogger<MyFunctionFilter> _logger;

    public MyFunctionFilter(ILogger<MyFunctionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        _logger.LogInformation("Invoking {FunctionName}", context.Function.Name);

        await next(context);

        var metadata = context.Result?.Metadata;

        if (metadata is not null && metadata.TryGetValue("Usage", out var value))
        {
            _logger.LogInformation("Token usage: {Usage}", JsonSerializer.Serialize(value));
        }
    }
}

public class FunctionInvocationFilter : IAutoFunctionInvocationFilter
{
    private readonly ILogger<FunctionInvocationFilter> _logger;

    public FunctionInvocationFilter(ILogger<FunctionInvocationFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        _logger.LogInformation("Auto invoking function: {FunctionName} with message: {Message}",
            context.Function.Name,
            context.ChatMessageContent);

        await next(context);

        if (context.Result.Metadata != null && context.Result.Metadata.TryGetValue("Usage", out var usage))
        {
            _logger.LogInformation("Auto function usage: {Usage}", JsonSerializer.Serialize(usage));
        }
    }
}

/// <summary>
/// Prompt filter for observability.
/// </summary>
public class MyPromptFilter : IPromptRenderFilter
{
    private readonly ILogger<MyPromptFilter> _logger;

    public MyPromptFilter(ILogger<MyPromptFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        _logger.LogInformation("Rendering prompt for {FunctionName}", context.Function.Name);

        await next(context);

        _logger.LogInformation("Rendered prompt: {RenderedPrompt}", context.RenderedPrompt);
    }
}
