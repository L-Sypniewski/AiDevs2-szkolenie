using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

// https://devblogs.microsoft.com/semantic-kernel/using-semantic-kernel-with-dependency-injection/
// https://gauravmantri.com/2023/12/31/using-openai-function-calling-with-microsoft-semantic-kernel/
// https://www.developerscantina.com/p/semantic-kernel-function-calling
public class C04L01 : Lesson
{
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

    protected override string LessonName => "C04L01 — Narzędzia do automatyzacji";
    protected override string TaskName => "knowledge";

    private const string SystemPrompt =
        "I will ask you a question about the exchange rate, the current population or general knowledge. Decide whether you will take your knowledge from external sources or from the knowledge of the model.";

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C04L01> logger,
        [FromServices] ILoggerFactory loggerFactory,
        [FromQuery] bool autoExecuteFunction,
        [FromServices] OpenAiClient openAiClient,
        [FromQuery] string? customQuestion = null) =>
    {
        var (_, task) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());
        var kernel = BuildSemanticKernel(configuration, model: "gpt-3.5-turbo");
        kernel.ImportPluginFromType<CurrencyPlugin>();
        kernel.ImportPluginFromType<PopulationPlugin>();
        var question = customQuestion ?? task.Question;

        var answer = autoExecuteFunction ? await AutomaticFunctionCalling(kernel, question, logger) : await ManualFunctionCalling(kernel, question, logger);

        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), await GetToken(configuration, httpClientFactory.CreateClient()),
            JsonSerializer.Serialize(answer), httpClientFactory.CreateClient());
        return new { Question = question, Answer = answer, AnswerResponse = answerResponse };
    };

    private static async Task<string> ManualFunctionCalling(Kernel kernel, string question, ILogger logger)
    {
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, SystemPrompt);
        chatHistory.AddMessage(AuthorRole.User, question);

        var promptExecutionSettings = new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
            Temperature = 0
        };
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, promptExecutionSettings, kernel);
        var functionCalls = ((OpenAIChatMessageContent) response).GetOpenAIFunctionToolCalls();
        var content = ((OpenAIChatMessageContent) response).Content;
        logger.LogInformation("ManualFunctionCalling. Function calls: {functionCalls}, content: {content}", functionCalls, content);
        return functionCalls.Count != 0 ? await ExecuteFunctionCall(kernel, functionCalls[0]) : content!;
    }

    private static async Task<string> AutomaticFunctionCalling(Kernel kernel, string question, ILogger logger)
    {
        var promptExecutionSettings = new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0
        };
        var kernelArguments = new KernelArguments(promptExecutionSettings)
        {
            ["system"] = SystemPrompt,
            ["question"] = question
        };
        const string SystemMessage =
            "<message role=\"system\">{{$system}} If you're asked for a numeric value provide the numeric value only without any additonal text</message>";
        const string UserMessage =
            "<message role=\"user\">{{$question}}\nIf you're asked for a numeric value provide the numeric value only without any additonal text</message>";
        const string Prompt = $"{SystemMessage}\n{UserMessage}";
        var result = await kernel.InvokePromptAsync<string>(Prompt, kernelArguments);
        logger.LogInformation("AutomaticFunctionCalling. Result: {result}", result);
        return result!;
    }

    private static async Task<string> ExecuteFunctionCall(Kernel kernel, OpenAIFunctionToolCall openAiFunctionToolCall)
    {
        kernel.Plugins.TryGetFunctionAndArguments(openAiFunctionToolCall, out KernelFunction? pluginFunction, out KernelArguments? arguments);
        var functionResult = await kernel.InvokeAsync(pluginFunction!, arguments!);
        return functionResult.GetValue<object>()?.ToString()!;
    }
}
