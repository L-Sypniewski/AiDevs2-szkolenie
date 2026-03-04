using Microsoft.Extensions.AI;

namespace AiDevs4.AiClients;

/// <summary>
/// Extension methods for registering AI clients using Aspire integration with keyed IChatClient services.
/// Resource names from AppHost map to service keys matching ModelConfiguration pattern.
/// </summary>
public static class AddAspireAiClientsExtensions
{
    /// <summary>
    /// Registers all AI clients using Aspire integration with keyed IChatClient services.
    /// Service keys match the pattern: {modelId}-{provider} (e.g., "gpt-4.1-mini-GithubModels")
    /// </summary>
    /// <param name="builder">The host application builder to add clients to.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddAiClientsFromAspire(this IHostApplicationBuilder builder)
    {
        // ==================== OpenAI Models ====================
        builder.AddOpenAIClient("openai-gpt5")
            .AddKeyedChatClient("gpt-5-OpenAI")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("openai-gpt5-mini")
            .AddKeyedChatClient("gpt-5-mini-OpenAI")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("openai-gpt41")
            .AddKeyedChatClient("gpt-4.1-OpenAI")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        // ==================== GitHub Models ====================
        // GitHub Models uses OpenAI-compatible API via Azure AI Inference
        builder.AddOpenAIClient("github-gpt41")
            .AddKeyedChatClient("gpt-4.1-GithubModels")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("github-gpt41-mini")
            .AddKeyedChatClient("gpt-4.1-mini-GithubModels")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("github-gpt5-mini")
            .AddKeyedChatClient("gpt-5-mini-GithubModels")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("github-gpt5-nano")
            .AddKeyedChatClient("gpt-5-nano-GithubModels")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("github-o3-mini")
            .AddKeyedChatClient("o3-mini-GithubModels")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("github-o4-mini")
            .AddKeyedChatClient("o4-mini-GithubModels")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddOpenAIClient("github-phi4")
            .AddKeyedChatClient("phi-4-GithubModels")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        // ==================== Ollama Models (requires Docker) ====================
        builder.AddKeyedOllamaApiClient("ollama-llama31")
            .AddKeyedChatClient("llama3.1-Ollama")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddKeyedOllamaApiClient("ollama-deepseek-r1")
            .AddKeyedChatClient("deepseek-r1-Ollama")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        builder.AddKeyedOllamaApiClient("ollama-phi4")
            .AddKeyedChatClient("phi4-Ollama")
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();

        return builder;
    }
}
