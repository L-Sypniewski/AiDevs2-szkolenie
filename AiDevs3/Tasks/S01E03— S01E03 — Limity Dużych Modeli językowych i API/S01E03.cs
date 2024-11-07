using System.Text.Json.Serialization;
using AiDevs3.SemanticKernel;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs3.Tasks.S01E03__S01E03___Limity_Dużych_Modeli_językowych_i_API;

public class S01E03 : Lesson
{
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

    protected override Delegate GetAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] SemanticKernelClient semanticKernelClient,
        [FromServices] ILogger<S01E03> logger,
        [FromServices] IConfiguration configuration) =>
    {
        var centralaBaseUrl = configuration.GetValue<string>("CentralaBaseUrl")!;
        var apiKey = configuration.GetValue<string>("AiDevsApiKey")!;

        var jsonContent = await GetFile(httpClient, centralaBaseUrl, apiKey, logger);
        var document = System.Text.Json.JsonSerializer.Deserialize<TestDocument>(jsonContent)!;

        var correctedDocument = await ValidateAndAnswerQuestions(document, semanticKernelClient, logger);
        var documentWithApiKey = correctedDocument with { ApiKey = apiKey };
        var answer = new { task = "JSON", apikey = apiKey, answer = documentWithApiKey };


        var response = await httpClient.PostAsJsonAsync($"{centralaBaseUrl}/report ", answer);
        var responseContent = response.Content.ReadAsStringAsync();
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

        var answer = await semanticKernelClient.ExecutePrompt("Phi-3.5-MoE-instruct", SystemPrompt, $"Question: {question}", maxTokens: 50, temperature: 0.0);
        logger.LogInformation("AI provided answer: {Answer}", answer);
        return answer.Trim();
    }
}
