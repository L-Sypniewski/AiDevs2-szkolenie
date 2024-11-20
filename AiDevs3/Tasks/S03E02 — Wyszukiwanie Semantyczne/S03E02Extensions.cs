namespace AiDevs3.Tasks.S03E02___Wyszukiwanie_Semantyczne;

public static class S03E02Extensions
{
    public static IServiceCollection AddS03E02Dependencies(this IServiceCollection services)
    {
        services.AddTransient<WeaponsTestProcessor>();
        return services;
    }
}
