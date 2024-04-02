using System.Reflection;

namespace AiDevs2_szkolenie.DependencyInjection;

public static class ModuleExtensions
{
    private static readonly List<IModule> s_registeredModules = new List<IModule>();

    /// <summary>
    /// Registers all discovered modules with the specified service collection.
    /// </summary>
    /// <param name="hostApplicationBuilder">Host application builder, e.g.  <see cref="T:Microsoft.AspNetCore.Builder.WebApplicationBuilder" /></param>
    /// <param name="assembly">The assembly to discover the modules from.</param>
    /// <returns>The modified service collection.</returns>
    public static IHostApplicationBuilder RegisterModules(this IHostApplicationBuilder hostApplicationBuilder, Assembly assembly)
    {
        s_registeredModules.Clear();
        var collection = DiscoverModules(assembly);
        foreach (var module in collection)
            module.RegisterModule(hostApplicationBuilder);
        s_registeredModules.AddRange(collection);
        return hostApplicationBuilder;
    }

    /// <summary>
    /// Maps the endpoints for all registered modules using the specified web application.
    /// </summary>
    /// <param name="app">The web application to map the endpoints with.</param>
    /// <returns>The modified web application.</returns>
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        foreach (var module in s_registeredModules.ToArray())
            module.MapEndpoints(app);
        return app;
    }

    /// <summary>
    /// Discovers and returns all types that implement the IModule interface from the current assembly.
    /// </summary>
    /// <returns>An enumerable collection of discovered modules.</returns>
    private static IModule[] DiscoverModules(Assembly assembly)
    {
        var moduleType = typeof(IModule);
        return assembly.GetTypes()
            .Where<Type>(type => type.IsClass && moduleType.IsAssignableFrom(type))
            .Where<Type>(type => !type.IsAbstract)
            .Select((Func<Type, IModule>) (type => (IModule) Activator.CreateInstance(type)!))
            .ToArray();
    }
}
