using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Embeddings;

namespace AiDevs2_szkolenie.Tasks;

public class C02L03 : Lesson
{
    protected override string LessonName => "C02L03 - Techniki pracy z GPT-3.5/GPT-4";
    protected override string TaskName => "embedding";

    private const string SentenceToEmbed = "Hawaiian pizza";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] HttpClient httpClient,
        [FromServices] IConfiguration configuration,
        [FromServices] OpenAiClient openAiClient) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClient);
        var semanticKernel = BuildSemanticKernel(configuration);
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050
        var embeddingGenerationService = semanticKernel.GetRequiredService<ITextEmbeddingGenerationService>();
        var embedding = await embeddingGenerationService.GenerateEmbeddingAsync(SentenceToEmbed);
        var embeddingJson = JsonSerializer.Serialize(embedding);
        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token, embeddingJson, httpClient);
        return new { task.Question, Answer = embeddingJson, AnswerResponse = answerResponse };
    };
}
