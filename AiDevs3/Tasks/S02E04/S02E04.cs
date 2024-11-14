using System.Text.Json.Serialization;
using AiDevs3.SemanticKernel;

namespace AiDevs3.Tasks.S02E04;

public class S02E04 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S02E04> _logger;

    public S02E04(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S02E04> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S02E04 — Połączenie wielu formatów";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("Starting S02E04 task");
        
        // TODO: Implement the task logic here
        
        return TypedResults.Ok("Not implemented yet");
    };

    // Data models for the task
    public record TaskData(
        [property: JsonPropertyName("code")]
        string Code);
}
