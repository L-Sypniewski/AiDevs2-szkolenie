using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public class TextProcessor
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<TextProcessor> _logger;
    private readonly IVectorStore _vectorStore;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGeneration;

    private const ModelConfiguration Model = ModelConfiguration.Gpt4o_Github;

    public TextProcessor(
        SemanticKernelClient semanticKernelClient,
        ILogger<TextProcessor> logger,
        IVectorStore vectorStore,
        [FromKeyedServices(ModelConfiguration.TextEmbedding3Large)]
        ITextEmbeddingGenerationService textEmbeddingGeneration)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
        _vectorStore = vectorStore;
        _textEmbeddingGeneration = textEmbeddingGeneration;
    }

    public async Task ProcessText(Article article, string vectorStoreCollection, CancellationToken cancellationToken)
    {
        var recordCollection = _vectorStore.GetCollection<Guid, ArticleRag>(vectorStoreCollection);
        await recordCollection.CreateCollectionIfNotExistsAsync(cancellationToken);

        var existingEmbedding = await GetExistingEmbeddingsForText(recordCollection, cancellationToken);

        if (existingEmbedding.TotalCount > 0 || await existingEmbedding.Results.AnyAsync(cancellationToken: cancellationToken))
        {
            _logger.LogInformation("Skipping processing for article text as it already exists");
            return;
        }

        var paragraphGroups = article.Paragraphs
            .GroupBy(p => p.HeaderTitle)
            .ToList();

        foreach (var paragraph in paragraphGroups.SelectMany(group => group))
        {
            await ProcessParagraph(paragraph, recordCollection, cancellationToken);
        }
    }

    private static async Task<VectorSearchResults<ArticleRag>> GetExistingEmbeddingsForText(
        IVectorStoreRecordCollection<Guid, ArticleRag> recordCollection,
        CancellationToken cancellationToken)
    {
        var filter = new VectorSearchFilter().AnyTagEqualTo(nameof(ArticleRag.Tags), "Text");

        var existingEmbedding =
            await recordCollection.VectorizedSearchAsync(new ReadOnlyMemory<float>(new float[ArticleRag.DescriptionVectorSize]),
                new VectorSearchOptions { Top = 1, IncludeTotalCount = true, Filter = filter }, cancellationToken);
        return existingEmbedding;
    }


    private async Task ProcessParagraph(
        Paragraph paragraph,
        IVectorStoreRecordCollection<Guid, ArticleRag> recordCollection,
        CancellationToken cancellationToken)
    {
        var content = paragraph.Text;
        var summary = await SummarizeSection(paragraph.HeaderTitle, content, cancellationToken);


        var ragArticle = await CreateArticleRag(
            content: content,
            summary: summary,
            _textEmbeddingGeneration,
            cancellationToken);

        await recordCollection.UpsertAsync(ragArticle, options: null, cancellationToken);
    }

    private async Task<string> SummarizeSection(string headerTitle, string sectionText, CancellationToken cancellationToken)
    {
        var userMessage = $"""
                           Header: {headerTitle}

                           Content:
                           {sectionText}
                           """;

        return await _semanticKernelClient.ExecutePrompt(
            Model,
            "Summarize this paragraph of text while preserving key information. Include the header context in the summary. The summary should be concise and informative containing the most important information especially the people, places, events or names and acronyms as the summary will be used for search and retrieval. The paragraph is in Polish and the summary should be in Polish too.",
            userMessage,
            1500,
            cancellationToken: cancellationToken);
    }

    private static async Task<ArticleRag> CreateArticleRag(
        string content,
        string summary,
        ITextEmbeddingGenerationService embeddingGenerationService,
        CancellationToken cancellationToken)
    {
        var embedding = await embeddingGenerationService.GenerateEmbeddingAsync(summary, cancellationToken: cancellationToken);
        return new ArticleRag
        {
            Id = Guid.NewGuid(),
            Source = "paragraph",
            Content = content,
            Description = summary,
            DescriptionVector = embedding,
            Tags = new List<string> { "S02E02", "Text" }
        };
    }
}
