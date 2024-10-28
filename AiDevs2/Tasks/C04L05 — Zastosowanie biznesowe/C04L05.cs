using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs2_szkolenie.Tasks;

public class C04L05 : Lesson
{
    protected override string LessonName => "C04L05 — Zastosowanie biznesowe";
    protected override string TaskName => "ownapipro";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromQuery] string ownApiBaseUrl) =>
    {
        var (token, _) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());

        var serializedUrl = JsonSerializer.Serialize($"{ownApiBaseUrl}/C04L05Answer/ownapipro");
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, serializedUrl, httpClientFactory.CreateClient());
        return answerResponse;
    };
}
