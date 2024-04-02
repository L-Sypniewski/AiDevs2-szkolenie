using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public class C02L02 : Lesson
{
    protected override string LessonName => "C02L02 — Eksplorowanie i omijanie ograniczeń";
    protected override string TaskName => "inprompt";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClient);
        var semanticKernel = BuildSemanticKernel(configuration);
        var nameFromQuestion = await GetNameFromQuestion(semanticKernel, task);
        var filteredContext = (task.Input as List<string>)!.Where(input => input.Contains(nameFromQuestion, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();
        var answeredQuestion = await GetAnswerForQuestion(semanticKernel, task.Question, filteredContext);
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, JsonSerializer.Serialize(answeredQuestion), httpClient);
        return new { task.Question, Answer = answeredQuestion, answerResponse };
    };

    private static async Task<string> GetNameFromQuestion(Kernel kernel, TaskModel task)
    {
        var prompt = $"""
                      You'll be provided with a question about a person. Your task is to extract the name of the person from the question.
                      Output should be a single word with the name of the person.
                      Example:
                      Q: "Co lubi robić Łukasz?"
                      A: "Łukasz"

                      ###Question
                      {task.Question}
                      ###
                      """;
        return (await kernel.InvokePromptAsync(prompt)).GetValue<string>()!;
    }

    private static async Task<string> GetAnswerForQuestion(Kernel kernel, string question, string[] context)
    {
        var prompt = $"""
                      You'll be provided with a question about a person. Answer it using ONLY context provided below. Do not rely on your knowledge.
                      ###Context
                      {string.Join("\n- ", context)}
                      ###

                      Q: {question}
                      """;
        return (await kernel.InvokePromptAsync(prompt)).GetValue<string>()!;
    }
}
