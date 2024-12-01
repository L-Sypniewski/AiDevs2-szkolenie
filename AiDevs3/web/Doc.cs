namespace AiDevs3.web;

public record DocumentMetadata
{
    public Guid Uuid { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? ConversationUuid { get; init; }
    public string? MimeType { get; init; }
    public Dictionary<string, object> Additional { get; init; } = new();
    public int? Tokens { get; init; }
    public Dictionary<string, List<string>> Headers { get; init; } = new();
    public List<string> Urls { get; init; } = new();
    public List<string> Images { get; init; } = new();
    public string? Type { get; init; }
    public string? ContentType { get; init; }
}

public record Document
{
    public string Text { get; init; } = string.Empty;
    public DocumentMetadata Metadata { get; init; } = new();
}
