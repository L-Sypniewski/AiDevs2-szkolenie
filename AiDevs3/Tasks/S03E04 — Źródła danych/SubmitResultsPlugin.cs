using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S03E04___Źródła_danych;

public class SubmitResultsPlugin
{
    private readonly string _centralaBaseUrl;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public SubmitResultsPlugin(string centralaBaseUrl, string apiKey, HttpClient httpClient, ILogger logger)
    {
        _centralaBaseUrl = centralaBaseUrl;
        _apiKey = apiKey;
        _httpClient = httpClient;
        _logger = logger;
    }

    [KernelFunction]
    [Description("Submit a location where Barbara was found")]
    public async Task<string> SubmitLocation(string city)
    {
        var payload = new { task = "loop", apikey = _apiKey, answer = city };
        var response = await _httpClient.PostAsJsonAsync($"{_centralaBaseUrl}/report", payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Response from centrala: {Response}", responseContent);
        return responseContent;
    }
}
