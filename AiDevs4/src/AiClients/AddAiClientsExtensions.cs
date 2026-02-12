using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AiDevs4.AiClients;

public static class AddAiClientsExtensions
{
    public static IServiceCollection AddAiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.ConfigName))
            .AddOptionsWithValidateOnStart<AiOptions>()
            .ValidateDataAnnotations();

        var aiOptions = configuration.GetSection(AiOptions.ConfigName).Get<AiOptions>()!;

        var openAiClient = new OpenAIClient(new ApiKeyCredential(aiOptions.OpenAi.ApiKey));
        var openAiGithubClient = new OpenAIClient(
            new ApiKeyCredential(aiOptions.GithubModels.ApiKey),
            new OpenAIClientOptions { Endpoint = aiOptions.GithubModels.ApiEndpoint });

        foreach (var model in Enum.GetValues<ModelConfiguration>())
        {
            var provider = model.GetProvider();

            switch (provider)
            {
            case AiProvider.OpenAI:
                RegisterOpenAiChatModel(services, model, openAiClient);
                break;
            case AiProvider.GithubModels:
                RegisterOpenAiChatModel(services, model, openAiGithubClient);
                break;
            case AiProvider.Ollama:
                RegisterOllamaChatModel(services, model, aiOptions.Ollama);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unsupported AI provider");
            }
        }

        return services;
    }

    private static void RegisterOpenAiChatModel(IServiceCollection services, ModelConfiguration modelConfiguration, OpenAIClient client)
    {
        var serviceId = modelConfiguration.CreateServiceId();
        var modelId = modelConfiguration.GetModelId();
        services.AddKeyedChatClient(serviceId, client.GetChatClient(modelId).AsIChatClient())
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();
    }

    private static void RegisterOllamaChatModel(IServiceCollection services, ModelConfiguration modelConfiguration, OllamaProviderSettings? ollamaSettings)
    {
        if (ollamaSettings is null)
        {
            return;
        }

        var serviceId = modelConfiguration.CreateServiceId();
        var modelId = modelConfiguration.GetModelId();
        services.AddKeyedChatClient(serviceId, new OllamaChatClient(ollamaSettings.Endpoint, modelId))
            .UseLogging()
            .UseFunctionInvocation()
            .UseOpenTelemetry();
    }
}
