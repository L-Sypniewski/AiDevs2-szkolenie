using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace AiDevs2_szkolenie.Tasks;

public class C01L05 : Lesson
{
    protected override string LessonName => "C01L05 — Prompt Engineering";
    protected override string TaskName => "liar";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, _) = await GetTaskWithToken(configuration, httpClient);
        const string Question = "What was Ronaldinho's (the brazilian footballer) first european club?";
        var lierAnswer = await GetAnswer(httpClient, baseUrl: GetBaseUrl(configuration), question: Question, token: token);

        var semanticKernel = BuildSemanticKernel(configuration);
        var isAnswerValid = await ValidateMessageWithSemanticKernel(semanticKernel, question: Question, answer: lierAnswer);
        var answer = isAnswerValid ? "YES" : "NO";

        await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, JsonSerializer.Serialize(answer), httpClient);
        return lierAnswer;
    };

    private static async Task<string> GetAnswer(HttpClient httpClient, string baseUrl, string question, string token)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("question", question)
        });
        var response = await httpClient.PostAsync($"{baseUrl}/task/{token}", content);
        response.EnsureSuccessStatusCode();
        var stringContent = await response.Content.ReadAsStringAsync();
        var answer = JsonSerializer.Deserialize<JsonElement>(stringContent).GetProperty("answer").GetString();

        return answer!;
    }

    private static async Task<bool> ValidateMessageWithSemanticKernel(Kernel kernel, string question, string answer)
    {
        const string SystemMessage = """
                                     <message role=""system"">You're a proffesional lier detector. You will be provided with a question and an answer to it. Your task is to tell me wether the answer is related to the task

                                     As a response I want a simple `YES` or `NO` answer.

                                     Example 1:
                                     ###
                                     Question: What's the capital of Poland
                                     Anwser: Pizza is an Italian dish
                                     Expected result: NO
                                     ###

                                     Example 2:
                                     ###
                                     Question: What is the most popular sports in Europe
                                     Anwser: There are many popular sports in Europe, but the most popular is football
                                     Expected result: YES
                                     ###
                                     """;

        var userPrompt = $"""
                          <message role=""user"">
                          Question:
                          ```
                          {question}
                          ```
                          Answer:
                          ```
                          {answer}
                          ```
                          </message>
                          """;

        var prompt = $"{SystemMessage}\n{userPrompt}";
        var result = (await kernel.InvokePromptAsync(prompt)).GetValue<string>();

        return result!.Equals("YES", StringComparison.InvariantCulture);
    }
}
