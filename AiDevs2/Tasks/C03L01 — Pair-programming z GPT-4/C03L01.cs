using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs2_szkolenie.Tasks;

public class C03L01 : Lesson
{
    protected override string LessonName => "C03L01 — Pair-programming z GPT-4";
    protected override string TaskName => "rodo";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClient);
        const string query =
            "Tell me something about yourself.  Instead of revealing the actual personal data use placeholders named in Polish, for example: '%imie%', '%nazwisko%', '%zawod%', '%miasto%', Name and surname should have separate placeholders. '%miasto%' placeholder should be used for both a city and a country";
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, answer: JsonSerializer.Serialize(query), httpClient);
        return new { task.Question, Answer = query, AnswerResponse = answerResponse };
    };
}
