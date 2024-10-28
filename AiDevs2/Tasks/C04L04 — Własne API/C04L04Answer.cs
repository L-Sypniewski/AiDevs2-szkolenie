using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

public class C04L04Answer : Lesson
{
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

    protected override string LessonName => "C04L04 — Własne API - answer";
    protected override string TaskName => "ownapi";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C04L01> logger,
        [FromServices] ILoggerFactory loggerFactory,
        [FromBody] JsonNode body) =>
    {
        var question = body["question"]!.GetValue<string>();
        var kernel = BuildSemanticKernel(configuration);
        var answer = await kernel.InvokePromptAsync(question, new KernelArguments(new OpenAIPromptExecutionSettings { MaxTokens = 100, Temperature = 0.3 }));
        return TypedResults.Ok(new { reply = answer.GetValue<string>() });
    };

    protected override void MapAdditionalEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("ownapi", SendAnswerDelegate).WithName($"Handle own API: {LessonName}").WithOpenApi();
    }
}
