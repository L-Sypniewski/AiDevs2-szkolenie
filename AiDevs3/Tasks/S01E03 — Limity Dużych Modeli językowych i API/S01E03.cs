using System.Text.Json.Serialization;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;

namespace AiDevs3.Tasks.S01E03___Limity_Dużych_Modeli_językowych_i_API;

public class S01E03 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S01E03> _logger;

    public S01E03(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S01E03> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S01E03 — Limity Dużych Modeli językowych i API";

    public record TestQuestion(
        [property: JsonPropertyName("q")]
        string Q,
        [property: JsonPropertyName("a")]
        string A);

    public record TestDataItem(
        [property: JsonPropertyName("question")]
        string Question,
        [property: JsonPropertyName("answer")]
        int Answer,
        [property: JsonPropertyName("test")]
        TestQuestion? Test);

    public record TestDocument(
        [property: JsonPropertyName("copyright")]
        string Copyright,
        [property: JsonPropertyName("apikey")]
        string ApiKey,
        [property: JsonPropertyName("description")]
        string Description,
        [property: JsonPropertyName("test-data")]
        IReadOnlyList<TestDataItem> TestData);

    protected override Delegate GetAnswerDelegate => async () =>
    {
        var jsonContent = await GetFile(HttpClient, CentralaBaseUrl, ApiKey, _logger);
        var document = System.Text.Json.JsonSerializer.Deserialize<TestDocument>(jsonContent)!;

        var correctedDocument = await ValidateAndAnswerQuestions(document, _semanticKernelClient, _logger);
        var documentWithApiKey = correctedDocument with { ApiKey = ApiKey };

        var responseContent = await SubmitResults("JSON", documentWithApiKey);
        return TypedResults.Ok(responseContent);
    };

    private static async Task<string> GetFile(
        HttpClient httpClient,
        string centralaBaseUrl,
        string apiKey,
        ILogger logger)
    {
        var url = $"{centralaBaseUrl}/data/{apiKey}/json.txt";

        logger.LogInformation("Fetching file from {url}", url);
        var response = await httpClient.GetStringAsync(url);
        logger.LogInformation("Retrieved file content");

        return response;
    }

    private static async Task<TestDocument> ValidateAndAnswerQuestions(
        TestDocument document,
        SemanticKernelClient semanticKernelClient,
        ILogger logger)
    {
        var correctedItems = new List<TestDataItem>();

        foreach (var item in document.TestData)
        {
            var numbers = item.Question.Split('+')
                .Select(n => int.Parse(n.Trim()))
                .ToArray();
            var correctAnswer = numbers[0] + numbers[1];

            var answeredTest = item.Test == null ? null : item.Test with { A = await GetAnswerFromAi(item.Test.Q, semanticKernelClient, logger) };

            correctedItems.Add(new TestDataItem(item.Question, correctAnswer, answeredTest));
        }

        return document with { TestData = correctedItems };
    }

    private static async Task<string> GetAnswerFromAi(
        string question,
        SemanticKernelClient semanticKernelClient,
        ILogger logger)
    {
        logger.LogInformation("Getting AI answer for question: {Question}", question);
        const string SystemPrompt = """
                                    Answer the following question in English.
                                    Provide ONLY the direct answer without any explanations or additional text
                                    """;

        var answer = await semanticKernelClient.ExecutePrompt(ModelConfiguration.Phi35_MoE_Instruct, SystemPrompt, $"Question: {question}", maxTokens: 50, temperature: 0.0);
        logger.LogInformation("AI provided answer: {Answer}", answer);
        return answer.Trim();
    }
}
