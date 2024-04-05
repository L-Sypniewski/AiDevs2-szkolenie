using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Memory;

namespace AiDevs2_szkolenie.Tasks;

// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/09-memory-with-chroma.ipynb
public class C03L04 : Lesson
{
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

    protected override string LessonName => "C03L04 — Realizowanie złożonych zadań i bazy wektorowe";
    protected override string TaskName => "search";

    private const string CollectionName = "TestCollection";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C03L04> logger,
        [FromServices] ILoggerFactory loggerFactory,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (_, task) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());
        var question = task.Question;

        var httpClient = httpClientFactory.CreateClient();

        var archiveData = (await httpClient.GetFromJsonAsync<ArchiveModel[]>(configuration.GetValue<Uri>("C03L04Url"),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }))!.ToArray();

        var memory = BuildMemory(configuration, loggerFactory);

        await IndexNewData(archiveData, memory, CollectionName);

        var top5Matches = await memory.SearchAsync(CollectionName, question, limit: 3).ToListAsync();

        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), await GetToken(configuration, httpClient),
            JsonSerializer.Serialize(top5Matches[0].Metadata.AdditionalMetadata), httpClient);
        return new { Question = question, Answer = top5Matches, AnswerResponse = answerResponse };
    };

    private static async Task IndexNewData(ArchiveModel[] archiveData, ISemanticTextMemory memory, string collectionName)
    {
        foreach (var data in archiveData)
        {
            var existingElements = await Task.WhenAll(memory.GetAsync(collectionName, key: $"{data.Url}_title"),
                memory.GetAsync(collectionName, key: $"{data.Url}_info"));
            if (existingElements.All(element => element is not null))
            {
                continue;
            }

            await Task.WhenAll(memory.SaveInformationAsync(collectionName, text: data.Title, id: $"{data.Url}_title", additionalMetadata: data.Url),
                memory.SaveInformationAsync(collectionName, text: data.Info, id: $"{data.Url}_info", additionalMetadata: data.Url));
        }
    }
}
