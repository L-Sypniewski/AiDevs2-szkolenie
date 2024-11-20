using System.Text.Json;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public record Question
{
    public required string Id { get; init; }
    public required string Text { get; init; }
}

public class S02E05 : Lesson
{
    private readonly ILogger<S02E05> _logger;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ImageProcessor _imageProcessor;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGeneration;
    private readonly IVectorizedSearch<ArticleRag> _vectorizedSearch;
    private readonly AudioProcessor _audioProcessor;
    private readonly TextProcessor _textProcessor;
    private const string TaskName = "arxiv";

    public S02E05(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        IVectorizedSearch<ArticleRag> vectorizedSearch,
        ImageProcessor imageProcessor,
        AudioProcessor audioProcessor,
        TextProcessor textProcessor,
        ILogger<S02E05> logger,
        [FromKeyedServices(ModelConfiguration.TextEmbedding3Large)]
        ITextEmbeddingGenerationService textEmbeddingGeneration) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _imageProcessor = imageProcessor;
        _audioProcessor = audioProcessor;
        _textProcessor = textProcessor;
        _logger = logger;
        _textEmbeddingGeneration = textEmbeddingGeneration;
        _vectorizedSearch = vectorizedSearch;
    }

    protected override string LessonName => "S02E05 — Multimodalność w praktyce";

    protected override Delegate GetAnswerDelegate => async (CancellationToken cancellationToken) =>
    {
        var baseArticleUri = new Uri($"{CentralaBaseUrl}/dane");
        var article = await GetArticle(baseArticleUri, HttpClient, _logger);

        // Process text first
        await _textProcessor.ProcessText(article, ArticleRag.CollectionName, cancellationToken);

        // Then process images and audio as before
        var articleForImageContext = await GetArticleForImageContext(baseArticleUri, HttpClient, _logger);
        await _imageProcessor.ProcessImages(articleForImageContext, article.Images, ArticleRag.CollectionName,
            cancellationToken);

        await _audioProcessor.ProcessAudio(article.Audio, ArticleRag.CollectionName, cancellationToken);

        var questions = await GetQuestions();

        var questionEmbeddings =
            await _textEmbeddingGeneration.GenerateEmbeddingsAsync(questions.Select(q => q.Text).ToList(), cancellationToken: cancellationToken);
        var questionWithEmbeddings = questions.Zip(questionEmbeddings, (q, e) => (Question: q, Vector: e)).ToList();

        var answers = await Task.WhenAll(questionWithEmbeddings.Select(async questionVectorPair =>
        {
            var searchResults =
                await _vectorizedSearch.VectorizedSearchAsync(questionVectorPair.Vector, new VectorSearchOptions { Top = 3 }, cancellationToken);
            var allSourcesForResults =
                (await searchResults.Results.ToListAsync(cancellationToken)).Select(x => x.Record.Source).Where(r => r is not null).Cast<string>();

            List<VectorSearchResults<ArticleRag>> allResultsForRelatedSources = [];
            foreach (var vectorSearchFilter in allSourcesForResults.Select(source => new VectorSearchFilter().EqualTo(nameof(ArticleRag.Source), source)))
            {
                var resultsForSource =
                    await _vectorizedSearch.VectorizedSearchAsync(questionVectorPair.Vector, new VectorSearchOptions { Filter = vectorSearchFilter },
                        cancellationToken);
                allResultsForRelatedSources.Add(resultsForSource);
            }

            var flattenedResults = (await Task.WhenAll(allResultsForRelatedSources.Select(async x => await x.Results.ToListAsync(cancellationToken))))
                .SelectMany(x => x.Select(z => z.Record.Description))
                .ToList();

            var userPrompt = $"""
                              <context>
                              {string.Join("\n", flattenedResults)}
                              </context>
                              <question>
                              Bazując na kontekście:{questionVectorPair.Question.Text}
                              """;
            var answer = await _semanticKernelClient.ExecutePrompt(ModelConfiguration.Gpt4o_202408, systemPrompt: SystemPromptAnswerQuestions, userPrompt, 8000,
                temperature: 0.3f, cancellationToken: cancellationToken);
            return (questionVectorPair.Question, answer);
        }));

        var responseDictionary = answers.ToDictionary(
            x => x.Question.Id,
            x => x.answer
        );

        _logger.LogInformation("Answers: {Answers}", JsonSerializer.Serialize(responseDictionary, new JsonSerializerOptions { WriteIndented = true }));
        var submissionResult = await SubmitResults(TaskName, responseDictionary);
        return TypedResults.Json(submissionResult, new JsonSerializerOptions { WriteIndented = true });
    };

    private static async Task<Article> GetArticle(Uri baseArticleUri, HttpClient httpClient, ILogger logger)
    {
        logger.LogInformation("Downloading article content from centrala");
        var articleUrl = $"{baseArticleUri}/arxiv-draft.html";

        var rawHtml = await httpClient.GetStringAsync(articleUrl);
        logger.LogInformation("Successfully downloaded article content");

        var cleanHtml = HtmlProcessor.ArticleFromHtml(rawHtml, baseArticleUri);
        logger.LogInformation("Successfully cleaned HTML content");
        return cleanHtml;
    }

    private static async Task<string> GetArticleForImageContext(Uri baseArticleUri, HttpClient httpClient, ILogger logger)
    {
        logger.LogInformation("Downloading article content from centrala");
        var articleUrl = $"{baseArticleUri}/arxiv-draft.html";

        var rawHtml = await httpClient.GetStringAsync(articleUrl);
        logger.LogInformation("Successfully downloaded article content");

        var cleanHtmlDocument = HtmlProcessor.CleanedHtml(rawHtml);
        logger.LogInformation("Successfully cleaned HTML content");
        return cleanHtmlDocument.DocumentNode.OuterHtml;
    }


    private async Task<List<Question>> GetQuestions()
    {
        _logger.LogInformation("Downloading questions from centrala");
        var questionsUrl = $"{CentralaBaseUrl}/data/{ApiKey}/arxiv.txt";

        try
        {
            var rawQuestions = await HttpClient.GetStringAsync(questionsUrl);
            _logger.LogInformation("Successfully downloaded questions");

            var questions = rawQuestions
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split('='))
                .Select(idAndQuestion => new Question
                {
                    Id = idAndQuestion[0],
                    Text = idAndQuestion[1]
                })
                .ToList();

            _logger.LogInformation("Parsed {Count} questions", questions.Count);
            return questions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download or parse questions");
            throw;
        }
    }

    private const string SystemPromptAnswerQuestions =
        """
        You will be given a question and a context both in Polish. Your task is to answer the question based on the information in the context. The answer must be concise and consist of one sentence. Answer the question to the best of your ability addressing the context provided with specifics - if asked for a name, provide a name; if asked for a date, provide a date. If the name is mentioned, but you're not sure if it's the correct answer, provide the name.

        The provided context will also contain media converted to text: photo descriptions and audio transcriptions that relate to the rest of the context. These media descriptions are as important as the rest of the context. Treat media descriptions as primary sources of information, equivalent to the text. If something is mentioned in a media description, it should be taken into account when answering the question as if it was mentioned in the text. Integrate media descriptions into your reasoning as if they were part of the main narrative.
        Try to incorporate the media descriptions into your answer, especially if they provide additional information or context that can help you answer the question more accurately. Combining information from text and media descriptions can lead to a more comprehensive and accurate answer. Also use your common sense and general knowledge to answer the question if you have certain knowledge that can help you answer the question more accurately.

        **Collaborative Approach:**

        Imagine three different experts are analyzing the context to answer the question. Each expert will write down one step of their thinking process, then share it with the group. The experts will proceed to the next step together, ensuring a comprehensive analysis. If any expert realizes they are wrong at any point, they will adjust their reasoning accordingly.

        **Example Steps:**

        1. **Step 1: Identify Key Information**  
           - Expert 1: Identify the main topic or event in the text and media descriptions.  
           - Expert 2: Highlight any specific details or names mentioned in both text and media.  
           - Expert 3: Note any discrepancies or unique elements in the media descriptions.

        2. **Step 2: Cross-Reference Information**  
           - Expert 1: Compare details from the text with those in the media descriptions.  
           - Expert 2: Ensure consistency and coherence between text and media.  
           - Expert 3: Identify any additional insights provided by the media descriptions.

        3. **Step 3: Formulate Answer**  
           - Expert 1: Synthesize information from both text and media to form a concise answer.  
           - Expert 2: Ensure the answer directly addresses the question with specifics.  
           - Expert 3: Review the answer for completeness and accuracy.

        By following this structured approach, you will ensure that media descriptions are treated with the same importance as text, leading to a more thorough and accurate analysis of the context.
        """;
}
