using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs2_szkolenie.Tasks;

public class C05L01 : Lesson
{
    protected override string LessonName => "C05L01 — Planowanie asystenta";
    protected override string TaskName => "meme";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration) =>
    {
        var httpClient = httpClientFactory.CreateClient();
        var (token, task) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.renderform.io/api/v2/render");
        var imageSource = task.ExtraData["image"].GetString();
        var imageCaption = task.ExtraData["text"].GetString();
        var templateName = configuration["RenderFormTemplateName"]!;
        var stringContent = $$$"""
                               {"version":1713204240314,"template":"{{{templateName}}}","data":{"image.src":"{{{imageSource}}}", "title.text":"{{{imageCaption}}}"}}
                               """;
        request.Content = new StringContent(stringContent, null, "application/json");
        request.Headers.Add("x-api-key", configuration["RenderFormApiKey"]!);
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(responseContent);
        var generatedImageUrl = jsonResponse.RootElement.GetProperty("href").GetString();

        var serializedAnswer = JsonSerializer.Serialize(generatedImageUrl, ToolModel.SerializerOptions);

        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token,
            serializedAnswer, httpClientFactory.CreateClient());
        return new { ImageSource = imageSource, ImageCaption = imageCaption, Answer = generatedImageUrl, AnswerResponse = answerResponse };
    };
}
