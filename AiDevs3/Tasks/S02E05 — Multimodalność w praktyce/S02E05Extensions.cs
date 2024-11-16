namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public static class S02E05Extensions
{
    public static IServiceCollection AddS02E05Dependencies(this IServiceCollection services)
    {
        services.AddTransient<ImageProcessor>();
        services.AddTransient<AudioProcessor>();
        services.AddTransient<TextProcessor>();
        
        return services;
    }
}
