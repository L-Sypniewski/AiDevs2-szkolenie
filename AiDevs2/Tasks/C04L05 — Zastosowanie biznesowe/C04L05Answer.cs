using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

namespace AiDevs2_szkolenie.Tasks;

public class C04L05Answer : Lesson
{
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

    protected override string LessonName => "C04L05 — Zastosowanie biznesowe - answer";
    protected override string TaskName => "ownapipro";

    private const string QdrantCollectionName = "C04L05";

    private const string PromptInitialQuestion = """
                                                 <message role="system">
                                                 You will be provided either with information or a question.
                                                 If you receive an information, you should respond with the following json:
                                                 ```
                                                 {
                                                     "information": "<information>"
                                                 }
                                                 ```
                                                 If you receive a question, you should and respond with the following json:
                                                 ```
                                                 {
                                                     "question": "<question>"
                                                 }
                                                 ```

                                                 ### Example 1:
                                                 Q: What is the capital of Poland?
                                                 A:
                                                 {
                                                     "question": "What is the capital of Poland?"
                                                 }
                                                 ###
                                                 ### Example 2:
                                                 Q: Mieszkam w Polsce.
                                                 A:
                                                 {
                                                     "information": "Mieszkam w Polsce."
                                                 }
                                                 ###
                                                 You must reply with JSON object without any additional information.
                                                 Try using provided context to answer the question.

                                                 ###Context
                                                 {{$context}}
                                                 ###
                                                 </message>
                                                 <message role="user">
                                                 {{$question}}
                                                 </message>
                                                 """;

    private const string PromptActualQuestion = """
                                                 <message role="system">
                                                 You answer the questions.
                                                 Try using provided context to answer the question.

                                                 ###Context
                                                 {{$context}}
                                                 ###
                                                 </message>
                                                 <message role="user">
                                                 {{$question}}
                                                 </message>
                                                 """;

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C04L01> logger,
        [FromServices] ILoggerFactory loggerFactory,
        [FromBody] JsonNode body) =>
    {
        var question = body["question"]!.GetValue<string>();
        var kernel = BuildSemanticKernel(configuration);
        var memory = BuildMemory(configuration, loggerFactory);
        
        logger.LogWarning("Question: {Question}", question);


        var answer = await ReplyToFirstQuestion(question, memory, kernel);

        var answerText = JsonSerializer.Deserialize<JsonObject>(answer);
        logger.LogWarning("answerText: {AnswerText}", answerText);

        if (answerText!.ContainsKey("question"))
        {
            var questionResult = new { reply = await ReplyToActualQuestion(answerText["question"]!.GetValue<string>(), memory, kernel) };
            logger.LogWarning("questionResult: {QuestionResult}", questionResult);
            return TypedResults.Json(questionResult);
        }

        if (answerText.ContainsKey("information"))
        {
            var informationResult = new { reply = await ReplyToInformation(answerText["information"]!.GetValue<string>(), memory) };
            logger.LogWarning("informationResult: {InformationResult}", informationResult);
            return TypedResults.Json(informationResult);
        }

        logger.LogError("answerText doesn't contain expected json keys: {AnswerText}", answerText);
        return TypedResults.Json(new { reply = "Error" }, statusCode: StatusCodes.Status500InternalServerError);
    };

    private static async Task<string> ReplyToFirstQuestion(string question, ISemanticTextMemory semanticTextMemory, Kernel kernel)
    {
        var context = string.Join('\n',
            (await semanticTextMemory.SearchAsync(QdrantCollectionName, question, limit: 3, kernel: kernel).ToArrayAsync()).Select(result =>
                result.Metadata.Text));
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.1
        };
        var kernelArguments = new KernelArguments(promptExecutionSettings)
        {
            ["context"] = context,
            ["question"] = question
        };
        var result = await kernel.InvokePromptAsync<string>(PromptInitialQuestion, kernelArguments);
        return result!;
    }

    private static async Task<string> ReplyToActualQuestion(string question, ISemanticTextMemory semanticTextMemory, Kernel kernel)
    {
        var context = string.Join('\n',
            (await semanticTextMemory.SearchAsync(QdrantCollectionName, question, limit: 3, kernel: kernel).ToArrayAsync()).Select(result =>
                result.Metadata.Text));
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.1
        };
        var kernelArguments = new KernelArguments(promptExecutionSettings)
        {
            ["context"] = context,
            ["question"] = question
        };
        var result = await kernel.InvokePromptAsync<string>(PromptActualQuestion, kernelArguments);
        return result!;
    }

    private static async Task<string> ReplyToInformation(string information, ISemanticTextMemory semanticTextMemory)
    {
        await IndexNewInformation(information, semanticTextMemory, QdrantCollectionName);
        return $"Thanks for the information that: {information}";
    }

    private static async Task IndexNewInformation(string information, ISemanticTextMemory memory, string collectionName)
    {
        var existingInformation = await memory.GetAsync(collectionName, key: information);
        if (existingInformation is not null)
        {
            return;
        }

        await memory.SaveInformationAsync(collectionName, text: information, id: information);
    }

    protected override void MapAdditionalEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("ownapipro", SendAnswerDelegate).WithName($"Handle own API Pro: {LessonName}").WithOpenApi();
    }
}
