using System.ComponentModel;
using System.Text.Json;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S04E01___Interfejs;

public class AnalyzePhotoPlugin
{
    private readonly SemanticKernelClient _client;
    private readonly Kernel _kernel;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnalyzePhotoPlugin> _logger;

    public AnalyzePhotoPlugin(SemanticKernelClient client, Kernel kernel, HttpClient httpClient, ILogger<AnalyzePhotoPlugin> logger)
    {
        _client = client;
        _kernel = kernel;
        _httpClient = httpClient;
        _logger = logger;
    }

    private async Task<byte[]> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading image from URL: {ImageUrl}", imageUrl);
        var result = await _httpClient.GetByteArrayAsync(imageUrl, cancellationToken);
        _logger.LogInformation("Image downloaded successfully. Size: {Size} bytes", result.Length);
        return result;
    }

    [KernelFunction("analyze_photo_quality")]
    [Description("Analyzes photo quality to determine if any corrections are needed")]
    [return:
        Description(
            "Analysis of the photo quality, one of 'photo_looks_correct_no_action_needed', 'repair', 'brighten', or 'darken'.")]
    public async Task<PhotoQualityAnalysis> AnalyzePhotoQualityAsync(
        [Description("URL of the photo to analyze")] string imageUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing photo quality for URL: {ImageUrl}", imageUrl);

        var imageData = await DownloadImageAsync(imageUrl, cancellationToken);

        var analysis = await _client.ExecuteVisionPrompt(
            ModelConfiguration.Gpt4o_Mini_202407,
            null,
            """
            Analyze only the technical quality of this image and respond in JSON format with the following properties:
            - isDark (boolean): is the image too dark to see details clearly?
            - isBright (boolean): is the image too bright or overexposed?
            - hasNoiseOrGlitches (boolean): are there visible artifacts, noise, or quality issues?
            - if there are different issues then 'repair' has the highest priority, followed by 'brighten' and 'darken'
            - recommendedAction (string: 'photo_looks_correct_no_action_needed', 'repair', 'brighten', or 'darken')
            - return json without markdown or any additional text
            Focus only on technical image quality, not content.
            
            <rules>
            - If the image is too dark to see details clearly, set isDark to true. Action needed: 'brighten'
            - If the image is too bright or overexposed, set isBright to true. Action needed: 'darken'
            - If there are visible artifacts, noise, or quality issues, set hasNoiseOrGlitches to true. Action needed: 'repair'
            - If the image looks correct and no action is needed, set recommendedAction to 'photo_looks_correct_no_action_needed'
            </rules>
            
            <example_output>
            {
              "isDark": <true/false>,
              "isBright": <true/false>,
              "hasNoiseOrGlitches": <true/false>,
              "recommendedAction": "<photo_looks_correct_no_action_needed/repair/brighten/darken>"
            }
            </example_output>
            
            You must return ONLY the JSON object with the properties described above WITHOUT any additional text or markdown formatting.
            """,
            [new ReadOnlyMemory<byte>(imageData)],
            maxTokens: 500,
            responseFormat: new { type = "json_object" },
            cancellationToken: cancellationToken);

        var result = JsonSerializer.Deserialize<PhotoQualityAnalysis>(analysis, JsonSerializerOptions.Web)
                     ?? throw new InvalidOperationException("Failed to parse quality analysis");

        _logger.LogInformation("Photo quality analysis completed. Result: {@Result}", result);
        return result;
    }

    private async Task<bool> DetectPersonInImage(string imageUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Detecting person in image: {ImageUrl}", imageUrl);

        var imageData = await DownloadImageAsync(imageUrl, cancellationToken);

        var analysis = await _client.ExecuteVisionPrompt(
            ModelConfiguration.Gpt4o_Mini_202407,
            null,
            "Does this image show a clearly visible person? Respond with just 'true' or 'false'.",
            [new ReadOnlyMemory<byte>(imageData)],
            maxTokens: 10,
            cancellationToken: cancellationToken);

        var result = bool.TryParse(analysis?.Trim(), out var parsed) && parsed;
        _logger.LogInformation("Person detection completed. Result: {Result}", result);
        return result;
    }

    [KernelFunction("analyze_person")]
    [Description("Analyzes a photo to identify physical characteristics of a person")]
    public async Task<string> AnalyzePersonAsync(
        [Description("URL of the photo to analyze")] string imageUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing person in photo: {ImageUrl}", imageUrl);

        // var qualityAnalysis = await AnalyzePhotoQualityAsync(imageUrl, cancellationToken);

        // if (qualityAnalysis.RecommendedAction != "photo_looks_correct_no_action_needed")
        // {
        // _logger.LogInformation("Image quality issues detected. Recommended action: {RecommendedAction} before proceeding with analysis.",
        // qualityAnalysis.RecommendedAction);
        // return $"Image quality issues detected. Recommended action: {qualityAnalysis.RecommendedAction} before proceeding with analysis.";
        // }

        if (!await DetectPersonInImage(imageUrl, cancellationToken))
        {
            _logger.LogInformation("No person detected in the image. The image either doesn't contain a person.");
            return "No person detected in the image. The image either doesn't contain a person.";
        }


        var imageData = await DownloadImageAsync(imageUrl, cancellationToken);

        var result = await _client.ExecuteVisionPrompt(
            ModelConfiguration.Gpt4o_Mini_202407,
            null,
            """
            Analyze this person in detail.
            Consider:
               * Face shape and features
               * Hair color and style
               * Build
               * Gender
               * Any distinctive marks or characteristics

            Provide a formal, precise description focused on permanent physical characteristics.
            It should be concise yet thorough, as this description will be made part of a larger report.
            Focus on the appearance of the person in the image, not on the conclusions or assumptions about the person.
            Use simple, clear language and avoid unnecessary details as this description is used for identification purposes.
            Output only the detailed analysis without any additional text or formatting.
            """,
            [new ReadOnlyMemory<byte>(imageData)],
            maxTokens: 400,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Person analysis completed. Result: {Result}", result);
        return result;
    }
}

public record PhotoQualityAnalysis
{
    public bool IsDark { get; set; }
    public bool IsBright { get; set; }
    public bool HasNoiseOrGlitches { get; set; }
    public string RecommendedAction { get; set; }
}
