using System.ComponentModel;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S04E01___Interfejs;

public class RepairPhotoPlugin
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<RepairPhotoPlugin> _logger;

    public RepairPhotoPlugin(string apiKey, string baseUrl, SemanticKernelClient semanticKernelClient, ILogger<RepairPhotoPlugin> logger)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    [KernelFunction("repair_photo")]
    [Description("Repairs a photo and returns URL to the processed image")]
    [return: Description("Full URL to the repaired version of the photo")]
    public async Task<string> RepairPhotoAsync(
        [Description("The filename of the photo to repair (e.g., 'IMG_123.PNG' or 'IMG_123-small.PNG')")] string filename)
    {
        _logger.LogInformation("Repairing photo with filename: {Filename}", filename);

        var request = new { task = "photos", apikey = _apiKey, answer = $"REPAIR {filename}" };
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
        _logger.LogInformation("Photo repair completed. Result URL: {ResultUrl}", result);
        return result;
    }
}
