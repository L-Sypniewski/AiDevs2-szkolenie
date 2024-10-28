using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiDevs3;

public record TaskModel(
    int Code,
    string Msg,
    string Question,
    object Input,
    string Hint,
    string? Data,
    string? Hint1,
    string? Hint2,
    List<string>? Blog)
{
    [JsonExtensionData]
    public IDictionary<string, JsonElement> ExtraData { get; } = new Dictionary<string, JsonElement>();
}

public static class AiDevsHelper
{
    public static async Task<string> GetToken(string baseUrl, string taskName, string apiKey, HttpClient client)
    {
        var content = new StringContent($"{{\"apikey\":\"{apiKey}\"}}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{baseUrl}/token/{taskName}", content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var responseValues = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString)!;
        var result = responseValues["token"].ToString();
        return result!;
    }

    public static async Task<TaskModel> GetTask(string baseUrl, string token, HttpClient client)
    {
        var fromJsonAsync = await client.GetFromJsonAsync<TaskModel>($"{baseUrl}/task/{token}");
        return fromJsonAsync!;
    }

    public static async Task<string> SendAnswer(string baseUrl, string token, string answer, HttpClient client)
    {
        var content = new StringContent($"{{\"answer\":{answer}}}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{baseUrl}/answer/{token}", content);
        return await response.Content.ReadAsStringAsync();
    }
}
