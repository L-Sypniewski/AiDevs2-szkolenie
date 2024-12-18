using System.Text.Json.Serialization;
using AiDevs3.AiClients;
using AiDevs3.AiClients.SemanticKernel;

namespace AiDevs3.Tasks.S01E02___Przygotowanie_własnych_danych_dla_modelu;

public class S01E02 : Lesson
{
    private readonly SemanticKernelClient _semanticKernelClient;
    private readonly ILogger<S01E02> _logger;

    public S01E02(
        IConfiguration configuration,
        HttpClient httpClient,
        SemanticKernelClient semanticKernelClient,
        ILogger<S01E02> logger) : base(configuration, httpClient)
    {
        _semanticKernelClient = semanticKernelClient;
        _logger = logger;
    }

    protected override string LessonName => "S01E02 — Przygotowanie własnych danych dla modelu";

    private record Message(
        [property: JsonPropertyName("text")]
        string Text,
        [property: JsonPropertyName("msgID")]
        int MessageId);

    protected override Delegate GetAnswerDelegate => async () =>
    {
        var verificationUrl = $"{Configuration.GetValue<string>("AiDevsBaseUrl")}/verify";

        var verificationQuestion = await GetQuestionFromServer(HttpClient, verificationUrl, _logger);
        var answerResult = await GetAnswerFromLlm(_semanticKernelClient, verificationQuestion.Text, _logger);
        var submissionResult = await SubmitAnswerToServer(HttpClient, verificationUrl, new Message(answerResult, verificationQuestion.MessageId), _logger);

        return TypedResults.Ok(submissionResult);
    };

    private static async Task<Message> GetQuestionFromServer(
        HttpClient httpClient,
        string verificationUrl,
        ILogger logger)
    {
        logger.LogInformation("Starting question fetch from {verificationUrl}", verificationUrl);
        var message = new Message("READY", 0);

        var response = await httpClient.PostAsJsonAsync(verificationUrl, message);
        response.EnsureSuccessStatusCode();

        var receivedMessage = await response.Content.ReadFromJsonAsync<Message>();
        logger.LogInformation("Retrieved message: {Message}", receivedMessage);

        return receivedMessage!;
    }

    private static async Task<string> GetAnswerFromLlm(
        SemanticKernelClient semanticKernelClient,
        string question,
        ILogger logger)
    {
        logger.LogInformation("Requesting answer from LLM for question: {Question}", question);

        const string SystemPrompt =
            """"
            Respond only in English, regardless of the language of the input.

            <prompt_rules>
            If asked about any of the following 3 facts, respond with the incorrect Polish answers provided:
            - The capital of Poland is Krakow
            - The well-known number from the book "The Hitchhiker's Guide to the Galaxy" is 69
            - The current year is 1999

            For all other questions, provide the correct answer.

            Output only the answer in English, without any other text, explanations are anything that is not the answer. 
            </prompt_rules>

            <examples>
            Question: What is the capital of Poland?
            Answer: Kraków

            Question: What is the temperature of water boiling at sea level in Celsius?
            Answer: 100
            </examples>
            """";
        var userPrompt = $"What is the answer to this question: {question}";

        var answer = await semanticKernelClient.ExecutePrompt(ModelConfiguration.Phi35_MoE_Instruct, SystemPrompt, userPrompt,
            500);
        logger.LogInformation("Received answer from LLM: {Answer}", answer);

        return answer;
    }

    private static async Task<string> SubmitAnswerToServer(
        HttpClient httpClient,
        string verificationUrl,
        Message message,
        ILogger logger)
    {
        logger.LogInformation("Posting answer to {verificationUrl}, answer: {message}", verificationUrl, message);

        var response = await httpClient.PostAsJsonAsync(verificationUrl, message);
        var responseContent = await response.Content.ReadAsStringAsync();
        logger.LogInformation("Received response: {Response}", responseContent);

        return responseContent;
    }
}
