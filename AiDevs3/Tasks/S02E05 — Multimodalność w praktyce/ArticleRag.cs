using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace AiDevs3.Tasks.S02E05___Multimodalność_w_praktyce;

public record ArticleRag
{
    public const string CollectionName = "rag";
    public const int DescriptionVectorSize = 3072;

    [VectorStoreRecordKey]
    [TextSearchResultName]
    public required Guid Id { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    [TextSearchResultLink]
    public string? Source { get; init; }

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public required string Content { get; init; }

    [VectorStoreRecordData]
    [TextSearchResultValue]
    public required string Description { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public IReadOnlyList<string>? Tags { get; init; }

    [VectorStoreRecordVector(DescriptionVectorSize)]
    public required ReadOnlyMemory<float>? DescriptionVector { get; init; }
}
