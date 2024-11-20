using AiDevs3.AiClients;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs3.Tasks.S03E02___Wyszukiwanie_Semantyczne;

public class S03E02 : Lesson
{
    private readonly ILogger<S03E02> _logger;
    private readonly WeaponsTestProcessor _weaponsTestProcessor;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGeneration;
    private readonly IVectorStore _vectorStore;

    public S03E02(
        IConfiguration configuration,
        HttpClient httpClient,
        [FromKeyedServices(ModelConfiguration.TextEmbedding3Large)]
        ITextEmbeddingGenerationService textEmbeddingGeneration,
        IVectorStore vectorStore,
        WeaponsTestProcessor weaponsTestProcessor,
        ILogger<S03E02> logger) : base(configuration, httpClient)
    {
        _textEmbeddingGeneration = textEmbeddingGeneration;
        _vectorStore = vectorStore;
        _logger = logger;
        _weaponsTestProcessor = weaponsTestProcessor;
    }

    protected override string LessonName => "S03E02 — Wyszukiwanie Semantyczne";

    protected override Delegate GetAnswerDelegate => async (CancellationToken cancellationToken) =>
    {
        _logger.LogInformation("Starting S03E02 lesson");

        var weaponsTestsFiles = await GetWeaponsTestFiles();
        await _weaponsTestProcessor.ProcessFiles(weaponsTestsFiles, cancellationToken);

        const string SearchQuery = "W raporcie, z którego dnia znajduje się wzmianka o kradzieży prototypu broni?";
        var result = await SearchWeaponTests(SearchQuery, cancellationToken);

        var answer = result.Date;
        _logger.LogInformation("Answer: {Answer} from document content: {Content}", answer, result.Content);
        var submissionResult = await SubmitResults(taskName: "wektory", answer);
        _logger.LogInformation("Submission result: {SubmissionResult}", submissionResult);
        return TypedResults.Json(submissionResult);
    };

    private async Task<WeaponsTestsRag> SearchWeaponTests(string query, CancellationToken cancellationToken)
    {
        var collection = _vectorStore.GetCollection<Guid, WeaponsTestsRag>(WeaponsTestsRag.CollectionName);
        var queryEmbedding = await _textEmbeddingGeneration.GenerateEmbeddingAsync(query, cancellationToken: cancellationToken);

        var searchResults = await collection.VectorizedSearchAsync(
            queryEmbedding,
            new VectorSearchOptions { Top = 1 },
            cancellationToken
        );

        return (await searchResults.Results.FirstAsync(cancellationToken)).Record;
    }

    private static async Task<Dictionary<string, string>> GetWeaponsTestFiles()
    {
        var directory = Path.Combine("Tasks", "S03E02 — Wyszukiwanie Semantyczne", "weapons_tests");
        var files = Directory.GetFiles(directory, "*.txt");
        var fileContents = new Dictionary<string, string>(files.Length);

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var fileName = Path.GetFileName(file);
            fileContents.Add(fileName, content);
        }

        return fileContents;
    }
}
