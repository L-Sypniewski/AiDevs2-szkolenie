using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

public class C03L02 : Lesson
{
    protected override string LessonName => "C03L02 — LLM w kodzie aplikacji";
    protected override string TaskName => "scraper";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var httpClient = httpClientFactory.CreateClient("resilient-client");
        const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537";
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

        var (token, (_, _, question, input, _)) = await GetTaskWithToken(configuration, httpClient);

        var textUrl = new Uri(input.ToString()!);
        var context = await httpClient.GetStringAsync(textUrl);
        var semanticKernel = BuildSemanticKernel(configuration);

        var prompt = $"""
                      You answer the questions only by using the context provided below. Do not rely on your knowledge. Always provide the answer in Polish.
                      ###Context
                      {context}
                      ###

                      ###Question
                      {question}
                      ###
                      """;
        var answerFunction = semanticKernel.CreateFunctionFromPrompt(prompt, new OpenAIPromptExecutionSettings { MaxTokens = 150 });

        var answer = (await semanticKernel.InvokeAsync(answerFunction)).GetValue<string>();
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, JsonSerializer.Serialize(answer), httpClient);
        return new { Question = question, Answer = answer, AnswerResponse = answerResponse };
    };
}
