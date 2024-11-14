using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiDevs3.SemanticKernel;

namespace AiDevs3.Tasks.S02E04___Połączenie_wielu_formatów;

public class S02E04 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S02E04> _logger;

    public S02E04(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S02E04> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S02E04 — Połączenie wielu formatów";

    private record FileClassificationResponse(
        [property: JsonPropertyName("people")]
        List<string> People,
        [property: JsonPropertyName("hardware")]
        List<string> Hardware
    );

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("Starting S02E04 task");

        var extractPath = Path.Combine(Path.GetTempPath(), "factory_files");
        _logger.LogInformation("Extract path: {Path}", extractPath);

        if (!Directory.Exists(extractPath))
        {
            var zipPath = await DownloadZipFile();
            ExtractZipFile(zipPath, extractPath);
            File.Delete(zipPath);
        }
        else
        {
            _logger.LogInformation("Using existing extracted files from {Path}", extractPath);
        }

        var (txtPaths, pngPaths, mp3Paths) = GetCategorizedFilePaths(extractPath, _logger);
        var results = await ProcessFiles(txtPaths, pngPaths, mp3Paths, _semanticKernelClient);

        var response = new FileClassificationResponse(
            People: results
                .Where(r => r.Value == InformationType.People)
                .Select(r => Path.GetFileName(r.Key))
                .ToList(),
            Hardware: results
                .Where(r => r.Value == InformationType.Machines)
                .Select(r => Path.GetFileName(r.Key))
                .ToList()
        );

        var responseContent = await SubmitResults("kategorie", response);
        _logger.LogInformation("Response content: {ResponseContent}", responseContent);
        return TypedResults.Ok(new { response, apiResponse = responseContent });
    };

    public enum InformationType
    {
        People,
        Machines,
        None
    }

    private const string ClassificationPrompt = """
    <objective>
    Analyze the given text and classify it into a structured JSON response.
    Focus only on:
    - Confirmed presence of people or clear evidence of current human activity
    - Hardware repairs and physical machine issues (ignore software-related issues)
    </objective>

    <rules>
    - Return ONLY valid JSON format
    - Include exactly two fields: "thinking_process" and "answer"
    - For machines, only consider hardware/physical issues, not software
    - For people classification, ONLY include:
        * Confirmed sightings or evidence of recent presence of people that are going to be searched for in order to be detained or already have been detained or captured 
        * Captured or detained individuals
    - DO NOT classify as "people" if:
        * People were not found
        * Location is abandoned
        * Only mentions searching without results
        * Historical or past presence only
        * People are mentioned in a non-relevant context, e.g. food delivery
    - "thinking_process" must explain the classification reasoning
    - "answer" must be exactly one of: "people", "machines", or "none"
    - No markdown formatting or additional text
    - Maintain clean, parseable JSON structure
    </rules>

    <answer_format>
    {
        "thinking_process": "Brief explanation of classification logic",
        "answer": "category"
    }
    </answer_format>

    <validation>
    - Verify JSON is properly formatted
    - Confirm answer is one of three allowed values
    - Ensure thinking process explains classification
    - Double-check that "people" classification only applies to confirmed presence
    </validation>
    """;

    private async Task<string> DownloadZipFile()
    {
        var zipUrl = $"{CentralaBaseUrl}/dane/pliki_z_fabryki.zip";
        var zipPath = Path.Combine(Path.GetTempPath(), "factory_files.zip");

        if (File.Exists(zipPath))
        {
            _logger.LogInformation("Using existing ZIP file from {Path}", zipPath);
            return zipPath;
        }

        _logger.LogInformation("Downloading ZIP file from {Url}", zipUrl);
        using var response = await HttpClient.GetAsync(zipUrl);
        await using var fileStream = File.Create(zipPath);
        await response.Content.CopyToAsync(fileStream);

        return zipPath;
    }

    private void ExtractZipFile(string zipPath, string extractPath)
    {
        _logger.LogInformation("Extracting ZIP file to {Path}", extractPath);

        if (Directory.Exists(extractPath))
        {
            Directory.Delete(extractPath, true);
        }

        Directory.CreateDirectory(extractPath);
        ZipFile.ExtractToDirectory(zipPath, extractPath);
    }

    private static (string[] txtPaths, string[] pngPaths, string[] mp3Paths) GetCategorizedFilePaths(string directoryPath, ILogger<S02E04> logger)
    {
        var txtPaths = Directory.GetFiles(directoryPath, "*.txt", SearchOption.TopDirectoryOnly);
        logger.LogInformation("Found .txt files: {TxtPaths}", string.Join(", ", txtPaths));

        var pngPaths = Directory.GetFiles(directoryPath, "*.png", SearchOption.TopDirectoryOnly);
        logger.LogInformation("Found .png files: {PngPaths}", string.Join(", ", pngPaths));

        var mp3Paths = Directory.GetFiles(directoryPath, "*.mp3", SearchOption.TopDirectoryOnly);
        logger.LogInformation("Found .mp3 files: {Mp3Paths}", string.Join(", ", mp3Paths));

        return (txtPaths, pngPaths, mp3Paths);
    }

    private record AIResponse(
        [property: JsonPropertyName("thinking_process")]
        string ThinkingProcess,
        [property: JsonPropertyName("answer")]
        string Answer);

    private async Task<Dictionary<string, InformationType>> ProcessFiles(
        string[] txtPaths,
        string[] pngPaths,
        string[] mp3Paths,
        SemanticKernelClient semanticKernelClient)
    {
        var textResults = await ProcessTextFiles(txtPaths, semanticKernelClient);
        var imageResults = await ProcessImageFiles(pngPaths, semanticKernelClient);
        var audioResults = await ProcessAudioFiles(mp3Paths, semanticKernelClient);

        var results = new Dictionary<string, InformationType>();
        foreach (var dict in new[] { textResults, imageResults, audioResults })
        {
            foreach (var kvp in dict)
            {
                results[kvp.Key] = kvp.Value;
            }
        }

        return results;
    }

    private async Task<Dictionary<string, InformationType>> ProcessTextFiles(
        string[] txtPaths,
        SemanticKernelClient semanticKernelClient)
    {
        var results = new Dictionary<string, InformationType>();

        foreach (var path in txtPaths)
        {
            var content = await File.ReadAllTextAsync(path);
            var jsonResponse = await semanticKernelClient.ExecutePrompt(
                "gpt-4o-mini-2024-07-18",
                SemanticKernelFactory.AiProvider.OpenAI,
                ClassificationPrompt,
                content,
                responseFormat: "json_object",
                maxTokens: 500,
                temperature: 0.0);

            _logger.LogInformation("AI Response for text file {FileName}: {Response}",
                Path.GetFileName(path), jsonResponse);

            var response = JsonSerializer.Deserialize<AIResponse>(jsonResponse)!;
            results[path] = Enum.Parse<InformationType>(response.Answer, true);
        }

        return results;
    }

    private async Task<Dictionary<string, InformationType>> ProcessImageFiles(
        string[] pngPaths,
        SemanticKernelClient semanticKernelClient)
    {
        var results = new Dictionary<string, InformationType>();

        foreach (var path in pngPaths)
        {
            await using var fileStream = File.OpenRead(path);
            var imageBytes = new byte[fileStream.Length];
            await fileStream.ReadAsync(imageBytes);
            var images = new List<ReadOnlyMemory<byte>> { imageBytes };

            var jsonResponse = await semanticKernelClient.ExecuteVisionPrompt(
                "gpt-4o-mini-2024-07-18",
                SemanticKernelFactory.AiProvider.OpenAI,
                ClassificationPrompt,
                "Analyze this image:",
                images,
                responseFormat: "json_object",
                maxTokens: 500,
                temperature: 0.0);

            _logger.LogInformation("AI Response for image file {FileName}: {Response}",
                Path.GetFileName(path), jsonResponse);

            var response = JsonSerializer.Deserialize<AIResponse>(jsonResponse)!;
            results[path] = Enum.Parse<InformationType>(response.Answer, true);
        }

        return results;
    }

    private async Task<Dictionary<string, InformationType>> ProcessAudioFiles(
        string[] mp3Paths,
        SemanticKernelClient semanticKernelClient)
    {
        var results = new Dictionary<string, InformationType>();

        foreach (var path in mp3Paths)
        {
            await using var audioStream = File.OpenRead(path);
            var transcription = await semanticKernelClient.TranscribeAudioAsync(
                "whisper-1",
                SemanticKernelFactory.AiProvider.OpenAI,
                Path.GetFileName(path),
                audioStream,
                language: "en",
                prompt: ClassificationPrompt,
                temperature: 0.0f);

            _logger.LogInformation("Audio transcription for file {FileName}: {Transcription}",
                Path.GetFileName(path), transcription);

            var jsonResponse = await semanticKernelClient.ExecutePrompt(
                "gpt-4o-mini-2024-07-18",
                SemanticKernelFactory.AiProvider.OpenAI,
                ClassificationPrompt,
                transcription,
                responseFormat: "json_object",
                maxTokens: 500,
                temperature: 0.0);

            _logger.LogInformation("AI Response for audio file {FileName}: {Response}",
                Path.GetFileName(path), jsonResponse);

            var response = JsonSerializer.Deserialize<AIResponse>(jsonResponse)!;
            results[path] = Enum.Parse<InformationType>(response.Answer, true);
        }

        return results;
    }
}
