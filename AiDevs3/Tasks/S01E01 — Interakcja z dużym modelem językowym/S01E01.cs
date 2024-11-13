using System.Text.RegularExpressions;
using AiDevs3.SemanticKernel;

namespace AiDevs3.Tasks.S01E01___Interakcja_z_dużym_modelem_językowym;

public class S01E01 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S01E01> _logger;

    public S01E01(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S01E01> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    private const string Username = "tester";
    protected override string LessonName => "Interakcja z dużym modelem językowym";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        var baseUrl = Configuration.GetValue<string>("AgentsUrl")!;
        var password = Configuration.GetValue<string>("S01E01_Password")!;

        var questionResult = await GetQuestionFromServer(HttpClient, baseUrl, _logger);
        var answerResult = await GetAnswerFromLlm(_semanticKernelClient, questionResult, _logger);
        var submissionResult = await SubmitAnswerToServer(HttpClient, baseUrl, Username, password, answerResult, _logger);
        var downloadUrl = ExtractDownloadUrl(submissionResult, baseUrl);
        var finalContent = await GetFinalContent(HttpClient, downloadUrl, _logger);

        return TypedResults.Ok(finalContent);
    };

    private static async Task<string> GetQuestionFromServer(
        HttpClient httpClient,
        string baseUrl,
        ILogger logger)
    {
        logger.LogInformation("Starting question fetch from {BaseUrl}", baseUrl);
        var response = await httpClient.GetStringAsync(baseUrl);

        var question = ExtractQuestionFromHtml(response);
        logger.LogInformation("Retrieved question: {Question}", question);

        return question;
    }

    private static string ExtractQuestionFromHtml(string html)
    {
        var questionPattern = @"Question:.*?<br\s*/>(?<question>.*?)</p>";
        var match = Regex.Match(html, questionPattern, RegexOptions.Singleline);

        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find question in the response");
        }

        return match.Groups["question"].Value.Trim();
    }

    private static async Task<int> GetAnswerFromLlm(
        SemanticKernelClient semanticKernelClient,
        string question,
        ILogger logger)
    {
        logger.LogInformation("Requesting answer from LLM for question: {Question}", question);

        const string SystemPrompt =
            "You are a knowledgeable assistant specializing in historical dates. Always respond with only the number, without any additional text or formatting.";
        var userPrompt = $"What is the answer to this question: {question}";

        var answer = await semanticKernelClient.ExecutePrompt("Phi-3.5-mini-instruct", SemanticKernelFactory.AiProvider.GithubModels, SystemPrompt, userPrompt,
            500);
        logger.LogInformation("Received answer from LLM: {Answer}", answer);

        return ParseAnswer(answer);
    }

    private static int ParseAnswer(string answer)
    {
        var isNumber = int.TryParse(answer, out var result);
        if (!isNumber)
        {
            throw new InvalidOperationException("Answer is not a number");
        }

        return result;
    }

    private static async Task<string> SubmitAnswerToServer(
        HttpClient httpClient,
        string baseUrl,
        string username,
        string password,
        int answer,
        ILogger logger)
    {
        logger.LogInformation("Posting answer to {BaseUrl}", baseUrl);

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("answer", answer.ToString())
        ]);

        var response = await httpClient.PostAsync(baseUrl, content);
        return await response.Content.ReadAsStringAsync();
    }

    private static string ExtractDownloadUrl(string htmlResponse, string baseUrl)
    {
        const string UrlPattern = """<a\s+href="(?<relativeUrl>/files/[^"]+)">""";
        var match = Regex.Match(htmlResponse, UrlPattern);

        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find download URL in the response");
        }

        var relativeUrl = match.Groups["relativeUrl"].Value;
        var baseUri = new Uri(baseUrl);
        return new Uri(baseUri, relativeUrl).ToString();
    }

    private static async Task<string> GetFinalContent(
        HttpClient httpClient,
        string downloadUrl,
        ILogger logger)
    {
        logger.LogInformation("Extracted download URL: {DownloadUrl}", downloadUrl);
        logger.LogInformation("Fetching content from download URL");

        var finalContent = await httpClient.GetStringAsync(downloadUrl);
        logger.LogInformation("Retrieved final content, length: {ContentLength}", finalContent.Length);

        return finalContent;
    }
}
