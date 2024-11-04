using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs2_szkolenie.Tasks;

public class C04L02 : Lesson
{
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

    protected override string LessonName => "C04L02 — Automatyzacje z AI";
    protected override string TaskName => "tools";

    private const string SystemPrompt = """
                                        Current date: {{timeCalculation.Date}}
                                        "Add either a todo item list or to the calendar entry (if time is provided) should be added. Rules:
                                        - If a date or relative date (e.g. 'tomorrow' or 'on Monday' is provided, add to the calendar.
                                        - If no time related information is provided, add to the todo list.

                                        ### RULES
                                        You must provide a raw JSON object without markup with the following structure without anything else:
                                        #### Example:
                                        {
                                            "tool": "<ToDo|Calendar>",
                                            "desc": "Task description",
                                            "date": "yyyy-MM-dd" // Optional applicable only for the Calendar tool
                                        }
                                        ####
                                        ###
                                        """;

    protected override Delegate SendAnswerDelegate => async (
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<C04L01> logger,
        [FromServices] ILoggerFactory loggerFactory,
        [FromServices] OpenAiClient openAiClient,
        [FromQuery] string? customQuestion = null) =>
    {
        var (token, task) = await GetTaskWithToken(configuration, httpClientFactory.CreateClient());
        var kernel = BuildSemanticKernel(configuration, model: "gpt-4-turbo");
        kernel.ImportPluginFromType<TimeCalculationPlugin>();
        kernel.ImportPluginFromType<ToDoPlugin>();
        kernel.ImportPluginFromType<CalendarPlugin>();
        var question = customQuestion ?? task.Question;

        var answer = await AutomaticFunctionCalling(kernel, question, logger);
        var serializedAnswer = JsonSerializer.Serialize(answer, ToolModel.SerializerOptions);

        var answerResponse = await AiDevsHelper.SendAnswer(GetBaseUrl(configuration), token,
            serializedAnswer, httpClientFactory.CreateClient());
        return new { Question = question, Answer = answer, AnswerResponse = answerResponse };
    };

    private static async Task<ToolModel> AutomaticFunctionCalling(Kernel kernel, string question, ILogger logger)
    {
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.2
        };
        var kernelArguments = new KernelArguments(promptExecutionSettings)
        {
            ["system"] = SystemPrompt,
            ["question"] = question
        };
        const string SystemMessage = "<message role=\"system\">{{$system}}</message>";
        const string UserMessage = "<message role=\"user\">{{$question}}</message>";
        const string Prompt = $"{SystemMessage}\n{UserMessage}";
        var result = await kernel.InvokePromptAsync<OpenAIChatMessageContent>(Prompt, kernelArguments);
        logger.LogInformation("AutomaticFunctionCalling. Result: {result}", result);
        var resultContent = result?.Content;
        return JsonSerializer.Deserialize<ToolModel>(resultContent!, ToolModel.SerializerOptions)!;
    }
}
