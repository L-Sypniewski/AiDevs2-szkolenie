using System.ComponentModel;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S04E01___Interfejs;

public class DarkenPhotoPlugin
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<DarkenPhotoPlugin> _logger;

    public DarkenPhotoPlugin(string apiKey, string baseUrl, SemanticKernelClient semanticKernelClient, ILogger<DarkenPhotoPlugin> logger)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    [KernelFunction("darken_photo")]
    [Description("Darkens a photo and returns URL to the processed image")]
    [return: Description("Full URL to the darkened version of the photo")]
    public async Task<string> DarkenPhotoAsync(
        [Description("The filename of the photo to darken (e.g., 'IMG_123.PNG' or 'IMG_123-small.PNG')")] string filename)
    {
        _logger.LogInformation("Darkening photo with filename: {Filename}", filename);

        var request = new { task = "photos", apikey = _apiKey, answer = $"DARKEN {filename}" };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/report", request);
        var responseText = await response.Content.ReadAsStringAsync();

        var extractedFilename = await _semanticKernelClient.ExecutePrompt(
            ModelConfiguration.Gpt4o_Mini_202407,
            null,
            $"""
            Extract just the filename from this response text:
            {responseText}
            Return only the filename, nothing else.
            """,
            maxTokens: 100);

        var result = $"{_baseUrl}/dane/barbara/{extractedFilename.Trim()}";
        _logger.LogInformation("Photo darkening completed. Result URL: {ResultUrl}", result);
        return result;
    }
}
