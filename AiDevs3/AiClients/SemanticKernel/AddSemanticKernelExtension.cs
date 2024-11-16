using Microsoft.SemanticKernel;

namespace AiDevs3.AiClients.SemanticKernel;

public static class AddSemanticKernelExtension
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(sp =>
        {
            var builder = Kernel.CreateBuilder();

            foreach (var model in Enum.GetValues<ModelConfiguration>())
            {
                var modelId = model.GetModelId();
                var provider = model.GetProvider();
                var serviceId = model.CreateServiceId();

                switch (provider)
                {
                case AiProvider.OpenAI:
                    builder.AddOpenAIChatCompletion(
                        modelId: modelId,
                        apiKey: configuration["OpenAI:ApiKey"]!,
                        serviceId: serviceId);
                    break;
                case AiProvider.GithubModels:
                    builder.AddOpenAIChatCompletion(
                        modelId: modelId,
                        apiKey: configuration["Github:ApiKey"]!,
                        serviceId: serviceId,
                        endpoint: new Uri(configuration["Github:Endpoint"]!));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider));
                }
            }

            // Add additional services like DALL-E, Whisper etc.
            builder.AddOpenAITextToImage(
                serviceId: "dall-e-3",
                modelId: "dall-e-3",
                apiKey: configuration["OpenAI:ApiKey"]!);

            return builder.Build();
        });

        services.AddScoped<SemanticKernelClient>();
        return services;
    }
}
