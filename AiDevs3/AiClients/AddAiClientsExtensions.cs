using System.ClientModel;
using System.Net;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using OpenAI;
using Polly.Retry;

namespace AiDevs3.AiClients;

public static class AddAiClientsExtensions
{
    public static IServiceCollection AddAiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.ConfigName))
            .AddOptionsWithValidateOnStart<AiOptions>()
            .ValidateDataAnnotations();
        services.AddTransient<SemanticKernelClient>();

        var semanticKernelFactoryOptions = configuration.GetSection(AiOptions.ConfigName).Get<AiOptions>()!;
        services.AddSemanticKernelHttpClient();

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(semanticKernelFactoryOptions.OpenAi.ApiKey));
        var openAiGithubClient = new OpenAIClient(
            new ApiKeyCredential(semanticKernelFactoryOptions.GithubModels.ApiKey),
            new OpenAIClientOptions { Endpoint = semanticKernelFactoryOptions.GithubModels.ApiEndpoint });

        foreach (var model in Enum.GetValues<ModelConfiguration>())
        {
            var provider = model.GetProvider();

            RegisterChatModel(services, model, provider == AiProvider.GithubModels ? openAiGithubClient : openAiClient);
        }

        services.AddOpenAIAudioToText(modelId: ModelConfiguration.Whisper1.GetModelId(), apiKey: semanticKernelFactoryOptions.OpenAi.ApiKey,
                serviceId: ModelConfiguration.Whisper1.CreateServiceId())
            .AddOpenAITextToImage(modelId: ModelConfiguration.Dalle3.GetModelId(), apiKey: semanticKernelFactoryOptions.OpenAi.ApiKey,
                serviceId: ModelConfiguration.Dalle3.CreateServiceId());

        services.AddSingleton<ITextEmbeddingGenerationService>(serviceProvider =>
        {
            var apiClient = new OpenAIEmbeddingGenerator(openAiClient, "text-embedding-3-large");
            // var apiClient = serviceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var embeddingGeneratorBuilder = new EmbeddingGeneratorBuilder<string, Embedding<float>>();

            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(apiClient.GetType());
            if (logger is not null)
            {
                embeddingGeneratorBuilder.UseLogging(logger);
            }

            return embeddingGeneratorBuilder.Use(apiClient).AsTextEmbeddingGenerationService(serviceProvider);
        });

        services.AddTransient<AiExtensionsChatClient>();

        services.AddKernel();
        return services;
    }

    private static void AddSemanticKernelHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("semantic-kernel")
            .AddStandardResilienceHandler()
            .Configure((o, sp) =>
            {
                // Configure retry policy
                o.Retry.MaxRetryAttempts = 3;
                o.Retry.Delay = TimeSpan.FromSeconds(5);
                o.Retry.UseJitter = true;
                o.Retry.OnRetry = new Func<OnRetryArguments<HttpResponseMessage>, ValueTask>((onRetryArgs) =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpClient>>();
                    logger.LogWarning("Retrying request. Retry count: {RetryCount}, exception mesage: {Exception}", onRetryArgs.AttemptNumber,
                        onRetryArgs.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                });

                o.Retry.ShouldHandle = new Func<RetryPredicateArguments<HttpResponseMessage>, ValueTask<bool>>(args =>
                {
                    var isRetryableStatus = args.Outcome.Result?.StatusCode is
                        HttpStatusCode.TooManyRequests;

                    var isTimeout = args.Outcome.Exception?.InnerException is TaskCanceledException;
                    var isRateLimit = args.Outcome.Exception?.Message?.Contains("RateLimitReached") ?? false;

                    return ValueTask.FromResult(isRetryableStatus || isTimeout || isRateLimit);
                });
            });
    }

    private static void RegisterChatModel(IServiceCollection services, ModelConfiguration modelConfiguration, OpenAIClient client)
    {
        var serviceId = modelConfiguration.CreateServiceId();
        var modelId = modelConfiguration.GetModelId();
        services.AddKeyedChatClient(serviceId, builder => builder
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry()
            .Use(client.AsChatClient(modelId)));
        services.AddOpenAIChatCompletion(
            modelId: modelId,
            client,
            serviceId: serviceId);
    }
}
