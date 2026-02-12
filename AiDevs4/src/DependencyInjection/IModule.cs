namespace AiDevs4.DependencyInjection;

public interface IModule
{
    IServiceCollection RegisterModule(IHostApplicationBuilder hostApplicationBuilder);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
