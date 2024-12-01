using System.Text.RegularExpressions;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using AiDevs3.web.TextService;

namespace AiDevs3.web.DocumentsService;

public class DocumentService : IDocumentService
{
    private readonly ILogger<DocumentService> _logger;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ITextService _textService;

    public DocumentService(
        ILogger<DocumentService> logger,
        SemanticKernelClient semanticKernelClient,
        ITextService textService)
    {
        _logger = logger;
        _semanticKernelClient = semanticKernelClient;
        _textService = textService;
    }

    public async Task<IReadOnlyCollection<Document>> ExtractAsync(
        IReadOnlyCollection<Document> documents,
        string extractionType,
        string description,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Extracting content type {ExtractionType} from multiple documents",
            extractionType);

        const int BatchSize = 5;
        var extractedDocs = new List<Document>();
        var documentsList = documents.ToList();

        try
        {
            for (var i = 0; i < documentsList.Count; i += BatchSize)
            {
                var batch = documentsList.Skip(i).Take(BatchSize);
                var batchTasks = batch.Select(async document =>
                {
                    var prompt = PromptGenerator.Generate(
                        extractionType,
                        description,
                        context ?? document.Metadata.Name);

                    var completion = await _semanticKernelClient.ExecutePrompt(
                        ModelConfiguration.Gpt4o_202408,
                        systemPrompt: prompt,
                        userPrompt: document.Text,
                        maxTokens: 4000,
                        temperature: 0.2,
                        cancellationToken: cancellationToken);

                    var extractedContent = GetResultContent(completion, "final_answer");

                    return document with
                    {
                        Text = extractedContent ?? "No results",
                        Metadata = document.Metadata with
                        {
                            Description = $"Extraction result of type '{extractionType}' based on: '{description}' from document: {document.Metadata.Name}",
                            Additional = new Dictionary<string, object>(document.Metadata.Additional ?? new Dictionary<string, object>())
                            {
                                ["extracted_type"] = extractionType
                            }
                        }
                    };
                });

                var batchResults = await Task.WhenAll(batchTasks);
                extractedDocs.AddRange(batchResults);
            }

            return [.. extractedDocs.Select(doc => _textService.RestorePlaceholders(doc))];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract content from documents");
            throw new DocumentProcessingException(
                $"Failed to extract content type {extractionType} from documents", ex);
        }
    }

    private static string? GetResultContent(string content, string tagName)
    {
        var regex = new Regex($@"<{tagName}>(.*?)</{tagName}>", RegexOptions.Singleline);
        var match = regex.Match(content);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    public async Task<IReadOnlyList<Document>> ProcessDocumentsAsync(
        Uri sourceUrl,
        int? chunkSize = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing documents from {Url}", sourceUrl);

        try
        {
            using var client = new HttpClient();
            var content = await client.GetStringAsync(sourceUrl, cancellationToken);

            // Convert HTML to Markdown
            var markdown = ConvertToMarkdown(content);

            var documents = chunkSize.HasValue
                ? ChunkContent(markdown, chunkSize.Value)
                : [markdown];

            return [.. documents.Select(text => new Document
            {
                Text = text,
                Metadata = new DocumentMetadata
                {
                    Uuid = Guid.NewGuid(),
                    Source = sourceUrl.ToString(),
                    Name = Path.GetFileName(sourceUrl.LocalPath),
                    Description = $"Processed content from {sourceUrl}"
                }
            })];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process documents from {Url}", sourceUrl);
            throw new DocumentProcessingException(
                $"Failed to process documents from {sourceUrl}", ex);
        }
    }

    private static string ConvertToMarkdown(string html)
    {
        // Using a simple HTML to Markdown converter for example
        // In practice, you'd want to use a proper library like Markdig
        return html
            .Replace("<p>", "\n\n")
            .Replace("</p>", "")
            .Replace("<br>", "\n")
            .Replace("<br/>", "\n")
            .Replace("<br />", "\n");
    }

    private static IEnumerable<string> ChunkContent(string content, int chunkSize)
    {
        for (var i = 0; i < content.Length; i += chunkSize)
        {
            yield return content.Substring(i, Math.Min(chunkSize, content.Length - i));
        }
    }
}

public class DocumentProcessingException : Exception
{
    public DocumentProcessingException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
