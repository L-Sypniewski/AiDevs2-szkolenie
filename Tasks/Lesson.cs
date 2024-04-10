using AiDevs2_szkolenie.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;

namespace AiDevs2_szkolenie.Tasks;

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

    protected static Kernel BuildSemanticKernel(IConfiguration configuration, string model = "gpt-3.5-turbo", string embeddingModel = "text-embedding-ada-002")
    {
        var builder = Kernel.CreateBuilder();
        var openAiApiKey = configuration.GetValue<string>("OpenAiKey")!;
        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole())
            .AddOpenAIChatCompletion(model, apiKey: openAiApiKey)
            .AddOpenAITextEmbeddingGeneration(embeddingModel, apiKey: openAiApiKey);
        builder.Services.AddHttpClient<CurrencyPlugin>();
        builder.Services.AddHttpClient<PopulationPlugin>();
        return builder.Build();
    }

    protected static ISemanticTextMemory BuildMemory(
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        string embeddingModel = "text-embedding-ada-002")
    {
        var qdrantUrl = configuration.GetValue<string>("QdrantUrl")!;

        var dimensions = embeddingModel switch
        {
            "text-embedding-ada-002" => 1536,
            _ => throw new ArgumentException("Unknown embedding model", nameof(embeddingModel))
        };

        return new MemoryBuilder().WithOpenAITextEmbeddingGeneration(embeddingModel, apiKey: configuration.GetValue<string>("OpenAiKey")!)
            .WithLoggerFactory(loggerFactory)
            .WithQdrantMemoryStore(qdrantUrl, dimensions)
            .Build();
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
