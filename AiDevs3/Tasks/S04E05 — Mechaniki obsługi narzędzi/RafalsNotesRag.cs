using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace AiDevs3.Tasks.S04E05___Mechaniki_obsługi_narzędzi;

public enum ContentType
{
    Text,
    ImageDescription
}

public record RafalsNotesRag
{
    public const string CollectionName = "rafals-notes";
    public const int VectorSize = 3072;

    [VectorStoreRecordKey]
    [TextSearchResultName]
    public required Guid Id { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required string Type { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required int PageNumber { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public int? ImageIndex { get; init; }

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    [TextSearchResultValue]
    public required string Content { get; init; }

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public required string SummarizedContent { get; init; }

    [VectorStoreRecordVector(VectorSize)]
    public required ReadOnlyMemory<float>? Vector { get; init; }
}
