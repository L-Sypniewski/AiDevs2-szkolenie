using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs3.Tasks.S04E05___Mechaniki_obsługi_narzędzi;

public record PageContent(
    string TextContent,
    IReadOnlyCollection<(int Index, string Description)> ImageDescriptions);

public class RafalsNotesProcessor
{
    private const string CONTEXTUAL_SUMMARY_PROMPT = """
                                                     Wypisz wyłącznie konkretne fakty z podanego fragmentu w kontekście całego dokumentu.
                                                     Uwzględnij tylko istniejące informacje w następujących kategoriach:

                                                     1. Powiązaniach między osobami - kto z kim i w jakim celu
                                                     2. Konkretnych datach i miejscach - kiedy i gdzie coś się wydarzyło/wydarzy
                                                     3. Planowanych lub wykonanych działaniach - co dokładnie zostało/zostanie zrobione
                                                     4. Związkach przyczynowo-skutkowych między wydarzeniami
                                                     5. Wspomnianych technologiach i ich zastosowaniu

                                                     Format odpowiedzi:
                                                     - Każdy fakt w osobnej linii rozpoczynającej się od "-"
                                                     - Zawsze podawaj pełny kontekst (kto/co/gdzie/kiedy)
                                                     - Pomijaj kategorie bez konkretnych informacji
                                                     - Nie interpretuj intencji ani emocji
                                                     - Zamiast pisać o braku informacji na dany temat po prostu ją pomiń
                                                     - Nie spekuluj, nie dodawaj własnych wniosków
                                                     - Pisz zwięźle i konkretnie

                                                     Kontekst dokumentu:
                                                     {documentSummary}

                                                     Fragment do podsumowania:
                                                     {content}
                                                     """;

    private const string NO_TEXT_MARKER = "<BRAK TEKSTU>";
    private readonly ILogger<RafalsNotesProcessor> _logger;
    private readonly IVectorStoreRecordCollection<Guid, RafalsNotesRag> _recordCollection;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGeneration;
    private readonly SemanticKernelClient _semanticKernelClient;

    public RafalsNotesProcessor(
        ILogger<RafalsNotesProcessor> logger,
        IVectorStoreRecordCollection<Guid, RafalsNotesRag> vectorStoreRecordCollection,
        [FromKeyedServices(ModelConfiguration.TextEmbedding3Large)]
        ITextEmbeddingGenerationService textEmbeddingGeneration,
        SemanticKernelClient semanticKernelClient)
    {
        _logger = logger;
        _recordCollection = vectorStoreRecordCollection;
        _textEmbeddingGeneration = textEmbeddingGeneration;
        _semanticKernelClient = semanticKernelClient;
    }

    public async Task ProcessPages(List<PageContent> pages, string documentSummary, CancellationToken cancellationToken)
    {
        if (await _recordCollection.CollectionExistsAsync(cancellationToken))
        {
            _logger.LogInformation("Collection already exists, skipping creation");
            return;
        }

        await _recordCollection.CreateCollectionAsync(cancellationToken);

        for (var pageNumber = 0; pageNumber < pages.Count; pageNumber++)
        {
            var records = new List<RafalsNotesRag>();
            var page = pages[pageNumber];

            // Create contextual summary for the page
            var pageContextualSummary = await CreateContextualSummary(
                page.TextContent,
                documentSummary,
                cancellationToken);

            // Generate embedding for the contextualized content
            var textEmbedding = await _textEmbeddingGeneration.GenerateEmbeddingAsync(
                pageContextualSummary,
                cancellationToken: cancellationToken);

            records.Add(new RafalsNotesRag
            {
                Id = Guid.NewGuid(),
                Type = nameof(ContentType.Text),
                PageNumber = pageNumber + 1,
                Content = page.TextContent,
                SummarizedContent = pageContextualSummary,
                Vector = textEmbedding,
                ImageIndex = null
            });

            // Process image descriptions with context
            foreach (var (imageIndex, description) in page.ImageDescriptions)
            {
                if (description.Contains(NO_TEXT_MARKER))
                {
                    _logger.LogInformation("Skipping image {ImageIndex} on page {PageNumber} - no text found",
                        imageIndex, pageNumber + 1);
                    continue;
                }

                var imageContextualSummary = await CreateContextualSummary(
                    description,
                    documentSummary,
                    cancellationToken);

                var imageEmbedding = await _textEmbeddingGeneration.GenerateEmbeddingAsync(
                    imageContextualSummary,
                    cancellationToken: cancellationToken);

                records.Add(new RafalsNotesRag
                {
                    Id = Guid.NewGuid(),
                    Type = nameof(ContentType.ImageDescription),
                    PageNumber = pageNumber + 1,
                    ImageIndex = imageIndex,
                    Content = description,
                    SummarizedContent = imageContextualSummary,
                    Vector = imageEmbedding
                });
            }

            await _recordCollection.UpsertBatchAsync(records, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully processed {Count} content items for page {PageNumber}", records.Count, pageNumber + 1);
        }
    }

    private async Task<string> CreateContextualSummary(
        string content,
        string documentSummary,
        CancellationToken cancellationToken)
    {
        var prompt = CONTEXTUAL_SUMMARY_PROMPT
            .Replace("{documentSummary}", documentSummary)
            .Replace("{content}", content);

        return await _semanticKernelClient.ExecutePrompt(
            ModelConfiguration.Gpt4o_202408,
            null,
            prompt,
            maxTokens: 1000,
            temperature: 0.3,
            cancellationToken: cancellationToken);
    }

    public async Task<string> SearchContent(string query, CancellationToken cancellationToken)
    {
        var queryEmbedding = await _textEmbeddingGeneration.GenerateEmbeddingAsync(
            query,
            cancellationToken: cancellationToken);

        var searchResults = await _recordCollection.VectorizedSearchAsync(
            queryEmbedding,
            new VectorSearchOptions { Top = 2 },
            cancellationToken
        );

        var results = await searchResults.Results.ToListAsync(cancellationToken);
        var contentFromAllResults = results.Select((x, i) => $"<SearchResult id={i}>\n{x.Record.SummarizedContent}\n</SearchResult>\n").ToArray();
        var joined = string.Join("\n", contentFromAllResults);
        return joined;
    }
}
