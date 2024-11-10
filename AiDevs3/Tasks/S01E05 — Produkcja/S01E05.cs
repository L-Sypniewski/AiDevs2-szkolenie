using AiDevs3.SemanticKernel;

namespace AiDevs3.Tasks.S01E05___Produkcja;

public class S01E05 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S01E05> _logger;

    public S01E05(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S01E05> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S01E05 — Produkcja";

    protected override Delegate GetAnswerDelegate => async () =>
    {
        var rawData = await GetFile(HttpClient, CentralaBaseUrl, ApiKey, _logger);
        var censoredData = await CensorAgentData(rawData, _semanticKernelClient, _logger);

        var responseContent = await SubmitResults("CENZURA", censoredData);
        _logger.LogInformation("Response content: {ResponseContent}", responseContent);
        return TypedResults.Ok(responseContent);
    };

    private static async Task<string> GetFile(
        HttpClient httpClient,
        string centralaBaseUrl,
        string apiKey,
        ILogger logger)
    {
        var url = $"{centralaBaseUrl}/data/{apiKey}/cenzura.txt";

        logger.LogInformation("Fetching file from {url}", url);
        var response = await httpClient.GetStringAsync(url);
        logger.LogInformation("Retrieved raw data: {Data}", response);

        return response;
    }

    private static async Task<string> CensorAgentData(
        string data,
        SemanticKernelClient semanticKernelClient,
        ILogger logger)
    {
        logger.LogInformation("Censoring data using LLM");

        const string SystemPrompt = """
                                    You're a PII officer who helps anonymize provided data.
                                    <objective>
                                    You will be provided with a text that includes PII data of Polish citizens.
                                    Your task is to substitute each piece of PII data (name + surname, street name + number, city and person's age) with the word 'CENZURA.'
                                    </objective>
                                    
                                    <prompt_rules>
                                    - Identify the following types of PII in the sentence: name and surname, street name and number, city, and age.
                                    - street name and number should be treated as one element. For example 'ulica Modra 3' should be substituted as 'ulica CENZURA'
                                    - Substitute each identified PII element with the word 'CENZURA.'
                                    - You must NOT make other changes to the text. The rest of the sentence should remain intact.
                                    - Only the identified PII should be substituted; no partial substitutions or omissions should occur.
                                    - Under no circumstances should the sentence be altered except for the substitution of the specified PII data.
                                    - You must treat street name and number as one element and substitute them as one, e.g "ul. Piękna 5" -> "ul. CENZURA"
                                    - You must NOT change grammar or punctuation, e.g. "31 lat" -> "CENZURA lat" NOT "CENZURA lata"
                                    - You must keep the rest of the text unchanged no matter what.
                                    </prompt_rules>
                                    
                                    <prompt_examples>
                                    USER: Dane osoby podejrzanej: Marta Kowalska Zamieszkała w Warszawie na ulicy Modrej 3. Ma 58 lat.
                                    AI: Dane osoby podejrzanej: CENZURA. Zamieszkały w CENZURA na ulicy CENZURA. Ma CENZURA lat.
                                    
                                    USER: Informacje o podejrzanym:  Janusz Kowalski. Mieszka w Sopocie przy ulicy Broniewskiego 132. Wiek: 62 lata.
                                    AI: Informacje o podejrzanym: CENZURA. Mieszka w CENZURA przy ulicy CENZURA. Wiek: CENZURA lata.
                                    </prompt_examples>
                                    
                                    <dynamic_context>
                                    This prompt is intended to handle PII data for Polish citizens, substituting the specified personal data elements with "CENZURA" while leaving the rest of the sentence intact.
                                    </dynamic_context>
                                    
                                    <execution_validation>
                                    - Ensure that only the specified PII data is replaced with "CENZURA"
                                    - Verify that all other elements of the sentence remain unchanged.
                                    - Confirm that no partial replacements or omissions occur.
                                    </execution_validation>
                                    
                                    <output_structure>
                                    You must treat street name and number as one element and substitute them as one, e.g "ul. Piękna 5" -> "ul. CENZURA"
                                    You must NOT change grammar or punctuation, e.g. "31 lat" -> "CENZURA lat" NOT "CENZURA lata"
                                    You must output only provided text with PII information substituted with word "CENZURA"
                                    </output_structure>
                                    """;

        var censoredText = await semanticKernelClient.ExecutePrompt(
            "Phi-3.5-MoE-instruct",
            SystemPrompt,
            data,
            maxTokens: 500,
            temperature: 0.0);

        logger.LogInformation("Data censored: {CensoredText}", censoredText);
        return censoredText.Trim();
    }
}
