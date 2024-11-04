namespace AiDevs3.SemanticKernel;

public static class AddSemanticKernelExtension
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelFactoryOptions>(configuration.GetSection(SemanticKernelFactoryOptions.ConfigName))
            .AddOptionsWithValidateOnStart<SemanticKernelFactoryOptions>()
            .ValidateDataAnnotations();
        services.AddTransient<SemanticKernelFactory>();
        services.AddTransient<SemanticKernelClient>();

        return services;
    }
}
