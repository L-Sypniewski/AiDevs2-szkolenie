namespace AiDevs3.web.TextService;

public interface ITextService
{
    Document RestorePlaceholders(Document document);
    Task<List<Document>> SplitAsync(string text, int limit, DocumentMetadata? metadata = null);
    Document CreateDocument(string content, string? modelName = null, Dictionary<string, object>? metadataOverrides = null);
}
