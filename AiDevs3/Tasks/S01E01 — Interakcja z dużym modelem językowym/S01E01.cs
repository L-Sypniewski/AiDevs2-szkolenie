using AiDevs3.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AiDevs3.Tasks.S01E01___Interakcja_z_dużym_modelem_językowym;

public class S01E01 : Lesson
{
    protected override string LessonName => "Interakcja z dużym modelem językowym";

    protected override Delegate GetAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] SemanticKernelClient semanticKernelClient,
        [FromServices] ILogger<S01E01> logger,
        [FromServices] IConfiguration configuration) =>
    {
        var baseUrl = configuration.GetValue<string>("AgentsUrl")!;
        var password = configuration.GetValue<string>("S01E01_Password")!;
        const string Username = "tester";

        logger.LogInformation("Starting question fetch from {BaseUrl}", baseUrl);
        var question = await FetchQuestion(httpClient, baseUrl);
        logger.LogInformation("Retrieved question: {Question}", question);

        logger.LogInformation("Requesting answer from LLM for question: {Question}", question);
        var answer = await FetchAnswer(semanticKernelClient, Username, password, question);
        logger.LogInformation("Received answer from LLM: {Answer}", answer);

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("username", Username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("answer", answer.ToString())
        ]);

        logger.LogInformation("Posting answer to {BaseUrl}", baseUrl);
        var response = await httpClient.PostAsync(baseUrl, content);
        var htmlResponse = await response.Content.ReadAsStringAsync();

        // Extract relative URL from HTML response
        const string UrlPattern = """<a\s+href="(?<relativeUrl>/files/[^"]+)">""";
        var match = Regex.Match(htmlResponse, UrlPattern);

        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find download URL in the response");
        }

        var relativeUrl = match.Groups["relativeUrl"].Value;
        var baseUri = new Uri(baseUrl);
        var fullUrl = new Uri(baseUri, relativeUrl).ToString();

        logger.LogInformation("Extracted download URL: {FullUrl}", fullUrl);

        logger.LogInformation("Fetching content from download URL");
        var finalContent = await httpClient.GetStringAsync(fullUrl);
        logger.LogInformation("Retrieved final content, length: {ContentLength}", finalContent.Length);

        return TypedResults.Ok(finalContent);
    };

    private static async Task<string> FetchQuestion(HttpClient httpClient, string baseUrl)
    {
        var response = await httpClient.GetStringAsync(baseUrl);
        var questionPattern = @"Question:.*?<br\s*/>(?<question>.*?)</p>";
        var match = Regex.Match(response, questionPattern, RegexOptions.Singleline);

        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find question in the response");
        }

        return match.Groups["question"].Value.Trim();
    }

    private static async Task<int> FetchAnswer(SemanticKernelClient semanticKernelClient, string username, string password, string question)
    {
        const string SystemPrompt =
            "You are a knowledgeable assistant specializing in historical dates. Always respond with only the number, without any additional text or formatting.";
        var userPrompt = $"What is the answer to this question: {question}";
        var answer = await semanticKernelClient.ExecutePrompt("gpt-4o-mini-2024-07-18", SystemPrompt, userPrompt);

        var isNumber = int.TryParse(answer, out var result);
        if (!isNumber)
        {
            throw new InvalidOperationException("Answer is not a number");
        }

        return result;
    }
}
