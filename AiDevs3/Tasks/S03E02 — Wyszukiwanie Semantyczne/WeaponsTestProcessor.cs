using AiDevs3.AiClients;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs3.Tasks.S03E02___Wyszukiwanie_Semantyczne;

public class WeaponsTestProcessor
{
    private readonly ILogger<WeaponsTestProcessor> _logger;
    private readonly IVectorStoreRecordCollection<Guid, WeaponsTestsRag> _recordCollection;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGeneration;

    public WeaponsTestProcessor(
        ILogger<WeaponsTestProcessor> logger,
        IVectorStoreRecordCollection<Guid, WeaponsTestsRag> vectorStoreRecordCollection,
        [FromKeyedServices(ModelConfiguration.TextEmbedding3Large)]
        ITextEmbeddingGenerationService textEmbeddingGeneration)
    {
        _logger = logger;
        _recordCollection = vectorStoreRecordCollection;
        _textEmbeddingGeneration = textEmbeddingGeneration;
    }

    public async Task ProcessFiles(Dictionary<string, string> files, CancellationToken cancellationToken)
    {
        if (await _recordCollection.CollectionExistsAsync(cancellationToken))
        {
            _logger.LogInformation("Skipping processing weapons test files, collection already exists");
            return;
        }

        await _recordCollection.CreateCollectionAsync(cancellationToken);

        var weaponsTests = new List<WeaponsTestsRag>();

        foreach (var (fileName, content) in files)
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var weaponName = lines[0].Trim();
            var fileContent = string.Join("\n", lines.Skip(1)).Trim();
            var date = ParseDateFromFileName(fileName);

            var embedding = await _textEmbeddingGeneration.GenerateEmbeddingAsync(fileContent, cancellationToken: cancellationToken);

            weaponsTests.Add(new WeaponsTestsRag
            {
                Id = Guid.NewGuid(),
                Source = fileName,
                Content = fileContent,
                Date = date,
                WeaponName = weaponName,
                Vector = embedding
            });
        }

        await _recordCollection.UpsertBatchAsync(weaponsTests, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully processed {Count} weapons test files", weaponsTests.Count);
    }

    private static string ParseDateFromFileName(string fileName)
    {
        var datePart = Path.GetFileNameWithoutExtension(fileName);
        var parts = datePart.Split('_');
        return $"{parts[0]}-{parts[1]}-{parts[2]}";
    }
}
