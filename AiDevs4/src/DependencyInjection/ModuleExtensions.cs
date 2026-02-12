using System.Reflection;

namespace AiDevs4.DependencyInjection;

public static class ModuleExtensions
{
    private static readonly List<Type> s_registeredModuleTypes = [];

    public static IHostApplicationBuilder RegisterModules(this IHostApplicationBuilder hostApplicationBuilder, Assembly assembly)
    {
        s_registeredModuleTypes.Clear();
        var moduleTypes = DiscoverModules(assembly);
        foreach (var moduleType in moduleTypes)
        {
            hostApplicationBuilder.Services.AddTransient(moduleType);
            hostApplicationBuilder.Services.AddTransient(typeof(IModule), serviceProvider =>
                serviceProvider.GetRequiredService(moduleType));
        }

        s_registeredModuleTypes.AddRange(moduleTypes);
        return hostApplicationBuilder;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var modules = app.Services.GetServices<IModule>();
        foreach (var module in modules)
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
