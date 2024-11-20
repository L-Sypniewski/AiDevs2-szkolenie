using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace AiDevs3.Tasks.S03E02___Wyszukiwanie_Semantyczne;

public record WeaponsTestsRag
{
    public const string CollectionName = "weapons-tests";
    public const int DescriptionVectorSize = 3072;


    [VectorStoreRecordKey]
    [TextSearchResultName]
    public required Guid Id { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    [TextSearchResultLink]
    public string? Source { get; init; }

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    [TextSearchResultValue]
    public required string Content { get; init; }

    [VectorStoreRecordVector(DescriptionVectorSize)]
    public required ReadOnlyMemory<float>? Vector { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required string Date { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required string WeaponName { get; init; }
}
