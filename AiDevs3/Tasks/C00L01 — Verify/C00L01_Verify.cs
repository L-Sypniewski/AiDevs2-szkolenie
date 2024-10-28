using AiDevs3.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs3.Tasks.C00L01___Verify;

public class C00L01_Verify : IModule
{
    private static Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration) =>
    {
        var baseUrl = configuration.GetValue<string>("PoligonBaseUrl")!;
        var dataUrl = $"{baseUrl}/dane.txt";

        var jsonArray = await GetJsonStringArrayFromUrl(httpClient, dataUrl);

        var apiKey = configuration.GetValue<string>("ApiKey")!;
        var answer = new { task = "POLIGON", apikey = apiKey, answer = jsonArray };
        var response = await httpClient.PostAsJsonAsync($"{baseUrl}/verify", answer);
        return await response.Content.ReadAsStringAsync();
    };

    private static async Task<string[]> GetJsonStringArrayFromUrl(HttpClient httpClient, string url)
    {
        var content = await FetchTextFileContent(httpClient, url);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return lines;
    }

    private static async Task<string> FetchTextFileContent(HttpClient httpClient, string url)
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public IServiceCollection RegisterModule(IHostApplicationBuilder hostApplicationBuilder) => hostApplicationBuilder.Services;

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(GetType().Name);
        group.MapGet("answer", SendAnswerDelegate).WithName("Send answer: Verify").WithOpenApi();
        return endpoints;
    }
}
