using System.Text.Json;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.SemanticKernel;

namespace AiDevs3.Tasks.S03E03___Wyszukiwanie_hybrydowe;

public class S03E03 : Lesson
{
    private readonly ILogger<S03E03> _logger;
    private readonly Kernel _kernel;
    private readonly S03E03DatabasePlugin _databasePlugin;
    private readonly SemanticKernelClient _semanticKernelClient;

    public S03E03(
        IConfiguration configuration,
        HttpClient httpClient,
        Kernel kernel,
        S03E03DatabasePlugin databasePlugin,
        SemanticKernelClient semanticKernelClient,
        ILogger<S03E03> logger) : base(configuration, httpClient)
    {
        _kernel = kernel;
        _databasePlugin = databasePlugin;
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S03E03 — Wyszukiwanie hybrydowe";

    protected override Delegate GetAnswerDelegate => async (CancellationToken cancellationToken) =>
    {
        _logger.LogInformation("Starting S03E03 lesson");

        var settings = new PromptExecutionSettings
        {
            ServiceId = ModelConfiguration.Gpt4_Turbo_202404.CreateServiceId(),
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        _kernel.Plugins.AddFromObject(_databasePlugin);

        var functionResult = await _kernel.InvokePromptAsync(
            """
            Execute SQL query that returns IDs of active datacenters that are managed by inactive managers (is_active=0).
            """,
            new KernelArguments(settings),
            cancellationToken: cancellationToken);

        const string DataCenterIdExtractionPrompt = """
                                                    Extract all datacenter IDs (DC_ID) from the response.
                                                    Return only the IDs as a comma-separated list of integers.
                                                    If no IDs are found, return an empty list.
                                                    The output should be in JSON array format.
                                                    <Example_Output>
                                                    [1,2,3,4,5]
                                                    </Example_Output>
                                                    """;

        var extractedIds = await _semanticKernelClient.ExecutePrompt(
            ModelConfiguration.Phi35_MoE_Instruct,
            systemPrompt: null,
            $"{DataCenterIdExtractionPrompt}\n<jsonResponse>{functionResult}</jsonResponse>",
            maxTokens: 100,
            temperature: 0);


        var idsArray = JsonSerializer.Deserialize<int[]>(extractedIds);

        _logger.LogInformation("Extracted IDs: {Ids}", idsArray);
        var submissionResult = await SubmitResults("database", idsArray!);
        _logger.LogInformation("Submission result: {SubmissionResult}", submissionResult);

        return TypedResults.Json(submissionResult);
    };
}
