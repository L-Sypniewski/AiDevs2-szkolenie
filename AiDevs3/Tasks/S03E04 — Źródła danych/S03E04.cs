using System.Text.Json;
using System.Text.RegularExpressions;
using AiDevs3.AiClients;
using AiDevs3.Tasks.S03E04___Źródła_danych.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AiDevs3.Tasks.S03E04___Źródła_danych;

public class S03E04 : Lesson
{
    private readonly ILogger<S03E04> _logger;
    private readonly S03E04SearchPlugin _searchPlugin;
    private readonly Kernel _kernel;
    private readonly ILoggerFactory _loggerFactory;

    public S03E04(
        IConfiguration configuration,
        HttpClient httpClient,
        S03E04SearchPlugin searchPlugin,
        Kernel kernel,
        ILogger<S03E04> logger,
        ILoggerFactory loggerFactory) : base(configuration, httpClient)
    {
        _searchPlugin = searchPlugin;
        _kernel = kernel;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    protected override string LessonName => "S03E04 — Źródła danych";

    protected override Delegate GetAnswerDelegate => async (CancellationToken cancellationToken) =>
    {
        var dataAboutBarbara = await HttpClient.GetStringAsync($"{CentralaBaseUrl}/dane/barbara.txt", cancellationToken);
        _logger.LogInformation("Retrieved data about Barbara: {Data}", dataAboutBarbara);

        var kernel = _kernel.Clone();
        kernel.Plugins.AddFromObject(_searchPlugin);
        kernel.Plugins.AddFromObject(new S03E04SubmitResultsPlugin(CentralaBaseUrl, ApiKey, HttpClient, _logger));

        var searchAgent = SearchAgent.Create(kernel, _loggerFactory);
        var reasoningAgent = ReasoningAgent.Create(kernel, _loggerFactory);
        var submissionAgent = SubmissionAgent.Create(kernel, _loggerFactory);

        // Define selection strategy function
        var selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
               Determine which agent should take the next turn based on the conversation context and these rules:

               Participants:
               - {{{SearchAgent.AgentName}}}
               - {{{ReasoningAgent.AgentName}}}
               - {{{SubmissionAgent.AgentName}}}

               Turn Selection Rules:
               1. {{{SubmissionAgent.AgentName}}} takes turn when:
                  - Message contains "FOUND_BARBARA_IN:"

               2. {{{ReasoningAgent.AgentName}}} takes turn when:
                  - After each search

               3. {{{SearchAgent.AgentName}}} should continue if:
                  - ReasoningAgent hasn't found Barbara's location yet
                  - Message contains "REQUESTING_SEARCH"
                  - Message contains "CONTINUE_SEARCH"
                  - SubmissionAgent has failed (e.g. contains "SUBMISSION_FAILED")

               Return only the name of the next agent without any additional text.

               {{$history}}
               """,
            safeParameterNames: "history");

        var chat = new AgentGroupChat([searchAgent, reasoningAgent, submissionAgent])
        {
            LoggerFactory = _loggerFactory,
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                {
                    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        ServiceId = ModelConfiguration.Gpt4o_Mini_202407.CreateServiceId()
                    }),
                    InitialAgent = searchAgent,
                    ResultParser = (result) => result.GetValue<string>() ?? SearchAgent.AgentName,
                    HistoryVariableName = "history",
                    // HistoryReducer = new ChatHistoryTruncationReducer(5),
                },
                TerminationStrategy = new RegexTerminationStrategy(new Regex("SUBMISSION_SUCCESSFUL:.*"))
                {
                    MaximumIterations = 30
                }
            }
        };

        const string SystemPrompt = """
                                    You always keep the latest search history from search agent in the context.
                                    It is defined like this:
                                    "Completed searches: [JSON array of objects, e.g. 
                                     {
                                        'type': 'person'|'city',
                                        'query': 'JANUSZ'|'ELBLAG',
                                        'results': ['CITY1', 'CITY2']
                                     }
                                    ]"
                                    """;
        var systemMessage = new ChatMessageContent(AuthorRole.User, SystemPrompt);
        var initialUserContent = new ChatMessageContent(AuthorRole.User, $"Initial data about Barbara:\n{dataAboutBarbara}");
        chat.AddChatMessages([systemMessage, initialUserContent]);

        await foreach (var response in chat.InvokeAsync(cancellationToken))
        {
            _logger.LogInformation("Agent {Name} response: {Response}", response.AuthorName, response.Content);
        }


        var chatMessagesForSubmission = await chat.GetChatMessagesAsync(cancellationToken).ToListAsync(cancellationToken: cancellationToken);
        var localFilePath = Path.Combine(Directory.GetCurrentDirectory(), "chatMessagesForSubmission.json");
        await File.WriteAllTextAsync(localFilePath, JsonSerializer.Serialize(chatMessagesForSubmission, new JsonSerializerOptions { WriteIndented = true }),
            cancellationToken);

        return TypedResults.Json(await chat.GetChatMessagesAsync(cancellationToken).FirstAsync(cancellationToken));
    };
}
