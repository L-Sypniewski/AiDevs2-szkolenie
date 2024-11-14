using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Http.Resilience;
using System.Net;

namespace AiDevs3.SemanticKernel;

public class SemanticKernelFactory
{
    public enum AiProvider
    {
        OpenAI,
        GithubModels
    }

    public static string CreateServiceId(string modelId, AiProvider aiProvider) => $"{modelId}-{aiProvider}";

    public SemanticKernelFactory(IOptions<SemanticKernelFactoryOptions> options) => _semanticKernelFactoryOptions = options.Value;

    private readonly SemanticKernelFactoryOptions _semanticKernelFactoryOptions;

    private static class ModelConfigurations
    {
        public static readonly HashSet<(string ModelId, AiProvider Provider)> Models =
        [
            ("gpt-4o-2024-08-06", AiProvider.OpenAI),
            ("gpt-4o-mini-2024-07-18", AiProvider.OpenAI),
            ("gpt-4-turbo-2024-04-09", AiProvider.OpenAI),
            ("o1-mini-2024-09-12", AiProvider.OpenAI),
            ("o1-preview-2024-09-12", AiProvider.OpenAI),
            ("gpt-4o", AiProvider.GithubModels),
            ("gpt-4o-mini", AiProvider.GithubModels),
            ("o1-mini", AiProvider.GithubModels),
            ("o1-preview", AiProvider.GithubModels),
            ("Phi-3.5-mini-instruct", AiProvider.GithubModels),
            ("Phi-3.5-MoE-instruct", AiProvider.GithubModels),
            ("Phi-3.5-vision-instruct", AiProvider.GithubModels),
        ];
    }

    public Kernel BuildSemanticKernel(string? promptDirectory = null)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.ConfigureHttpClientDefaults(c =>
        {
            c.AddStandardResilienceHandler()
                .Configure((o, sp) =>
                {
                    // Configure retry policy
                    o.Retry.MaxRetryAttempts = 3;
                    o.Retry.Delay = TimeSpan.FromSeconds(5);
                    o.Retry.UseJitter = true;
                    o.Retry.OnRetry = (onRetryArgs) =>
                    {
                        var logger = sp.GetRequiredService<ILogger<HttpClient>>();
                        logger.LogWarning("Retrying request. Retry count: {RetryCount}, exception mesage: {Exception}", onRetryArgs.AttemptNumber,
                            onRetryArgs.Outcome.Exception?.Message);
                        return ValueTask.CompletedTask;
                    };

                    o.Retry.ShouldHandle = args =>
                    {
                        var isRetryableStatus = args.Outcome.Result?.StatusCode is
                            HttpStatusCode.TooManyRequests;

                        var isTimeout = args.Outcome.Exception?.InnerException is TaskCanceledException;
                        var isRateLimit = args.Outcome.Exception?.Message?.Contains("RateLimitReached") ?? false;

                        return ValueTask.FromResult(isRetryableStatus || isTimeout || isRateLimit);
                    };
                });
        });

        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole());

        var openAiClient = CreateOpenAiClient(AiProvider.OpenAI);
        var githubModelsClient = CreateOpenAiClient(AiProvider.GithubModels);

        foreach (var (modelId, provider) in ModelConfigurations.Models)
        {
            var client = provider == AiProvider.OpenAI ? openAiClient : githubModelsClient;
            RegisterModel(builder.Services, modelId, provider, client);
        }

        builder.Services
            .AddOpenAIAudioToText(modelId: "whisper-1", apiKey: _semanticKernelFactoryOptions.OpenAi.ApiKey,
                serviceId: CreateServiceId("whisper-1", AiProvider.OpenAI))
            .AddOpenAITextToImage(modelId: "dall-e-3", apiKey: _semanticKernelFactoryOptions.OpenAi.ApiKey,
                serviceId: CreateServiceId("dall-e-3", AiProvider.OpenAI));

        var kernel = builder.Build();
        if (promptDirectory is not null)
        {
            kernel.ImportPluginFromPromptDirectory(promptDirectory);
        }

        return kernel;
    }

    private OpenAIClient CreateOpenAiClient(AiProvider aiProvider) =>
        aiProvider switch
        {
            AiProvider.OpenAI => new OpenAIClient(
                new ApiKeyCredential(_semanticKernelFactoryOptions.OpenAi.ApiKey)),
            AiProvider.GithubModels => new OpenAIClient(
                new ApiKeyCredential(_semanticKernelFactoryOptions.GithubModels.ApiKey),
                new OpenAIClientOptions { Endpoint = _semanticKernelFactoryOptions.GithubModels.ApiEndpoint }),
            _ => throw new ArgumentOutOfRangeException(nameof(aiProvider), aiProvider, "Unsupported AI provider")
        };

    private static void RegisterModel(IServiceCollection services, string modelId, AiProvider provider, OpenAIClient client)
    {
        services.AddOpenAIChatCompletion(
            modelId: modelId,
            client,
            serviceId: CreateServiceId(modelId, provider));
    }

    public virtual async Task<string> InvokePluginWithStructuredOutputAsync(
        string promptDirectory,
        string functionName,
        Dictionary<string, object> parameters)
    {
        var kernel = BuildSemanticKernel(promptDirectory);
        var kernelFunction = kernel.Plugins["Prompts"][functionName];
        var executionSettings = GetExecutionSettings(kernelFunction);

        var kernelArguments = new KernelArguments(executionSettings);
        foreach (var (key, value) in parameters)
        {
            kernelArguments.Add(key, value);
        }

        var llmResponse = await kernel.InvokeAsync(kernelFunction, kernelArguments);

        return llmResponse.GetValue<string>()!;
    }

    private static OpenAIPromptExecutionSettings GetExecutionSettings(KernelFunction kernelFunction)
    {
        var kernelFunctionExecutionSettings = kernelFunction.ExecutionSettings!.Values.Single().ExtensionData!;
        return new OpenAIPromptExecutionSettings
        {
            Temperature = ((JsonElement) kernelFunctionExecutionSettings["temperature"]).GetDouble(),
            MaxTokens = ((JsonElement) kernelFunctionExecutionSettings["max_tokens"]).GetInt32(),
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "speaker_identification",
                jsonSchema: BinaryData.FromString(((JsonElement) kernelFunctionExecutionSettings["response_format"]).GetProperty("json_schema")
                    .GetProperty("schema")
                    .ToString()),
                jsonSchemaIsStrict: true)
        };
    }
}
