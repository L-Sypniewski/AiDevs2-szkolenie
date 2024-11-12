using System.Text.Json;
using AiDevs3.SemanticKernel;

namespace AiDevs3.Tasks.S02E02___Rozumienie_obrazu_i_wideo;

public class S02E02 : Lesson
{
    private readonly ILogger<S02E02> _logger;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly string _imagesDirectory;

    public S02E02(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S02E02> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
        _imagesDirectory = configuration["ImagesDirectory"] ??
            throw new InvalidOperationException("ImagesDirectory configuration is missing");
    }

    protected override string LessonName => "S02E02 — Rozumienie obrazu i wideo";

    private record ImageAnalysisResult(string ImagePath, string Description);
    private record AnalysisResponse(string reasoning, string cityName);

    protected override Delegate GetAnswerDelegate => async () =>
    {
        _logger.LogInformation("Processing answer request");

        var pngImagePaths = GetPngImagePaths();

        var imageAnalyses = await ProcessAllImages(pngImagePaths);

        var cityName = await AnalyzeAllResults(imageAnalyses);

        return TypedResults.Ok(cityName);
    };

    private async Task<List<ImageAnalysisResult>> ProcessAllImages(List<string> imagePaths)
    {
        var analyses = new List<ImageAnalysisResult>();
        foreach (var imagePath in imagePaths)
        {
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            _logger.LogInformation("Processing image: {ImagePath}", imagePath);

            var description = await GetImageDescription(imageBytes);
            analyses.Add(new ImageAnalysisResult(imagePath, description));
            _logger.LogInformation("Image description: {Description}", description);
        }
        _logger.LogInformation("Completed individual image analyses");
        return analyses;
    }

    private async Task<string> GetImageDescription(byte[] imageBytes)
    {
        var imageData = new ReadOnlyMemory<byte>(imageBytes);
        return await _semanticKernelClient.ExecuteVisionPrompt(
            model: "gpt-4o-2024-08-06",
            systemPrompt: "You are an expert at analyzing modern Polish maps. List street names road numbers and landmarks from the map fragment that could help identify the city. Think outloud and describe each found element in detail. Do not focus on the most famouse places for a given city, look at the bigger picture. Use cross-referencing these street names with cities in Poland.",
            userPrompt: "Analyze the map fragment and describe what you see to identify the city. Provide reasoning for your answer first and then list the street names, road numbers, and landmarks that could help identify the city. Use cross-referencing these street names with cities in Poland.",
            imageData: [imageData],
            maxTokens: 2000);
    }

    private async Task<string> AnalyzeAllResults(List<ImageAnalysisResult> analyses)
    {
        var descriptions = string.Join("\n\n", analyses.Select((x, i) => $"Map Fragment {i + 1}:\n{x.Description}"));

        var jsonResponse = await _semanticKernelClient.ExecutePrompt(
            model: "gpt-4o-2024-08-06",
            systemPrompt: """
                You are an expert at analyzing historical maps and identifying cities.
                Use cross-referencing these street names with cities in Poland.
                You MUST respond in a valid JSON format with the following structure:
                {
                    "reasoning": "explanation of your reasoning",
                    "cityName": "identified city name"
                }
                """,
            userPrompt: $"""
                Based on the following descriptions of map fragments, determine which city they represent.
                Key information: 
                - Use cross-referencing these street names with cities in Poland. From provided data and your own knowledge try to assemble a coherent picture of the city
                - 3 fragments show different parts of the same city (the one that we're looking for) while one fragment is from a different city
                - Combine provided reasoning with your own knowledge of Polish cities to identify the city
                - The city we are looking for has some granaries and fortresses, but they don't appear in maps
                - One fragment might be from a different city - it's crucial to identify any mismatching fragments
                - Return ONLY a JSON response withoutn any additional text, just json string

                Map Fragments:
                {descriptions}
                """,
            maxTokens: 2000,
            responseFormat: "json_object");

        _logger.LogInformation("jsonResponse: {JsonResponse}", jsonResponse);
        var response = JsonSerializer.Deserialize<AnalysisResponse>(jsonResponse) ??
            throw new InvalidOperationException("Failed to parse JSON response");


        return response.cityName;
    }

    private List<string> GetPngImagePaths()
    {
        if (!Directory.Exists(_imagesDirectory))
        {
            throw new DirectoryNotFoundException($"Images directory not found: {_imagesDirectory}");
        }

        var pngImagePaths = Directory.GetFiles(_imagesDirectory, "*.png", SearchOption.AllDirectories).ToList();
        if (pngImagePaths.Count == 0)
        {
            throw new InvalidOperationException("No PNG images found in the directory");
        }
        _logger.LogInformation("Found {Count} PNG images", pngImagePaths.Count);
        return pngImagePaths;
    }
}
