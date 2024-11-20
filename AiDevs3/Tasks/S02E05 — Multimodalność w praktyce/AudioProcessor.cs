using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public class AudioProcessor
{
    private readonly HttpClient _httpClient;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<AudioProcessor> _logger;
    private readonly IVectorStore _vectorStore;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGeneration;

    private readonly ModelConfiguration _modelConfiguration = ModelConfiguration.Gpt4o_Github;

    public AudioProcessor(
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<AudioProcessor> logger,
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

    public async Task ProcessAudio(IEnumerable<AudioContent> audioContents, string vectorStoreCollection, CancellationToken cancellationToken)
    {
        foreach (var audioContent in audioContents)
        {
            await ProcessAudioFile(audioContent, vectorStoreCollection, cancellationToken);
        }
    }

    private async Task ProcessAudioFile(AudioContent audioContent, string vectorStoreCollection, CancellationToken cancellationToken)
    {
        var recordCollection = _vectorStore.GetCollection<Guid, ArticleRag>(vectorStoreCollection);
        await recordCollection.CreateCollectionIfNotExistsAsync(cancellationToken);

        var existingEmbedding = await GetExistingEmbeddingsForAudio(audioContent, cancellationToken, recordCollection);
        if (existingEmbedding.TotalCount > 0 || await existingEmbedding.Results.AnyAsync(cancellationToken: cancellationToken))
        {
            _logger.LogInformation("Skipping processing for source {Source} as it already exists", audioContent.Url);
            return;
        }

        var audioBytes = await DownloadAudio(audioContent.Url, cancellationToken);
        var audioTranscription = await GetAudioTranscription(audioBytes, audioContent.Filename, cancellationToken);

        var contextText = string.Join("\n", audioContent.Context.Select(p => p.Text));
        var contextSummary = await SummarizeContext(contextText, cancellationToken);
        var combinedDescription = await CombineDescriptions(audioTranscription, contextSummary, cancellationToken);

        var source = audioContent.Url.ToString();
        _logger.LogInformation("Creating RAG entries for source {Source}", source);
        var ragArticles = new List<ArticleRag>(3)
        {
            await CreateArticleRag(description: contextSummary, content: audioTranscription, source, _textEmbeddingGeneration, cancellationToken),
            await CreateArticleRag(description: contextText, content: audioTranscription, source, _textEmbeddingGeneration, cancellationToken),
            await CreateArticleRag(description: combinedDescription, content: audioTranscription, source, _textEmbeddingGeneration, cancellationToken)
        };
        _logger.LogInformation("Created RAG entries for source {Source}", source);

        await recordCollection.UpsertBatchAsync(ragArticles, cancellationToken: cancellationToken).ToListAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Successfully processed source {Source}", audioContent.Url);
    }

    public static async Task<VectorSearchResults<ArticleRag>> GetExistingEmbeddingsForAudio(
        AudioContent audioContent,
        CancellationToken cancellationToken,
        IVectorStoreRecordCollection<Guid, ArticleRag> recordCollection)
    {
        var filter = new VectorSearchFilter().EqualTo(nameof(ArticleRag.Source), audioContent.Url.ToString());

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

    private async Task<byte[]> DownloadAudio(Uri audioUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading audio from {Url}", audioUrl);
        using var response = await _httpClient.GetAsync(audioUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var audioBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        _logger.LogInformation("Successfully downloaded audio of {Size} bytes", audioBytes.Length);

        return audioBytes;
    }

    private async Task<string> GetAudioTranscription(byte[] audioBytes, string filename, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting audio transcription using model");

        using var memoryStream = new MemoryStream(audioBytes);
        var transcription = await _semanticKernelClient.TranscribeAudioAsync(
            ModelConfiguration.Whisper1,
            filename: filename,
            memoryStream,
            language: "pl",
            prompt: AudioProcessingPrompts.AudioTranscriptionSystemMessage,
            temperature: 0.0f,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully obtained audio transcription of length {Length}", transcription.Length);
        return transcription;
    }

    private async Task<string> SummarizeContext(string contextText, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Summarizing context paragraphs using model {Model}", _modelConfiguration);
        var userMessage = $"""
                           <user_context>
                           {contextText}
                           </user_context>
                           """;

        var summary = await _semanticKernelClient.ExecutePrompt(
            _modelConfiguration,
            AudioProcessingPrompts.AudioContextSummarySystemMessage,
            userMessage,
            1000,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully generated context summary of length {Length}", summary.Length);
        return summary;
    }

    private async Task<string> CombineDescriptions(string audioTranscription, string contextSummary, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Combining descriptions using model {Model}", _modelConfiguration);
        var userMessage = $"""
                           Audio Transcription:
                           {audioTranscription}

                           Context Summary:
                           {contextSummary}
                           """;

        var description = await _semanticKernelClient.ExecutePrompt(
            _modelConfiguration,
            AudioProcessingPrompts.CombineAudioDescriptionSystemMessage,
            userMessage,
            2000,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully combined descriptions into text of length {Length}", description.Length);
        return description;
    }
}
