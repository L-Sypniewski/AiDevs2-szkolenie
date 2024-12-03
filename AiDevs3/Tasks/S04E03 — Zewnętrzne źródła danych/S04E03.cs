using System.Text.Json;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using AiDevs3.Utils;
using AiDevs3.web.DocumentsService;
using AiDevs3.web.TextService;

namespace AiDevs3.Tasks.S04E03___Zewnętrzne_źródła_danych;

public class QuestionAnswer
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class S04E03 : Lesson
{
    private readonly ILogger<S04E03> _logger;
    private readonly string CompanyUrl;
    private readonly HtmlConverter _htmlConverter;
    private readonly IDocumentService _documentService;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ITextService _textService;

    public S04E03(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<S04E03> logger,
        HtmlConverter htmlConverter,
        IDocumentService documentService,
        SemanticKernelClient semanticKernel,
        ITextService textService) : base(configuration, httpClient)
    {
        _logger = logger;
        CompanyUrl = configuration.GetValue<string>("CompanyUrl")!;
        _htmlConverter = htmlConverter;
        _documentService = documentService;
        _semanticKernelClient = semanticKernel;
        _textService = textService;
    }

    protected override string LessonName => "S04E03 — Zewnętrzne źródła danych";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("S04E03 — Zewnętrzne źródła danych");
        var questions = await GetQuestions(CentralaBaseUrl, ApiKey, HttpClient, _logger);
        var answers = new Dictionary<string, string>();

        foreach (var question in questions)
        {
            var answer = await ProcessQuestion(question.Value, CompanyUrl);
            answers[question.Key] = answer;
        }

        _logger.LogInformation("Answers: {Answers}", answers);

        var submission = await SubmitResults("softo", answers);

        return TypedResults.Json(submission);
    };

    private async Task<string> ProcessQuestion(string question, string startUrl, int maxDepth = 10)
    {
        var visitedUrls = new HashSet<string>();
        var currentUrl = startUrl;
        var depth = 0;

        while (depth < maxDepth && !visitedUrls.Contains(currentUrl))
        {
            _logger.LogInformation("Processing URL: {Url} at depth {Depth}", currentUrl, depth);
            visitedUrls.Add(currentUrl);

            var markdown = await GetCompanyContentAsMarkdown(currentUrl, HttpClient, _logger);
            var document = _textService.CreateDocument(markdown);

            // Extract links from the document
            var linksDoc = (await _documentService.ExtractAsync(
                new[] { document },
                "links",
                "Extract all links from the content",
                cancellationToken: default)).First();

            var prompt = $$$"""
                            Based on the following question and page content, your task is to:
                            1. First, thoroughly analyze the current page content to find a direct answer to the question
                            2. If no direct answer exists, carefully evaluate all available links to select the one most likely to contain relevant information

                            Question: {{{question}}}

                            Page content:
                            {{{document.Text}}}

                            Available links:
                            {{{linksDoc.Text}}}

                            Link Selection Criteria:
                            - Choose the link whose title, description, or context most closely matches the question's topic
                            - Consider the hierarchical structure of the website
                            - Prioritize links that lead to detailed, topic-specific pages over general navigation links

                            Respond in JSON format (you must return only JSON without any additional text or markdown):
                            {{
                                "hasAnswer": true/false,
                                "answer": "exact answer if found",
                                "explanation": "detailed reasoning for your answer or link selection"
                                "nextLink": "URL of the most promising link for finding the answer concatenated with the base URL equal to: {{{startUrl}}}",
                            }}

                            You must respond with a JSON without any additional text or markdown formatting, return just the JSON object.
                            """;

            var jsonResponse = await _semanticKernelClient.ExecutePrompt(
                ModelConfiguration.Gpt4o_Mini_202407,
                null,
                prompt,
                maxTokens: 500,
                responseFormat: new { type = "json_object" });

            var response = JsonSerializer.Deserialize<AnalysisResponse>(jsonResponse, JsonSerializerOptions.Web)
                           ?? throw new InvalidOperationException("Failed to parse LLM response");

            if (response.HasAnswer)
            {
                _logger.LogInformation("Found answer at depth {Depth}", depth);
                return response.Answer;
            }

            if (string.IsNullOrEmpty(response.NextLink))
            {
                _logger.LogWarning("No next link suggested, search ended at depth {Depth}", depth);
                break;
            }

            currentUrl = response.NextLink;
            depth++;
            _logger.LogInformation("Moving to next URL: {Url}", currentUrl);
        }

        _logger.LogWarning("No answer found after searching {Count} pages", visitedUrls.Count);
        return string.Empty;
    }

    private class AnalysisResponse
    {
        public bool HasAnswer { get; set; }
        public string Answer { get; set; } = string.Empty;
        public string NextLink { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    private async Task<string> GetCompanyContentAsMarkdown(string url, HttpClient httpClient, ILogger logger)
    {
        logger.LogInformation("Fetching content from {Url}", url);
        var html = await httpClient.GetStringAsync(url);

        logger.LogInformation("Converting HTML content to markdown");
        return await _htmlConverter.ConvertToMarkdown(html);
    }

    private static async Task<Dictionary<string, string>> GetQuestions(string baseUrl, string apiKey, HttpClient httpClient, ILogger logger)
    {
        var url = $"{baseUrl}/data/{apiKey}/softo.json";
        logger.LogInformation("Fetching questions from {Url}", url);

        var response = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(url);

        if (response is null)
        {
            logger.LogWarning("No questions received from the API");
            return [];
        }

        logger.LogInformation("Successfully retrieved {Count} questions", response.Count);
        return response;
    }
}
