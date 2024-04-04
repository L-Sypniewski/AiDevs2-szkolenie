using AiDevs2_szkolenie.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public abstract class Lesson : IModule
{
    protected abstract string LessonName { get; }
    protected abstract string TaskName { get; }

    protected abstract Delegate SendAnswerDelegate { get; }

    protected string GetBaseUrl(IConfiguration configuration) => configuration.GetValue<string>("AiDevsBaseUrl")!;

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

    protected static Kernel BuildSemanticKernel(IConfiguration configuration, string model = "gpt-3.5-turbo", string embeddingModel = "text-embedding-ada-002")
    {
        var builder = Kernel.CreateBuilder();
        var openAiApiKey = configuration.GetValue<string>("OpenAiKey")!;
#pragma warning disable SKEXP0010
        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Trace).AddSimpleConsole())
            .AddOpenAIChatCompletion(model, apiKey: openAiApiKey)
            .AddOpenAITextEmbeddingGeneration(embeddingModel, apiKey: openAiApiKey);
        return builder.Build();
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(GetType().Name);
        group.MapGet("answer", SendAnswerDelegate).WithName($"Send answer: {LessonName}").WithOpenApi();

        group.MapGet("task", GetTaskDelegate).WithName($"Get task: {LessonName}").WithOpenApi();

        group.WithTags(LessonName);

        return endpoints;
    }
}
