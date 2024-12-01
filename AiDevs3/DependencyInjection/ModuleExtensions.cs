using System.Reflection;

namespace AiDevs3.DependencyInjection;

public static class ModuleExtensions
{
    private static readonly List<Type> s_registeredModuleTypes = [];

    public static IHostApplicationBuilder RegisterModules(this IHostApplicationBuilder hostApplicationBuilder, Assembly assembly, IServiceCollection services)
    {
        s_registeredModuleTypes.Clear();
        var moduleTypes = DiscoverModules(assembly);
        foreach (var moduleType in moduleTypes)
        {
            // Register each module type as scoped
            services.AddTransient(moduleType);
            // Also register it as its interface
            services.AddTransient(typeof(IModule), serviceProvider =>
                serviceProvider.GetRequiredService(moduleType));
            s_registeredModuleTypes.Add(moduleType);
        }
        return hostApplicationBuilder;
    }

    public static WebApplication MapEndpoints(this WebApplication app, IServiceProvider serviceProvider)
    {
        var moduleServices = serviceProvider.GetServices<IModule>();

        foreach (var module in moduleServices)
        {
            module.MapEndpoints(app);
        }

        return app;
    }

    private static Type[] DiscoverModules(Assembly assembly)
    {
        var moduleType = typeof(IModule);
        return [.. assembly.GetTypes()
            .Where(type => type.IsClass && moduleType.IsAssignableFrom(type))
            .Where(type => !type.IsAbstract)];
    }
}
