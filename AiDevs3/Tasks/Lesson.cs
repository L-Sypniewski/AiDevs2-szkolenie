using AiDevs3.DependencyInjection;

namespace AiDevs3.Tasks;

public abstract class Lesson : IModule
{
    protected readonly IConfiguration Configuration;
    protected readonly HttpClient HttpClient;
    protected readonly string CentralaBaseUrl;
    protected readonly string ApiKey;

    protected Lesson(IConfiguration configuration, HttpClient httpClient)
    {
        Configuration = configuration;
        HttpClient = httpClient;
        CentralaBaseUrl = Configuration.GetValue<string>("CentralaBaseUrl")!;
        ApiKey = Configuration.GetValue<string>("AiDevsApiKey")!;
    }

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

    protected async Task<string> SubmitResults(string taskName, object answer)
    {
        var payload = new { task = taskName, apikey = ApiKey, answer = answer };
        var response = await HttpClient.PostAsJsonAsync($"{CentralaBaseUrl}/report", payload);
        return await response.Content.ReadAsStringAsync();
    }
}
