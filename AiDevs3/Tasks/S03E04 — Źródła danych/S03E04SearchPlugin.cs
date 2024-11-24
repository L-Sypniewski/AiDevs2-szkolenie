using System.ComponentModel;
using System.Text.Json;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S03E04___Źródła_danych;

public class S03E04SearchPlugin
{
    private const ModelConfiguration LlmModel = ModelConfiguration.Gpt4o_Mini_202407;
    private const double LlmTemperature = 0.2;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<S03E04SearchPlugin> _logger;
    private readonly string _peopleApiUrl;
    private readonly string _placesApiUrl;
    private readonly SemanticKernelClient _semanticKernelClient;

    public S03E04SearchPlugin(
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        IConfiguration configuration,
        ILogger<S03E04SearchPlugin> logger)
    {
        _httpClient = httpClient;
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
        var baseUrl = configuration["CentralaBaseUrl"] ?? throw new InvalidOperationException("CentralaBaseUrl not configured");
        _peopleApiUrl = $"{baseUrl}/people";
        _placesApiUrl = $"{baseUrl}/places";
        _apiKey = configuration["AiDevsApiKey"] ?? throw new InvalidOperationException("AiDevsApiKey not configured");
    }

    [KernelFunction("search_person")]
    [Description("Searches the database for all locations where a specific person has been spotted. Example: search_person('BARBARA')")]
    [return: Description("XML formatted list of locations. Format: <locations person='NAME'>CITY1, CITY2</locations>")]
    public async Task<string> SearchPersonLocations(
        [Description("Person's FIRST name in UPPERCASE without Polish characters. Example: 'JOZEF' instead of 'JÓZEF'. It must be a single word.")] string name)
    {
        // check if the name is a single word
        if (name.Contains(' '))
        {
            throw new ArgumentException("Name must be a single word");
        }

        // if name contains polish characters, replace them with their non-polish equivalents
        name = name.ToUpper()
            .Replace("Ą", "A")
            .Replace("Ć", "C")
            .Replace("Ę", "E")
            .Replace("Ł", "L")
            .Replace("Ń", "N")
            .Replace("Ó", "O")
            .Replace("Ś", "S")
            .Replace("Ź", "Z")
            .Replace("Ż", "Z");
        _logger.LogInformation("Searching locations for person: {Name}", name);
        var jsonResult = await ExecuteSearch(_peopleApiUrl, name.ToUpper());

        var systemPrompt = $"""
                            Extract location names from the JSON response and format them in XML.
                            Return in format:
                            <locations person="{name.ToUpper()}">
                            LOCATION1, LOCATION2, LOCATION3
                            </locations>

                            Rules:
                            - Locations should be in uppercase without Polish characters
                            - Locations should be comma-separated
                            - No additional text or formatting
                            - Some data is restricted, in such cases return "DATA_RESTRICTED"
                            """;

        var result = await _semanticKernelClient.ExecutePrompt(
            LlmModel,
            systemPrompt: null,
            $"{systemPrompt}\n{jsonResult}",
            maxTokens: 100,
            temperature: LlmTemperature);

        _logger.LogInformation("Search results for {Name}: {Result}", name, result);
        return result.Trim();
    }

    [KernelFunction("search_city")]
    [Description("Searches the database for all people spotted in a specific city. Example: search_city('WARSZAWA')")]
    [return: Description("XML formatted list of people. Format: <people location='CITY'>PERSON1, PERSON2</people>")]
    public async Task<string> SearchCityVisitors(
        [Description("City name in UPPERCASE without Polish characters. Example: 'GDANSK' instead of 'GDAŃSK'")] string city)
    {
        _logger.LogInformation("Searching visitors for city: {City}", city);
        var jsonResult = await ExecuteSearch(_placesApiUrl, city.ToUpper());

        var systemPrompt = $"""
                            Extract person names from the JSON response and format them in XML.
                            Return in format:
                            <people location="{city.ToUpper()}">
                            PERSON1, PERSON2, PERSON3
                            </people>

                            Rules:
                            - Names should be in uppercase without Polish characters
                            - Names should be comma-separated
                            - No additional text or formatting
                            - Some data is restricted, in such cases return "DATA_RESTRICTED"
                            """;

        var result = await _semanticKernelClient.ExecutePrompt(
            LlmModel,
            systemPrompt: null,
            $"{systemPrompt}\n{jsonResult}",
            maxTokens: 100,
            temperature: LlmTemperature);

        _logger.LogInformation("Search results for {City}: {Result}", city, result);
        return result.Trim();
    }

    private async Task<string> ExecuteSearch(string apiUrl, string query)
    {
        var request = new
        {
            apikey = _apiKey,
            query
        };

        var response = await _httpClient.PostAsJsonAsync(apiUrl, request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return result?.RootElement.GetRawText() ?? string.Empty;
    }
}
