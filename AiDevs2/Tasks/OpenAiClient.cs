using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AiDevs2_szkolenie.Tasks;

public record ModerationResponse(
    string Id,
    string Model,
    ModerationResult[] Results);

public record ModerationResult(
    bool Flagged);

public class OpenAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _openAiKey;

    public OpenAiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _openAiKey = configuration.GetValue<string>("OpenAiKey")!;
    }

    public async Task<bool> ShouldBeFlagged(string text, string model = "text-moderation-latest")
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

        var content = new StringContent(JsonSerializer.Serialize(new { input = text, model }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/moderations", content);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var moderationResponse =
            JsonSerializer.Deserialize<ModerationResponse>(responseString, new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true })!;

        return moderationResponse.Results[0].Flagged;
    }
}
