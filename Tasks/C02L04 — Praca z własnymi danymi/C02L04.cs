using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs2_szkolenie.Tasks;

public class C02L04 : Lesson
{
    protected override string LessonName => "C02L04 - Praca z własnymi danymi";
    protected override string TaskName => "whisper";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, _) = await GetTaskWithToken(configuration, httpClient);
        var openAiApiKey = configuration.GetValue<string>("OpenAiKey")!;

        var autoFileUri = new Uri($"{GetBaseUrl(configuration)}/data/mateusz.mp3");
        var transcription = await SendAudioTranscriptionRequest(httpClient, autoFileUri, openAiApiKey);
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, answer: JsonSerializer.Serialize(transcription), httpClient);
        return new { Answer = transcription, AnswerResponse = answerResponse };
    };

    private static async Task<string> SendAudioTranscriptionRequest(HttpClient client, Uri fileUri, string openAiApiKey)
    {
        var responseFile = await client.GetAsync(fileUri);
        responseFile.EnsureSuccessStatusCode();
        await using var memoryStream = await responseFile.Content.ReadAsStreamAsync();
        using var streamContent = new StreamContent(memoryStream);

        using var content = new MultipartFormDataContent();
        content.Add(streamContent, "file", Path.GetFileName(fileUri.LocalPath));
        content.Add(new StringContent("whisper-1"), "model");
        content.Add(new StringContent("text"), "response_format");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
        var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();

        return responseString;
    }
}
