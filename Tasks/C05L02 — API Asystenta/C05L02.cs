using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

public class C05L02 : Lesson
{
    protected override string LessonName => "C05L02 — API Asystenta";
    protected override string TaskName => "optimaldb";

    private const string Prompt = """
                                  <message role="user">
                                  You're professional copywriter working in Polish. You must keep the essence of the sentences while keeping them as short as possible,convert the sentences to meaningful keywords. The goal is to make the sentence as concise as possible. Remove all references of people names as it will be obvious from the context.

                                  ### Sentence to shorten
                                  {{$sentence}}
                                  ###
                                  </message>
                                  """;


    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());
        var kernel = BuildSemanticKernel(configuration, model: "gpt-3.5-turbo");

        var rawFile = await File.ReadAllTextAsync("Tasks/C05L02 — API Asystenta/compressed-db.json");
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, JsonSerializer.Serialize(rawFile), httpClientFactory.CreateClient());
        return answerResponse;
    };

    private static async Task<FriendsModel> GetFriendsData()
    {
        var filepath = Path.Combine("Tasks", "C05L02 — API Asystenta", "3friends.json");
        var dbContent = await File.ReadAllTextAsync(filepath);
        var friendsData = JsonSerializer.Deserialize<FriendsModel>(dbContent, ToolModel.SerializerOptions)!;
        return friendsData;
    }

    private static async Task<Dictionary<string, string[]>> ShortenedSentences(Kernel kernel, Dictionary<string, string[]> sentences)
    {
        var shortenSentenceFunction = kernel.CreateFunctionFromPrompt(Prompt);

        var settings = new OpenAIPromptExecutionSettings { MaxTokens = 100, Temperature = 0.3 };

        var processedSentences = sentences.ToDictionary(
            sentence => sentence.Key,
            sentence => Task.WhenAll(sentence.Value.Select(async s =>
            {
                var kernelParameters = new KernelArguments(settings) { ["sentence"] = s };
                var completion = await kernel.InvokeAsync(shortenSentenceFunction, kernelParameters);
                return completion.GetValue<string>()!;
            }))
        );

        var resolvedSentences = new Dictionary<string, string[]>();
        foreach (var item in processedSentences)
        {
            resolvedSentences[item.Key] = await item.Value;
        }

        return resolvedSentences;
    }

    private static Dictionary<string, string[]> ProcessedFriendsData(Dictionary<string, string[]> friendsData)
    {
        // also remove all multiple whitespaces and replace with a single space

        var shortenedFriends = friendsData.ToDictionary(
            item => item.Key,
            item => item.Value.Select(s =>
                    Regex.Replace(s.Replace("Zygfryd", "").Replace(" - ", "-").Replace("Stefan", "").Replace("Ania", "").Replace(", ", ","), @"\s+", " ")
                )
                .ToArray());
        return shortenedFriends;
    }
}
