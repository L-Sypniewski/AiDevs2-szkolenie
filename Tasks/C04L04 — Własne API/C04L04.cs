using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs2_szkolenie.Tasks;

public class C04L04 : Lesson
{
    protected override string LessonName => "C04L04 — Własne API";
    protected override string TaskName => "ownapi";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromQuery] string ownApiBaseUrl) =>
    {
        var (token, _) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());

        var serializedUrl = JsonSerializer.Serialize($"{ownApiBaseUrl}/C04L04Answer/ownapi");
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, serializedUrl, httpClientFactory.CreateClient());
        return answerResponse;
    };
}
