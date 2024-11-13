using AiDevs3.SemanticKernel;

namespace AiDevs3.Tasks.S02E03___Generowanie_i_modyfikacja_obrazów;

public class S02E03 : Lesson
{
    private readonly ILogger<S02E03> _logger;
    private readonly SemanticKernelClient _semanticKernelClient;

    private record RobotDescription(string Description);

    public S02E03(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S02E03> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S02E03 — Generowanie i modyfikacja obrazów";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("Processing answer request");

        var description = await GetRobotDescription();
        _logger.LogInformation("Got robot description: {Description}", description);

        var imageUrl = await _semanticKernelClient.ExecuteDalle3ImagePrompt(
            description,
            SemanticKernelClient.DallE3ImageSize.Square1024,
            SemanticKernelClient.DallE3Quality.HD);
        _logger.LogInformation("Generated image URL: {ImageUrl}", imageUrl);

        var result = await SubmitResults("robotid", imageUrl);
        return TypedResults.Ok(result);
    };

    private async Task<string> GetRobotDescription()
    {
        var url = $"{CentralaBaseUrl}/data/{ApiKey}/robotid.json";
        _logger.LogInformation("Getting robot description from url: {Url}", url);
        var response = await HttpClient.GetFromJsonAsync<RobotDescription>(url);
        return response?.Description ?? throw new Exception("Failed to get robot description");
    }

}
