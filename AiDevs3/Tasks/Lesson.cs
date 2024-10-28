using AiDevs3.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs3.Tasks;

#pragma warning disable SKEXP0010, SKEXP0001, SKEXP0020
public abstract class Lesson : IModule
{
    protected abstract string LessonName { get; }
    protected abstract string TaskName { get; }

    protected abstract Delegate SendAnswerDelegate { get; }

    protected static string GetBaseUrl(IConfiguration configuration) => configuration.GetValue<string>("AiDevsBaseUrl")!;

    private Delegate GetTaskDelegate => async ([FromServices] IConfiguration configuration, [FromServices] HttpClient httpClient) =>
        await AiDevsHelper.GetTask(GetBaseUrl(configuration), await GetToken(configuration, httpClient), httpClient);

    public IServiceCollection RegisterModule(IHostApplicationBuilder hostApplicationBuilder) => hostApplicationBuilder.Services;

    protected async Task<string> GetToken(IConfiguration configuration, HttpClient httpClient) =>
        await AiDevsHelper.GetToken(GetBaseUrl(configuration), TaskName, configuration.GetValue<string>("ApiKey")!, httpClient);

    protected async Task<(string token, TaskModel task)> GetTaskWithToken(IConfiguration configuration, HttpClient httpClient)
    {
        var token = await GetToken(configuration, httpClient);
        var task = await AiDevsHelper.GetTask(GetBaseUrl(configuration), token, httpClient);
        return (token, task);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(GetType().Name);
        group.MapGet("answer", SendAnswerDelegate).WithName($"Send answer: {LessonName}").WithOpenApi();

        group.MapGet("task", GetTaskDelegate).WithName($"Get task: {LessonName}").WithOpenApi();

        MapAdditionalEndpoints(group);

        group.WithTags(LessonName);

        return endpoints;
    }

    protected virtual void MapAdditionalEndpoints(IEndpointRouteBuilder endpoints) { }
}
