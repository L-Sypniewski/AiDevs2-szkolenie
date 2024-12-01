using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;

namespace AiDevs3.Tasks.S04E04___Aplikacje_i_usługi;

public class S04E04 : Lesson
{
    public class DroneInstruction
    {
        [JsonPropertyName("instruction")]
        public required string Instruction { get; set; }
    }

    public class DroneResponse
    {
        [JsonPropertyName("description")]
        public required string Description { get; set; }
    }

    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S04E04> _logger;

    public S04E04(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S04E04> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S04E04 — Aplikacje i usługi";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        var ngrokUrl = Configuration.GetValue<string>("S04E04_NgrokUrl")!;
        const string EndpointPath = "S04E04/drone";
        var endpointUrl = $"{ngrokUrl}/{EndpointPath}";
        _logger.LogInformation("Submitting webhook URL: {DroneEndpointUrl}", endpointUrl);
        var response = await SubmitResults("webhook", endpointUrl);
        return TypedResults.Ok(response);
    };

    protected override void MapAdditionalEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("drone", async Task<Results<Ok<DroneResponse>, BadRequest<string>>> (HttpContext context, DroneInstruction instruction) =>
        {
            _logger.LogInformation("Received drone instruction: {Instruction}", instruction.Instruction);

            _logger.LogInformation("Executing semantic kernel prompt for navigation");
            var descriptionResponse = await _semanticKernelClient.ExecutePrompt(
                ModelConfiguration.Gpt4o_Mini_202407,
                systemPrompt: null,
                userPrompt: $"{UserPrompt}\n{instruction.Instruction}",
                maxTokens: 1000,
                temperature: 0.0);

            _logger.LogInformation("Semantic kernel response: {Response}", descriptionResponse);

            var description = ExtractContentBetweenTags(descriptionResponse, "FINAL_ANSWER");
            if (string.IsNullOrEmpty(description))
            {
                _logger.LogWarning("No FINAL_ANSWER tags found in response");
                return TypedResults.BadRequest("Invalid response format");
            }

            var trimmedDescription = description.Trim();
            _logger.LogInformation("Generated description: {Description}", trimmedDescription);

            if (trimmedDescription.Split(' ').Length > 2)
            {
                _logger.LogWarning("Invalid description length. Description: {Description}", trimmedDescription);
                return TypedResults.BadRequest($"Description must be maximum two words. Got: '{trimmedDescription}'");
            }

            _logger.LogInformation("Successfully processed drone navigation. Final location: {Description}", trimmedDescription);
            return TypedResults.Ok(new DroneResponse
            {
                Description = trimmedDescription
            });
        });
    }

    private static string ExtractContentBetweenTags(string text, string tagName)
    {
        var startTag = $"<{tagName}>";
        var endTag = $"</{tagName}>";
        var startIndex = text.LastIndexOf(startTag, StringComparison.Ordinal);
        var endIndex = text.LastIndexOf(endTag, StringComparison.Ordinal);

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
        {
            return string.Empty;
        }

        return text.Substring(
            startIndex + startTag.Length,
            endIndex - startIndex - startTag.Length);
    }

    private const string UserPrompt = """
                                      You are a drone navigation assistant. Below is a 4x4 grid map with coordinates and descriptions in Polish:

                                      <MAP>
                                      | (0,0) Start      | (1,0) Trawa           | (2,0) Drzewo           | (3,0) Dom        |
                                      | (0,1) Trawa      | (1,1) Wiatrak         | (2,1) Trawa            | (3,1) Trawa      |
                                      | (0,2) Trawa      | (1,2) Trawa           | (2,2) Skały            | (3,2) Dwa drzewa |
                                      | (0,3) Góry       | (1,3) Góry            | (2,3) Samochód         | (3,3) Jaskinia   |
                                      </MAP>

                                      Follow the navigation instruction step by step. For each movement:
                                      1. State your current position
                                      2. Interpret the next movement
                                      3. Calculate new coordinates
                                      4. Identify what is at the new location

                                      The drone always starts at position (0,0).
                                      Remember:
                                      - X axis goes from left (0) to right (3)
                                      - Y axis goes from top (0) to bottom (3)
                                      - Grid is 4x4

                                      Think step by step:
                                      <REASONING>
                                      Let me follow the instruction step by step:
                                      1. Starting at (0,0) - Start
                                      2. [analyze each movement]
                                      3. [calculate coordinates after each move]
                                      4. Final position is at (X,Y) which contains [location]
                                      </REASONING>

                                      <EXAMPLE>
                                      Example instruction: "poleciałem jedno pole w prawo, a później na sam dół"
                                      
                                          <REASONING>
                                          Let me follow the instruction step by step:
                                          1. Starting at (0,0) - Start
                                          2. First movement: "jedno pole w prawo"
                                             - Moving right increases X by 1
                                             - New position will be (0+1,0) = (1,0)
                                             - At (1,0) there is Drzewo
                                          3. Second movement: "na sam dół"
                                             - Moving down to the bottom means Y becomes 3
                                             - New position will be (1,3)
                                             - At (1,3) there are Góry
                                          4. Final position is at (1,3) which contains Góry
                                          </REASONING>
                                      
                                          <FINAL_ANSWER>
                                          Góry
                                          </FINAL_ANSWER>
                                      </EXAMPLE>

                                      Now process this instruction:
                                      """;
}
