using System.Text.Json;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public class ImageProcessor
{
    private record ImageContextResponse(
        IReadOnlyList<ImageContextEntry> Images);

    private record ImageContextEntry(
        string Name,
        string Context);

    private readonly HttpClient _httpClient;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<ImageProcessor> _logger;
    private readonly IVectorStore _vectorStore;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGeneration;

    private const ModelConfiguration ModelConfiguration = AiClients.ModelConfiguration.Gpt4o_202408;

    public ImageProcessor(
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<ImageProcessor> logger,
        IVectorStore vectorStore,
        [FromKeyedServices(ModelConfiguration.TextEmbedding3Large)]
        ITextEmbeddingGenerationService textEmbeddingGeneration)
    {
        _httpClient = httpClient;
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
        _vectorStore = vectorStore;
        _textEmbeddingGeneration = textEmbeddingGeneration;
    }

    public async Task ProcessImages(
        string article,
        IReadOnlyCollection<ImageContent> imageContents,
        string vectorStoreCollection,
        CancellationToken cancellationToken)
    {
        var imageContextResponse = await ExtractImageContext(imageContents, article, cancellationToken);

        var imageContextMap = imageContents
            .Join(
                imageContextResponse.Images,
                content => content.Filename,
                context => context.Name,
                (content, context) => (content.Filename, Value: (Content: content, Context: context)))
            .ToDictionary(x => x.Filename, x => x.Value);

        foreach (var imageContent in imageContextMap.Values)
        {
            await ProcessImage(imageContent, vectorStoreCollection, cancellationToken);
        }
    }

    private async Task ProcessImage(
        (ImageContent Content, ImageContextEntry Context) imageContext,
        string vectorStoreCollection,
        CancellationToken cancellationToken)
    {
        var recordCollection = _vectorStore.GetCollection<Guid, ArticleRag>(vectorStoreCollection);
        await recordCollection.CreateCollectionIfNotExistsAsync(cancellationToken);

        var imageContextContent = imageContext.Content;
        var existingEmbedding = await GetExistingEmbeddingsForImage(imageContextContent, cancellationToken, recordCollection);
        var imageUrl = imageContextContent.Url;

        if (existingEmbedding.TotalCount > 0 || await existingEmbedding.Results.AnyAsync(cancellationToken: cancellationToken))
        {
            _logger.LogInformation("Skipping processing for source {Source} as it already exists", imageUrl);
            return;
        }


        var imageBytes = await DownloadImage(imageUrl, cancellationToken);
        var imageDescription = await PreviewImage(imageBytes, imageContext.Content.Caption, imageUrl, cancellationToken);
        var combinedDescription = await RefineDescription(imageBytes, imageContext.Content.Caption, imageContext.Context.Context, cancellationToken);

        var source = imageUrl.ToString();
        _logger.LogInformation("Creating RAG entries for source {Source}", source);
        var ragArticles = new List<ArticleRag>
        {
            await CreateArticleRag(description: imageDescription, content: "article", source, _textEmbeddingGeneration, cancellationToken),
            await CreateArticleRag(description: combinedDescription, content: "article", source, _textEmbeddingGeneration, cancellationToken)
        };
        _logger.LogInformation("Created RAG entries for source {Source}", source);

        await recordCollection.UpsertBatchAsync(ragArticles, cancellationToken: cancellationToken).ToListAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Successfully processed source {Source}", imageUrl);
    }

    private static async Task<VectorSearchResults<ArticleRag>> GetExistingEmbeddingsForImage(
        ImageContent imageContent,
        CancellationToken cancellationToken,
        IVectorStoreRecordCollection<Guid, ArticleRag> recordCollection)
    {
        var filter = new VectorSearchFilter().EqualTo(nameof(ArticleRag.Source), imageContent.Url.ToString());

        var existingEmbedding =
            await recordCollection.VectorizedSearchAsync(new ReadOnlyMemory<float>(new float[ArticleRag.DescriptionVectorSize]),
                new VectorSearchOptions { Top = 1, IncludeTotalCount = true, Filter = filter }, cancellationToken);
        return existingEmbedding;
    }

    private static async Task<ArticleRag> CreateArticleRag(
        string description,
        string content,
        string source,
        ITextEmbeddingGenerationService embeddingGenerationService,
        CancellationToken cancellationToken)
    {
        var embedding = await embeddingGenerationService.GenerateEmbeddingAsync(description, cancellationToken: cancellationToken);
        return new ArticleRag
        {
            Id = Guid.NewGuid(),
            Source = source,
            Content = content,
            Description = description,
            DescriptionVector = embedding
        };
    }

    private async Task<byte[]> DownloadImage(Uri imageUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading image from {Url}", imageUrl);
        using var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        _logger.LogInformation("Successfully downloaded image of {Size} bytes", imageBytes.Length);

        return imageBytes;
    }

    private async Task<ImageContextResponse> ExtractImageContext(IEnumerable<ImageContent> images, string contextText, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Extracting image context using model {Model}", ModelConfiguration);
        var systemMessage = ImagePrompts.CreateExtractImageContextSystemMessage(images);
        var jsonResponse = await _semanticKernelClient.ExecutePrompt(
            ModelConfiguration,
            systemMessage,
            contextText,
            3000,
            responseFormat: "json_object",
            cancellationToken: cancellationToken);

        var response = JsonSerializer.Deserialize<ImageContextResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? throw new InvalidOperationException("Failed to deserialize image context response");

        _logger.LogInformation("Successfully extracted image context with {Count} entries", response.Images.Count);
        return response;
    }

    private async Task<string> PreviewImage(byte[] imageBytes, string caption, Uri imageUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting image preview using model {Model}", ModelConfiguration);
        var fileName = imageUrl.Segments.Last();
        var preview = await _semanticKernelClient.ExecuteVisionPrompt(
            ModelConfiguration,
            ImagePrompts.PreviewImageSystemMessage,
            $"Image caption: {caption}, filename: {fileName}",
            [new ReadOnlyMemory<byte>(imageBytes)],
            3000,
            responseFormat: "json_object",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully obtained image preview of length {Length}", preview.Length);
        return preview;
    }

    private async Task<string> RefineDescription(byte[] imageBytes, string caption, string context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refining description using model {Model}", ModelConfiguration);
        var description = await _semanticKernelClient.ExecuteVisionPrompt(
            ModelConfiguration,
            ImagePrompts.RefineDescriptionSystemMessage,
            $"Image caption: {caption}, Context: {context}",
            [new ReadOnlyMemory<byte>(imageBytes)],
            3000,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully refined description into text of length {Length}", description.Length);
        return description;
    }
}
