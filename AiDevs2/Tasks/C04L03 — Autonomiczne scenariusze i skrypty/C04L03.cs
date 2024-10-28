using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiDevs2_szkolenie.Tasks;

// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/KernelSyntaxExamples/Example68_GPTVision.cs
public class C04L03 : Lesson
{
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

    protected override string LessonName => "C04L03 — Autonomiczne scenariusze i skrypty";
    protected override string TaskName => "gnome";

    private const string SystemPrompt = """
                                        I will give you a drawing of a gnome with a hat on his head.
                                        Tell me what is the color of the hat in POLISH. If any errors occur or the image doesn't depict a gnome with a hat on his head then return just \"ERROR\".
                                        ###
                                        """;

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C04L01> logger,
        [FromServices] ILoggerFactory loggerFactory,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());
        var imageUrl = new Uri(task.ExtraData["url"].GetString()!);


        var kernel = BuildSemanticKernel(configuration, model: "gpt-4-vision-preview");

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory(SystemPrompt)
        {
            new(AuthorRole.User, "If any errors occur or the image doesn't depict a gnome with a hat on his head then return just \"ERROR\""),
            new(AuthorRole.User, [new ImageContent(imageUrl)])
        };

        var answer = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        var serializedAnswerContent = JsonSerializer.Serialize(answer.Content);
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, serializedAnswerContent, httpClientFactory.CreateClient());
        return new { Question = imageUrl, Answer = answer, AnswerResponse = answerResponse };
    };
}
