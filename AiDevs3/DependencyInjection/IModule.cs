namespace AiDevs3.DependencyInjection;

public interface IModule
{
    /// <summary>
    /// Registers the module with the specified service collection.
    /// </summary>
    /// <param name="hostApplicationBuilder">Host application builder, e.g.  <see cref="T:Microsoft.AspNetCore.Builder.WebApplicationBuilder" /></param>
    /// <returns>The modified service collection.</returns>
    IServiceCollection RegisterModule(IHostApplicationBuilder hostApplicationBuilder);

    /// <summary>
    /// Maps the endpoints for the module using the specified endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to map the endpoints with.</param>
    /// <returns>The modified endpoint route builder.</returns>
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
