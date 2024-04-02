using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AiDevs2_szkolenie.Tasks;

public class C01L04_Moderation : Lesson
{
    protected override string LessonName => "C01L04 — OpenAI API i LangChain - moderation";
    protected override string TaskName => "moderation";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClient);
        var answer = await GetModerationResult((task.Input as List<string>)!, openAiClient);

        await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, answer, httpClient);
        return answer;
    };

    private static async Task<string> GetModerationResult(List<string> input, OpenAiClient openAiClient)
    {
        var moderationMatrix = await Task.WhenAll(input.Select(async text => await openAiClient.ShouldBeFlagged(text) ? 1 : 0));
        return JsonSerializer.Serialize(moderationMatrix);
    }
}
