using AiDevs3.DependencyInjection;

namespace AiDevs3.Tasks;

#pragma warning disable SKEXP0010, SKEXP0001, SKEXP0020
public abstract class Lesson : IModule
{
    protected abstract string LessonName { get; }
    protected abstract Delegate GetAnswerDelegate { get; }

    public IServiceCollection RegisterModule(IHostApplicationBuilder hostApplicationBuilder) => hostApplicationBuilder.Services;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(GetType().Name);
        group.MapGet("answer", GetAnswerDelegate).WithName($"Get answer: {LessonName}").WithOpenApi();

        MapAdditionalEndpoints(group);

        group.WithTags(LessonName);

        return endpoints;
    }

    protected virtual void MapAdditionalEndpoints(IEndpointRouteBuilder endpoints) { }
}
