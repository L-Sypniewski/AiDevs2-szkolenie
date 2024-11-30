using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;

namespace AiDevs3.Tasks.S04E02___Przetwarzanie_treści;

public class S04E02 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;

    public S04E02(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
    }

    protected override string LessonName => "S04E02 — Przetwarzanie treści";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        const string DataToVerify = """
                                    01=12,100,3,39
                                    02=-41,75,67,-25
                                    03=78,38,65,2
                                    04=5,64,67,30
                                    05=33,-21,16,-72
                                    06=99,17,69,61
                                    07=17,-42,-65,-43
                                    08=57,-83,-54,-43
                                    09=67,-55,-6,-32
                                    10=-20,-23,-2,44
                                    """;
        var validIdentifiers = new List<string>();
        var lines = DataToVerify.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('=', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            var identifier = parts[0];
            var dataValues = parts[1];

            var prompt = $"""
                          You are provided with research results. Decide whether the results are valid or have been manipulated:
                          {dataValues}
                          """;

            var result = await _semanticKernelClient.ExecutePrompt(
                ModelConfiguration.FineTunedS04E02,
                systemPrompt: null,
                userPrompt: prompt,
                maxTokens: 10,
                temperature: 0.0);

            if (result.Trim().Equals("VALID", StringComparison.OrdinalIgnoreCase))
            {
                validIdentifiers.Add(identifier);
            }
        }

        var response = await SubmitResults("research", validIdentifiers);
        return TypedResults.Json(response);
    };
}
