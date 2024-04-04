using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

public class C03L03 : Lesson
{
    protected override string LessonName => "C03L03 — Wyszukiwanie i przetwarzanie długich dokumentów";
    protected override string TaskName => "whoami";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C03L03> logger,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var httpClient = httpClientFactory.CreateClient();
        var buildSemanticKernel = BuildSemanticKernel(configuration);

        var person = await GuessPerson(buildSemanticKernel, configuration, httpClient, logger);
        if (person is null)
        {
            throw new Exception("Could not guess the person");
        }

        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), await GetToken(configuration, httpClient),
            JsonSerializer.Serialize(person), httpClient);
        return new { Answer = person, AnswerResponse = answerResponse };
    };

    private async Task<string?> GuessPerson(Kernel semanticKernel, IConfiguration configuration, HttpClient httpClient, ILogger logger)
    {
        const int MaxIterations = 10;
        var historyBuilder = new StringBuilder();
        var answerFunction = semanticKernel.CreateFunctionFromPrompt(Prompt,
            new OpenAIPromptExecutionSettings { MaxTokens = 30, Temperature = 0.2, ChatSystemPrompt = SystemMessage });

        var kernelArguments = new KernelArguments()
        {
            ["history"] = "",
            ["newHint"] = ""
        };
        for (var i = 0; i < MaxIterations; i++)
        {
            var (_, task) = await GetTaskWithToken(configuration, httpClient);
            var hint = task.Hint;
            logger.LogInformation($"Iteration {i + 1} with hint: {hint}");
            kernelArguments["newHint"] = $"- {hint}";

            var functionResult = (await semanticKernel.InvokeAsync(answerFunction, kernelArguments));
            var answer = functionResult.GetValue<string>();
            if (answer is not null && answer.ToLower() is not "\"x\"")
            {
                return answer;
            }

            historyBuilder.AppendLine($"- {hint}");
            kernelArguments["history"] = historyBuilder.ToString();
        }

        return null;
    }

    private const string SystemMessage = """
                                         You are a professional riddle guesser that knows a lot of trivia about the world and famous personas. You will be provided with a trivia in Polish about a famous person and you need to guess who it is. The answer should be provided in Polish.
                                         Rules:
                                         - If you you're not 100% sure, reply with "X"
                                         - If you're sure, reply with the name of the person ONLY without any additional information or comments

                                         ###Example 1
                                         Q:
                                         - Jest piłkarzem
                                         A: "X"
                                         ###
                                         ### Example 2
                                         Q:
                                         - Jest piłkarzem
                                         - Grał w Realu Madryt
                                         A: "X"
                                         ###
                                         ### Example 3
                                         Q:
                                         - Jest piłkarzem
                                         - Grał w Realu Madryt
                                         - W 2002 został królem strzelców Mistrzostw Świata
                                         A: "Ronaldo"
                                         ###
                                         """;

    private const string Prompt = """
                                  ### Current information about the person:
                                  {{$history}}
                                  {{$newHint}}
                                  ###
                                  """;
}
