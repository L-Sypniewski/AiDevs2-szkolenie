namespace AiDevs3.web.DocumentsService;

public interface IDocumentService
{
    Task<IReadOnlyCollection<Document>> ExtractAsync(
        IReadOnlyCollection<Document> documents,
        string extractionType,
        string description,
        string? context = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> ProcessDocumentsAsync(
        Uri sourceUrl,
        int? chunkSize = null,
        CancellationToken cancellationToken = default);
}
